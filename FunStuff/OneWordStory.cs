using Discord;
using Discord.WebSocket;
using Obscure;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Azure;
using System.Net;

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
            var guild = ((SocketGuildChannel)msg.Channel).Guild;
            ulong? channel = Program.guilds.GetGuild(guild.Id).config.onewordstorychannel;
            if (msg != null && channel != null && msg.Channel.Id == channel)
            {
                var LastMessage = msg.Channel.GetMessagesAsync(2).FlattenAsync().Result.LastOrDefault() ?? null;
                await Task.Delay(1);
                if (LastMessage.Author == msg.Author)
                {
                    await msg.DeleteAsync();
                    return;
                }
                else
                {
                    if (!await CheckWord(msg.Content))
                    {
                        await msg.DeleteAsync();
                        return;
                    }
                }
            }
            else { return; }
        }

        private static async Task<bool> CheckWord(string word)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://www.urbandictionary.com/define.php?term={word}");
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)await req.GetResponseAsync();
            }catch(WebException e)
            {
                return false;
            }

            if(response.StatusCode != HttpStatusCode.NotFound)
            {
                return true;
            }
            else
                return false;
        }
    }
}
