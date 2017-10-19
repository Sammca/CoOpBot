using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

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

        [Command("stats")]
        [Summary("Text chat stats.")]
        private async Task statsCommand(int messageCount = 100)
        {
            await statsCommand(this.Context.Channel, messageCount);
        }

        [Command("stats")]
        [Summary("Text chat stats for the given text channel.")]
        private async Task statsCommand(IMessageChannel textChannel, int messageCount = 100)
        {
            try
            {
                //IMessageChannel textChannel;
                IAsyncEnumerable<IReadOnlyCollection<IMessage>> messagesAsync;
                IAsyncEnumerator<IReadOnlyCollection<IMessage>> messagesAsyncEnumerator;
                IReadOnlyCollection<IMessage> messages;
                IEnumerator<IMessage> messagesEnumerator;
                IMessage curMessage;
                Dictionary<string, int> userMessageCounter = new Dictionary<string, int>();
                Dictionary<string, int> userCharacterCounter = new Dictionary<string, int>();
                string messageSender;
                string output = "";
                int actualMessageCount = 0;
                List<KeyValuePair<string, int>> orderedList = new List<KeyValuePair<string, int>>();
                int outputCounter = 1;
                int userCounter = 0;

                /*if (messageCount > 10000)
                {
                    await ReplyAsync("Max message count of 10000");
                    return;
                }*/

                //textChannel = this.Context.Channel;

                messagesAsync = textChannel.GetMessagesAsync(messageCount);

                messagesAsyncEnumerator = messagesAsync.GetEnumerator();

                while (await messagesAsyncEnumerator.MoveNext())
                {
                    messages = messagesAsyncEnumerator.Current;

                    if (messages.Count > 0)
                    {
                        messagesEnumerator = messages.GetEnumerator();
                        while (messagesEnumerator.MoveNext())
                        {
                            curMessage = messagesEnumerator.Current;
                            messageSender = curMessage.Author.Username;

                            if (!userMessageCounter.ContainsKey(messageSender))
                            {
                                userMessageCounter[messageSender] = 1;
                                userCharacterCounter[messageSender] = curMessage.Content.Length;
                                userCounter++;
                            }
                            else
                            {
                                userMessageCounter[messageSender]++;
                                userCharacterCounter[messageSender] += curMessage.Content.Length;
                            }
                            actualMessageCount++;
                        }
                    }
                }

                userCounter = Math.Min(10, userCounter);

                output = $"Top {userCounter} from the past {actualMessageCount} messages: \n";

                orderedList = userMessageCounter.ToList();

                orderedList.Sort( delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) { return pair2.Value.CompareTo(pair1.Value); } );

                foreach (KeyValuePair<string, int> entry in orderedList)
                {
                    if (outputCounter > userCounter)
                    {
                        break;
                    }
                    output += $"{outputCounter}) {entry.Key} - {entry.Value} messages averaging {(userCharacterCounter[entry.Key] / entry.Value)} characters per message \n";
                    outputCounter++;
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

