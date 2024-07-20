using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using YamlDotNet;
using YamlDotNet.Serialization;
using System.IO;
using System.Linq;

namespace Obscure
{
    public class config : InteractionModuleBase<SocketInteractionContext>
    {
        public static InteractionService commands { get; set; }
        private static InteractionHandler _handler;
        private static DiscordSocketClient _client;


        public config(InteractionHandler handler, DiscordSocketClient client)
        {
            _handler = handler;
            _client = client;
        }

        public static Serializer _serializer = new Serializer();
        public static Deserializer _deserializer = new Deserializer();
        public static string path = @"C:\botdata\";
        

        public static async Task InitStorage()
        {
            Console.WriteLine("Starting storage shit");
            string[] dirs = Directory.GetDirectories(path);
            foreach(IGuild g in _client.Guilds)
            {
                int i = 0;
                if (dirs.FirstOrDefault(x => x.Contains(g.Id.ToString())) == null)
                {
                    enums.Guild gtemp = new enums.Guild() { users = new List<enums.User>(), config = new enums.GuildConfig() { defaultRole = 0, levelToggle = false }, id = g.Id };
                    Directory.CreateDirectory($"{path}{g.Id}");
                    string rawCFG = _serializer.Serialize(new enums.GuildConfig() { defaultRole = 0, levelToggle = false});
                    await File.WriteAllTextAsync($"{path}{g.Id}\\config.yaml", rawCFG);
                    foreach(IGuildUser u in await g.GetUsersAsync())
                    {
                        if (u.IsBot) continue;
                        enums.Profile profile = new enums.Profile() { id = u.Id, username = u.Username, totalRecordedMessages = 0, level = 0, exp = 0, currency = 0, bank = 0, heistInProgress = false, lastBankRobbery = DateTime.Now.AddDays(-99), lastDaily = DateTime.Now.AddDays(-99), lastRobbery = DateTime.Now.AddDays(-99), lastWeekly = DateTime.Now.AddDays(-99), robberyInProgress = false };
                        enums.Punishments punishments = new enums.Punishments() { criminalRecord = new List<enums.Punishment>() };
                        string rawP = _serializer.Serialize(profile);
                        string rawPu = _serializer.Serialize(punishments);
                        Directory.CreateDirectory($"{path}{g.Id}\\{u.Id}");
                        //File.Create($"{path}{g.Id}\\{u.Id}\\storage.yaml");
                        await File.WriteAllTextAsync($"{path}{g.Id}\\{u.Id}\\storage.yaml", rawP);
                        //File.Create($"{path}{g.Id}\\{u.Id}\\punishments.yaml");
                        await File.WriteAllTextAsync($"{path}{g.Id}\\{u.Id}\\punishments.yaml", rawPu);
                        Console.WriteLine($"Grabbed user : {u.Username} from discord");
                        gtemp.users.Add(new enums.User() { profile = profile, punishments = punishments });
                        i++;
                    }
                    Program.guilds.Add(gtemp);
                }
                else
                {
                    enums.Guild gtemp = new enums.Guild() { users = new List<enums.User>() };
                    enums.GuildConfig guildConfig = _deserializer.Deserialize<enums.GuildConfig>(await File.ReadAllTextAsync($"{path}{g.Id}\\config.yaml"));
                    gtemp.config = guildConfig;
                    gtemp.id = g.Id;
                    foreach(string d in Directory.EnumerateDirectories($"{path}{g.Id}"))
                    {
                        enums.Profile p = _deserializer.Deserialize<enums.Profile>(await File.ReadAllTextAsync($"{d}\\storage.yaml"));
                        enums.Punishments poo = _deserializer.Deserialize<enums.Punishments>(await File.ReadAllTextAsync($"{d}\\punishments.yaml"));
                        p.robberyInProgress = false;
                        try
                        {
                            p.startingHeist = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        p.heistInProgress = false;
                        gtemp.users.Add(new enums.User() { profile = p, punishments = poo });
                        Console.WriteLine($"Fetched user: {p.username} from storage");
                        i++;
                    }
                    Console.WriteLine("Scanning for new users");
                    foreach(IUser u in await g.GetUsersAsync())
                    {
                        if (gtemp.users.FirstOrDefault(x => x.profile.id == u.Id) != null) continue;
                        if (u.IsBot) continue;
                        enums.Profile profile = new enums.Profile() { id = u.Id, username = u.Username, totalRecordedMessages = 0, level = 0, exp = 0, currency = 0, bank = 0, heistInProgress = false, lastBankRobbery = DateTime.Now.AddDays(-99), lastDaily = DateTime.Now.AddDays(-99), lastRobbery = DateTime.Now.AddDays(-99), lastWeekly = DateTime.Now.AddDays(-99), robberyInProgress = false };
                        enums.Punishments punishments = new enums.Punishments() { criminalRecord = new List<enums.Punishment>() };
                        string rawP = _serializer.Serialize(profile);
                        string rawPu = _serializer.Serialize(punishments);
                        Directory.CreateDirectory($"{path}{g.Id}\\{u.Id}");
                        //File.Create($"{path}{g.Id}\\{u.Id}\\storage.yaml");
                        await File.WriteAllTextAsync($"{path}{g.Id}\\{u.Id}\\storage.yaml", rawP);
                        //File.Create($"{path}{g.Id}\\{u.Id}\\punishments.yaml");
                        await File.WriteAllTextAsync($"{path}{g.Id}\\{u.Id}\\punishments.yaml", rawPu);
                        Console.WriteLine($"Fetched new user : {u.Username} from discord");
                        i++;
                    }
                    Program.guilds.Add(gtemp);
                }
                Console.WriteLine($"Successfully fetched {i} total users");
            }
        }

        public static async Task refreshStorage()
        {
            foreach(IGuild Ig in _client.Guilds)
            {
                enums.Guild g = Program.guilds.guilds.FirstOrDefault(x => x.id == Ig.Id);
                if (g == null) return;
                string RawGuildConfig = _serializer.Serialize(new enums.GuildConfig { defaultRole = g.config.defaultRole, levelToggle = g.config.levelToggle, auditlogChannel = g.config.auditlogChannel, announcementChannel = g.config.announcementChannel, auditlogToggle = g.config.auditlogToggle, blacklistedChannels = g.config.blacklistedChannels, starboardChannel = g.config.starboardChannel, starboardToggle = g.config.starboardToggle });
                await File.WriteAllTextAsync($"{path}{g.id}\\config.yaml", RawGuildConfig);
                foreach (enums.User u in g.users)
                {
                    if (Directory.Exists($"{path}{g.id}\\{u.profile.id}"))
                    {
                        string raw = _serializer.Serialize(u.profile);
                        await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\storage.yaml", raw);

                        string rawPu = _serializer.Serialize(u.punishments);
                        await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\punishments.yaml", rawPu);
                    }
                    else
                    {
                        Directory.CreateDirectory($"{path}{g.id}\\{u.profile.id}");
                        enums.Profile profile = new enums.Profile() { id = u.profile.id, username = u.profile.username, totalRecordedMessages = u.profile.totalRecordedMessages, level = u.profile.level, exp = u.profile.exp, currency = u.profile.currency, bank = u.profile.bank, heistInProgress = u.profile.heistInProgress, lastBankRobbery = u.profile.lastBankRobbery, lastDaily = u.profile.lastDaily, lastRobbery = u.profile.lastRobbery, lastWeekly = u.profile.lastWeekly, robberyInProgress = u.profile.robberyInProgress};
                        enums.Punishments punishments = new enums.Punishments() { criminalRecord = u.punishments.criminalRecord };
                        string rawP = _serializer.Serialize(profile);
                        string rawPu = _serializer.Serialize(punishments);
                        //File.Create($"{path}{g.id}\\{u.profile.id}\\storage.yaml");
                        await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\storage.yaml", rawP);
                        //File.Create($"{path}{g.id}\\{u.profile.id}\\punishments.yaml");
                        await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\punishments.yaml", rawPu);
                    }
                }
            }
            Console.WriteLine("finished refresh");
        }
    }
}