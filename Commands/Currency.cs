﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Obscure;
using System.Text;
using Fergun.Interactive;
using Obscure.API;
using System.Net;

namespace Harmony_Utilities.Commands
{
    public class Currency : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;




        private readonly InteractiveService _interactive;

        public Currency(InteractionHandler handler, DiscordSocketClient client, InteractiveService interactive)
        {
            _handler = handler;
            _client = client;
            _interactive = interactive;
        }


        [SlashCommand("heist", "Group together and try to break into another persons bank (2 PEOPLE REQUIRED MINIMUM)")]
        public async Task heist(IGuildUser target)
        {
            var victim = Program.guilds.GetGuild(target.Guild.Id).GetUser(target.Id);
            var robber = (IGuildUser)Context.User;
            if (target.IsBot == true || target == Context.User) { await RespondAsync("You cannot heist that user!", ephemeral: true); return; };
            if (Obscure.API.User.GetUser(robber).profile.lastBankRobbery.AddDays(1) > DateTime.UtcNow) { await RespondAsync("You can only heist once every 24 hours!", ephemeral: true); return; }
            if (Obscure.API.User.GetUser(robber).profile.startingHeist) { await RespondAsync("You are already starting another heist!", ephemeral: true); return; };
            Obscure.API.User.GetUser((IGuildUser)Context.User).profile.startingHeist = true;
            List<IGuildUser> robbers = new List<IGuildUser>();
            EmbedFieldBuilder dynamicList = new EmbedFieldBuilder()
                .WithName("Participants")
                .WithValue($"{Context.User.Mention}\n")
                .WithIsInline(true);
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Bank robbery")
                .WithDescription($"Heist on {target.Mention} initated by {robber.Mention} - To join the heist react to this message.")
                .AddField(dynamicList)
                .AddField("Time left to join:", $"<t:{DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds()}:R>", false)
                .WithFooter("Obscūrus • Team Unity Development")
                .WithCurrentTimestamp();
            await RespondAsync($"{target.Mention}", embed: embed.Build());
            IEmote e = new Emoji("\uD83D\uDD10");
            await Context.Interaction.GetOriginalResponseAsync().Result.AddReactionAsync(e);
            _client.ReactionAdded += reaction;
            async Task reaction(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction r)
            {

                Console.WriteLine("Reaction recieved");
                if (!r.User.Value.IsBot && r.User.Value != target)
                {
                    Console.WriteLine("passed check 1");
                    if (r.Channel == Context.Channel)
                    {
                        Console.WriteLine("passed check 2");
                        if (r.User.Value != robber && Obscure.API.User.GetUser((IGuildUser)r.User.Value).profile.startingHeist == false)
                        {
                            Console.WriteLine("passed check 3");
                            if (r.Emote.Name == e.Name)
                            {
                                Console.WriteLine("passed check 4");
                                if (Obscure.API.User.GetUser((IGuildUser)r.User.Value).profile.lastBankRobbery.AddDays(1) > DateTime.UtcNow)
                                {
                                }
                                else
                                {
                                    Obscure.API.User.GetUser((IGuildUser)r.User.Value).profile.startingHeist = true;
                                    robbers.Add((IGuildUser)r.User.Value);
                                    Console.WriteLine($"Added {r.User.Value.Username}");
                                    embed.Fields.Find(x => x.Name == "Participants").Value += $"{r.User.Value.Mention}\n";
                                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                                }
                            }
                        }
                    }
                }
            }
            for(int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
            }
            Console.WriteLine("Passed the wait");
            _client.ReactionAdded -= reaction;

            if (robbers.Count() == 0)
            {
                foreach (var r in robbers)
                {
                    Obscure.API.User.GetUser(r).profile.startingHeist = false;
                }
                embed.Description = $"The heist on {target.Mention} didn't start due to the team being too small :(";
                Obscure.API.User.GetUser((IGuildUser)Context.User).profile.startingHeist = false;
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                return;
            }
            Console.WriteLine("Starting heist");
            EmbedBuilder embed1 = new EmbedBuilder();
            embed1.Title = "Bank robbery In Progress";
            embed1.Description = $"";
            embed1.AddField("Target", $"{target.GlobalName}", false);
            string participants = "";
            participants += $"{robber.Mention}\n";
            foreach (var r in robbers)
            {
                Obscure.API.User.GetUser(r).profile.startingHeist = false;
                participants += $"{r.Mention}\n";
            }
            embed1.AddField("Participants", participants, false);
            embed1.AddField("Time until completion", $"<t:{DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}:R>", false);
            
            await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed1.Build());
            victim.profile.heistInProgress = true;
            Obscure.API.User.GetUser((IGuildUser)Context.User).profile.startingHeist = false;
            Obscure.API.User.GetUser(robber).profile.lastBankRobbery = DateTime.UtcNow;
            foreach (var r in robbers)
            {
                Obscure.API.User.GetUser(r).profile.lastBankRobbery = DateTime.UtcNow;
            }
            await Task.Delay(300000);
            victim.profile.heistInProgress = false;
            await Context.Interaction.DeleteOriginalResponseAsync();
            var rnd = new Random();
            int chance = rnd.Next(0, 101);
            if (chance > 66 && chance < 100)
            {
                int amount = rnd.Next(0, (int)victim.profile.bank);
                int amountPP = amount / (robbers.Count() + 1);
                EmbedBuilder fE = new EmbedBuilder()
                    .WithTitle("Profits")
                    .WithDescription($"**Amount of pickles stolen from \"{target.GlobalName}\" **: {amount}\n{participants} all recieved {amountPP}pickles each!");
                victim.profile.bank -= amount;
                foreach (var r in robbers)
                {
                    Obscure.API.User.GetUser(r).profile.currency += amountPP;
                }
                Obscure.API.User.GetUser(robber).profile.currency += amountPP;
                await Context.Channel.SendMessageAsync(embed: fE.Build());
            }
            else
            {
                EmbedBuilder em = new EmbedBuilder()
                    .WithTitle("Bank robbery failed!")
                    .AddField("Target", $"{target.GlobalName}", false)
                    .AddField("Participants", participants, false);
                await Context.Channel.SendMessageAsync(embed: em.Build());
            }
        }

        [SlashCommand("rob", "Try steal money from someone else's wallet")]
        public async Task rob(IGuildUser target)
        {
            var victim = Program.guilds.GetGuild(target.Guild.Id).GetUser(target.Id);
            var robber = (IGuildUser)Context.User;
            if (target.IsBot == true || target == Context.User) { await RespondAsync("You cannot rob that user!", ephemeral: true); return; };
            if (victim.profile.lastRobbery.AddMinutes(2) > DateTime.UtcNow)
            {
                await RespondAsync($"You cannot rob that user, they were only robbed: <t:{((DateTimeOffset)victim.profile.lastRobbery).ToUnixTimeSeconds()}:R>, give it a while!", ephemeral: true); return;
            }
            else
            {
                if (victim.profile.robberyInProgress == true) { await RespondAsync($"You cannot rob \"{target.Mention}\", {robber.Mention} they are already being robbed by someone else!", ephemeral: true); return; }

                if (victim.profile.currency == 0) { await RespondAsync($"You cannot rob \"{target.Mention}\", {robber.Mention} they are poor!", ephemeral: true); return; }
                else
                {
                    victim.profile.robberyInProgress = true;
                    victim.profile.lastRobbery = DateTime.UtcNow;
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = $"lol"
                    };
                    await RespondAsync($"{robber.Mention} is trying to rob: {target.Mention}...");
                    await Task.Delay(5000);
                    var rnd = new Random();
                    var chance = rnd.Next(0, 100);
                    Console.WriteLine($"Chance: {chance}");
                    var amount = rnd.Next(1, (int)victim.profile.currency + 1);
                    if (chance > 30 && chance < 60)
                    {
                        victim.profile.currency -= amount;
                        Obscure.API.User.GetUser(robber).profile.currency += amount;
                        embed.Title = $"SUCCESS!";
                        embed.Description = $"{robber.Mention} is a thieving bastard and stole: {amount} from: {target.Mention}'s wallet!";
                    }
                    else
                    {
                        embed.Title = $"Failure!";
                        embed.Description = $"{target.Mention}'s money is safe... for now. (they should probably store their money in their bank to keep it safer)";
                    }
                    embed.WithFooter("Obscūrus • Team Unity Development");
                    embed.WithCurrentTimestamp();
                    victim.profile.robberyInProgress = false;
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                }
            }
        }


        [SlashCommand("balance", "See how poor you are")]
        public async Task balcheck(IGuildUser user = null)
        {
            if (user == null) user = Context.Guild.GetUser(Context.User.Id);
            var p = Program.guilds.GetGuild(user.Guild.Id).GetUser(user.Id);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"{user.GlobalName}'s balance:",
            };
            embed.AddField("Wallet:", $"{p.profile.currency}pickles", false);
            embed.AddField("Bank:", $"{p.profile.bank}pickles", false);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("bonuspay", "Give out a bonus check!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task bonuspay(IRole role, int amount)
        {
            foreach (IUser user in Context.Guild.Users)
            {
                if (user.IsBot == true) { continue; }
                var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);
                Task.Delay(10);
                var useringuild = Context.Guild.GetUser(user.Id);
                if (useringuild.Roles.Contains(role))
                {
                    p.profile.currency += amount;
                }
                else;

            }
            await RespondAsync($"{Context.User.Mention} has granted all users in {role.Mention} a free payment of **{amount}**pickles to gamble with!");
        }

        [SlashCommand("deposit", "Put money in your bank to keep it safe :)")]
        public async Task deposit(string amount)
        {
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);


            if (amount != null)
            {
                if (float.TryParse(amount, out float numamount))
                {

                    if (p.profile.currency < numamount) { await RespondAsync("You don't have that much in your wallet!", ephemeral: true); return; }
                    if (p.profile.robberyInProgress == true || p.profile.heistInProgress == true) { await RespondAsync("You can't do that while you're being robbed or heisted!", ephemeral: true); return; }
                    p.profile.currency -= numamount;
                    p.profile.bank += numamount;
                }
                else if (amount.ToLower() == "all")
                {
                    p.profile.bank += p.profile.currency;
                    p.profile.currency -= p.profile.currency;
                }
                else
                {
                    await RespondAsync("You entered an incorrect value, valid arguments are <amount> or \"all\"", ephemeral: true); return;
                }
            }









            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Deposit",
                Description = $"Success! You deposited *{amount}*pickles into your bank"
            };
            embed.AddField("Wallet:", $"{p.profile.currency}pickles", false);
            embed.AddField("Bank:", $"{p.profile.bank}pickles", false);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("withdraw", "Take money out of your bank to lose on gambling :)")]
        public async Task withdraw(int amount)
        {
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);
            if (p.profile.bank < amount) { await RespondAsync("You don't have that much in your bank!", ephemeral: true); return; }
            if (p.profile.robberyInProgress == true || p.profile.heistInProgress == true) { await RespondAsync("You can't do that while you're being robbed or heisted!", ephemeral: true); return; }
            if (amount.ToString() == "all")
            {
                p.profile.currency += p.profile.bank;
                p.profile.bank -= p.profile.bank;


            }
            p.profile.currency += amount;
            p.profile.bank -= amount;

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Withdrawal",
                Description = $"Success! You withdrew *{amount}*pickles from your bank"
            };
            embed.AddField("Wallet:", $"{p.profile.currency}pickles", false);
            embed.AddField("Bank:", $"{p.profile.bank}pickles", false);
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("reductpay", "Take some money away from people!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task reducepay(IRole role, int amount)
        {
            foreach (IUser user in Context.Guild.Users)
            {
              
                    if (user.IsBot == true ) { continue; }
                    var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);

                    await Task.Delay(10);
                var useringuild = Context.Guild.GetUser(user.Id);
                if (useringuild.Roles.Contains(role) && p.profile.currency >= amount)
                {
                    p.profile.currency -= amount;
                }
                else
                {
                    p.profile.currency = 0;
                }
                
            }
            await RespondAsync($"{Context.User.Mention} has fined all users in {role.Mention} a payment of **{amount}**pickles!");
        }

        [SlashCommand("pay", "Give someone some of your money")]
        public async Task pay(IUser user, int amount)
        {
            var author = Program.guilds.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);
            if (user.IsBot == true || user == Context.User) { await RespondAsync("You cannot pay that user!", ephemeral: true); return; };

            if (author.profile.currency < amount) { await RespondAsync("You don't have enough pickles for that in your wallet!", ephemeral: true); return; }
            if (p.profile.robberyInProgress == true || p.profile.heistInProgress == true) { await RespondAsync("You can't do that while you're being robbed or heisted!", ephemeral: true); return; }
            author.profile.currency -= amount;
            p.profile.currency += amount;
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Payment",
                Description = $"{Context.User.Mention} has paid {user.Mention}: *{amount}*pickles! \n{Context.User.Mention} now has *{author.profile.currency}*pickles! \n{user.Mention} now has *{p.profile.currency}*pickles!"
            };

            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync($"{Context.User.Mention} - {user.Mention}", embed: embed.Build());

        }




        [SlashCommand("unscramble", "Unscramble the word before the timer runs out!")]
        public async Task unscramble()
        {
            var user = Guild.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);
            var author = Context.User;
            var rnd = new Random();

            var wclient = new WebClient();
            var downloadedString = "";

            var wordType = rnd.Next(0, 2);
            if (wordType == 0) { downloadedString = wclient.DownloadString("http://www.wordgenerator.net/application/p.php?id=nouns&type=1&spaceflag=false"); }
            if (wordType == 1) { downloadedString = wclient.DownloadString("http://www.wordgenerator.net/application/p.php?id=adjectives&type=1&spaceflag=false"); }
            //if (wordType == 2) { downloadedString = wclient.DownloadString("http://www.wordgenerator.net/application/p.php?id=verbs&type=1&spaceflag=false"); }
                // id= can be: nouns, adjectives, verbs, dictionary_words.
            string[] words = downloadedString.Split(',');

            int index = rnd.Next(0, words.Length);
            var word = words[index].ToString();
            Console.Write($"\n{word}\n");

            StringBuilder jumble = new StringBuilder(word);
            int length = jumble.Length;
            for (int i = length - 1; i > 0; i--)
            {
                int j = rnd.Next(i);
                char temp = jumble[j];
                jumble[j] = jumble[i];
                jumble[i] = temp;
            }

            var rewrnd = new Random();
            var reward = (int)Math.Ceiling(rewrnd.Next(50, 500) / 0.67 * word.Length);
            Console.WriteLine($"Reward is {reward}");
            long timeleft = DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds();
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Scrambled",
                Description = $"**Word: {jumble}** \nYou have to unscramble this word <t:{timeleft}:R> to get a reward! \nThe reward is: **{reward}pickles**!"
            };
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            await RespondAsync($"{Context.User.Mention}", embed: embed.Build());
            user.profile.lastUnscramble = DateTime.UtcNow.ToUniversalTime();
            await Task.Delay(50);
     
            bool win = false;
            while (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < timeleft)
            {
                var timer = timeleft - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await Task.Delay(1000);

                Console.WriteLine("Getting message");
                var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
                Console.WriteLine("Got message");
                var uresponse = messages.FirstOrDefault();
                var msgRef = new MessageReference(messages.First().Id);
                Console.WriteLine($"Current time (unix) {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                Console.WriteLine($"Time left (unix): {timer}");
                Console.WriteLine($"Time left: {timer} seconds");
                if (uresponse == null) { Console.WriteLine("response null"); }

                if (uresponse.Content == null) { Console.WriteLine("response content null"); }

                if (uresponse.Author != author) { Console.WriteLine("Recieved response from someone who was not the author"); }
                else
                {
                    if (uresponse.Content.ToLower() == word.ToLower())
                    {
                        Console.WriteLine("Recieved response from someone who WAS the author \nThe response was right!");
                        embed.Description = $"**You got it right** - You had {timer} seconds left! \nYou have won: **{reward}pickles**! \nThe word was: **{word}**";
                        uresponse.DeleteAsync();
                        user.profile.currency += reward;
                        win = true;
                        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Recieved response from someone who WAS the author \nBut the response was wrong");
                    }

                }

            }
            if (!win)
            {
                embed.Description = $"**You ran out of time** :( \nThe reward was: **{reward}pickles** \nThe word was: **\"{word}**\"";
                Console.WriteLine("Ran out of time :)");
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
            }
            else
            {
                return;
            }
            user.profile.lastUnscramble = DateTime.UtcNow.ToUniversalTime();
        }

        [SlashCommand("slotmachine", "Gamble your life away")]
        public async Task slots([Choice("10", 10)][Choice("50", 50)][Choice("100", 100)][Choice("500", 500)][Choice("1000", 1000)][Choice("5000", 5000)][Choice("10000", 10000)] uint amount = 10)
        {

            var user = Context.User;
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(user.Id);
            if (p.profile.currency < amount) { await RespondAsync("You don't have enough pickles for that in your wallet!", ephemeral: true); return; }
            await RespondAsync("Spinning....");
            p.profile.currency -= amount;
            int win = -1;
            string[] emojis = { ":cucumber:", ":eggplant:", ":diamond_shape_with_a_dot_inside:", ":sun:" };

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"Slot Machine! - {Context.User.GlobalName}"
            };
            embed.WithFooter("Obscūrus • Team Unity Development");
            embed.WithCurrentTimestamp();
            var rnd = new Random();

            await Task.Delay(1500);
            var slot1 = rnd.Next(0, emojis.Count());
            var slot2 = rnd.Next(0, emojis.Count());
            var slot3 = rnd.Next(0, emojis.Count());
            var e1 = emojis[slot1];
            var e2 = emojis[slot2];
            var e3 = emojis[slot3];
            if (slot1 != slot2 || slot2 != slot3 || slot1 != slot3) { win = 0; }
            else if (slot1 == slot2 && slot2 == slot3) { win = 1; }

            if (win == 0)
            {
                embed.Description = $"**YOU LOSE! BETTER LUCK NEXT TIME** \nYou rolled: {e1}{e2}{e3} \nNew balance: *{p.profile.currency}*pickles";
            }
            else
            {
                p.profile.currency += amount * 10;
                embed.Description = $"**YOU WIN:** *{amount * 10}*pickles! CONGRATULATIONS! \nYou rolled: {e1}{e2}{e3} \nNew balance: *{p.profile.currency}*pickles";

            }
            await Context.Interaction.ModifyOriginalResponseAsync(x => { x.Content = ""; x.Embed = embed.Build(); });


        }

        [SlashCommand("daily", "Grants you a plebs daily allowance.")]
        public async Task daily()
        {
            var u = Obscure.API.User.GetUser((IGuildUser)Context.User);
            var ubank = u.profile.currency;
            if(u.profile.lastDaily.AddDays(1) > DateTime.UtcNow)
            {
                await RespondAsync($"You can do that again <t:{((DateTimeOffset)u.profile.lastDaily.AddDays(1)).ToUnixTimeSeconds()}:R>.", ephemeral: true); return;
            }
            else
            {
                u.profile.lastDaily = DateTime.UtcNow;
                u.profile.currency += 1000;
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = $"{Context.User.GlobalName}'s Daily Allowance (+1000pickles)",
                };
                embed.AddField("Old Wallet balance:", $"{ubank}pickles", false);
                embed.AddField("New Wallet balance:", $"{u.profile.currency}pickles", false);
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await RespondAsync(embed: embed.Build());
            }
        }

        [SlashCommand("weekly", "Grants you a plebs weekly allowance.")]
        public async Task weekly()
        {
            var u = Obscure.API.User.GetUser((IGuildUser)Context.User);
            var ubank = u.profile.currency;
            if (u.profile.lastWeekly.AddDays(7) > DateTime.UtcNow)
            {
                await RespondAsync($"You can do that again <t:{((DateTimeOffset)u.profile.lastWeekly.AddDays(7)).ToUnixTimeSeconds()}:R>.", ephemeral: true); return;
            }
            else
            {
                u.profile.lastWeekly = DateTime.UtcNow;
                u.profile.currency += 13000;
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = $"{Context.User.GlobalName}'s Weekly Allowance (+13000pickles)",
                };
                embed.AddField("Old Wallet balance:", $"{ubank}pickles", false);
                embed.AddField("New Wallet balance:", $"{u.profile.currency}pickles", false);
                embed.WithFooter("Obscūrus • Team Unity Development");
                embed.WithCurrentTimestamp();
                await RespondAsync(embed: embed.Build());
            }
        }
    }
}
