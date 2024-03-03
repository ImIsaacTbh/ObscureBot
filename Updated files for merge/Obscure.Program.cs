// Obscura, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Obscure.Program
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obscure;

public class Program
{
	public class key
	{
		public string auth { get; set; }
	}

	private readonly IConfiguration _configuration;

	private readonly IServiceProvider _services;

	private DiscordSocketClient _client;

	private readonly DiscordSocketConfig _socketConfig = new DiscordSocketConfig
	{
		GatewayIntents = GatewayIntents.All,
		AlwaysDownloadUsers = true
	};

	public static bool kill;

	public static enums.Guilds guilds = new enums.Guilds
	{
		guilds = new List<enums.Guild>()
	};

	public Program()
	{
		_configuration = new ConfigurationBuilder().Build();
		_services = new ServiceCollection().AddSingleton(_configuration).AddSingleton(_socketConfig).AddSingleton<DiscordSocketClient>()
			.AddSingleton((IServiceProvider x) => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
			.AddSingleton<InteractionHandler>()
			.BuildServiceProvider();
	}

	public static void Main(string[] args)
	{
		new Program().RunAsync().GetAwaiter().GetResult();
	}

	public async Task RunAsync()
	{
		DiscordSocketClient client = (_client = _services.GetRequiredService<DiscordSocketClient>());
		client.Log += LogAsync;
		await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
		key key = JsonSerializer.Deserialize<key>(await File.ReadAllTextAsync("c:/botdata/toki.json"));
		await _client.LoginAsync(TokenType.Bot, key.auth);
		await client.StartAsync();
		client.Ready += Client_Ready;
		client.MessageReceived += OnMessageRecieved;
		client.UserJoined += OnUserJoinedGuild;
		client.UserLeft += OnUserLeftGuild;
		while (!kill)
		{
			await Task.Delay(1);
		}
	}

	private async Task Client_Ready()
	{
		new Thread((ThreadStart)async delegate
		{
			await config.InitStorage();
			await Task.Run(async delegate
			{
				while (true)
				{
					await Task.Delay(120000);
					Console.WriteLine("Refreshing Data");
					await config.refreshStorage();
				}
			});
		}).Start();
		await _client.SetStatusAsync(UserStatus.DoNotDisturb);
		await _client.SetGameAsync("Obscurities", null, ActivityType.Listening);
	}

	private async Task LogAsync(LogMessage message)
	{
		Console.WriteLine(message.ToString(null, fullException: true, prependTimestamp: true, DateTimeKind.Local, 11));
	}

	private async Task OnUserLeftGuild(SocketGuild guild, SocketUser user)
	{
		await guild.GetTextChannel(1184481866116501555uL).SendMessageAsync("***" + user.Username + "*** has left the server :(");
	}

	private async Task OnUserJoinedGuild(SocketGuildUser user)
	{
		try
		{
			if (!user.IsBot)
			{
				guilds.GetGuild(user.Guild.Id).AddNew(user);
				if (guilds.GetGuild(user.Guild.Id).config.defaultRole != 0L)
				{
					await user.Guild.GetUser(user.Id).AddRoleAsync(guilds.GetGuild(user.Guild.Id).config.defaultRole);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private async Task OnMessageRecieved(SocketMessage msg)
	{
		try
		{
			if (!msg.Author.IsBot)
			{
				SocketGuild guild = (msg.Channel as SocketGuildChannel).Guild;
				if (guilds.GetGuild(guild.Id).config.levelToggle)
				{
					xpEvent(msg);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static async void xpEvent(SocketMessage msg)
	{
		SocketGuild guild = (msg.Channel as SocketGuildChannel).Guild;
		Random random = new Random();
		enums.User user = guilds.GetGuild(guild.Id).GetUser(msg.Author.Id);
		user.profile.AddXP((ulong)random.Next(1, 5));
		Console.WriteLine("added xp to " + user.profile.username);
		int level = user.profile.level;
		user.profile.totalRecordedMessages++;
		user.profile.RecaculateLevel();
		int level2 = guilds.GetGuild(guild.Id).GetUser(msg.Author.Id).profile.level;
		if (level2 > level)
		{
			int num = level2;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += 5 * (i ^ 2) + 50 * i + 100;
			}
			Embed embed = new EmbedBuilder().WithDescription($"Congrats {msg.Author.Mention}! You leveled up to level {level2} and earned *1000*pickles!\nTotal Messages Send: **{guilds.GetGuild(guild.Id).GetUser(msg.Author.Id).profile.totalRecordedMessages}**\nTotal XP: **{guilds.GetGuild(guild.Id).GetUser(msg.Author.Id).profile.exp}**xp\n Level **{level2 + 1}** Requirement: {num2}\n").WithFooter($"You are {(long)num2 - (long)user.profile.exp}xp away from level {level2 + 1}! \nObscūrus • Team Unity Development").WithCurrentTimestamp()
				.WithThumbnailUrl("https://images-ext-2.discordapp.net/external/tNJAAj1zNcCC76NWawahfu9_EjfXAWBl5wyJD0f4Ots/https/upload.wikimedia.org/wikipedia/commons/thumb/2/24/Stonks_emoji.svg/2425px-Stonks_emoji.svg.png?format=webp&quality=lossless&width=794&height=671")
				.Build();
			user.profile.currency += 1000f;
			await msg.Channel.SendMessageAsync(null, isTTS: false, embed);
		}
	}
}
