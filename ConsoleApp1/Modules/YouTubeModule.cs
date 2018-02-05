using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoOpBot.Modules.YouTube
{
    [Name("YouTube")]
    public class YouTubeModule : ModuleBase
    {
        #region Commands
        [Command("Recording")]
        [Alias("rec")]
        [Summary("Moves the current voice channel to a private recording voice room.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task RecordingCommand()
        {
            try
            {
                string messageOutput = "";
                SocketGuildUser[] users;
                SocketGuildUser messageAuthor = this.Context.Message.Author as SocketGuildUser;
                SocketVoiceChannel curVoiceChannel = messageAuthor.VoiceChannel;
                IVoiceChannel recordingVoiceChannel = null;
                IReadOnlyCollection<IVoiceChannel> voiceChannels;
                CoOpBot.Modules.Admin.RolesModule roleModule = new Modules.Admin.RolesModule();
                IRole recordingRole = null;

                // Check that the caller is in a voice channel
                if (curVoiceChannel == null)
                {
                    messageOutput += string.Format("{0}, you must be in a voice channel to use this command", messageAuthor.Username);
                }
                // Check that the caller is not already in the recording voice channel
                else if (curVoiceChannel.Name == "Now Recording")
                {
                    messageOutput += "You are already in the recording voice channel";
                }
                else
                {
                    // Make sure the Now Recording channel exists
                    voiceChannels = await this.Context.Guild.GetVoiceChannelsAsync();

                    for (int i = 0; i < voiceChannels.Count; i++)
                    {
                        SocketVoiceChannel voiceChannel;

                        voiceChannel = voiceChannels.ElementAt(i) as SocketVoiceChannel;

                        if (voiceChannel.Name == "Now Recording")
                        {
                            recordingVoiceChannel = voiceChannel;
                        }
                    }

                    if (recordingVoiceChannel != null)
                    {
                        // make array of users in the current voice channel
                        users = curVoiceChannel.Users.ToArray();

                        // Move users to the recording voice channel
                        foreach (SocketGuildUser curUser in users)
                        {
                            List<IRole> roleList = new List<IRole>();
                            List<ulong> userList = new List<ulong>();
                            IReadOnlyCollection<SocketRole> userRoles;
                            Boolean userHasRole = false;
                            userRoles = curUser.Roles;

                            foreach (SocketRole curRole in userRoles)
                            {
                                if (curRole.Name == "YouTube recorders")
                                {
                                    userHasRole = true;
                                    recordingRole = curRole;
                                    break;
                                }
                            }

                            if (!userHasRole)
                            {
                                roleList.Add(recordingRole);
                                userList.Add(curUser.Id);
                                await roleModule.RoleAddUsers(this.Context.Guild as SocketGuild, userList, roleList);
                            }
                            await (curUser)?.ModifyAsync(x =>
                            {
                                x.ChannelId = recordingVoiceChannel.Id;
                            });
                        }

                        messageOutput += "Done";
                    }
                    else
                    {
                        messageOutput += "Now Recording voice channel does not exist";
                    }
                }

                await ReplyAsync(messageOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}
