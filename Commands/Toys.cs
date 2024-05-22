using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RequireUserPermissionAttribute = Discord.Interactions.RequireUserPermissionAttribute;
using Color = System.Drawing.Color;
using System.Text.RegularExpressions;
using System.Text.Json;
using Newtonsoft.Json;

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

        public class ColorInfo
        {
            public string Name { get; set; }
            public Images Images { get; set; }
        }

        public class Images
        {
            public string Square { get; set; }
            public string Gradient { get; set; }
        }

        [SlashCommand("gradient", "Generate a gradiant from a hex color")]
        [RequireUserPermission(GuildPermission.UseApplicationCommands)]
        public async Task grandient(string hex)
        {
            EmbedBuilder embed = new EmbedBuilder();

            if (!IsValidHex(hex))
            {
                await RespondAsync("That hex is invalid.", ephemeral: true); return;
            }
            hex = hex.Replace("#", "");
            string url = $"https://api.alexflipnote.dev/color/{hex}"; // Replace with the actual API endpoint

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("parse", "application/json");
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine(jsonResponse);

                // Parse the JSON response
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ColorInfo colorInfo = JsonConvert.DeserializeObject<ColorInfo>(jsonResponse);

                if (colorInfo != null)
                {
                    //Console.WriteLine($"Name: {colorInfo.Name}");
                    //Console.WriteLine($"Square Image URL: {colorInfo.Images?.Square}");
                    //Console.WriteLine($"Gradient Image URL: {colorInfo.Images?.Gradient}");
                    embed.Title = $"Gradient for {colorInfo.Name}";
                    embed.ImageUrl = colorInfo.Images?.Gradient;
                    embed.Color = new Discord.Color(HexToRGB(hex).R, HexToRGB(hex).G, HexToRGB(hex).B);
                    embed.WithFooter("Obscūrus • Team Unity Development");
                    embed.WithCurrentTimestamp();
                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    //Console.WriteLine("Error parsing JSON response.");
                    embed.Title = "Error";
                    embed.Description = "Error parsing JSON response.";
                    embed.Color = Discord.Color.Red;
                    embed.WithFooter("Obscūrus • Team Unity Development");
                    embed.WithCurrentTimestamp();
                    await RespondAsync(embed: embed.Build(), ephemeral: true);
                }
            }
            else
            {
                embed.Title = "Error";
                embed.Description = $"HTTP Error: {response.StatusCode}";
                embed.Color = Discord.Color.Red;
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await RespondAsync(embed: embed.Build(), ephemeral: true);
            }

        }
    }
}
