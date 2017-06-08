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
    };

    [Name("Admin")]
    public class RolesModule : ModuleBase
    {

        #region Commands

        [Command("AddRole")]
        [Alias("AddMe", "ar")]
        [Summary("Adds the user(s) to the requested Role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task AddRoleCommand(string RoleName, params IUser[] users)
        {
            try
            {
                await RoleAddUsers(this.Context.Guild as SocketGuild, this.Context.Message.MentionedUserIds.ToList(), RoleName.Split(' ').ToList(), this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RemoveRole")]
        [Alias("RemoveMe", "rr")]
        [Summary("Removes the user(s) from the requested Role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task RemoveRoleCommand(string RoleName, params IUser[] users)
        {
            try
            {
                await RoleRemoveUsers(this.Context.Guild as SocketGuild, this.Context.Message.MentionedUserIds.ToList(), RoleName.Split(' ').ToList(), this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("DeleteRoll")]
        [Alias("dr")]
        [Summary("Deletes a Role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task DeleteRoleCommand(string RoleName)
        {
            try
            {
                await RoleDelete(this.Context.Guild as SocketGuild, RoleName, this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("NewRole")]
        [Alias("nr", "CreateRole")]
        [Summary("Creates a new Role containing no users.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task NewRoleCommand(string RoleName)
        {
            try
            {
                await RoleCreate(this.Context.Guild as SocketGuild, RoleName, this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        [Command("RoleMembers")]
        [Alias("rm", "listRole", "WhoIs")]
        [Summary("Returns a list of users with requested Roles. Roleplay is not permitted.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task RoleListCommand(string roleName)
        {
            IRole role;
            string output = "";
            SocketGuild server;
            IEnumerable<IGuildUser> users;
            IGuildUser curUser;
            int userCount;

            server = this.Context.Guild as SocketGuild;
            userCount = 0;


            role = FindRoleFromName(roleName, server);
            if (role != null)
            {
                users = await this.Context.Guild.GetUsersAsync();

                for (int i = 0; i < users.Count(); i++)
                {
                    curUser = users.ElementAt(i);

                    if (curUser.RoleIds.Contains(role.Id))
                    {
                        output += "\r\n";
                        userCount++;
                        if (curUser.Nickname != null)
                        {
                            output += curUser.Nickname;
                        }
                        else
                        {
                            output += curUser.Username;
                        }
                    }
                }

                output = string.Format("{0} members in {1}: {2}", userCount, roleName, output);
                await ReplyAsync(output);
            }

        }

        #endregion


        #region Functions
        private async Task RoleAddUsers(SocketGuild server, List<ulong> userList, List<string> roleNameList, SocketChannel channel = null, bool outputMessages = false)
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

                    if (!roleExists)
                    {
                        await RoleCreate(server, curRoleName, channel, true);
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
        
        private async Task RoleRemoveUsers(SocketGuild server, List<ulong> userList, List<string> roleNameList, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;
            List<IRole> roleList;
            string roleNamesString;
            SocketGuildUser user;

            // Initialise variables
            output = "";
            roleNamesString = "";
            roleList = new List<IRole>();

            try
            {
                foreach (string curRoleName in roleNameList)
                {

                    foreach (SocketRole curRole in server.Roles)
                    {
                        Boolean roleExists = false;

                        if (curRole.Name == curRoleName)
                        {
                            roleList.Add(curRole);
                            roleNamesString += string.Format("{0} ", curRoleName);
                            roleExists = true;
                            break;
                        }

                        if (!roleExists)
                        {
                            await ReplyAsync(string.Format("Role {0} not found", curRoleName));
                        }
                    }
                }
                foreach (ulong curUserID in userList)
                {
                    user = server.GetUser(curUserID);
                    await user.RemoveRolesAsync(roleList.AsEnumerable<IRole>());
                }
                output += string.Format("Removed {0} user(s) from {1}", userList.Count, roleNamesString);
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

        private async Task RoleCreate(SocketGuild server, string roleName, SocketChannel channel = null, bool outputMessages = false)
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
        
        private async Task RoleDelete(SocketGuild server, string roleName, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;

            // Initialise variables
            output = "";

            try
            {
                //roleToDelete = server.FindRoles(roleName, true).First();
                Boolean roleExists = false;
                foreach (SocketRole curRole in server.Roles)
                {
                    if (curRole.Name == roleName)
                    {
                        await curRole.DeleteAsync();
                        output += string.Format("Role {0} deleted.", roleName);
                        output += "\r\n";
                        roleExists = true;
                        break;
                    }
                }

                if (!roleExists)
                {
                    output += string.Format("Role {0} not found", roleName);
                }
                
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

        private IRole FindRoleFromName(string roleName, SocketGuild server)
        {
            IRole role;

            foreach (IRole curRole in server.Roles)
            {
                if (curRole.Name == roleName)
                {
                    role = curRole;
                    return role;
                }
            }
            
            return null;
        }

        #endregion
    };
};

