using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GScraper;
using Obscure.API;
using Obscura;
using Newtonsoft.Json;
using Obscura.FunStuff;
using GScraper.Google;

namespace Obscure
{
    public class Program
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private DiscordSocketClient _client;
        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.All | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 2048,
            
        };
        public static bool kill;
        public static enums.Guilds guilds = new enums.Guilds() { guilds = new List<enums.Guild>() };
        public static AuditLog auditlog = null;
        public Program()
        {
            _configuration = new ConfigurationBuilder()
                .Build();

            _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<AuditLog>()
                .BuildServiceProvider();

        }

        public static void Main(string[] args)
        {
            new Program().RunAsync()
                .GetAwaiter()
                .GetResult();
        }

        public class key
        {
            public string auth { get; set; }
        }

        public async Task RunAsync()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();
            _client = client;
            client.Log += LogAsync;
            await _services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();
            #region secret_shhh
            string text = await File.ReadAllTextAsync($"c:/botdata/toki.json");
            var key = System.Text.Json.JsonSerializer.Deserialize<key>(text);
            await _client.LoginAsync(TokenType.Bot, key.auth);
            #endregion
            await client.StartAsync();
            client.Ready += Client_Ready;
            client.MessageReceived += OnMessageRecieved;
            client.UserJoined += OnUserJoinedGuild;
            client.UserLeft += OnUserLeftGuild;
            client.MessageDeleted += auditlog.MessageDeleted;
            client.MessageUpdated += auditlog.MessageUpdated;
            client.UserJoined += auditlog.UserJoined;
            client.UserLeft += auditlog.UserLeft;
            client.UserVoiceStateUpdated += auditlog.UserVoiceStateUpdated;
            client.ReactionAdded += OnReactionAdded;

            OneWordStory.Register(client);
            ObscuraController.ControllerProgram.StartObscuraController();
            while (!kill)
            {
                await Task.Delay(1);
            }

        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction reaction)
        {
            if (cacheable1.Value.Reactions.FirstOrDefault(x => x.Key.Name == reaction.Emote.Name).Value.ReactionCount > 2)
            {
                if(Guild.GetGuild(((IGuildChannel)cacheable2.Value).GuildId) != null)
                {
                    var guildConfig = Guild.GetGuild(((IGuildChannel)cacheable2.Value).GuildId).config;
                    if(guildConfig.starboardToggle)
                    {
                        await _client.GetGuild(((IGuildChannel)cacheable2.Value).GuildId).GetTextChannel(guildConfig.starboardChannel).SendMessageAsync();
                    }
                }
            }
            await Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            Thread stuff = new Thread(new ThreadStart(async () =>
            {
                await config.InitStorage();
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(120000);
                        Console.WriteLine("Refreshing Data");
                        await config.refreshStorage();
                    }
                });
            }));
            stuff.Start();
            Thread msgOfHr = new Thread(new ThreadStart(async () =>
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                            (_client.GetChannelAsync(1207080636679069786).Result as IMessageChannel).GetMessagesAsync(1)
                            .FlattenAsync().Result.First().Timestamp.ToUnixTimeSeconds() > 3600) return;
                        using var scraper = new GoogleScraper();
                        IEnumerable<IImageResult> images = null;
                        string word = null;
                        try
                        {
                            using HttpClient client = new HttpClient();
                            client.DefaultRequestHeaders.Add("parse", "application/json");
                            HttpResponseMessage response =
                                await client.GetAsync("https://random-word-api.herokuapp.com/word");

                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();

                                Console.WriteLine(jsonResponse);
                                word = jsonResponse.Replace("[", "").Replace("\"", "").Replace("]", "");
                            }

                            Console.WriteLine(word);
                            images = await scraper.GetImagesAsync(word);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        foreach (var image in images)
                        {
                            var builder = new Discord.EmbedBuilder()
                                .WithTitle("Image of the hour!")
                                .WithDescription($"Discuss..")
                                .WithImageUrl(image.Url)
                                .WithFooter("Obscūrus • Team Unity Development")
                                .WithCurrentTimestamp();
                            await (_client.GetChannelAsync(1207080636679069786).Result as IMessageChannel)
                                .SendMessageAsync(embed: builder.Build());
                            break;
                        }

                        Thread.Sleep(3600000);
                    }
                });
            }));
