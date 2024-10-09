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
using System.Xml.Schema;
using Obscura.Commands.EmbedBuilder;
using System.Security.Cryptography;
using Color = System.Drawing.Color;
using Embed = Discord.Embed;

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
            _client.ModalSubmitted += ModalSubmitted;

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
            string payload = arg.Data.CustomId;
            if(payload.StartsWith("embed:"))
            {
                payload = payload.Remove(0, 6);
                string[] data = payload.Split('_');
                switch (data[0])
                {
                    case "field": 
                        //                        ModalComponentBuilder cB = new ModalComponentBuilder()
                        Modal b = new ModalBuilder(
                                title: $"Field Builder - Field {data[2]}",
                                customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("Title", $"modal:{arg.Data.CustomId}_title", TextInputStyle.Short,
                                "PLACEHOLDER TITLE").AddTextInput("Content", $"modal:{arg.Data.CustomId}_content",
                                TextInputStyle.Paragraph, "PLACEHOLDER CONTENT").AddTextInput("Inline (True or false)", $"modal:{arg.Data.CustomId}inline").Build();
                        //Command.embedBuilders.FirstOrDefault(x => x.embedID == eID);
                        await arg.RespondWithModalAsync(b);
                        break;
                    case "title": 
                        Modal b2 = new ModalBuilder(title: $"Title Builder", customId: $"modal:{arg.Data.CustomId}").AddTextInput("Title", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Short, "PLACEHOLDER TITLE").Build();
                        await arg.RespondWithModalAsync(b2);
                        break;
                    case "color":
                        Modal b3 = new ModalBuilder(title: $"Color", customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("HEX (With #)", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Short,
                                "PLACEHOLDER COLOR").Build();
                        await arg.RespondWithModalAsync(b3);
                        break;
                    case "channel":
                        Modal b4 = new ModalBuilder(title: $"Channel", customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("Channel ID", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Short,
                                                               "PLACEHOLDER CHANNEL ID").Build();
                        await arg.RespondWithModalAsync(b4);
                        break;
                    case "send":
                        int.TryParse(data[1], out int eID);
                        Embed e = Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.Build();
                        Obscura.Commands.EmbedBuilder.Embed embedData =
                            Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed;
                        await ((arg.Channel as IGuildChannel).Guild.GetChannelAsync(embedData.ChannelId).Result as IMessageChannel).SendMessageAsync(embed: e);
                        await arg.DeleteOriginalResponseAsync();
                        await arg.RespondAsync(embed: new EmbedBuilder().WithTitle("Embed Sent").WithDescription($"Embed sent to channel {embedData.ChannelId}").WithColor(Discord.Color.Green).Build());
                        break;
                    case "image":
                        Modal b5 = new ModalBuilder(title: $"Image", customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("Image URL", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Paragraph,
                                                               "PLACEHOLDER IMAGE URL").Build();
                        break;
                    case "thumbnail":
                        Modal b6 = new ModalBuilder(title: $"Thumbnail", customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("Thumbnail URL", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Paragraph,
                                                                                          "PLACEHOLDER THUMBNAIL URL").Build();
                        await arg.RespondWithModalAsync(b6);
                        break;
                    case "description":
                        Modal b7 = new ModalBuilder(title: $"Description", customId: $"modal:{arg.Data.CustomId}")
                            .AddTextInput("Description", $"modal:{arg.Data.CustomId}_data", TextInputStyle.Paragraph,
                                                                                                                     "PLACEHOLDER DESCRIPTION").Build();
                        break;
                }
            }
        }

        async Task ModalSubmitted(SocketModal arg)
        {
            List<SocketMessageComponentData> components = arg.Data.Components.ToList();
            string[] parts = arg.Data.CustomId.Split(':');
            if (parts[2].Contains("field"))
            {
                string payload = parts[2].Remove(0, 6);
                string[] data = payload.Split('_');
                //Console.WriteLine(payload);
                int.TryParse(data[0], out int eID);
                int.TryParse(data[1], out int fID);
                //Console.WriteLine(components.FirstOrDefault(x => x.CustomId == $"{arg.Data.CustomId}_title").Value);
                //Console.WriteLine(components.FirstOrDefault(x => x.CustomId == $"{arg.Data.CustomId}_content").Value);
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.Fields.Add(new Field
                {
                    id = fID,
                    Name = components[0].Value,
                    Value = components[1].Value,
                    Inline = bool.Parse(components[2].Value)
                });
                await arg.RespondAsync($"Field {fID} for embed {eID} set to \"{components[0].Value}\" : \"{components[1].Value}\"", ephemeral: true);
            }

            if (parts[2].Contains("title"))
            {
                string payload = parts[2].Remove(0, 6);
                int.TryParse(payload, out int eID);
                //Console.WriteLine($"TITLE : CUSTOMID : {arg.Data.CustomId} EMBED ID : {eID}");
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.Title = components[0].Value;
                await arg.RespondAsync($"Title for embed {eID} set to \"{components[0].Value}\"", ephemeral: true);
            }

            if (parts[2].Contains("color"))
            {
                string payload = parts[2].Remove(0, 6);
                int.TryParse(payload, out int eID);
                //Console.WriteLine($"COLOR : CUSTOMID : {arg.Data.CustomId} EMBED ID : {eID}");
                Color c;
                try
                {
                    c = (Color)System.Drawing.ColorTranslator.FromHtml(components[0].Value);
                }
                catch
                {
                    await arg.RespondAsync("Invalid color", ephemeral: true);
                    return;
                }

                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.Color = c;
                await arg.RespondAsync($"Color for embed {eID} set to \"{components[0].Value}\"", ephemeral: true);
            }

            if (parts[2].Contains("channel"))
            {
                string payload = parts[2].Remove(0, 8);
                int.TryParse(payload, out int eID);
                //Console.WriteLine($"CHANNEL : CUSTOMID : {arg.Data.CustomId} EMBED ID : {eID}");
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.ChannelId = ulong.Parse(components[0].Value);
                await arg.RespondAsync($"Channel for embed {eID} set to \"{components[0].Value}\"", ephemeral: true);
            }

            if (parts[2].Contains("image"))
            {
                string payload = parts[2].Remove(0, 6);
                int.TryParse(payload, out int eID);
                //Console.WriteLine($"CHANNEL : CUSTOMID : {arg.Data.CustomId} EMBED ID : {eID}");
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.ImageURL = components[0].Value;
                await arg.RespondAsync($"Image for embed {eID} set to \"[IMAGE]({components[0].Value})\"");
            }

            if (parts[2].Contains("thumbnail"))
            {
                string payload = parts[2].Remove(0, 10);
                int.TryParse(payload, out int eID);
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.ThumbnailImageUrl = components[0].Value;
                await arg.RespondAsync($"Thumbnail for embed {eID} set to \"[THUMBNAIL]({components[0].Value})\"", ephemeral: true);
            }

            if (parts[2].Contains("description"))
            {
                string payload = parts[2].Remove(0, 12);
                int.TryParse(payload, out int eID);
                Command.embedBuilders.FirstOrDefault(x => x.embedID == eID).embed.Description = components[0].Value;
                await arg.RespondAsync($"Description for embed {eID} set to \"{components[0].Value}\"", ephemeral: true);
            }
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
                        case InteractionCommandError.Unsuccessful:
                            await context.Interaction.RespondAsync("Command execution unsuccessful.", ephemeral: true); break;
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
