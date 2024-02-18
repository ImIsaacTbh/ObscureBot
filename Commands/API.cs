using Discord;

namespace Obscure.API
{
    public static class User
    {
        public static enums.User GetUser(IGuildUser user)
        {
            if (user == null) return null;
            return Program.guilds.GetGuild(user.GuildId).GetUser(user.Id);
        }

        //public static 
    }

    public static class Guild
    {
        public static enums.Guild GetGuild(ulong guildId)
        {
            return Program.guilds.GetGuild(guildId);
        }
    }
}
