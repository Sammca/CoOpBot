using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
                xmlParameters.Save(FileLocations.backupXML());

                await ReplyAsync($"XML parameters backed up to {FileLocations.backupXML()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("BackupGW")]
        [Summary("Creates a backup of the GW items file.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task BackupGWCommand()
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("RestoreXML")]
        [Alias("restore")]
        [Summary("Restores a backup of the XML parameters file.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task RestoreXMLCommand()
        {
            XmlDocument xmlParameters = new XmlDocument();
            try
            {
                xmlParameters.Load(FileLocations.backupXML());
                xmlParameters.Save(FileLocations.xmlParameters());

                await ReplyAsync("XML parameters restored");
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
            try
            {
                xmlParameters.Load(FileLocations.backupXML());
                xmlParameters.Save(FileLocations.xmlParameters());
                gwItems.Load(FileLocations.gwItemNamesBackup());
                gwItems.Save(FileLocations.gwItemNames());

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

