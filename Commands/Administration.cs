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
    public class Administration : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;


        public Administration(InteractionHandler handler, DiscordSocketClient client)
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

        [SlashCommand("setstatus", "Set the bot status")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task setStatus(options type, string status)
        {
            await RespondAsync("Status set!", ephemeral: true);
            switch (type)
            {
                case options.Playing:
                    await _client.SetGameAsync(status, null, ActivityType.Playing); break;
                case options.Listening:
                    await _client.SetGameAsync(status, null, ActivityType.Listening); break;
                case options.Competing:
                    await _client.SetGameAsync(status, null, ActivityType.Competing); break;
                case options.Watching:
                    await _client.SetGameAsync(status, null, ActivityType.Watching); break;

            }

        }

        [SlashCommand("users", "See how many users are in a role")]
        [RequireUserPermission(GuildPermission.UseApplicationCommands)]
        public async Task calcRoleUsers(IRole role, bool detailed)
        {

            int count = 0;
            string boosting;
            StringBuilder desc = new StringBuilder();
            if (detailed) { desc.AppendLine("### Detailed View:\n"); }
            foreach (IUser user in Context.Guild.Users)
            {
                Task.Delay(10);
                var useringuild = Context.Guild.GetUser(user.Id);
                if (useringuild.Roles.Contains(role))
                {
                    if (useringuild.PremiumSince == null)
                    {
                        boosting = "No";
                    }
                    else
                    {
                        boosting = $"Boosting since: {useringuild.PremiumSince}";
                    }
                    count++;

                    if (!detailed) { desc.AppendLine($"**{count}:** {user.GlobalName} ({user.Mention}"); }
                    else { desc.AppendLine($"**{count}:** {user.GlobalName} ({user.Mention}) \n> **Joined at:** <t:{useringuild.JoinedAt.Value.ToUnixTimeSeconds()}:R> \n> **Is boosting:** {boosting} \n"); }

                }
                else;

            }
            var embed = new EmbedBuilder()
            {
                Title = $"\nThere are {count} users in \"{role.Name}\"!",
                Description = desc.ToString(),
            };
            embed.WithColor(role.Color);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
        }


        [SlashCommand("roles", "See info about the server's roles")]
        [RequireUserPermission(GuildPermission.UseApplicationCommands)]
        public async Task showRolesInfo()
        {
            int count = 0;
            int usersInRole = 0;
            bool moderative;


            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "",
                Description = "",

            };

            IRole[] roles = Context.Guild.Roles.OrderBy(x => x.Position).Reverse().ToArray();

            foreach (IRole role in roles)
            {
                count++;
                Thread.Sleep(10);
                foreach (IUser user in Context.Guild.Users)
                {
                    var p = Context.Guild.GetUser(user.Id);
                    if (p.Roles.Contains(role)) { usersInRole++; }
                }

                if (role.Permissions.KickMembers == true) { moderative = true; }
                else { moderative = false; }

                embed.Title = $"This server has: {count} roles";
                embed.AddField($"⠀", $"> {role.Mention} \n> Users: {usersInRole} \n> Moderative role: {moderative} \n> Taggable: {role.IsMentionable}", true);
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                usersInRole = 0;
            }


            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("setdefaultrole", "Sets the server's auto role")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task setdrole(IRole role)
        {
            Program.guilds.GetGuild(Context.Guild.Id).config.defaultRole = role.Id;
            await RespondAsync($"Set default role to {role.Mention}", ephemeral: true);
        }

        [SlashCommand("toggleleveling", "Toggles the bot's xp system")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task togglelevel()
        {
            Program.guilds.GetGuild(Context.Guild.Id).config.levelToggle = !Program.guilds.GetGuild(Context.Guild.Id).config.levelToggle;
            await RespondAsync("Success", ephemeral: true);
        }

        [SlashCommand("refreshdata", "Writes to all data files")]
        [RequireUserPermissionAttribute(GuildPermission.Administrator)]
        public async Task aaa()
        {
            await config.refreshStorage();
            await RespondAsync("done");
        }
    }
}
