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
        char prefixCharacter = '!';

        static void Main(string[] args)
        {
            Program bot = new Program();
        }

        public Program()
        {
            bot = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            bot.UsingCommands(x =>
            {
                x.PrefixChar = prefixCharacter;
                x.AllowMentionPrefix = true;
            });
            
            commands = bot.GetService<CommandService>();
            
            RegisterRollDiceCommand();
            RegisterHelloCommand();
            RegisterAddMeCommand();
            RegisterNonStandardCommands();

            //commands.CreateCommand("SteamMe") // Command name
            //        .Parameter("SteamProfile", ParameterType.Required) // Steam name

            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MzA5Nzg4NjU2MDAzMDU1NjE2.C-0k9A.7_v7ulouST3I358v2oMThI6yCPE", TokenType.Bot);
            });
        }

        private void RegisterHelloCommand()
        {
            commands.CreateCommand("Hello") // Command name
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Hello world" + e.User.Mention);
                });
        }

        private void RegisterAddMeCommand()
        {
            commands.CreateCommand("Addme") // Command name
                .Parameter("GroupName", ParameterType.Required)
                .Do(async (e) =>
                {
                    try
                    {
                        var groupName = e.GetArg("GroupName");
                        if (e.Server.FindRoles(groupName).Count() < 1)
                        {
                            await e.Channel.SendMessage("Could not find " + groupName);
                            var newRole = await e.Server.CreateRole(groupName);
                            await e.User.AddRoles(newRole);
                        }
                        else
                        {
                                await e.Channel.SendMessage("Found " + groupName);
                                Role roleExists = e.Server.FindRoles(groupName).First();
                                await e.User.AddRoles(roleExists);
                        }
                        await e.Channel.SendMessage("I have added you to group " + groupName + " " + e.User.Mention);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }

        private void RegisterRollDiceCommand()
        {
            commands.CreateCommand("roll") // Command name
                .Parameter("modifier", ParameterType.Optional)
                .Do(async (e) =>
                {
                    try
                    {
                        string modifier;
                        string output;

                        modifier = e.GetArg("modifier");
                        modifier = modifier == "" ? "NOTHING" : modifier;
                        output = RollDice(modifier);

                        await e.Channel.SendMessage(output);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }

        private void RegisterNonStandardCommands()
        {
            bot.MessageReceived += async (s, e) => 
            {
                // Check to make sure that the bot is not the author
                if (!e.Message.IsAuthor)
                {
                    if (e.Message.RawText == "ayyy")
                    {
                        await e.Channel.SendMessage("Ayyy, lmao");
                    }
                    // Message starts with "!"
                    if (e.Message.RawText.Substring(0,1) == prefixCharacter.ToString())
                    {
                        string[] messageArray;
                        string command;
                        ChannelPermissions userPermissions;

                        userPermissions = e.Message.User.GetPermissions(e.Message.Channel);
                        messageArray = e.Message.RawText.Split(' ');
                        command = messageArray[0].Substring(1);
                        switch (command.ToLower())
                        {
                            case "newrole":
                                try
                                {
                                    if (userPermissions.ManagePermissions)
                                    {
                                        string newRoleName;
                                        Role role;
                                        int addedUsers = 0;

                                        newRoleName = messageArray[1];

                                        if (e.Server.FindRoles(newRoleName).Count() < 1)
                                        {
                                            role = await e.Server.CreateRole(newRoleName);
                                        }
                                        else
                                        {
                                            role = e.Server.FindRoles(newRoleName).First();
                                        }
                                        foreach (User userToAdd in e.Message.MentionedUsers)
                                        {
                                            if (!userToAdd.HasRole(role))
                                            {
                                                await userToAdd.AddRoles(role);
                                                addedUsers++;
                                            }
                                        }

                                        await e.Channel.SendMessage(string.Format("Added {0} users to role {1}", addedUsers, newRoleName));
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("You are not authorised to do that");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
                
            /*if (e.message.slice(0, 1) == trigger)
            {
                var cmd = message.substr(1, message.indexOf(' ') - 1).toLowerCase();
                // Get what command the user is trying to trigger
                if (commands.indexOf(cmd) > -1)
                {
                    // Check if the command exists in the commands array
                    var words = message.split(" ");
                    // Get all arguments
                    switch (cmd)
                    {
                        // Do stuff
                        case "help":
                            console.log(cmd); // help
                            console.log(words[1]); // 123
                            console.log(words[2]); // 456
                            console.log(words[3]); // 789
                            break;
                        case "random":
                            console.log("Hey, someone hit !random");
                            break;
                    }
                }
            }*/
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

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
