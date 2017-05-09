using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            RegisterRoleListCommand();
            RegisterMakeTeamsCommand();
            RegisterNonStandardCommands();
            RegisterCountdownCommand();
            RegisterRemoveTeamChannelsCommand();

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

        private void RegisterCountdownCommand()
        {
            commands.CreateCommand("Countdown") // Command name
                .Parameter("time", ParameterType.Required)
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

        private void RegisterRemoveTeamChannelsCommand()
        {
            commands.CreateCommand("removeTeams") // Command name
                .Do(async (e) =>
                {
                    Channel[] serverChannelList;

                    serverChannelList = e.Server.VoiceChannels.ToArray();
                    foreach (Channel voiceChannel in serverChannelList)
                    {
                        if (voiceChannel.Name.Substring(0,4) == "Team")
                        {
                            await voiceChannel.Delete();
                        }
                    }
                });
        }

        private void RegisterMakeTeamsCommand()
        {
            commands.CreateCommand("MakeTeams") // Command name
                .Parameter("NumberOfTeams",ParameterType.Required)
                .Description("Split the current channel into teams")
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

        private void RegisterRoleListCommand()
        {
            commands.CreateCommand("Whois") // Command name
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
        
        private void RegisterNonStandardCommands()
        {
            bot.MessageReceived += async (s, e) => 
            {
                // Check to make sure that the bot is not the author
                if (!e.Message.IsAuthor)
                {
                    if (e.Message.RawText.ToLower() == "ayyy")
                    {
                        await e.Channel.SendMessage("Ayyy, lmao");
                    }
                    // Messa
                    if (e.Message.RawText.ToLower() == "winner winner")
                    {
                        await e.Channel.SendMessage("Chicken dinner");
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
                                            role = await e.Server.CreateRole(newRoleName,null,null,false,true);
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


    // Define array holding [user, teamNumber]
    public class TeamAssignment
    {
        public User user { get; set; }
        public int teamNumber { get; set; }
    }
}
