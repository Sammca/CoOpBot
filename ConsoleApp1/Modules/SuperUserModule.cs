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
    public class SuperUserModule : ModuleBase
    {
        #region Commands

        [Command("BackupXML")]
        [Alias("backup")]
        [Summary("Creates a backup of the XML parameters file.")]
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]

        private async Task BackupDBCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.gwItemNames());
                xmlParameters.Save(FileLocations.gwItemNamesBackup());

                await ReplyAsync($"GW item names file backed up to {FileLocations.gwItemNamesBackup()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        [Command("BackupDB")]
        [Summary("Creates a backup of the XML database file.")]
        [RequireUserPermission(GuildPermission.Administrator)]

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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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
        [RequireUserPermission(GuildPermission.Administrator)]
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

        #endregion
    };
};

