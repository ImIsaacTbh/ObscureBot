using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscure
{
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
            public ulong userid { get; set; }
            public string offender { get; set; }
            public string punisher { get; set; }
            public PunishmentType type { get; set; }
            public string reason { get; set; }
            public DateTime date { get; set; }
            public bool permenant { get; set; }
            public DateTimeOffset duration { get; set; }
        }
        /// <summary>
        ///  
        /// </summary>
        public class Guild
        {
            public ulong id { get; set; }
            public List<User> users { get; set; }
            public GuildConfig config { get; set; }

            public virtual User GetUser(ulong userId)
            {
                return users.FirstOrDefault(x => x.profile.id == userId);
            }

            public virtual void AddNew(IUser u)
            {
                users.Add(new User { profile = new Profile { username = u.Username, currency = 0, exp = 0, id = u.Id, level = 0, totalRecordedMessages = 0 }, punishments = new Punishments { criminalRecord =  new List<Punishment>() } });
            }
        }

        public class GuildConfig
        {
            public ulong defaultRole { get; set; } = 0;
            public bool levelToggle { get; set; } = false;
            public ulong starboardChannel { get; set; } = 0;
            public bool starboardToggle { get; set; } = false;
            public ulong auditlogChannel { get; set; } = 0;
            public bool auditlogToggle { get; set; } = false;
            public ulong announcementChannel { get; set; } = 0;
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
                return guilds.FirstOrDefault(x => x.id == guildId);
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

            public virtual void Punish(IUser shithead, IUser mod, PunishmentType type, string reason, bool permenant, DateTimeOffset duration)
            {
                criminalRecord.Add(new Punishment { id = criminalRecord.Count() + 1, userid = shithead.Id, offender = shithead.Username, type = type, reason = reason, permenant = permenant, duration = duration });
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
            public bool robberyInProgress { get; set; } = false;
            public DateTime lastBankRobbery { get; set; }
            public bool heistInProgress { get; set; } = false;
            public bool startingHeist { get; set; } = false;


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
                bool done = false;
                int lvl = 0;
                while (!done)
                {
                    int res = 0;
                    for (int i = 0; i < lvl; i++)
                    {
                        res += (5 * (i ^ 2) + 50 * i + 100);
                    }
                    if(exp > (ulong)res)
                    {
                        lvl++;
                    }
                    else
                    {
                        done = true;
                    }
                }
                level = lvl;
            }
        }
    }
}
