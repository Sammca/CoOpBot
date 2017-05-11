using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace CoOpBot
{
    class Program
    {
        // Define & initialise variables
        DiscordClient bot;
        CommandService commands;
        char prefixCharacter = '!';
        NameValueCollection userRecentMessageCounter = new NameValueCollection();

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
                x.HelpMode = HelpMode.Public;
            });
            
            commands = bot.GetService<CommandService>();
            
            RegisterAddRoleCommand();
            RegisterAntiSpamFunctionality();
            RegisterCountdownCommand();
            RegisterMakeTeamsCommand();
            RegisterNoveltyResponseCommands();
            RegisterRemoveTeamChannelsCommand();
            RegisterRoleListCommand();
            RegisterRollDiceCommand();

            //commands.CreateCommand("SteamMe") // Command name
            //        .Parameter("SteamProfile", ParameterType.Required) // Steam name

            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MzA5Nzg4NjU2MDAzMDU1NjE2.C-0k9A.7_v7ulouST3I358v2oMThI6yCPE", TokenType.Bot);
            });
        }

        /************************************************
         * 
         * Command template
         * 
        ************************************************/
        // Good command template, but basically useless.
        /*private void RegisterHelloCommand() 
        {
            commands.CreateCommand("Hello") // Command name
                .Do(async (e) =>
                {
                    await e.Channel.SendMessage("Hello world" + e.User.Mention);
                });
        }*/



        /************************************************
         * 
         * Bot admin functions
         * 
        ************************************************/

        private void RegisterAntiSpamFunctionality()
        {
            bot.MessageReceived += async (s, e) =>
            {
                User messageSender;
                int messageCount;
                ChannelPermissionOverrides channelPermissionOverrides;

                messageSender = e.Message.User;

                // Check to make sure that a bot is not the author
                // Also check if admin, since admins ignore the channel permission override
                if (!messageSender.ServerPermissions.Administrator && !messageSender.IsBot)
                {
                    channelPermissionOverrides = new ChannelPermissionOverrides(sendMessages: PermValue.Deny);

                    if (userRecentMessageCounter[messageSender.Name] == null)
                    {
                        userRecentMessageCounter[messageSender.Name] = 0.ToString();
                    }

                    messageCount = CountMessage(messageSender, 1);

                    if (messageCount > 2)
                    {
                        await e.Channel.SendMessage("#StopCamSpam");
                        await e.Channel.AddPermissionsRule(messageSender, channelPermissionOverrides);

                        //await Task.Delay(5000).ContinueWith(t => e.Channel.SendMessage("5 seconds passed"));
                        await Task.Delay(8000).ContinueWith(t => e.Channel.RemovePermissionsRule(messageSender));
                    }

                    await Task.Delay(8000).ContinueWith(t => CountMessage(messageSender, -1));
                }
            };
        }

        /************************************************
         * 
         * Roles & permissions
         * 
        ************************************************/

        private void RegisterAddRoleCommand()
        {
            commands.CreateCommand("AddRole") // Command name
                .Alias("AddMe", "ar") // Alternate command names
                .Parameter("RoleName", ParameterType.Required)
                .Parameter("users", ParameterType.Multiple)
                .Description("Adds the user to the requested Role.")
                .Do(async (e) =>
                {
                    try
                    {
                        var RoleName = e.GetArg("RoleName");
                        var mentionedUsers = e.Args.Length > 1 ? true : false;

                        Role roleObj;
                        int addedUsers = 0;

                        ChannelPermissions userPermissions;
                        userPermissions = e.Message.User.GetPermissions(e.Message.Channel);
                        if (userPermissions.ManagePermissions)
                        {

                            if (e.Server.FindRoles(RoleName).Count() < 1)
                            {
                                await e.Channel.SendMessage("New role " + RoleName + " created.");
                                roleObj = await e.Server.CreateRole(RoleName, null, null, false, true);
                            }
                            else
                            {
                                roleObj = e.Server.FindRoles(RoleName).First();
                            }

                            if (mentionedUsers)
                            {
                                foreach (User userToAdd in e.Message.MentionedUsers)
                                {
                                    if (!userToAdd.HasRole(roleObj))
                                    {
                                        await userToAdd.AddRoles(roleObj);
                                        addedUsers++;
                                    } /*else
                                    { // removed, to be replaced with one message at the end of function
                                        await e.Channel.SendMessage(userToAdd.Nickname + " is already in role " + RoleName);
                                    }*/
                                }
                            }
                            else
                            {
                                if (!e.User.HasRole(roleObj))
                                {
                                    await e.User.AddRoles(roleObj);
                                    addedUsers++;
                                }
                                else
                                {
                                    await e.Channel.SendMessage("You are already in " + RoleName);
                                }
                            }
                            await e.Channel.SendMessage(string.Format("Added {0} users to role {1}", addedUsers, RoleName));
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
                });
        }

        private void RegisterRoleListCommand()
        {
            commands.CreateCommand("Whois") // Command name
                .Alias("RoleList","ListRole")
                .Description("Returns a list of users with requested Roles. Roleplay is not permitted.")
                .Parameter("RoleName", ParameterType.Required)
                .Do(async (e) =>
                {
                    string roleName;
                    Role role;
                    string output = "";

                    roleName = e.GetArg("RoleName");

                    if (e.Server.FindRoles(roleName).Count() < 1)
                    {
                        await e.Channel.SendMessage(string.Format("Role {0} not found", roleName));
                    }
                    else
                    {
                        role = e.Server.FindRoles(roleName).First();
                        foreach (User user in role.Members)
                        {
                            if (user.Nickname != null)
                            {
                                output += user.Nickname + "\r\n";
                            }
                            else
                            {
                                output += user.Name + "\r\n";
                            }
                        }

                        await e.Channel.SendMessage(output);
                    }
                });
        }


        /************************************************
         * 
         * Voice & text channels
         * 
        ************************************************/

        private void RegisterRemoveTeamChannelsCommand()
        {
            commands.CreateCommand("removeTeams") // Command name
                .Description("Removes any channels created by the MakeTeams command. Cleanup your toys when your done playing!")
                .Do(async (e) =>
                {
                    Channel[] serverChannelList;

                    serverChannelList = e.Server.VoiceChannels.ToArray();
                    foreach (Channel voiceChannel in serverChannelList)
                    {
                        if (voiceChannel.Name.Substring(0, 4) == "Team")
                        {
                            await voiceChannel.Delete();
                        }
                    }
                });
        }

        /************************************************
         * 
         * Co-op gaming
         * 
        ************************************************/

        private void RegisterMakeTeamsCommand()
        {
            commands.CreateCommand("MakeTeams") // Command name
                .Parameter("NumberOfTeams", ParameterType.Required)
                .Description("Split the current channel into number of teams specified, and moves them into their own channel.")
                .Do(async (e) =>
                {
                    try
                    {
                        // Define variables
                        int numberOfTeams;
                        int teamNumber;
                        int randomUserPosition;
                        int userCount;
                        List<int> usedPositions;
                        Channel curVoiceChannel;
                        string messageOutput;
                        Random rng;
                        User[] users;
                        List<TeamAssignment> teamAssignmentList;
                        TeamAssignment curUserTeamAssignment;
                        List<Channel> teamChannels;

                        // Initialise variables
                        numberOfTeams = int.Parse(e.GetArg("NumberOfTeams"));
                        rng = new Random();
                        messageOutput = "";
                        teamAssignmentList = new List<TeamAssignment> { };
                        usedPositions = new List<int> { };
                        teamChannels = new List<Channel> { };
                        teamNumber = 1;
                        curVoiceChannel = e.User.VoiceChannel;

                        // Check that the caller is in a voice channel
                        if (curVoiceChannel == null)
                        {
                            messageOutput += string.Format("{0}, you must be in a voice channel to use this command", e.User.Name);
                        }
                        else
                        {
                            // make array of users in the current voice channel
                            users = curVoiceChannel.Users.ToArray();
                            userCount = users.Length;

                            if (numberOfTeams > userCount)
                            {
                                numberOfTeams = userCount;
                            }

                            // Assign team numbers to users
                            while (userCount > usedPositions.Count)
                            {
                                curUserTeamAssignment = new TeamAssignment();
                                // Choose a random user from who is left
                                do
                                {
                                    randomUserPosition = rng.Next(0, userCount);
                                }
                                while (usedPositions.Contains(randomUserPosition));


                                curUserTeamAssignment.user = users[randomUserPosition];
                                curUserTeamAssignment.teamNumber = teamNumber;

                                teamAssignmentList.Add(curUserTeamAssignment);

                                // Mark this position as being used
                                usedPositions.Add(randomUserPosition);

                                teamNumber++;

                                if (teamNumber > numberOfTeams)
                                {
                                    teamNumber = 1;
                                }
                            }

                            // Make sure the team chat channels exist
                            for (int i = 1; i <= numberOfTeams; i++)
                            {
                                string teamChannelName;
                                Channel[] serverChannelList;
                                Boolean teamChannelExists = false;
                                Channel teamVoiceChannel = null;

                                teamChannelName = string.Format("Team {0}", i);

                                serverChannelList = e.Server.VoiceChannels.ToArray();
                                foreach (Channel voiceChannel in serverChannelList)
                                {
                                    if (voiceChannel.Name == teamChannelName)
                                    {
                                        teamVoiceChannel = voiceChannel;
                                        teamChannelExists = true;
                                    }
                                }

                                if (teamChannelExists == false)
                                {
                                    teamVoiceChannel = await e.Server.CreateChannel(teamChannelName, ChannelType.Voice);
                                }

                                teamChannels.Add(teamVoiceChannel);
                            }

                            // Move teams to their voice channels
                            foreach (TeamAssignment assignment in teamAssignmentList)
                            {
                                User teamUser;
                                int team;
                                Channel teamChannel;

                                teamUser = assignment.user;
                                team = assignment.teamNumber;

                                teamChannel = teamChannels.ElementAt(team - 1);

                                await teamUser.Edit(voiceChannel: teamChannel);
                            }

                            messageOutput += string.Format("Made {0} teams", numberOfTeams);
                        }

                        await e.Channel.SendMessage(messageOutput);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }

        /************************************************
         * 
         * Miscellaneous
         * 
        ************************************************/

        private void RegisterCountdownCommand()
        {
            commands.CreateCommand("Countdown") // Command name
                .Parameter("time", ParameterType.Required)
                .Description("Sends a message counting down every second, maximum of 5 Seconds.")
                .Do(async (e) =>
                {
                    int maxAllowed = 5;
                    int counter;

                    counter = int.Parse(e.GetArg("time"));

                    if (counter > maxAllowed)
                    {
                        await e.Channel.SendMessage(string.Format("Maximum count of {0} allowed", maxAllowed));
                    }
                    while (counter >= 0)
                    {
                        await e.Channel.SendMessage(string.Format("{0}", counter));
                        Thread.Sleep(1000);
                        counter--;
                    }
                });
        }
        
        private void RegisterRollDiceCommand()
        {
            commands.CreateCommand("roll") // Command name
                .Description("Rolls a specified number of dice, with a specified number of sides. Time to Die.")
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
        
        private void RegisterNoveltyResponseCommands()
        {
            // Define variables
            string messageTextLowercase;
            string responseText = "";

            bot.MessageReceived += async (s, e) =>
            {
                // Check to make sure that the author is not a bot
                if (!e.Message.User.IsBot)
                {
                    // Prevent the bot crashing on image only messages
                    // TODO - do we want to include images in the anti spam message counter?
                    if (e.Message.RawText != "")
                    {
                        messageTextLowercase = e.Message.RawText.ToLower();

                        if (messageTextLowercase.Substring(0, 1) != prefixCharacter.ToString())
                        {
                            switch (messageTextLowercase)
                            {
                                case "ayyy":
                                    responseText = "Ayyy, lmao";
                                    break;
                                case "winner winner":
                                    responseText = "Chicken dinner";
                                    break;
                                default:
                                    break;
                            }
                            if (responseText != "")
                            {
                                await e.Channel.SendMessage(responseText);
                            }
                        }
                    }
                }
            };
        }

        /************************************************
         * 
         * Separated functions
         * (Functions not directly usable in discord)
         * 
        ************************************************/
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
        
        private int CountMessage(User messageSender, int changeAmount)
        {
            int messageCount;

            if (userRecentMessageCounter[messageSender.Name] == null)
            {
                userRecentMessageCounter[messageSender.Name] = 0.ToString();
            }

            messageCount = int.Parse(userRecentMessageCounter[messageSender.Name]) + changeAmount;
            userRecentMessageCounter[messageSender.Name] = messageCount.ToString();

            Console.WriteLine(string.Format("{0}: {1}", messageSender.Name, messageCount));

            return messageCount;

        }
        
        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