//#error 'RMV FOR PROD OR GAE'
           /msgOfHr.Start();

            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync("Obscurities", type: ActivityType.Listening);
            //foreach(SocketGuild g in _client.Guilds)
            //{
            //    foreach(ISocketMessageChannel c in g.Channels)
            //    {
            //        c.GetMessagesAsync(100, CacheMode.AllowDownload);
            //    }
            //}

        }

        private async Task LogAsync(LogMessage message)
            => Console.WriteLine(message.ToString());

        private async Task OnUserLeftGuild(SocketGuild guild, SocketUser user)
        {
            await guild.GetTextChannel(1184481866116501555).SendMessageAsync($"***{user.Username}*** has left the server :(");
        }

        private async Task OnUserJoinedGuild(SocketGuildUser user)
        {
            try
            {
                if (!user.IsBot)
                {
                    Program.guilds.GetGuild(user.Guild.Id).AddNew(user);
                    if (Program.guilds.GetGuild(user.Guild.Id).config.defaultRole == 0) { return; }
                    await user.Guild.GetUser(user.Id).AddRoleAsync(Program.guilds.GetGuild(user.Guild.Id).config.defaultRole);

                }
                else { return; }

            }
            catch (Exception ex) { }
        }

        private async Task OnMessageRecieved(SocketMessage msg)
        {
 
            try
            {
                if (msg.Author.IsBot||msg.Content == null||msg.Type == MessageType.GuildMemberJoin||msg.Type == MessageType.UserPremiumGuildSubscription)
                    return;

                try
                {
                    if ((msg.Channel as SocketGuildChannel) == null)
                    {
                        return;
                    }
                    else if (msg.Channel.Id == 1265596627234590720) Spot.Trigger(msg);

                    var _serializer = new Newtonsoft.Json.JsonSerializer();
                    using (var sw = new StreamWriter("c:/botdata/latestmsg.json"))
                        using (var jw = new JsonTextWriter(sw))
                    {
                        _serializer.Serialize(jw, msg);
                    }

                    //if (!Program.guilds.GetGuild((msg.Channel as SocketGuildChannel).Guild.Id).GetUser(msg.Author.Id).profile.isVerified)
                    //{
                    //    Console.WriteLine($"User: {msg.Author.Username} is not verified");
                    //    try
                    //    {
                    //        await (msg.Author as IGuildUser).SetTimeOutAsync(TimeSpan.FromSeconds(10));
                    //    }

                    //    catch { }
                    //    await msg.DeleteAsync();
                    //    var warning = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} you are not verified. please use /verify to talk in this server");
                    //    await Task.Delay(5000);

                    //    await warning.DeleteAsync();
                    //    return;
                    //}
                    SocketGuild g = (msg.Channel as SocketGuildChannel).Guild;
                    if (Program.guilds.GetGuild(g.Id).config.levelToggle == false) { return; }

                    xpEvent(msg);
                }
                catch (Exception ex) { return; };   


            }
            catch (Exception ex) { }
        }

        private static async void xpEvent(SocketMessage msg)
        {
            SocketGuild g = (msg.Channel as SocketGuildChannel).Guild;
            var rnd = new Random();
            enums.User u = guilds.GetGuild(g.Id).GetUser(msg.Author.Id);
            u.profile.AddXP((ulong)rnd.Next(1, 5));
            Console.WriteLine($"added xp to {u.profile.username}");
            int level = u.profile.level;
            u.profile.totalRecordedMessages++;
            u.profile.RecaculateLevel();
            int newlevel = guilds.GetGuild(g.Id).GetUser(msg.Author.Id).profile.level;
            if (newlevel > level)
            {
                int lvl = newlevel;
                int res = 0;


                for (int i = 0; i < lvl; i++)
                {
                    res += (5 * (i ^ 2) + 50 * i + 100);
                }


                var embed = new EmbedBuilder()
                    .WithDescription($"Congrats {msg.Author.Mention}! You leveled up to level {newlevel} and earned *1000*pickles!\nTotal Messages Send: **{guilds.GetGuild(g.Id).GetUser(msg.Author.Id).profile.totalRecordedMessages}**\nTotal XP: **{guilds.GetGuild(g.Id).GetUser(msg.Author.Id).profile.exp}**xp\n Level **{newlevel + 1}** Requirement: {res}\n")
                    .WithFooter($"You are {(ulong)res - u.profile.exp}xp away from level {newlevel + 1}! \nObscūrus • Team Unity Development")
                    .WithCurrentTimestamp()
                    .WithThumbnailUrl(@"https://images-ext-2.discordapp.net/external/tNJAAj1zNcCC76NWawahfu9_EjfXAWBl5wyJD0f4Ots/https/upload.wikimedia.org/wikipedia/commons/thumb/2/24/Stonks_emoji.svg/2425px-Stonks_emoji.svg.png?format=webp&quality=lossless&width=794&height=671").Build();
                u.profile.currency += 1000;
                await msg.Channel.SendMessageAsync(embed: embed);
            }
        }
    }
}
