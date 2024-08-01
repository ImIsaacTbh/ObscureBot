using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscura.Commands.Google
{
    public class command : ObscureInteractionModuleBase
    {
        public enum searchType
        {
            search,
            image
        }

        [SlashCommand("google", "searches google")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task google(searchType g, string search)
        {
            if (g == searchType.image)
            {
                await imageSearch.RequestImageSearch(search, Context.Interaction);
            }
            else if (g == searchType.search)
            {

            }
        }
    }
}
