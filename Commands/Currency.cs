using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Obscure;
using Obscure.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using static Obscure.enums;

namespace Harmony_Utilities.Commands
{
    public class Currency : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private InteractionHandler _handler;
        private readonly DiscordSocketClient _client;


        public Currency(InteractionHandler handler, DiscordSocketClient client)
        {
            _handler = handler;
            _client = client;
        }


        [SlashCommand("heist", "Group together and try to break into another persons bank (2 PEOPLE REQUIRED MINIMUM)")]
        public async Task heist(IGuildUser target)
        {
            var victim = Program.guilds.GetGuild(target.Guild.Id).GetUser(target.Id);
            var robber = (IGuildUser)Context.User;
            Obscure.API.User.GetUser((IGuildUser)Context.User).profile.startingHeist = true;
            if (target.IsBot == true || target == Context.User) { await RespondAsync("You cannot heist that user!", ephemeral: true); return; };
            if (Obscure.API.User.GetUser(robber).profile.lastBankRobbery.AddDays(1) > DateTime.UtcNow) { await RespondAsync("You can only heist once every 24 hours!", ephemeral: true); return; }
            if (Obscure.API.User.GetUser(robber).profile.startingHeist) { await RespondAsync("You are already starting another heist!", ephemeral: true); return; };
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
        public async Task deposit(int amount)
        {
            var p = Program.guilds.GetGuild(Context.Guild.Id).GetUser(Context.User.Id);
            if (p.profile.currency < amount) { await RespondAsync("You don't have that much in your wallet!", ephemeral: true); return; }
            if (p.profile.robberyInProgress == true || p.profile.heistInProgress == true) { await RespondAsync("You can't do that while you're being robbed or heisted!", ephemeral: true); return; }
            p.profile.currency -= amount;
            p.profile.bank += amount;

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



        [SlashCommand("slotmachine", "Gamble your life away")]
        public async Task slots([Choice("10", 10)][Choice("50", 50)][Choice("100", 100)][Choice("500", 500)][Choice("1000", 1000)] uint amount = 10)
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
