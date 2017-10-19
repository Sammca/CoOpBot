using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CoOpBot.Modules.Stats
{
    [Name("Stats")]
    public class RolesModule : ModuleBase
    {

        #region Commands

        [Command("uptime")]
        [Summary("Gets the uptime of the bot.")]
        private async Task uptimeCommand()
        {
            try
            {
                DateTime timeNow = DateTime.UtcNow;
                TimeSpan diff;
                string diffStr;
                string[] diffStrSplit;
                string[] hoursStrSplit;
                string days, hours, minutes, seconds;
                string output = "";

                diff = timeNow.Subtract(CoOpGlobal.bootupDateTime);
                diffStr = diff.ToString();

                diffStrSplit = diffStr.Split(':');

                hours = diffStrSplit[0];
                minutes = diffStrSplit[1];
                seconds = diffStrSplit[2].Substring(0,2);

                if (hours != "00")
                {
                    if (hours.Length > 2)
                    {
                        hoursStrSplit = hours.Split('.');
                        days = hoursStrSplit[0];
                        hours = hoursStrSplit[1];

                        output = $"{days}d {hours}h {minutes}m {seconds}s";
                    }
                    else
                    {
                        output = $"{hours}h {minutes}m {seconds}s";
                    }
                }
                else if (minutes != "00")
                {
                    output = $"{minutes}m {seconds}s";
                }
                else
                {
                    output = $"{seconds}s";
                }

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion


        #region Functions
        #endregion
    };
};

