using CoOpBot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoOpBot.Modules.CoOpGaming
{

    [Name("Co-op")]
    public class CoOpGamingModule : ModuleBase
    {

        #region Commands

        [Command("MakeTeams")]
        [Alias("mt")]
        [Summary("Split the current channel into number of teams specified, and moves them into their own channel.")]
        private async Task MakeTeamsCommand(int numberOfTeams)
        {
            try
            {
                // Define variables
                int teamNumber;
                int randomUserPosition;
                int userCount;
                List<int> usedPositions;
                SocketVoiceChannel curVoiceChannel;
                string messageOutput;
                SocketGuildUser[] users;
                SocketGuildUser messageAuthor;
                List<TeamAssignment> teamAssignmentList;
                TeamAssignment curUserTeamAssignment;
                List<IChannel> teamChannels;

                // Initialise variables
                messageOutput = "";
                teamAssignmentList = new List<TeamAssignment> { };
                usedPositions = new List<int> { };
                teamChannels = new List<IChannel> { };
                teamNumber = 1;
                messageAuthor = this.Context.Message.Author as SocketGuildUser;
                curVoiceChannel = messageAuthor.VoiceChannel;

                // Check that the caller is in a voice channel
                if (curVoiceChannel == null)
                {
                    messageOutput += string.Format("{0}, you must be in a voice channel to use this command", messageAuthor.Username);
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
                            randomUserPosition = CoOpGlobal.rng.Next(0, userCount);
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
                        IReadOnlyCollection<IVoiceChannel> voiceChannels;
                        Boolean teamChannelExists = false;
                        IVoiceChannel teamVoiceChannel = null;

                        teamChannelName = string.Format("Team {0}", i);

                        voiceChannels = await this.Context.Guild.GetVoiceChannelsAsync();
                        
                        for (int j = 0; j < voiceChannels.Count; j++)
                        {
                            SocketVoiceChannel voiceChannel;

                            voiceChannel = voiceChannels.ElementAt(j) as SocketVoiceChannel;

                            if (voiceChannel.Name == teamChannelName)
                            {
                                teamVoiceChannel = voiceChannel;
                                teamChannelExists = true;
                            }
                        }

                        if (teamChannelExists == false)
                        {
                            teamVoiceChannel = await this.Context.Guild.CreateVoiceChannelAsync(teamChannelName);
                        }

                        teamChannels.Add(teamVoiceChannel);
                    }

                    // Move teams to their voice channels
                    foreach (TeamAssignment assignment in teamAssignmentList)
                    {
                        IGuildUser teamUser;
                        int team;
                        IVoiceChannel teamChannel;
                        
                        teamUser = assignment.user as IGuildUser;
                        team = assignment.teamNumber;

                        teamChannel = teamChannels.ElementAt(team - 1) as IVoiceChannel;

                        await (teamUser)?.ModifyAsync(x =>
                        {
                            x.ChannelId = teamChannel.Id;
                        });
                    }

                    messageOutput += string.Format("Made {0} teams", numberOfTeams);
                }

                await ReplyAsync(messageOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RemoveTeams")]
        [Alias("rt")]
        [Summary("Removes any channels created by the MakeTeams command. Cleanup your toys when you're done playing!")]
        private async Task RemoveTeamsCommand()
        {
            IReadOnlyCollection<IVoiceChannel> voiceChannels;
            IReadOnlyCollection<IVoiceChannel> voiceChannelsSearch;
            SocketVoiceChannel voiceChannelMoveTo = null;
            IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> channelMembersEnumerable;
            IAsyncEnumerator<IReadOnlyCollection<IGuildUser>> channelMembersEnumerator;
            IReadOnlyCollection<IGuildUser> channelMembers;
            int usercount = 0;
            Boolean emptyChannelFound = false;

            voiceChannelsSearch = await this.Context.Guild.GetVoiceChannelsAsync();
            foreach (IVoiceChannel voiceChannel in voiceChannelsSearch)
            {
                if (voiceChannel.Name.Substring(0, 4) != "Team")
                {
                    channelMembersEnumerable = voiceChannel.GetUsersAsync();
                    channelMembersEnumerator = channelMembersEnumerable.GetEnumerator();

                    while (await channelMembersEnumerator.MoveNext())
                    {
                        channelMembers = channelMembersEnumerator.Current;
                        usercount = channelMembers.Count();

                        if (usercount == 0)
                        {
                            voiceChannelMoveTo = voiceChannel as SocketVoiceChannel;
                            emptyChannelFound = true;
                            break;
                        }
                    }

                    if (emptyChannelFound)
                    {
                        break;
                    }
                }
            }

            voiceChannels = await this.Context.Guild.GetVoiceChannelsAsync();
            foreach (IVoiceChannel voiceChannel in voiceChannels)
            {
                if (voiceChannel.Name.Substring(0, 4) == "Team")
                {
                    if (emptyChannelFound)
                    {
                        channelMembersEnumerable = voiceChannel.GetUsersAsync();
                        channelMembersEnumerator = channelMembersEnumerable.GetEnumerator();

                        while (await channelMembersEnumerator.MoveNext())
                        {
                            channelMembers = channelMembersEnumerator.Current;

                            foreach (IGuildUser user in channelMembers)
                            {
                                await (user)?.ModifyAsync(x =>
                                {
                                    x.Channel = voiceChannelMoveTo;
                                });
                            }

                        }
                    }

                    await voiceChannel.DeleteAsync();
                }
            }
        }

        [Command("SteamUsers")]
        [Alias("SteamNames")]
        [Summary("Shows the Steam account names that the bot knows of")]
        private async Task SteamUsersCommand()
        {
            List<User> userList = new User().dbAsList<User>();
            EmbedBuilder builder = new EmbedBuilder();
            List<string> outputList = new List<string>();

            builder.Author = new EmbedAuthorBuilder()
            {
                Name = "Steam users"
            };

            builder.Footer = new EmbedFooterBuilder()
            {
                Text = $"Add to this list by using the command \"{CoOpGlobal.prefixCharacter}steam RegisterKey [YOUR STEAM ID KEY HERE]\""
            };

            foreach (User curUser in userList)
            {
                if (curUser.steamID != null && curUser.steamID != "")
                {
                    string userStr = "";
                    userStr = $"{Steam.SteamModule.DisplayNameFromID(curUser.userID)}: https://steamcommunity.com/profiles/{curUser.steamID}";

                    outputList.Add(userStr);
                }
            }

            if (outputList.Count < 1)
            {
                await ReplyAsync($"No users have registered their Steam account with the bot yet!\r\nUse the command \"{CoOpGlobal.prefixCharacter}steam RegisterKey [YOUR STEAM ID KEY HERE]\" to register your account");
                return;
            }

            builder.Description = string.Join("\r\n", outputList);

            await ReplyAsync("", false, builder.Build());
        }

        [Command("BattleNetUsers")]
        [Alias("BattleNetNames", "BNetUsers", "BNetNames")]
        [Summary("Shows the Battle Net names that the bot knows of")]
        private async Task BattleNetUsersCommand()
        {
            List<User> userList = new User().dbAsList<User>();
            EmbedBuilder builder = new EmbedBuilder();
            List<string> outputList = new List<string>();

            builder.Author = new EmbedAuthorBuilder()
            {
                Name = "Battle Net users"
            };

            builder.Footer = new EmbedFooterBuilder()
            {
                Text = $"Add to this list by using the command \"{CoOpGlobal.prefixCharacter}user BattleNetName [YOUR USERNAME HERE]\""
            };

            foreach (User curUser in userList)
            {
                if (curUser.battleNetName != null && curUser.battleNetName != "")
                {
                    outputList.Add(curUser.battleNetName);
                }
            }

            if (outputList.Count < 1)
            {
                await ReplyAsync($"No users have registered their Battle Net account name with the bot yet!\r\nUse the command \"{CoOpGlobal.prefixCharacter}user BattleNetName [YOUR USERNAME HERE]\" to register your username");
                return;
            }

            builder.Description = string.Join("\r\n", outputList);

            await ReplyAsync("", false, builder.Build());
        }

        [Command("OriginUsers")]
        [Alias("OriginNames")]
        [Summary("Shows the Origin account names that the bot knows of")]
        private async Task OriginUsersCommand()
        {
            List<User> userList = new User().dbAsList<User>();
            EmbedBuilder builder = new EmbedBuilder();
            List<string> outputList = new List<string>();

            builder.Author = new EmbedAuthorBuilder()
            {
                Name = "Origin users"
            };

            builder.Footer = new EmbedFooterBuilder()
            {
                Text = $"Add to this list by using the command \"{CoOpGlobal.prefixCharacter}user OriginName [YOUR USERNAME HERE]\""
            };

            foreach (User curUser in userList)
            {
                if (curUser.OriginName != null && curUser.OriginName != "")
                {
                    outputList.Add(curUser.OriginName);
                }
            }

            if (outputList.Count < 1)
            {
                await ReplyAsync($"No users have registered their Origin account name with the bot yet!\r\nUse the command \"{CoOpGlobal.prefixCharacter}user OriginName [YOUR USERNAME HERE]\" to register your username");
                return;
            }

            builder.Description = string.Join("\r\n", outputList);

            await ReplyAsync("", false, builder.Build());
        }
        #endregion


        #region Functions

        #endregion
    };
};

