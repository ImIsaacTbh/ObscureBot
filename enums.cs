using Discord;
using Obscure;

public class enums
{
	public enum PunishmentType
	{
		Warning,
		Mute,
		Kick,
		Ban
	}

	public class Punishment
	{
		public int id { get; set; }

		public int userid { get; set; }

		//public bool permanent { get; set; }

		public string punisher { get; set; }

		public PunishmentType type { get; set; }

		public string reason { get; set; }

		public DateTime date { get; set; }

		public TimeSpan duration { get; set; }

	}

	public class Guild
	{
		public ulong id { get; set; }

		public List<User> users { get; set; }

		public GuildConfig config { get; set; }



		public virtual User GetUser(ulong userId)
		{
			return users.FirstOrDefault((User x) => x.profile.id == userId);
		}

		public virtual void AddNew(IUser u)
		{
			users.Add(new User
			{
				profile = new Profile
				{
					username = u.Username,
					currency = 0f,
					exp = 0uL,
					id = u.Id,
					level = 0,
					totalRecordedMessages = 0
				},
				punishments = new Punishments
				{
					criminalRecord = new List<Punishment>()
				}
			});
		}
	}

	public class GuildConfig
	{
		public ulong defaultRole { get; set; }

		public bool levelToggle { get; set; }

		public ulong starboardChannel { get; set; }

		public bool starboardToggle { get; set; }

		public ulong auditlogChannel { get; set; }

		public bool auditlogToggle { get; set; }

		public ulong announcementChannel { get; set; }
		public ulong onewordstorychannel { get; set; }
		public List<ulong> blacklistedChannels { get; set; } = new List<ulong>();

	}

	public class Guilds
	{
		public List<Guild> guilds { get; set; }

		public virtual void Add(Guild g)
		{
			guilds.Add(g);
		}

		public virtual void Remove(Guild g)
		{
			guilds.Remove(g);
		}

		public virtual Guild GetGuild(ulong guildId)
		{
			return guilds.FirstOrDefault((Guild x) => x.id == guildId);
		}
	}

	public class User
	{
		public Punishments punishments { get; set; }

		public Profile profile { get; set; }
	}

	public class Punishments
	{
		public List<Punishment> criminalRecord { get; set; }

		public virtual void Punish(IUser shithead, IUser mod, PunishmentType type, string reason, bool permenant, TimeSpan duration)
		{
			criminalRecord.Add(new Punishment
			{
				id = criminalRecord.Count() + 1,
				type = type,
				reason = reason,
				duration = duration
			});
		}
	}

	public class Profile
	{
		public ulong id { get; set; }

		public string username { get; set; }

		public int totalRecordedMessages { get; set; }

		public ulong exp { get; set; }

		public int level { get; set; }

		public float currency { get; set; }

		public float bank { get; set; }

		public DateTime lastDaily { get; set; }

		public DateTime lastWeekly { get; set; }

		public DateTime lastRobbery { get; set; }

		public bool robberyInProgress { get; set; }

		public DateTime lastBankRobbery { get; set; }

		public bool heistInProgress { get; set; }

		public bool startingHeist { get; set; }

		public DateTime lastUnscramble { get; set; }

        public Boolean isVerified { get; set; } = false;

        public virtual void AddLevel()
		{
			level++;
		}

		public virtual void SetLevel(int lvl)
		{
			lvl = level;
		}

		public virtual void AddXP(ulong xp)
		{
			exp += xp;
		}

		public virtual void RemoveXP(ulong xp)
		{
			exp -= xp;
		}

		public virtual void RecaculateLevel()
		{
			bool flag = false;
			int num = 0;
			while (!flag)
			{
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					num2 += 5 * (i ^ 2) + 50 * i + 100;
				}
				if (exp > (ulong)num2)
				{
					num++;
				}
				else
				{
					flag = true;
				}
			}
			level = num;
		}
	}
}
