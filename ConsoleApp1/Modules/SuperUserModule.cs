using Discord;
using Discord.Commands;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot.Modules
{
    [Name("SuperUser")]
    [Group("su")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class SuperUserModule : ModuleBase
    {
        #region Commands

        [Command("BackupXML")]
        [Alias("backup")]
        [Summary("Creates a backup of the XML parameters file.")]
        private async Task BackupXMLCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.xmlParameters());
                xmlParameters.Save(FileLocations.backupXMLParameters());

                await ReplyAsync($"XML parameters backed up to {FileLocations.backupXMLParameters()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("BackupGW")]
        [Summary("Creates a backup of the GW items file.")]

        private async Task BackupDBCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.gwItemNames());
                xmlParameters.Save(FileLocations.gwItemNamesBackup());

                await ReplyAsync($"XML database file backed up to {FileLocations.gwItemNamesBackup()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        [Command("BackupDB")]
        [Summary("Creates a backup of the XML database file.")]

        private async Task BackupGWCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.xmlDatabase());
                xmlParameters.Save(FileLocations.backupXMLDatabase());

                await ReplyAsync($"GW item names file backed up to {FileLocations.backupXMLDatabase()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("BackupAll")]
        [Summary("Creates a backup of all XML files.")]
        private async Task BackupAllCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                await BackupGWCommand();
                await BackupXMLCommand();
                await BackupDBCommand();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RestoreXML")]
        [Summary("Restores a backup of the XML parameters file.")]
        private async Task RestoreXMLCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.backupXMLParameters());
                xmlParameters.Save(FileLocations.xmlParameters());

                await ReplyAsync("XML parameters restored");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RestoreDB")]
        [Summary("Restores a backup of the XML parameters file.")]
        private async Task RestoreDBCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.backupXMLDatabase());
                xmlParameters.Save(FileLocations.xmlDatabase());

                await ReplyAsync("XML database restored");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RestoreAll")]
        [Summary("Restores all files from their backups.")]
        private async Task RestoreAllCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            XmlDocument gwItems = new XmlDocument();
            XmlDocument xmlDB = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.backupXMLParameters());
                xmlParameters.Save(FileLocations.xmlParameters());
                gwItems.Load(FileLocations.gwItemNamesBackup());
                gwItems.Save(FileLocations.gwItemNames());
                xmlDB.Load(FileLocations.backupXMLDatabase());
                xmlDB.Save(FileLocations.xmlDatabase());

                await ReplyAsync("XML parameters restored");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("ShutDown")]
        [Alias("TurnOff")]
        [Summary("Turns the bot off.")]
        private async Task ShutDownCommand()
        {
            try
            {
                await ReplyAsync("Shutting down");

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("Restart")]
        [Summary("Restarts the bot.")]
        private async Task RestartCommand()
        {
            try
            {
                await ReplyAsync("Restarting");

                System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("sendXML")]
        [Summary("Makes the bot send a copy of the XML parameters file to the chat.")]
        private async Task sendXMLCommand()
        {
            try
            {
                await this.Context.Message.Channel.SendFileAsync(FileLocations.xmlParameters());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("sendDB")]
        [Summary("Makes the bot send a copy of the XML database file to the chat.")]
        private async Task sendDBCommand()
        {
            try
            {
                await this.Context.Message.Channel.SendFileAsync(FileLocations.xmlDatabase());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("sendGWItemsXML")]
        [Summary("Makes the bot send a copy of the GW items file to the chat.")]
        private async Task sendGWItemsXMLCommand()
        {
            try
            {
                await this.Context.Message.Channel.SendFileAsync(FileLocations.gwItemNames());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("say")]
        [Summary("Makes the bot send a message to the channel you tell it to.")]
        private async Task sayCommand(ITextChannel textChannel, params string[] text)
        {
            try
            {
                string output = "";

                foreach (string s in text)
                {
                    output += $"{s} ";
                }

                await textChannel.SendMessageAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("revoke")]
        [Summary("Revokes permissions for the user to use role commands.")]
        private async Task revokeCommand(IUser user)
        {

            try
            {
                string output = "";
                XmlDocument xmlDatabase = new XmlDocument();
                ulong callerID = user.Id;

                xmlDatabase.Load(FileLocations.xmlDatabase());
                XmlNode root = xmlDatabase.DocumentElement;
                XmlNode revokedRoleCommandAccessUsersNode = CoOpGlobal.xmlFindOrCreateChild(xmlDatabase, root, "RevokedRoleCommandAccessUsers");

                if (CoOpGlobal.xmlSearchChildNodes(xmlDatabase, revokedRoleCommandAccessUsersNode, $"{callerID}"))
                {
                    output += "User already on list";
                }
                else
                {
                    CoOpGlobal.xmlCreateChildNode(xmlDatabase, revokedRoleCommandAccessUsersNode, "bannedUser", $"{callerID}");
                    output += "User added to list";
                }

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("unrevoke")]
        [Summary("Unrevokes permissions for the user to use role commands.")]
        private async Task unrevokeCommand(IUser user)
        {

            try
            {
                string output = "";
                XmlDocument xmlDatabase = new XmlDocument();
                ulong callerID = user.Id;

                xmlDatabase.Load(FileLocations.xmlDatabase());
                XmlNode root = xmlDatabase.DocumentElement;
                XmlNode revokedRoleCommandAccessUsersNode = CoOpGlobal.xmlFindOrCreateChild(xmlDatabase, root, "RevokedRoleCommandAccessUsers");

                if (CoOpGlobal.xmlSearchChildNodes(xmlDatabase, revokedRoleCommandAccessUsersNode, $"{callerID}"))
                {
                    foreach (XmlNode curNode in revokedRoleCommandAccessUsersNode.ChildNodes)
                    {
                        if (curNode.InnerText == $"{callerID}")
                        {
                            revokedRoleCommandAccessUsersNode.RemoveChild(curNode);
                            xmlDatabase.Save(FileLocations.xmlDatabase());

                            output += "User removed from the list";
                        }
                    }
                }
                else
                {
                    output += "User not found on the list";
                }

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        [Name("Set parameters")]
        [Group("set")]
        public class setParams : ModuleBase
        {
            [Command("SpamTimer")]
            [Alias("st")]
            [Summary("Sets the number of seconds messages are counted for to check for spamming.")]
            private async Task setSpamTimerCommand(int newTime)
            {
                try
                {
                    XmlDocument xmlParameters = new XmlDocument();
                    xmlParameters.Load(FileLocations.xmlParameters());
                    XmlNode root = xmlParameters.DocumentElement;

                    CoOpGlobal.xmlUpdateOrCreateChildNode(xmlParameters, root, "SpamTimer", newTime.ToString());

                    await ReplyAsync($"Spam message timer changed to {newTime} seconds");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            [Command("SpamMessageCount")]
            [Alias("smc")]
            [Summary("Sets the number of messages within the time limit that counts as spamming.")]
            private async Task setSpamMessageCountCommand(int newCount)
            {
                try
                {
                    XmlDocument xmlParameters = new XmlDocument();
                    xmlParameters.Load(FileLocations.xmlParameters());
                    XmlNode root = xmlParameters.DocumentElement;

                    CoOpGlobal.xmlUpdateOrCreateChildNode(xmlParameters, root, "SpamMessageCount", newCount.ToString());

                    await ReplyAsync($"Spam message count threshold changed to {newCount} messages");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        };
    };
};

