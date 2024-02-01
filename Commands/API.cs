using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using static System.Net.WebRequestMethods;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;
using Color = System.Drawing.Color;
using System.Reflection;

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
