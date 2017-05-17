﻿using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Reflection;
using CoOpBot.Modules.Roll;
using CoOpBot.Modules.Admin;

namespace CoOpBot
{
    public class Program
    {
        // Define & initialise variables
        private DiscordSocketClient client;
        private CommandService commands = new CommandService();
        private DependencyMap map = new DependencyMap();

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        
        char prefixCharacter = '!';
        NameValueCollection userRecentMessageCounter = new NameValueCollection();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            
            client.Log += Log;
            //client.MessageReceived += MessageReceived;
            
            await InstallCommands();

            string token = "MzA5Nzg4NjU2MDAzMDU1NjE2.C_JsQg.OJbqgCRKN_VzA5Rxad--uzmBze8"; // Remember to keep this private!
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            //map.Add(bot);

            //handler = new CommandHandler();
            //await handler.Install(map);

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        public async Task InstallCommands()
        {
            map.Add(client);

            // Add modules containing the commands
            map.Add(new RollModule());
            map.Add(new RolesModule());

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            //if (!(message.HasCharPrefix(prefixCharacter, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            if (message.Author.IsBot || (!(message.HasCharPrefix(prefixCharacter, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))))
            {
                return;
            }

            // Create a Command Context
            var context = new CommandContext(message.Discord, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await commands.ExecuteAsync(context, argPos, map);
            
            // Uncomment the following lines if you want the bot
            // to send a message if it failed (not advised for most situations).
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
        
        /************************************************
         * 
         * Bot admin functions
         * 
        ************************************************/
        /*
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
         * Voice & text channels
         * 
        ************************************************/
        /*
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
        /*
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
        /*
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
        
        /*
        private void RegisterNoveltyResponseCommands()
        {
            // Define variables
            string messageTextLowercase;
            string responseText;
            User userSentBy;
            Dictionary<ulong, int> openKnockKnockThreadState;

            // Initialise variables
            responseText = "";
            //openKnockKnockThreadState = new int[] { };
            openKnockKnockThreadState = new Dictionary<ulong, int>();

            bot.MessageReceived += async (s, e) =>
            {
                userSentBy = e.Message.User;
                // Check to make sure that the author is not a bot
                if (!userSentBy.IsBot)
                {
                    // Prevent the bot crashing on image only messages
                    if (e.Message.RawText != "")
                    {
                        messageTextLowercase = e.Message.RawText.ToLower();

                        if (messageTextLowercase.Substring(0, 1) != prefixCharacter.ToString())
                        {
                            if (openKnockKnockThreadState.ContainsKey(userSentBy.Id) && openKnockKnockThreadState[userSentBy.Id] > 0)
                            {
                                switch (openKnockKnockThreadState[userSentBy.Id])
                                {
                                    case 1:
                                        responseText = string.Format("{0} who?", e.Message.RawText);
                                        break;
                                    case 2:
                                        responseText = "Ayyy, lmao";
                                        break;
                                    default:
                                        break;
                                }

                                openKnockKnockThreadState[userSentBy.Id]++;
                                if (openKnockKnockThreadState[userSentBy.Id] > 2)
                                {
                                    openKnockKnockThreadState[userSentBy.Id] = 0;
                                }
                            }
                            else
                            {
                                switch (messageTextLowercase)
                                {
                                    case "ayyy":
                                        responseText = "Ayyy, lmao";
                                        break;
                                    case "winner winner":
                                        responseText = "Chicken dinner";
                                        break;
                                    case "new number":
                                        responseText = "Who dis?";
                                        break;
                                    case "knock knock":
                                        responseText = "Who's there?";
                                        if (!openKnockKnockThreadState.ContainsKey(userSentBy.Id))
                                        {
                                            openKnockKnockThreadState.Add(userSentBy.Id, 1);
                                        }
                                        else
                                        {
                                            openKnockKnockThreadState[userSentBy.Id] = 1;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (messageTextLowercase.Length >= 5 && messageTextLowercase.Substring(0, 5) == "pixis")
                                {
                                    responseText = "PIXISUUUUUUUU";
                                }
                            }
                            if (responseText != "")
                            {
                                await e.Channel.SendMessage(responseText);
                                // reset responseText to prevent repeated messages
                                responseText = "";
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

        private int CountMessage(SocketGuildUser messageSender, int changeAmount)
        {
            int messageCount;

            if (userRecentMessageCounter[messageSender.Username] == null)
            {
                userRecentMessageCounter[messageSender.Username] = 0.ToString();
            }

            messageCount = int.Parse(userRecentMessageCounter[messageSender.Username]) + changeAmount;
            userRecentMessageCounter[messageSender.Username] = messageCount.ToString();

            Console.WriteLine(string.Format("{0}: {1}", messageSender.Username, messageCount));

            return messageCount;

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
