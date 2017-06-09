using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Reflection;
using CoOpBot.Modules.Roll;
using CoOpBot.Modules.Admin;
using CoOpBot.Modules.CoOpGaming;
using CoOpBot.Modules.GuildWars;
using CoOpBot.Modules.Steam;
using System.Xml;
using System.IO;

namespace CoOpBot
{
    public class Program
    {
        // Define & initialise variables
        private DiscordSocketClient client;
        private CommandService commands = new CommandService();
        private DependencyMap map = new DependencyMap();
        XmlDocument xmlParameters = new XmlDocument();

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
        
        char prefixCharacter = '!';
        NameValueCollection userRecentMessageCounter = new NameValueCollection();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            
            client.Log += Log;
            //client.MessageReceived += MessageReceived;

            if (!File.Exists(FileLocations.xmlParameters()))
            {
                if (!File.Exists(FileLocations.backupXML()))
                {
                    Console.WriteLine("XML parameters not found, no backup found");

                    throw new FileNotFoundException(@"XML parameters not found, and no backup found");
                }
                else
                {
                    xmlParameters.Load(FileLocations.backupXML());
                    xmlParameters.Save(FileLocations.xmlParameters());

                    Console.WriteLine("XML parameters restored from backup");
                }
            }

            xmlParameters.Load(FileLocations.xmlParameters());
            XmlNode root = xmlParameters.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("descendant::BotToken");

            await InstallCommands();
            
            string token = myNode.InnerText;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        public async Task InstallCommands()
        {
            map.Add(client);

            // Add modules containing the commands
            map.Add(new RollModule());
            map.Add(new RolesModule());
            map.Add(new CoOpGamingModule());
            map.Add(new GuildWarsModule());
            map.Add(new SteamModule());

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            //RegisterAntiSpamFunctionality();

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            //if (!(message.HasCharPrefix(prefixCharacter, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            if (message.Author.IsBot || (!(message.HasCharPrefix(prefixCharacter, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))))
            {
                return;
            }

            // Create a Command Context
            var context = new CommandContext(message.Discord, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await commands.ExecuteAsync(context, argPos, map);

            // Uncomment the following lines if you want the bot
            // to send a message if it failed (not advised for most situations).

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            return;
        }

        /************************************************
         * 
         * Bot admin functions
         * 
        ************************************************/

        /*private void RegisterAntiSpamFunctionality()
        {
            client.MessageReceived += async (message) =>
            {
                SocketGuildUser messageSender;
                ISocketMessageChannel channel;
                IGuildChannel guildChannel;
                IUser iuser;
                int messageCount;
                //ChannelPermissionOverrides channelPermissionOverrides;

                messageSender = message.Author as SocketGuildUser;
                channel = message.Channel;
                guildChannel = message.Channel as IGuildChannel;

                // Check to make sure that a bot is not the author
                // Also check if admin, since admins ignore the channel permission override
                if (!messageSender.GuildPermissions.Administrator && !messageSender.IsBot)
                {
                    //channelPermissionOverrides = new ChannelPermissionOverrides(sendMessages: PermValue.Deny);

                    if (userRecentMessageCounter[messageSender.Username] == null)
                    {
                        userRecentMessageCounter[messageSender.Username] = 0.ToString();
                    }

                    messageCount = await CountMessage(messageSender, 1);

                    if (messageCount > 2)
                    {
                        await channel.SendMessageAsync("#StopCamSpam");

                        iuser = await channel.GetUserAsync(messageSender.Id);

                        messageSender.GetPermissions(guildChannel).Modify(sendMessages: false);

                        //await channel.AddPermissionsRule(messageSender, channelPermissionOverrides);

                        //await Task.Delay(5000).ContinueWith(t => e.Channel.SendMessage("5 seconds passed"));
                        await Task.Factory.StartNew(() => { messageSender.GetPermissions(guildChannel).Modify(sendMessages: null); });
                    }

                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, -1, 8); });
                }
                else
                {
                    messageCount = await CountMessage(messageSender, 1);
                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, -1, 8); });
                }
            };
        }*/


        /************************************************
         * 
         * Miscellaneous
         * 
        ************************************************/
        /*
        private void RegisterNoveltyResponseCommands()
        {
            // Define variables
            string messageTextLowercase;
            string responseText;
            User userSentBy;
            Dictionary<ulong, int> openKnockKnockThreadState;

            // Initialise variables
            responseText = "";
            //openKnockKnockThreadState = new int[] { };
            openKnockKnockThreadState = new Dictionary<ulong, int>();

            bot.MessageReceived += async (s, e) =>
            {
                userSentBy = e.Message.User;
                // Check to make sure that the author is not a bot
                if (!userSentBy.IsBot)
                {
                    // Prevent the bot crashing on image only messages
                    if (e.Message.RawText != "")
                    {
                        messageTextLowercase = e.Message.RawText.ToLower();

                        if (messageTextLowercase.Substring(0, 1) != prefixCharacter.ToString())
                        {
                            if (openKnockKnockThreadState.ContainsKey(userSentBy.Id) && openKnockKnockThreadState[userSentBy.Id] > 0)
                            {
                                switch (openKnockKnockThreadState[userSentBy.Id])
                                {
                                    case 1:
                                        responseText = string.Format("{0} who?", e.Message.RawText);
                                        break;
                                    case 2:
                                        responseText = "Ayyy, lmao";
                                        break;
                                    default:
                                        break;
                                }

                                openKnockKnockThreadState[userSentBy.Id]++;
                                if (openKnockKnockThreadState[userSentBy.Id] > 2)
                                {
                                    openKnockKnockThreadState[userSentBy.Id] = 0;
                                }
                            }
                            else
                            {
                                switch (messageTextLowercase)
                                {
                                    case "ayyy":
                                        responseText = "Ayyy, lmao";
                                        break;
                                    case "winner winner":
                                        responseText = "Chicken dinner";
                                        break;
                                    case "new number":
                                        responseText = "Who dis?";
                                        break;
                                    case "knock knock":
                                        responseText = "Who's there?";
                                        if (!openKnockKnockThreadState.ContainsKey(userSentBy.Id))
                                        {
                                            openKnockKnockThreadState.Add(userSentBy.Id, 1);
                                        }
                                        else
                                        {
                                            openKnockKnockThreadState[userSentBy.Id] = 1;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (messageTextLowercase.Length >= 5 && messageTextLowercase.Substring(0, 5) == "pixis")
                                {
                                    responseText = "PIXISUUUUUUUU";
                                }
                            }
                            if (responseText != "")
                            {
                                await e.Channel.SendMessage(responseText);
                                // reset responseText to prevent repeated messages
                                responseText = "";
                            }
                        }
                    }
                }
            };
        }

        /************************************************
         * 
         * Separated functions
         * (Functions not directly usable in discord)
         * 
        ************************************************/

        private async Task<int> CountMessage(SocketGuildUser messageSender, int changeAmount, int delaySeconds = 0)
        {
            int messageCount;

            await Task.Delay(delaySeconds * 1000);

            if (userRecentMessageCounter[messageSender.Username] == null)
            {
                userRecentMessageCounter[messageSender.Username] = 0.ToString();
            }

            messageCount = int.Parse(userRecentMessageCounter[messageSender.Username]) + changeAmount;
            userRecentMessageCounter[messageSender.Username] = messageCount.ToString();

            Console.WriteLine(string.Format("{0}: {1}", messageSender.Username, messageCount));

            // TODO move banning to here
            // Only if messageCount = 3 and changeamount = +1

            return messageCount;

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    };
};
