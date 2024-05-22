using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Obscure;

namespace Obscura.Commands.EmbedBuilder
{
    public class Command : ObscureInteractionModuleBase
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;




        private readonly InteractiveService _interactive;

        public Command(InteractionHandler handler, DiscordSocketClient client, InteractiveService interactive)
        {
            _handler = handler;
            _client = client;
            _interactive = interactive;
        }

        public static List<EmbedBuilderData> embedBuilders = new List<EmbedBuilderData>();

        [SlashCommand("buildembed", "Starts an embed builder instance")]
        [RequireUserPermission(Discord.GuildPermission.ManageMessages)]
        public async Task embedBuilder(
            [Choice("0", 0)]
            [Choice("1", 1)]
            [Choice("2", 2)]
            [Choice("3", 3)]
            [Choice("4", 4)]
            [Choice("5", 5)]
            [Choice("6", 6)]
            [Choice("7", 7)]
            [Choice("8", 8)]
            [Choice("9", 9)]
            [Choice("9", 9)]
            [Choice("10", 10)]
            int numberOfFields)
        {
            int embedID = embedBuilders.Count + 1;
            embedBuilders.Add(new EmbedBuilderData()
            {
                embedID = embedID,
                embed = new Embed()
            });

            embedBuilders[embedID-1].numberOfFields = numberOfFields;
            Discord.EmbedBuilder e = new Discord.EmbedBuilder();
            e.WithTitle("Embed Builder")
                .WithDescription($"New embed builder instance with ID: {embedID} created with {numberOfFields} fields.")
                .WithFooter("Obscūrus • Team Unity Development")
                .WithCurrentTimestamp();

            var builder = new ComponentBuilder();
            builder.WithButton($"Title", customId: $"embed:title_{embedID}", ButtonStyle.Primary);
            builder.WithButton($"Color", customId: $"embed:color_{embedID}", ButtonStyle.Primary);
            builder.WithButton("Channel", customId: $"embed:channel_{embedID}", ButtonStyle.Primary);
            builder.WithButton("Image", customId: $"embed:image_{embedID}", ButtonStyle.Primary);
            for (int i = 0; i < numberOfFields; i++)
            {
                builder.WithButton($"Field {i + 1}", customId: $"embed:field_{embedID}_{i}", ButtonStyle.Secondary);
            }
            builder.WithButton("Send", customId: $"embed:send_{embedID}", ButtonStyle.Danger);
            await RespondAsync(embed: e.Build(), components: builder.Build());
        }

    }
}
