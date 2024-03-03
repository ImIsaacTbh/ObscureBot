// Obscura, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Obscure.Commands.Administration
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Obscure;
using Obscure.Commands;

public class Administration : InteractionModuleBase<SocketInteractionContext>
{
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

	private InteractionHandler _handler;

	private readonly DiscordSocketClient _client;

	public InteractionService commands { get; set; }

	public Administration(InteractionHandler handler, DiscordSocketClient client)
	{
		_handler = handler;
		_client = client;
	}

	[SlashCommand("setstatus", "Set the bot status", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.Administrator)]
	public async Task setStatus(options type, string status)
	{
		await RespondAsync("Status set!", null, isTTS: false, ephemeral: true);
		switch (type)
		{
		case options.Playing:
			await _client.SetGameAsync(status);
			break;
		case options.Listening:
			await _client.SetGameAsync(status, null, ActivityType.Listening);
			break;
		case options.Competing:
			await _client.SetGameAsync(status, null, ActivityType.Competing);
			break;
		case options.Watching:
			await _client.SetGameAsync(status, null, ActivityType.Watching);
			break;
		}
	}

	[SlashCommand("users", "See how many users are in a role", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.UseApplicationCommands)]
	public async Task calcRoleUsers(IRole role, bool detailed)
	{
		int count = 0;
		StringBuilder desc = new StringBuilder();
		if (detailed)
		{
			desc.AppendLine("### Detailed View:\n");
		}
		foreach (SocketGuildUser user in base.Context.Guild.Users)
		{
			await Task.Delay(10);
			SocketGuildUser user2 = base.Context.Guild.GetUser(((IEntity<ulong>)user).Id);
			if (user2.Roles.Contains(role))
			{
				string value = (user2.PremiumSince.HasValue ? $"Boosting since: {user2.PremiumSince}" : "No");
				count++;
				if (!detailed)
				{
					StringBuilder stringBuilder = desc;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(8, 3, stringBuilder);
					handler.AppendLiteral("**");
					handler.AppendFormatted(count);
					handler.AppendLiteral(":** ");
					handler.AppendFormatted(((IUser)user).GlobalName);
					handler.AppendLiteral(" (");
					handler.AppendFormatted(((IMentionable)user).Mention);
					stringBuilder2.AppendLine(ref handler);
				}
				else
				{
					StringBuilder stringBuilder = desc;
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(54, 5, stringBuilder);
					handler.AppendLiteral("**");
					handler.AppendFormatted(count);
					handler.AppendLiteral(":** ");
					handler.AppendFormatted(((IUser)user).GlobalName);
					handler.AppendLiteral(" (");
					handler.AppendFormatted(((IMentionable)user).Mention);
					handler.AppendLiteral(") \n> **Joined:** <t:");
					handler.AppendFormatted(user2.JoinedAt.Value.ToUnixTimeSeconds());
					handler.AppendLiteral(":R> \n> **Is boosting:** ");
					handler.AppendFormatted(value);
					handler.AppendLiteral(" \n");
					stringBuilder3.AppendLine(ref handler);
				}
			}
		}
		EmbedBuilder embedBuilder = new EmbedBuilder
		{
			Title = $"\nThere are {count} users in \"{role.Name}\"!",
			Description = desc.ToString()
		};
		embedBuilder.WithColor(role.Color);
		embedBuilder.WithFooter("Obscūrus • Team Unity Development");
		embedBuilder.WithCurrentTimestamp();
		await RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, null, embedBuilder.Build());
	}

	[SlashCommand("roles", "See info about the server's roles", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.UseApplicationCommands)]
	public async Task showRolesInfo()
	{
		int num = 0;
		int num2 = 0;
		EmbedBuilder embedBuilder = new EmbedBuilder
		{
			Title = "",
			Description = ""
		};
		IRole[] array = base.Context.Guild.Roles.OrderBy((SocketRole x) => x.Position).Reverse().ToArray();
		IRole[] array2 = array;
		array = array2;
		foreach (IRole role in array)
		{
			num++;
			Thread.Sleep(10);
			foreach (SocketGuildUser user in base.Context.Guild.Users)
			{
				if (base.Context.Guild.GetUser(((IEntity<ulong>)user).Id).Roles.Contains(role))
				{
					num2++;
				}
			}
			bool value = (role.Permissions.KickMembers ? true : false);
			embedBuilder.Title = $"This server has: {num} roles";
			embedBuilder.AddField("⠀", $"> {role.Mention} \n> Users: {num2} \n> Moderative role: {value} \n> Taggable: {role.IsMentionable}", inline: true);
			embedBuilder.WithFooter("Obscūrus • Team Unity Development");
			embedBuilder.WithCurrentTimestamp();
			num2 = 0;
		}
		if (array2.Count() % 3 != 0)
		{
			embedBuilder.AddField("⠀", "⠀", inline: true);
		}
		await RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, null, embedBuilder.Build());
	}

	[SlashCommand("setdefaultrole", "Sets the server's auto role", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.Administrator)]
	public async Task setdrole(IRole role)
	{
		Program.guilds.GetGuild(base.Context.Guild.Id).config.defaultRole = role.Id;
		await RespondAsync("Set default role to " + role.Mention, null, isTTS: false, ephemeral: true);
	}

	[SlashCommand("toggleleveling", "Toggles the bot's xp system", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.Administrator)]
	public async Task togglelevel()
	{
		Program.guilds.GetGuild(base.Context.Guild.Id).config.levelToggle = !Program.guilds.GetGuild(base.Context.Guild.Id).config.levelToggle;
		await RespondAsync("Success", null, isTTS: false, ephemeral: true);
	}

	[SlashCommand("refreshdata", "Writes to all data files", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.Administrator)]
	public async Task aaa()
	{
		await config.refreshStorage();
		await RespondAsync("done");
	}
}
