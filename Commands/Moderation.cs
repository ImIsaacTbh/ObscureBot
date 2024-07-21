// Obscura, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Obscura.Commands.Moderation
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Obscure.Commands;
using Obscure;
using Obscure.API;

public class Moderation : InteractionModuleBase
{
	public enum WarnUnit
	{
		Day,
		Hour,
		Minute,
		Second,
		Year
	}

	public class user
	{
		public string UserName { get; set; }

		public int msgSent { get; set; }

		public int xp { get; set; }

		public int level { get; set; }

		public int warns { get; set; }
	}

	private InteractionHandler _handler;

	private readonly DiscordSocketClient _client;

	public InteractionService commands { get; set; }

	public Moderation(InteractionHandler handler, DiscordSocketClient client)
	{
		_handler = handler;
		_client = client;
	}

	[SlashCommand("slowmode", "Sets the slowmode for a channel.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ManageChannels)]
	public async Task slowmode(int time)
	{
		await (base.Context.Channel as SocketTextChannel).ModifyAsync(delegate(TextChannelProperties x)
		{
			x.SlowModeInterval = time;
		});
		EmbedBuilder embedBuilder = new EmbedBuilder();
		embedBuilder.WithTitle("Slowmode Enabled").WithDescription($"User <@{base.Context.Interaction.User.Id}> has enabled **slowmode** in <#{base.Context.Channel.Id}>. \n Time between messages: **{time}**s").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
		await base.Context.Interaction.RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, embedBuilder.Build());
	}

	[SlashCommand("ban", "Banishes a user from the plane of existence.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.BanMembers)]
	public async Task ban(IGuildUser u, string reason = "Unspecified")
	{
		User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Add(new enums.Punishment
		{
			date = DateTime.UtcNow,
			duration = TimeSpan.FromMicroseconds(1.0),
			id = User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Count + 1,
			reason = reason,
			type = enums.PunishmentType.Ban,
			punisher = base.Context.User.Username
		});
		await u.BanAsync(0, reason);
		new EmbedBuilder().WithTitle("User Banned").WithDescription($"User <@{u.Id}> has been **banned** by <@{base.Context.User.Id}>. \nReason: `{reason}` \n<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
	}

	[SlashCommand("unban", "Allows a user passage to the plane of existence.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.BanMembers)]
	public async Task unban(string u, string reason = "Unspecified")
	{
		ulong id = ulong.Parse(u);
		IUser usr = await _client.GetUserAsync(id);
		if (await base.Context.Guild.GetBanAsync(usr) == null)
		{
			await ReplyAsync("User is not banned!?!?");
			return;
		}
		await base.Context.Guild.RemoveBanAsync(usr);
		new EmbedBuilder().WithTitle("User Unbanned").WithDescription($"User <@{usr.Id}> has been **unbanned** by <@{base.Context.User.Id}>. \nReason: `{reason}` \n<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
	}

	[SlashCommand("kick", "Kicks a user from the server.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.KickMembers)]
	public async Task kick(IUser u, string reason = "Unspecified", bool reinvite = false)
	{
		User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Add(new enums.Punishment
		{
			date = DateTime.UtcNow,
			duration = TimeSpan.FromMicroseconds(1.0),
			id = User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Count + 1,
			reason = reason,
			type = enums.PunishmentType.Mute,
			punisher = base.Context.User.Username
		});
		EmbedBuilder dmBuilder = new EmbedBuilder().WithTitle("Kick Notice").WithDescription($"You have been kicked from {base.Context.Guild.Name} by {base.Context.User} for `{reason}`.").WithFooter("Harmony Moderation");
		if (!reinvite)
		{
			try
			{
				await u.SendMessageAsync(null, isTTS: false, dmBuilder.Build());
			}
			catch (Exception)
			{
			}
		}
		//RestInviteMetadata invite = (await base.Context.Guild.GetInvitesAsync()).SingleOrDefault((RestInviteMetadata x) => x.Code == "GxdJMRcwsG");
		try
		{
			await u.SendMessageAsync(null, isTTS: false, dmBuilder.Build());
			await u.SendMessageAsync("https://discord.gg/58TaSJbyJm"); //hardcoded this for now until isaac fixes error above
		}
		catch (Exception)
		{
		}
		await ((IGuildUser)u).KickAsync(reason);
		EmbedBuilder embedBuilder = new EmbedBuilder();
		embedBuilder.WithTitle("User kicked").WithDescription($"User <@{u.Id}> has been **kicked** by <@{base.Context.User.Id}>. \nReason: `{reason}` \nIssued: <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>\nImmediately Reinvited: `{reinvite}`").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
		await RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, null, embedBuilder.Build());
	}

	[SlashCommand("mute", "Times a user out for a specified amount of time", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ModerateMembers)]
	public async Task timeout(IGuildUser u, string reason = "Unspecified", WarnUnit unit = WarnUnit.Minute, int time = 1)
	{
		TimeSpan t = TimeSpan.Zero;
		switch (unit)
		{
		case WarnUnit.Day:
			t = TimeSpan.FromDays(time);
			break;
		case WarnUnit.Hour:
			t = TimeSpan.FromHours(time);
			break;
		case WarnUnit.Minute:
			t = TimeSpan.FromMinutes(time);
			break;
		case WarnUnit.Second:
			t = TimeSpan.FromSeconds(time);
			break;
		case WarnUnit.Year:
			t = TimeSpan.FromDays(time * 365);
			break;
		}
		await u.SetTimeOutAsync(t);
		User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Add(new enums.Punishment
		{
			date = DateTime.UtcNow,
			duration = t,
			id = User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Count + 1,
			reason = reason,
			type = enums.PunishmentType.Mute,
			punisher = base.Context.User.Username
		});
		EmbedBuilder embedBuilder = new EmbedBuilder();
		embedBuilder.WithTitle("User Timed out").WithDescription($"User <@{u.Id}> has been **timed-out** by <@{base.Context.User.Id}>. \n\nReason: **{reason}** \n\nIssued: <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F> \n\nMuted until: <t:{(DateTimeOffset.Now + t).ToUnixTimeSeconds()}:F>").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
		await base.Context.Interaction.RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, embedBuilder.Build());
	}

	[SlashCommand("unmute", "removes the timeout", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ModerateMembers)]
	public async Task untimeout(IGuildUser u)
	{
		await u.RemoveTimeOutAsync();
		EmbedBuilder embedBuilder = new EmbedBuilder();
		embedBuilder.WithTitle("User Un-timed-out").WithDescription($"User <@{u.Id}> has had their **time-out** revoked by <@{base.Context.User.Id}>.").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
		await base.Context.Interaction.RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, embedBuilder.Build());
	}

	[SlashCommand("warn", "Adds an infraction to a users profile", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ModerateMembers)]
	public async Task warn(IUser u, string reason = "Unspecified")
	{
		User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Add(new enums.Punishment
		{
			date = DateTime.UtcNow,
			id = User.GetUser(u as SocketGuildUser).punishments.criminalRecord.Count + 1,
			type = enums.PunishmentType.Warning,
			punisher = base.Context.User.Username,
			reason = reason
		});
		string value = "No";
		EmbedBuilder embedBuilder = new EmbedBuilder();
		embedBuilder.WithTitle("User warned").WithDescription($"User <@{u.Id}> has been **warned** by <@{base.Context.User.Id}>. \n\nReason: **{reason}** \n\nIssued: <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
		await base.Context.Interaction.RespondAsync(null, null, isTTS: false, ephemeral: false, null, null, embedBuilder.Build());
	}

	[SlashCommand("purge", "Removes a specified amount of messages from a channel.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ManageMessages)]
	public async Task purge(int amount)
	{
		ITextChannel cnl = (ITextChannel)base.Context.Guild.GetChannelAsync(base.Context.Channel.Id);
		cnl.GetMessagesAsync(amount);
		await cnl.DeleteMessagesAsync(await cnl.GetMessagesAsync(amount).FlattenAsync());
		new EmbedBuilder().WithTitle("Messages Deleted").WithDescription($"<@{base.Context.Interaction.User.Id}> has deleted **{amount}** messages in <#{base.Context.Channel.Id}> \n\n<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>").WithColor(Color.Red)
			.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
	}

	[SlashCommand("setlogchannel", "Sets the log channel for the server.", false, RunMode.Default)]
	[RequireUserPermission(GuildPermission.ManageChannels)]
	public async Task setLogChannel(ITextChannel channel)
	{
        Program.guilds.GetGuild(base.Context.Guild.Id).config.auditlogChannel = channel.Id;
        new EmbedBuilder().WithTitle("Log Channel Set").WithDescription($"<@{base.Context.Interaction.User.Id}> has set the log channel to <#{channel.Id}>").WithColor(Color.Red)
            .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/5e/Hammer-keyboard-2.svg/1990px-Hammer-keyboard-2.svg.png");
    }
}
