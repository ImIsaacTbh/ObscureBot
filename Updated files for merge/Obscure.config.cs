// Obscura, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Obscure.config
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Obscure;
using YamlDotNet.Serialization;

public class config : InteractionModuleBase<SocketInteractionContext>
{
	private static InteractionHandler _handler;

	private static DiscordSocketClient _client;

	public static Serializer _serializer = new Serializer();

	public static Deserializer _deserializer = new Deserializer();

	public static string path = "C:\\botdata\\";

	public static InteractionService commands { get; set; }

	public config(InteractionHandler handler, DiscordSocketClient client)
	{
		_handler = handler;
		_client = client;
	}

	public static async Task InitStorage()
	{
		Console.WriteLine("Starting storage shit");
		string[] dirs = Directory.GetDirectories(path);
		foreach (SocketGuild g in _client.Guilds)
		{
			int i = 0;
			if (dirs.FirstOrDefault((string x) => x.Contains(((IEntity<ulong>)g).Id.ToString())) == null)
			{
				enums.Guild gtemp = new enums.Guild
				{
					users = new List<enums.User>(),
					config = new enums.GuildConfig
					{
						defaultRole = 0uL,
						levelToggle = false
					},
					id = ((IEntity<ulong>)g).Id
				};
				Directory.CreateDirectory($"{path}{((IEntity<ulong>)g).Id}");
				string contents = _serializer.Serialize(new enums.GuildConfig
				{
					defaultRole = 0uL,
					levelToggle = false
				});
				await File.WriteAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\config.yaml", contents);
				foreach (IGuildUser u2 in await ((IGuild)g).GetUsersAsync(CacheMode.AllowDownload, (RequestOptions)null))
				{
					if (!u2.IsBot)
					{
						enums.Profile profile = new enums.Profile
						{
							id = u2.Id,
							username = u2.Username,
							totalRecordedMessages = 0,
							level = 0,
							exp = 0uL,
							currency = 0f,
							bank = 0f,
							heistInProgress = false,
							lastBankRobbery = DateTime.Now.AddDays(-99.0),
							lastDaily = DateTime.Now.AddDays(-99.0),
							lastRobbery = DateTime.Now.AddDays(-99.0),
							lastWeekly = DateTime.Now.AddDays(-99.0),
							robberyInProgress = false
						};
						enums.Punishments punishments = new enums.Punishments
						{
							criminalRecord = new List<enums.Punishment>()
						};
						string contents2 = _serializer.Serialize(profile);
						string rawPu = _serializer.Serialize(punishments);
						Directory.CreateDirectory($"{path}{((IEntity<ulong>)g).Id}\\{u2.Id}");
						await File.WriteAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\{u2.Id}\\storage.yaml", contents2);
						await File.WriteAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\{u2.Id}\\punishments.yaml", rawPu);
						Console.WriteLine("Grabbed user : " + u2.Username + " from discord");
						gtemp.users.Add(new enums.User
						{
							profile = profile,
							punishments = punishments
						});
						i++;
					}
				}
				Program.guilds.Add(gtemp);
			}
			else
			{
				enums.Guild gtemp = new enums.Guild
				{
					users = new List<enums.User>()
				};
				Deserializer deserializer = _deserializer;
				enums.GuildConfig guildConfig = deserializer.Deserialize<enums.GuildConfig>(await File.ReadAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\config.yaml"));
				gtemp.config = guildConfig;
				gtemp.id = ((IEntity<ulong>)g).Id;
				foreach (string rawPu in Directory.EnumerateDirectories($"{path}{((IEntity<ulong>)g).Id}"))
				{
					deserializer = _deserializer;
					enums.Profile profile = deserializer.Deserialize<enums.Profile>(await File.ReadAllTextAsync(rawPu + "\\storage.yaml"));
					deserializer = _deserializer;
					enums.Punishments punishments2 = deserializer.Deserialize<enums.Punishments>(await File.ReadAllTextAsync(rawPu + "\\punishments.yaml"));
					profile.robberyInProgress = false;
					try
					{
						profile.startingHeist = false;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
					profile.heistInProgress = false;
					gtemp.users.Add(new enums.User
					{
						profile = profile,
						punishments = punishments2
					});
					Console.WriteLine("Grabbed user : " + profile.username + " from storage");
					i++;
				}
				Console.WriteLine("Getting new users");
				foreach (IGuildUser u in await ((IGuild)g).GetUsersAsync(CacheMode.AllowDownload, (RequestOptions)null))
				{
					if (gtemp.users.FirstOrDefault((enums.User x) => x.profile.id == u.Id) == null && !u.IsBot)
					{
						enums.Profile graph = new enums.Profile
						{
							id = u.Id,
							username = u.Username,
							totalRecordedMessages = 0,
							level = 0,
							exp = 0uL,
							currency = 0f,
							bank = 0f,
							heistInProgress = false,
							lastBankRobbery = DateTime.Now.AddDays(-99.0),
							lastDaily = DateTime.Now.AddDays(-99.0),
							lastRobbery = DateTime.Now.AddDays(-99.0),
							lastWeekly = DateTime.Now.AddDays(-99.0),
							robberyInProgress = false
						};
						enums.Punishments graph2 = new enums.Punishments
						{
							criminalRecord = new List<enums.Punishment>()
						};
						string contents3 = _serializer.Serialize(graph);
						string rawPu = _serializer.Serialize(graph2);
						Directory.CreateDirectory($"{path}{((IEntity<ulong>)g).Id}\\{u.Id}");
						await File.WriteAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\{u.Id}\\storage.yaml", contents3);
						await File.WriteAllTextAsync($"{path}{((IEntity<ulong>)g).Id}\\{u.Id}\\punishments.yaml", rawPu);
						Console.WriteLine("Grabbed user : " + u.Username + " from discord");
						i++;
					}
				}
				Program.guilds.Add(gtemp);
			}
			Console.WriteLine($"Successfully grabbed {i} users");
		}
	}

	public static async Task refreshStorage()
	{
		foreach (SocketGuild Ig in _client.Guilds)
		{
			enums.Guild g = Program.guilds.guilds.FirstOrDefault((enums.Guild x) => x.id == ((IEntity<ulong>)Ig).Id);
			if (g == null)
			{
				return;
			}
			string contents = _serializer.Serialize(new enums.GuildConfig
			{
				defaultRole = g.config.defaultRole,
				levelToggle = g.config.levelToggle
			});
			await File.WriteAllTextAsync($"{path}{g.id}\\config.yaml", contents);
			foreach (enums.User u in g.users)
			{
				if (Directory.Exists($"{path}{g.id}\\{u.profile.id}"))
				{
					string contents2 = _serializer.Serialize(u.profile);
					await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\storage.yaml", contents2);
					string contents3 = _serializer.Serialize(u.punishments);
					await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\punishments.yaml", contents3);
					continue;
				}
				Directory.CreateDirectory($"{path}{g.id}\\{u.profile.id}");
				enums.Profile graph = new enums.Profile
				{
					id = u.profile.id,
					username = u.profile.username,
					totalRecordedMessages = u.profile.totalRecordedMessages,
					level = u.profile.level,
					exp = u.profile.exp,
					currency = u.profile.currency,
					bank = u.profile.bank,
					heistInProgress = u.profile.heistInProgress,
					lastBankRobbery = u.profile.lastBankRobbery,
					lastDaily = u.profile.lastDaily,
					lastRobbery = u.profile.lastRobbery,
					lastWeekly = u.profile.lastWeekly,
					robberyInProgress = u.profile.robberyInProgress
				};
				enums.Punishments graph2 = new enums.Punishments
				{
					criminalRecord = u.punishments.criminalRecord
				};
				string contents4 = _serializer.Serialize(graph);
				string rawPu = _serializer.Serialize(graph2);
				await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\storage.yaml", contents4);
				await File.WriteAllTextAsync($"{path}{g.id}\\{u.profile.id}\\punishments.yaml", rawPu);
			}
		}
		Console.WriteLine("finished refresh");
	}
}
