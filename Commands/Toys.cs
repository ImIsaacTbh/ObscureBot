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
using System.Net;
using System.Text.RegularExpressions;

namespace Obscure.Commands
{
    public class Toys : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;


        public Toys(InteractionHandler handler, DiscordSocketClient client)
        {
            _handler = handler;
            _client = client;
        }
        public class key
        {
            public string auth { get; set; }
        }

        public enum options
        {
            Listening,
            Watching,
            Playing,
            Competing
        }

        public static bool IsValidHex(string hex)
        {
            string pattern = @"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$";
            return Regex.IsMatch(hex, pattern);
        }

        public string GetColorName(string hex)
        {
            string url = "https://www.color-name.com/hex/" + hex;
            string html = new HttpClient().GetStringAsync(url).Result;
            string pattern = @"(?<=Color Name: ).*?(?=</h4>)";
            string colorName = Regex.Match(html, pattern).Value;
            return colorName;
        }

        public static Color HexToRGB(string hex)
        {
            string pattern = @"[0-9a-fA-F]{2}";
            MatchCollection matches = Regex.Matches(hex, pattern);
            int[] rgb = new int[3];
            for (int i = 0; i < 3; i++)
            {
                rgb[i] = int.Parse(matches[i].Value, System.Globalization.NumberStyles.HexNumber);
            }
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

        [SlashCommand("randomcolor", "Generate a random color")]
        [RequireUserPermission(GuildPermission.UseApplicationCommands)]
        public async Task randomColor()
        {
            Random rnd = new Random();
            Color myColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
            String url = $"https://singlecolorimage.com/get/{hex}/300x80";
            string name = "";
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Random color generator!",
                Description = $"**Name:** *fetching*\n**Hex:** #{hex} \n**R:** {myColor.R} *({(float)myColor.R * 4 / 1000})* \n**G:** {myColor.G} *({(float)myColor.G * 4 / 1000})* \n**B:** {myColor.B}  *({(float)myColor.B * 4 / 1000})*",
                ImageUrl = url,

            };
            
            embed.Color = (Discord.Color)myColor;
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
            name = GetColorName(hex);
            embed.Description = $"**Name:** {name}\n**Hex:** #{hex} \n**R:** {myColor.R} *({(float)myColor.R * 4 / 1000})* \n**G:** {myColor.G} *({(float)myColor.G * 4 / 1000})* \n**B:** {myColor.B}  *({(float)myColor.B * 4 / 1000})*";
            Console.WriteLine(name);
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        }

        [SlashCommand("hex", "Generate a color profile")]
        [RequireUserPermission(GuildPermission.UseApplicationCommands)]
        public async Task randomColor(string hex)
        {
            if (!IsValidHex(hex))
            {
                await RespondAsync("That hex is invalid.", ephemeral: true); return;
            }
            Random rnd = new Random();
            Color myColor = HexToRGB(hex);
            String url = $"https://singlecolorimage.com/get/{hex}/300x80";
            string name = "";
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Color profile!",
                Description = $"**Name:** *fetching*\n**Hex:** #{hex} \n**R:** {myColor.R} *({(float)myColor.R * 4 / 1000})* \n**G:** {myColor.G} *({(float)myColor.G * 4 / 1000})* \n**B:** {myColor.B}  *({(float)myColor.B * 4 / 1000})*",
                ImageUrl = url,

            };

            embed.Color = (Discord.Color)myColor;
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
            name = GetColorName(hex);
            embed.Description = $"**Name:** {name}\n**Hex:** #{hex} \n**R:** {myColor.R} *({(float)myColor.R * 4 / 1000})* \n**G:** {myColor.G} *({(float)myColor.G * 4 / 1000})* \n**B:** {myColor.B}  *({(float)myColor.B * 4 / 1000})*";
            Console.WriteLine(name);
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        }
    }
}
