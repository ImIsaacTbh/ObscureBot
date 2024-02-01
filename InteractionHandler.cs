using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Obscure
{
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
            => Console.WriteLine(log);

        private async Task ReadyAsync()
        {
            await _handler.RegisterCommandsGloballyAsync();
        }

        async Task ButtonExecuted(SocketMessageComponent arg)
        {
            await arg.Message.DeleteAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _handler.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            Console.WriteLine("FUCK");
                            Console.WriteLine(InteractionCommandError.UnmetPrecondition.ToString());
                            break;
                        default:
                            break;
                    }
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
