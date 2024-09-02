using AutoGen.Core;
using Discord;
using Discord.WebSocket;
using Obscure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscura.FunStuff
{
    internal class OneWordStory
    {
        public static void Register(DiscordSocketClient _client)
        {
            _client.MessageReceived += OnMessageRecieved;
        }
        private async static Task OnMessageRecieved(SocketMessage msg)
        {
            Console.WriteLine("A");
            var guild = ((SocketGuildChannel)msg.Channel).Guild;
            ulong? channel = Program.guilds.GetGuild(guild.Id).config.onewordstorychannel;
            if (msg != null && channel != null && msg.Channel.Id == channel)
            {
                Console.WriteLine("B");
                var LastMessage = msg.Channel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault() ?? null;
                if (LastMessage.Author == msg.Author)
                {
                    Console.WriteLine("C");
                    await msg.DeleteAsync();
                    return;
                }
                else { return; }
            }
            else { return; }
        }

    }
}
