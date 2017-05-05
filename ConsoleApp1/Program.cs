using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoOpBot
{
    class Program
    {
        DiscordClient bot;
        CommandService commands;

        static void Main(string[] args)
        {
            Program bot = new Program();
        }

        public Program()
        {
            //var bot = new DiscordClient();
            bot = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                //x.LogHandler = Log;
            });

            bot.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;

            });

            


            commands = bot.GetService<CommandService>();


            RegisterRollDiceCommand();

            //commands.CreateCommand("SteamMe") // Command name
            //        .Parameter("SteamProfile", ParameterType.Required) // Steam name
            commands.CreateCommand("Hello") // Command name
                    .Do(async (e) =>
                    {
                        await e.Channel.SendMessage("Hello world" + e.User.Mention);
                    });

            commands.CreateCommand("Addme") // Command name
                    .Parameter("GroupName", ParameterType.Required)
                    .Do(async (e) =>
                        {
                            var groupName = e.GetArg("GroupName");
                            //Console.WriteLine(e.Server.FindRoles(groupName,true));
                            var roleExists = e.Server.FindRoles(groupName, true);

                            if (e.Server.FindRoles(groupName).Count() != 1)
                            {
                                await e.Channel.SendMessage("Could not find "+groupName);
                                await e.Server.CreateRole(groupName);
                            } else
                            {
                                await e.Channel.SendMessage("Found " + groupName);
                                await e.User.AddRoles(e.Server.FindRoles(groupName).First());
                            }
                            await e.Channel.SendMessage("I have added you to group " + groupName + " " + e.User.Mention);
                        });

            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MzA5Nzg4NjU2MDAzMDU1NjE2.C-0k9A.7_v7ulouST3I358v2oMThI6yCPE", TokenType.Bot);
            });
        }

        private void RegisterRollDiceCommand()
        {
            commands.CreateCommand("roll")
                .Parameter("modifier", ParameterType.Optional)
                .Do(async (e) =>
                {
                    string modifier;
                    string output;

                    modifier = e.GetArg("modifier");
                    modifier = modifier == "" ? "NOTHING" : modifier;
                    output = RollDice(modifier);

                    await e.Channel.SendMessage(output);
                });
        }

        private string RollDice(string inputMessage)
        {
            string output;
            string[] splitInput;
            int numberOfDice;
            int sidesOnDice;
            int totalRoll;
            Random rng;

            rng = new Random();
            numberOfDice = 1;
            sidesOnDice = 6;
            totalRoll = 0;

            // check if the roll has been modified with an input
            if (inputMessage.Contains("d"))
            {
                splitInput = inputMessage.Split('d');
                if (splitInput.Length == 2)
                {
                    numberOfDice = int.Parse(splitInput[0]);
                    sidesOnDice = int.Parse(splitInput[1]);
                }
            }

            if (sidesOnDice == 1)
            {
                return "Why would you even try to roll a d1?!";
            }
            else if (sidesOnDice == 0)
            {
                return "Wow... good try... a 0 sided dice";
            }
            else if (sidesOnDice < 0)
            {
                return "Negative sides! Now you're just being silly";
            }

            // do the roll
            for (int rollNumber = 1; rollNumber <= numberOfDice; rollNumber++)
            {
                totalRoll += rng.Next(1, sidesOnDice);
            }

            output = string.Format("You rolled {0}d{1} and got {2}", numberOfDice, sidesOnDice, totalRoll);

            return output;
        }
    }
}
