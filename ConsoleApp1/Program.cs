using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //var bot = new DiscordClient();
            var bot = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                //x.LogHandler = Log;
            });

            bot.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;

            });

            


        var commands = bot.GetService<CommandService>();

            //commands.CreateCommand("SteamMe") // Command name
            //        .Parameter("SteamProfile", ParameterType.Required) // Steam name
            commands.CreateCommand("Hello") // Command name
                    .Do(async (e) =>
                    {
                        await e.Channel.SendMessage("Hello world" + e.User.Mention);
                    });

            commands.CreateCommand("Addme") // Command name
                    .Parameter("GroupName", ParameterType.Required)
                    .Do(async (e) =>
                        {
                            var groupName = e.GetArg("GroupName");
                            //Console.WriteLine(e.Server.FindRoles(groupName,true));
                            var roleExists = e.Server.FindRoles(groupName, true);

                            if (e.Server.FindRoles(groupName).Count() != 1)
                            {
                                await e.Channel.SendMessage("Could not find "+groupName);
                                await e.Server.CreateRole(groupName);
                            } else
                            {
                                await e.Channel.SendMessage("Found " + groupName);
                                await e.User.AddRoles(e.Server.FindRoles(groupName).First());
                            }
                            await e.Channel.SendMessage("I have added you to group " + groupName + " " + e.User.Mention);
                        });

            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MzA5Nzg4NjU2MDAzMDU1NjE2.C-0k9A.7_v7ulouST3I358v2oMThI6yCPE", TokenType.Bot);
            });
        }
    }
}
