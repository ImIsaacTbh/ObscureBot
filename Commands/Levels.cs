using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using static System.Net.WebRequestMethods;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;
using Color = System.Drawing.Color;
using System.Reflection;

namespace Obscure.Commands
{
    public class Levels : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;


        public Levels(InteractionHandler handler, DiscordSocketClient client)
        {
            _handler = handler;
            _client = client;
        }
        public class key
        {
            public string auth { get; set; }
        }

        public enum options
        {
            Listening,
            Watching,
            Playing,
            Competing
        }

        public enum leaderboard
        {
            Experience,
            Pickles
        }

        [SlashCommand("leaderboard", "Display server's leaderboard")]
        public async Task leaderboardT(leaderboard option, int top)
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Leaderboards!",
            };
            if (option == leaderboard.Experience)
            {
                enums.User[] people = Program.guilds.GetGuild(Context.Guild.Id).users.ToArray().OrderBy(x => x.profile.exp).Reverse().ToArray();
                if (top > people.Length || top > 25) { await RespondAsync("Fuck off that's too many!", ephemeral: true); return; }
                for (int i = 0; i < top; i++)
                {
                    var p = people[i];
                    embed.AddField($"#{i + 1} - {Context.Guild.GetUser(p.profile.id).GlobalName} *({p.profile.username})*", $"**Level:** {p.profile.level} with **{p.profile.exp}**exp", false);
                }
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
            }
            else if (option == leaderboard.Pickles)
            {
                enums.User[] people = Program.guilds.GetGuild(Context.Guild.Id).users.ToArray().OrderBy(x => (x.profile.currency + x.profile.bank)).Reverse().ToArray();

                if (top > people.Length || top > 25) { await RespondAsync("Fuck off that's too many!", ephemeral: true); return; }
                for (int i = 0; i < top; i++)
                {
                    var p = people[i];
                    embed.AddField($"#{i + 1} - {Context.Guild.GetUser(p.profile.id).GlobalName} *({p.profile.username})*", $"**{p.profile.currency + p.profile.bank}**pickles", false);
                }
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
            }
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("profile", "See a users stats!")]
        public async Task profileCmd(IGuildUser user)
        {
            if (user.IsBot) { await RespondAsync("Fuck off that's a bot", ephemeral: true); return; }
            var uP = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"{uP.profile.username}'s Profile",
                ThumbnailUrl = user.GetAvatarUrl(),


            };
            embed.AddField("Total XP", $"{uP.profile.exp}exp", true);
            embed.AddField("Level:", uP.profile.level, true);
            embed.AddField("Total Messages Recorded:", uP.profile.totalRecordedMessages, false);
            embed.AddField("Punishments:", uP.punishments.criminalRecord.Count, false);
            embed.AddField("Wallet:", $"{uP.profile.currency}pickles", false);
            embed.AddField("Bank:", $"{uP.profile.bank}pickles", false);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
        }
    }
}
