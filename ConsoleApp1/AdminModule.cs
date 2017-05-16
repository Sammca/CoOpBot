using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoOpBot.Modules.Admin
{
    public class AntiSpamModule : ModuleBase
    {
        /*[Command("roll")]
        //[Alias("AddMe", "ar")]
        [Summary("Rolls a specified number of dice, with a specified number of sides.Time to Die.")]
        private void RegisterAntiSpamFunctionality()
        {
            SocketGuildUser messageSender;
            int messageCount;
            ChannelPermissionOverrides channelPermissionOverrides;

            messageSender = e.Message.User;

            // Check to make sure that a bot is not the author
            // Also check if admin, since admins ignore the channel permission override
            if (!messageSender.GuildPermissions.Administrator && !messageSender.IsBot)
            {
                channelPermissionOverrides = new ChannelPermissionOverrides(sendMessages: PermValue.Deny);

                /*if (userRecentMessageCounter[messageSender.Name] == null)
                {
                    userRecentMessageCounter[messageSender.Name] = 0.ToString();
                }

                messageCount = CountMessage(messageSender, 1);*/

                /*if (messageCount > 2)
                {
                    await e.Channel.SendMessage("#StopCamSpam");
                    await e.Channel.AddPermissionsRule(messageSender, channelPermissionOverrides);

                    //await Task.Delay(5000).ContinueWith(t => e.Channel.SendMessage("5 seconds passed"));
                    await Task.Delay(8000).ContinueWith(t => e.Channel.RemovePermissionsRule(messageSender));
                }

                await Task.Delay(8000).ContinueWith(t => CountMessage(messageSender, -1));
            }
        }*/
    }
    
    public class RolesModule : ModuleBase
    {
        [Command("AddRole")]
        //[Alias("AddMe", "ar")]
        [Summary("Adds the user(s) to the requested Role.")]
        //[RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task RegisterAddRoleCommand(string RoleName, SocketGuildUser[] users)
        {
            try
            {
                await this.RoleAddUsers(this.Context.User as SocketGuildUser, this.Context.Guild as SocketGuild, this.Context.Message.MentionedUserIds.ToList(), RoleName.Split(' ').ToList(), this.Context.Channel as SocketChannel, true);
                await ReplyAsync("test");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /*
        private void RegisterRemoveRoleCommand()
        {
            commands.CreateCommand("RemoveRole") // Command name
                .Alias("rr") // Alternate command names
                .Parameter("RoleName", ParameterType.Required)
                .Parameter("users", ParameterType.Multiple)
                .Description("Removes the user(s) to the requested Role.")
                .Do(async (e) =>
                {
                    try
                    {
                        string RoleName = e.GetArg("RoleName");

                        await this.RoleRemoveUsers(e.Message.User, e.Server, e.Message.MentionedUsers.ToList(), RoleName.Split(' ').ToList(), e.Channel, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }
        */
        /*
        private void RegisterNewRoleCommand()
        {
            commands.CreateCommand("NewRole") // Command name
                .Alias("nr") // Alternate command names
                .Parameter("roleName", ParameterType.Required)
                .Description("Creates a new Role containing no users.")
                .Do(async (e) =>
                {
                    try
                    {
                        string roleName = e.GetArg("roleName");

                        await this.RoleCreate(e.Message.User, e.Server, roleName, e.Channel, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }
        */
        /*
        private void RegisterDeleteRoleCommand()
        {
            commands.CreateCommand("DeleteRole") // Command name
                .Alias("dr") // Alternate command names
                .Parameter("roleName", ParameterType.Required)
                .Description("Deletes a Role.")
                .Do(async (e) =>
                {
                    try
                    {
                        string roleName = e.GetArg("roleName");

                        await this.RoleDelete(e.Message.User, e.Server, roleName, e.Channel, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }
        *//*
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
        */

        private async Task RoleAddUsers(SocketGuildUser callingUser, SocketGuild server, List<ulong> userList, List<string> roleNameList, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;
            List<SocketRole> roleList;
            string roleNamesString;

            // Initialise variables
            output = "";
            roleNamesString = "";
            roleList = new List<SocketRole>();

            try
            {
                foreach (string curRoleName in roleNameList)
                {
                    Boolean roleExists = false;

                    foreach (SocketRole curRole in server.Roles)
                    {
                        if (curRole.Name == curRoleName)
                        {
                            roleExists = true;
                            break;
                        }
                    }

                    if (roleExists)
                    {
                        await this.RoleCreate(callingUser, server, curRoleName);
                    }

                    foreach (SocketRole curRole in server.Roles)
                    {
                        if (curRole.Name == curRoleName)
                        {
                            roleList.Add(curRole);
                            break;
                        }
                    }
                    roleNamesString += string.Format("{0} ", curRoleName);
                }
                foreach (ulong curUserId in userList)
                {
                    SocketGuildUser curUser = server.GetUser(curUserId);
                    await curUser.AddRolesAsync(roleList.ToArray());
                }
                output += string.Format("Added {0} user(s) to {1}", userList.Count, roleNamesString);
                output += "\r\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            if (channel != null && outputMessages)
            {
                await ReplyAsync(output);
            }
        }
        
        private async Task RoleCreate(SocketGuildUser callingUser, SocketGuild server, string roleName, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;

            // Initialise variables
            output = "";

            try
            {
                await server.CreateRoleAsync(roleName);
                output += string.Format("New role {0} created.", roleName);
                output += "\r\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (channel != null && outputMessages)
            {
                await ReplyAsync(output);
            }
        }
    }
}

