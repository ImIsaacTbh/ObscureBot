using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Obscure.API;
using System.Data;
using System.Text;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;

namespace Obscure.Commands
{
    public class Administration : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;

        public readonly InteractiveService _interactive;
        public Administration(InteractionHandler handler, DiscordSocketClient client, InteractiveService interactive)
        {
            _handler = handler;
            _client = client;
            _interactive = interactive; 
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
                await Task.Delay(10);
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
                    else { desc.AppendLine($"**{count}:** {user.GlobalName} ({user.Mention}) \n> **Joined:** <t:{useringuild.JoinedAt.Value.ToUnixTimeSeconds()}:R> \n> **Is boosting:** {boosting} \n"); }

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

            if (roles.Count() % 3 != 0) { embed.AddField($"⠀", $"⠀", true); }
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("setlogchannel", "Sets the server's audit log channel")]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task setrole(IGuildChannel channel)
        {
            Program.guilds.GetGuild(Context.Guild.Id).config.auditlogChannel = channel.Id;
            await RespondAsync($"Set audit log channel to <#{channel.Id}>", ephemeral: true);
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

        public string list = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public char GetRandomCharacter(string text, Random rng)
        {
            int index = rng.Next(text.Length);
            return text[index];
        }

        [SlashCommand("verifyfor", "Verify or unverify other users")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task verifyothers(string id, bool truefalse)
        {
            ulong.TryParse(id, out var value);
            int isrole = -1;
            if (Context.Guild.GetRole(value) != null) { isrole = 1; }
            else if (Context.Guild.GetUser(value) != null) { isrole = 2; }
            else if (Context.Guild.GetUser(value) == null && Context.Guild.GetRole(value) == null) { isrole = 0; }
            else { isrole = 0; }
            Console.WriteLine(isrole);
            if (isrole == 0) { Context.Interaction.RespondAsync("Not a valid ID", ephemeral: true); return;}
            else if (isrole == 1)
            {
                foreach (IUser user in Context.Guild.GetRole(value).Members)
                {
                    var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);
                    p.profile.isVerified = truefalse;

                }
                await RespondAsync($"Doned", ephemeral: true);
            }
            else if (isrole == 2)
            {
                var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(value);
                p.profile.isVerified = truefalse;
                await RespondAsync("Doned", ephemeral: true);
            }

        }


        [SlashCommand("verify", "Recieve a captcha test to verify yourself")]
        public async Task captchamoment()
        {
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);
            var g = Context.Guild;
            string captcha = "";
            var rnd = new Random();
            for (int i = 0; i < 5; i++)
            {
                captcha += GetRandomCharacter(list, rnd);
            }
            try
            {
                IMessageChannel dm = await Context.User.CreateDMChannelAsync();
                await dm.SendMessageAsync($"Please reply with the following string to prove you are not a robot: `{captcha}` \nThis captcha will expire in 5 minutes from now if you do not respond!");
                await Context.Interaction.DeferAsync();
            captcha:
                var result = await _interactive.NextMessageAsync(x => x.Channel.Id == dm.Id, timeout: TimeSpan.FromSeconds(300));
                if (result.Value.Content.Contains(captcha))
                {
                    Console.WriteLine("Correct");
                    await dm.SendMessageAsync("Verified!");
                    p.profile.isVerified = true;
                    return;
                }
                else
                {
                    Console.WriteLine("Balls");
                    await dm.SendMessageAsync("This is incorrect");
                    goto captcha;
                }
                await dm.SendMessageAsync("Time ran out, please use the captcha command to request a new captcha");
            }
            catch (Exception ex)
            {
                await RespondAsync("Your dms are disabled, please open them to allow the bot to dm you for verification.");
                return;
            }
            await Context.Interaction.DeferAsync();
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
