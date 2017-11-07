using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot.Modules.Admin
{
    [Name("Admin")]
    public class RolesModule : ModuleBase
    {

        #region Commands

        [Command("AddRole")]
        [Alias("AddMe", "ar")]
        [Summary("Adds the user(s) to the requested Role.")]
        private async Task AddRoleCommand(IRole role, params IUser[] users)
        {
            List<IRole> roleList = new List<IRole>();
            if (!isPermitted())
            {
                await ReplyAsync("Command access denied");
                return;
            }

            roleList.Add(role);
            try
            {
                await RoleAddUsers(this.Context.Guild as SocketGuild, this.Context.Message.MentionedUserIds.ToList(), roleList, this.Context.Channel as SocketChannel, true);
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
        private async Task RemoveRoleCommand(IRole role, params IUser[] users)
        {
            List<IRole> roleList = new List<IRole>();

            roleList.Add(role);
            try
            {
                await RoleRemoveUsers(this.Context.Guild as SocketGuild, this.Context.Message.MentionedUserIds.ToList(), roleList, this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("DeleteRole")]
        [Alias("dr")]
        [Summary("Deletes a Role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task DeleteRoleCommand(IRole Role)
        {
            try
            {
                await RoleDelete(this.Context.Guild as SocketGuild, Role, this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("NewRole")]
        [Alias("nr", "CreateRole")]
        [Summary("Creates a new Role containing no users.")]
        private async Task NewRoleCommand(params string[] RoleName)
        {
            string fullRoleName = "";

            if (!isPermitted())
            {
                await ReplyAsync("Command access denied");
                return;
            }

            try
            {
                foreach (string s in RoleName)
                {
                    fullRoleName += $" {s}";
                }

                await RoleCreate(this.Context.Guild as SocketGuild, fullRoleName, this.Context.Channel as SocketChannel, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        [Command("RoleMembers")]
        [Alias("rm", "listRole", "WhoIs", "in")]
        [Summary("Returns a list of users with requested Roles. Roleplay is not permitted.")]
        private async Task RoleListCommand(IRole role)
        {
            string output = "";
            SocketGuild server;
            IEnumerable<IGuildUser> users;
            IGuildUser curUser;
            int userCount;

            server = this.Context.Guild as SocketGuild;
            userCount = 0;
            
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

                output = string.Format("{0} members in {1}: {2}", userCount, role.Name, output);
                await ReplyAsync(output);
            }

        }

        #endregion


        #region Functions
        public async Task RoleAddUsers(SocketGuild server, List<ulong> userList, List<IRole> roleList, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;
            SocketGuildUser user;

            // Initialise variables
            output = "";

            try
            {
                foreach (ulong curUserId in userList)
                {
                    user = server.GetUser(curUserId);
                    await user.AddRolesAsync(roleList.ToArray());
                }

                output += $"Added {userList.Count} users to {roleList.Count} roles";
                output += "\r\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            finally
            {
                if (channel != null && outputMessages)
                {
                    await ReplyAsync(output);
                }
            }
        }

        public async Task RoleRemoveUsers(SocketGuild server, List<ulong> userList, List<IRole> roleList, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;
            SocketGuildUser user;

            // Initialise variables
            output = "";

            try
            {
                foreach (ulong curUserID in userList)
                {
                    user = server.GetUser(curUserID);
                    await user.RemoveRolesAsync(roleList);
                }
                output += $"Removed {userList.Count} users from {roleList.Count} roles";
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

        public async Task RoleCreate(SocketGuild server, string roleName, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;
            Discord.Rest.RestRole newRole;
            Color roleColor;

            // Initialise variables
            output = "";
            roleColor = new Color(CoOpGlobal.rng.Next(257), CoOpGlobal.rng.Next(257), CoOpGlobal.rng.Next(257));


            try
            {
                newRole = await server.CreateRoleAsync(roleName, color: roleColor);
                await newRole.ModifyAsync((rp) => {rp.Mentionable = true; });
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

        public async Task RoleDelete(SocketGuild server, IRole role, SocketChannel channel = null, bool outputMessages = false)
        {
            // Define variables
            string output;

            // Initialise variables
            output = "";

            try
            {
                output += $"Role {role.Name} deleted.";
                await role.DeleteAsync();
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

        public IRole FindRoleFromName(string roleName, SocketGuild server)
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

        private Boolean isPermitted()
        {
            XmlDocument xmlDatabase = new XmlDocument();
            ulong callerID = this.Context.User.Id;

            xmlDatabase.Load(FileLocations.xmlDatabase());
            XmlNode root = xmlDatabase.DocumentElement;
            XmlNode revokedRoleCommandAccessUsersNode = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, root, "RevokedRoleCommandAccessUsers");

            if (CoOpGlobal.XML.searchChildNodes(xmlDatabase, revokedRoleCommandAccessUsersNode, $"{callerID}"))
            {
                return false;
            }
            return true;
        }
        #endregion
    };
};

