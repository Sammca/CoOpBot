using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
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
                Random rng;
                SocketGuildUser[] users;
                SocketGuildUser messageAuthor;
                List<TeamAssignment> teamAssignmentList;
                TeamAssignment curUserTeamAssignment;
                List<IChannel> teamChannels;

                // Initialise variables
                rng = new Random();
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
                        SocketVoiceChannel teamChannel;
                        
                        teamUser = assignment.user as IGuildUser;
                        team = assignment.teamNumber;

                        teamChannel = teamChannels.ElementAt(team - 1) as SocketVoiceChannel;

                        await (teamUser)?.ModifyAsync(x =>
                        {
                            x.Channel = teamChannel;
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

            voiceChannels = await this.Context.Guild.GetVoiceChannelsAsync();
            foreach (IVoiceChannel voiceChannel in voiceChannels)
            {
                if (voiceChannel.Name.Substring(0, 4) == "Team")
                {
                    await voiceChannel.DeleteAsync();
                }
            }
        }

        
        #endregion

        
        #region Functions
        
        #endregion
    };
};

