// Obscura, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Obscure.InteractionHandler
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

public class InteractionHandler
{
	private readonly DiscordSocketClient _client;

	private readonly InteractionService _handler;

	private readonly IServiceProvider _services;

	private readonly IConfiguration _configuration;

	public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
	{
		_client = client;
		_handler = handler;
		_services = services;
		_configuration = config;
	}

	public async Task InitializeAsync()
	{
		_client.Ready += ReadyAsync;
		_handler.Log += LogAsync;
		await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
		_client.ButtonExecuted += ButtonExecuted;
		_client.InteractionCreated += HandleInteraction;
	}

	private async Task LogAsync(LogMessage log)
	{
		Console.WriteLine(log);
	}

	private async Task ReadyAsync()
	{
		await _handler.RegisterCommandsGloballyAsync();
	}

	private async Task ButtonExecuted(SocketMessageComponent arg)
	{
		await arg.Message.DeleteAsync();
	}

	private async Task HandleInteraction(SocketInteraction interaction)
	{
		try
		{
			SocketInteractionContext context = new SocketInteractionContext(_client, interaction);
			IResult result = await _handler.ExecuteCommandAsync(context, _services);
			if (!result.IsSuccess)
			{
				InteractionCommandError? error = result.Error;
				if (error.HasValue && error.GetValueOrDefault() == InteractionCommandError.UnmetPrecondition)
				{
					Console.WriteLine("FUCK");
					Console.WriteLine(InteractionCommandError.UnmetPrecondition.ToString());
				}
			}
		}
		catch
		{
			if (interaction.Type == InteractionType.ApplicationCommand)
			{
				await interaction.GetOriginalResponseAsync().ContinueWith((Func<Task<RestInteractionMessage>, Task>)async delegate(Task<RestInteractionMessage> msg)
				{
					await msg.Result.DeleteAsync();
				});
			}
		}
	}
}
