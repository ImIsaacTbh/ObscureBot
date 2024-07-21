using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Obscure;
using Obscure.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using API = Obscure.API.Guild;
namespace Obscura
{
    public class AuditLog : InteractionModuleBase<SocketInteractionContext>
    {

        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;
        

        public AuditLog(InteractionHandler handler, DiscordSocketClient client)
        {
            _handler = handler;
            _client = client;
            Program.auditlog = this;
        }

        public  static void RegisterEvents()
        {
            Console.WriteLine("Cunt_");

        }


        public async Task MessageDeleted(Cacheable<IMessage, ulong> m, Cacheable<IMessageChannel, ulong> channel)
        {
            ulong id = ((SocketGuildChannel)channel.Value).Guild.Id;
            IMessageChannel? logChannel = (IMessageChannel)_client.GetChannel(API.GetGuild(id).config.auditlogChannel);
            if (m.Value == null || logChannel == null)
            {
                return;
            }
            IMessage msg = await m.GetOrDownloadAsync();
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Message updated in channel: <#{channel.Value.Id}>",
            };

                embed.Description = "Message Deleted";
                embed.ThumbnailUrl = msg.Author.GetAvatarUrl();
                embed.AddField($"Message: ", $"{msg.Content}", false);
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await logChannel.SendMessageAsync(embed: embed.Build());
        }

        public async Task MessageUpdated(Cacheable<IMessage, ulong> oldmsg, SocketMessage newmsg, ISocketMessageChannel channel)
        {

            ulong id = ((SocketGuildChannel)channel).Guild.Id;
            IMessageChannel? logChannel = (IMessageChannel)_client.GetChannel(API.GetGuild(id).config.auditlogChannel);
            if (logChannel == null)
            {
                return;
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Message updated in channel: <#{channel.Id}>",
            };

            embed.Description = "Message Changed";
            embed.ThumbnailUrl = oldmsg.Value.Author.GetAvatarUrl();
            embed.AddField($"Original Message: ", $"{oldmsg.Value.Content}", false);
            embed.AddField($"Updated Message: ", $"{newmsg.Content}", false);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await logChannel.SendMessageAsync(embed: embed.Build());

        }

        public async Task UserJoined(IGuildUser user)
        {
            ulong id = user.Guild.Id;
            IMessageChannel logChannel = (IMessageChannel)_client.GetChannel(API.GetGuild(id).config.auditlogChannel);
            if (logChannel == null)
            {
                return;
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"User Joined: {user.GlobalName}",
            };
            embed.AddField($"User: {user.DisplayName}", true);
            embed.ThumbnailUrl = user.GetAvatarUrl();
            embed.AddField($"Account Created: <t:{user.CreatedAt.ToUniversalTime}:F>", true);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();


            await logChannel.SendMessageAsync(embed: embed.Build());

        }

        public async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            ulong id = guild.Id;
            IMessageChannel logChannel = (IMessageChannel)_client.GetChannel(API.GetGuild(id).config.auditlogChannel);
            if (logChannel == null)
            {
                return;
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"User Left: {user.GlobalName}",
            };
            embed.AddField($"User:", $"{user.Username}", true);
            embed.ThumbnailUrl = user.GetAvatarUrl();
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();

            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            ulong id = ((SocketGuildUser)user).Guild.Id;
            IMessageChannel logChannel = (IMessageChannel)_client.GetChannel(API.GetGuild(id).config.auditlogChannel);
            if (logChannel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"User: {user.GlobalName}, has changed voice state",
            };
            if (after.VoiceChannel == null)
            {

                embed.AddField($"Disconnected from channel", $"Channel: <#{before.VoiceChannel.Id}>", true);
                embed.ThumbnailUrl = user.GetAvatarUrl();
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();

                await logChannel.SendMessageAsync(embed: embed.Build());



            }
            if (after.VoiceChannel != null && before.VoiceChannel != null)
            {

                embed.AddField($"Moved channels", $"Previous Channel: <#{before.VoiceChannel.Id}> \nNew Channel: <#{after.VoiceChannel.Id}>", true);
                embed.ThumbnailUrl = user.GetAvatarUrl();
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await logChannel.SendMessageAsync(embed: embed.Build());
            }
            if (after.VoiceChannel != null && before.VoiceChannel == null)
            {

                embed.AddField($"Connected to channel", $"Channel: <#{after.VoiceChannel.Id}>", true);
                embed.ThumbnailUrl = user.GetAvatarUrl();
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await logChannel.SendMessageAsync(embed: embed.Build());
            }
            }
        }
    }

