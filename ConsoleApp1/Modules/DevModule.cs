﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion
    };
};

