using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GScraper.Google;
using GScraper;
using System.Net.Http.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using static Obscure.Commands.Toys;
using System.Text.Json;

namespace Obscura.Commands.Google
{
    public class command : ObscureInteractionModuleBase
    {
        public enum searchType
        {
            search,
            image
    }
        public class Word
        {
            public string word { get; set; }
        }

    [SlashCommand("google", "searches google")]
    [RequireUserPermission(GuildPermission.UseApplicationCommands)]
    public async Task google(searchType g, string search)
    {
            //if (g == searchType.image)
            //{
            //    await imageSearch.RequestImageSearch(search, Context.Interaction);
            //}
            //else if (g == searchType.search)
            //{

            //}

            if (g == searchType.image)
            {
                using var scraper = new GoogleScraper();
                IEnumerable<IImageResult> images = null;
                try
                {
                    images = await scraper.GetImagesAsync(search);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var rnd = new Random();

                var builder = new Discord.EmbedBuilder()
                    .WithTitle("Google Image Search")
                    .WithDescription($"Here is an image of {search}")
                    .WithImageUrl(images.First().Url)
                    .WithFooter("Obscūrus • Team Unity Development")
                    .WithCurrentTimestamp();
                await RespondAsync(embed: builder.Build());

                //foreach (var image in images)
                //{
                //    var builder = new Discord.EmbedBuilder()
                //        .WithTitle("Google Image Search")
                //        .WithDescription($"Here is an image of {search}")
                //        .WithImageUrl(image.Url)
                //        .WithFooter("Obscūrus • Team Unity Development")
                //        .WithCurrentTimestamp();
                //    await RespondAsync(embed: builder.Build());
                //    break;
                //}
            }
    }
}
}
