using CoOpBot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot.Modules.Admin
{
    [Name("Admin")]
    public class RolesModule : ModuleBase
    {
        #region Commands
        /*[Command("test")]
        [Summary("Test.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task testCommand()
        {
            string output = "";
            RoleTranslations rt = new RoleTranslations();
            List<RoleTranslations> rtList = new List<RoleTranslations>();

            try
            {
                rtList = rt.dbAsList<RoleTranslations>();


                foreach (RoleTranslations curRt in rtList)
                {
                    //rt = rt.find(test) as RoleTranslations;
                    output += $"RecId: {curRt.recId} \n";
                    output += $"From: {curRt.translateFrom} \n";
                    output += $"To: {curRt.translateTo} \n";
                }
                
                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }*/

        [Command("regexTest")]
        [Summary("Test game name regex.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task regexTestCommand(IRole role)
        {
            string output = "";
            Regex specialCharRegex = new Regex("[^a-zA-Z0-9 ]");
            Regex multipleSpaceRegex = new Regex("[ ]{2,}");
            try
            {
                output = specialCharRegex.Replace(role.Name, "");
                output = multipleSpaceRegex.Replace(output, " ");
                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("convertToNewDb")]
        [Summary("Convert old DB to New DB.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task convertToNewDbCommand()
        {
            XmlDocument xmlDatabase = new XmlDocument();
            XmlNode dbRoot;
            XmlNode usersNode;
            IEnumerator usersEnumerator;

            try
            {
                xmlDatabase.Load(FileLocations.xmlDatabase());
                dbRoot = xmlDatabase.DocumentElement;
                usersNode = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, dbRoot, "Users");
                usersEnumerator = usersNode.GetEnumerator();

                while (usersEnumerator.MoveNext())
                {
                    XmlElement curNode = usersEnumerator.Current as XmlElement;
                    User user = new User();

                    user.userID = ulong.Parse(curNode.GetAttribute("id"));
                    user.steamID = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, curNode, "steamID").InnerText;// curNode.SelectSingleNode("descendant::steamID").InnerText;
                    user.gwAPIKey = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, curNode, "gwAPIKey").InnerText;//curNode.SelectSingleNode("descendant::gwAPIKey").InnerText;
                    user.insert();
                }

                await ReplyAsync("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

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
        
        [Command("MergeRoles")]
        [Alias("mr", "RoleMerge", "merge")]
        [Summary("Merges two Roles. Second role is merged into first then deleted")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        private async Task MergeRolesCommand(IRole role1, IRole role2)
        {
            try
            {
                string output = "";
                SocketGuild server;
                IEnumerable<IGuildUser> users;
                List<ulong> usersToMerge = new List<ulong>();
                List<IRole> roleToMergeTo = new List<IRole>();
                IGuildUser curUser;

                RoleTranslations roleTranslations = new RoleTranslations();

                roleTranslations.translateFrom = role2.Name;
                roleTranslations.translateTo = role1.Name;

                roleTranslations.insert();

                server = this.Context.Guild as SocketGuild;
                roleToMergeTo.Add(role1);


                users = await this.Context.Guild.GetUsersAsync();
                // Get users from Role 2 that aren't in Role 1
                for (int i = 0; i < users.Count(); i++)
                {
                    curUser = users.ElementAt(i);

                    if (curUser.RoleIds.Contains(role2.Id) && !curUser.RoleIds.Contains(role1.Id))
                    {
                        usersToMerge.Add(curUser.Id);
                    }
                }
                // Add users to Role 1
                if (usersToMerge.Count > 0)
                {
                    await this.RoleAddUsers(server, usersToMerge, roleToMergeTo);
                }
                // Delete Role 2
                await this.RoleDelete(server, role2);

                output = string.Format("Roles {0} and {1} merged", role1.Name, role2.Name);
                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            RevokedRoleCommandAccessUsers revokedRoleCommandAccessUsers = new RevokedRoleCommandAccessUsers();
            ulong callerID = this.Context.User.Id;

            revokedRoleCommandAccessUsers = revokedRoleCommandAccessUsers.find($"{callerID}") as RevokedRoleCommandAccessUsers;

            if (revokedRoleCommandAccessUsers.userID == callerID)
            {
                return false;
            }
            return true;
        }
        #endregion
    };
};

