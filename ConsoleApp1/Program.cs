using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace CoOpBot
{
    public class Program
    {
        // Define & initialise variables
        private DiscordSocketClient client;
        private CommandService commands = new CommandService();
        XmlDocument xmlParameters = new XmlDocument();
        XmlDocument xmlDatabase = new XmlDocument();
        char prefixCharacter;
        int spamTimer;
        int spamMessageCount;
        NameValueCollection userRecentMessageCounter = new NameValueCollection();
        Random goodBotRNG = new Random();
        string token;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();


        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            
            client.Log += Log;

            loadXMLParameters();
            
            await InstallCommands();
            
            //string token = botTokenNode.InnerText;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        public async Task InstallCommands()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            client.MessageReceived += RegisterNoveltyResponseCommands;
            RegisterAntiSpamFunctionality();
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null)
            {
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command, based on if it starts with the prefix from the parameters file (default '!') or a mention prefix
            if (message.Author.IsBot || (!(message.HasCharPrefix(prefixCharacter, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))))
            {
                return;
            }

            // Create a Command Context
            var context = new CommandContext(client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            if(commands.Search(context, argPos).IsSuccess)
            {
                Emoji emoji = new Emoji("👍");
                
                await context.Message.AddReactionAsync(emoji);
            }

            var result = await commands.ExecuteAsync(context, argPos);

            if (commands.Search(context, argPos).IsSuccess)
            {
                await context.Message.RemoveAllReactionsAsync();
            }

            // Uncomment the following lines if you want the bot
            // to send a message if it failed (not advised for most situations).

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
            else if (!result.IsSuccess && result.Error == CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync($"Command not recognised, try {prefixCharacter}help to see the available commands");
            }
            return;
        }

        private void RegisterAntiSpamFunctionality()
        {
            client.MessageReceived += async (message) =>
            {
                SocketGuildUser messageSender;
                ISocketMessageChannel channel;

                messageSender = message.Author as SocketGuildUser;
                channel = message.Channel;
                xmlParameters.Load(FileLocations.xmlParameters());
                XmlNode root = xmlParameters.DocumentElement;
                XmlNode spamTimerNode = CoOpGlobal.xmlFindOrCreateChild(xmlParameters, root, "SpamTimer", "8");
                spamTimer = int.Parse(spamTimerNode.InnerText);

                // Check to make sure that a bot is not the author
                // Also check if admin, since admins ignore the channel permission override
                if (/*!messageSender.GuildPermissions.Administrator && */!messageSender.IsBot)
                {
                    // Increment the counter by 1
                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, channel, 1); });

                    // Decrese the counter by 1 after parameteriesed number of seconds (default 8)
                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, channel, -1, spamTimer); });
                }
            };
        }

        /************************************************
         * 
         * Miscellaneous
         * 
        ************************************************/

        private async Task RegisterNoveltyResponseCommands(SocketMessage messageParam)
        {
            // Define variables
            string messageTextLowercase;
            string responseText;
            IUser userSentBy;
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Initialise variables
            responseText = "";
            userSentBy = message.Author;
            // Check to make sure that the author is not a bot
            if (!userSentBy.IsBot)
            {
                messageTextLowercase = message.Content.ToLower();

                if (messageTextLowercase.Substring(0, 1) != prefixCharacter.ToString())
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
                        case "good bot":
                            List<string> goodBotResponseList = new List<string>();

                            goodBotResponseList.Add("Ayyy, lmao");
                            goodBotResponseList.Add("Good human");
                            goodBotResponseList.Add("Why thank you!");
                            goodBotResponseList.Add("What is \"good\"? Baby don't hurt me");
                            goodBotResponseList.Add("(◠﹏◠✿)");
                            goodBotResponseList.Add("ｖ(◠ｏ◠)ｖ");
                            goodBotResponseList.Add("( ͡° ͜ʖ ͡°)");

                            responseText = goodBotResponseList[goodBotRNG.Next(goodBotResponseList.Count)];
                            break;
                        default:
                            break;
                    }
                    if (messageTextLowercase.Length >= 5 && messageTextLowercase.Contains("pixis"))
                    {
                        responseText = "PIXISUUUUUUUU";
                    }
                    if (responseText != "")
                    {
                        await message.Channel.SendMessageAsync(responseText);
                        // reset responseText to prevent repeated messages
                        responseText = "";
                    }
                }
            }
            return;
        }

        /************************************************
         * 
         * Separated functions
         * (Functions not directly usable in discord)
         * 
        ************************************************/

        private async Task<int> CountMessage(SocketGuildUser messageSender, ISocketMessageChannel channel, int changeAmount, int delaySeconds = 0)
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

            // TODO try to find a way to mute people here
            if (changeAmount == 1)
            {
                xmlParameters.Load(FileLocations.xmlParameters());
                XmlNode root = xmlParameters.DocumentElement;
                XmlNode spamMessageCountNode = CoOpGlobal.xmlFindOrCreateChild(xmlParameters, root, "SpamMessageCount", "3");
                spamMessageCount = int.Parse(spamMessageCountNode.InnerText);
                if (messageCount == spamMessageCount)
                {
                    await channel.SendMessageAsync("#StopCamSpam");
                }
            }

            return messageCount;

        }

        private void loadXMLParameters()
        {
            XmlNode prefixNode;

            if (!File.Exists(FileLocations.xmlParameters()))
            {
                if (!File.Exists(FileLocations.backupXMLParameters()))
                {
                    Console.WriteLine("XML parameters not found, no backup found");

                    throw new FileNotFoundException(@"XML parameters not found, and no backup found");
                }
                else
                {
                    xmlParameters.Load(FileLocations.backupXMLParameters());
                    xmlParameters.Save(FileLocations.xmlParameters());

                    Console.WriteLine("XML parameters restored from backup");
                }
            }

            if (!File.Exists(FileLocations.xmlDatabase()))
            {
                if (!File.Exists(FileLocations.backupXMLDatabase()))
                {
                    xmlParameters.Load(FileLocations.xmlParameters());
                    XmlNode dbRoot = xmlDatabase.CreateElement("CoOpBotDB");
                    XmlNode paramsRoot = xmlParameters.DocumentElement;
                    XmlNode paramsUsersNode = paramsRoot.SelectSingleNode("descendant::Users");
                    XmlNode paramsGuildWarsNode = paramsRoot.SelectSingleNode("descendant::GuildWars");

                    if (paramsUsersNode != null && paramsGuildWarsNode != null)
                    {

                        XmlNode importUsersNode;
                        XmlNode importGuildWarsNode;

                        importUsersNode = xmlDatabase.ImportNode(paramsUsersNode, true);
                        importGuildWarsNode = xmlDatabase.ImportNode(paramsGuildWarsNode, true);

                        dbRoot.AppendChild(importUsersNode);
                        dbRoot.AppendChild(importGuildWarsNode);
                        xmlDatabase.AppendChild(dbRoot);
                        xmlDatabase.Save(FileLocations.xmlDatabase());

                        paramsRoot.RemoveChild(paramsUsersNode);
                        paramsRoot.RemoveChild(paramsGuildWarsNode);
                        xmlParameters.Save(FileLocations.xmlParameters());
                        Console.WriteLine("XML database created from data in parameters file");
                    }
                    else
                    {
                        Console.WriteLine("XML database not found, no backup found");
                        Console.WriteLine("File cannot be initialsed from old parameters file since required db nodes do not exist there");

                        throw new FileNotFoundException(@"XML database not found, and no backup found");
                    }
                }
                else
                {
                    xmlParameters.Load(FileLocations.backupXMLDatabase());
                    xmlParameters.Save(FileLocations.xmlDatabase());

                    Console.WriteLine("XML parameters restored from backup");
                }
            }

            xmlParameters.Load(FileLocations.xmlParameters());
            XmlNode root = xmlParameters.DocumentElement;
            XmlNode botTokenNode = root.SelectSingleNode("descendant::BotToken");
            XmlNode spamTimerNode;
            XmlNode spamMessageCountNode;
            token = botTokenNode.InnerText;

            prefixNode = CoOpGlobal.xmlFindOrCreateChild(xmlParameters, root, "PrefixChar", "!");
            spamTimerNode = CoOpGlobal.xmlFindOrCreateChild(xmlParameters, root, "SpamTimer", "8");
            spamMessageCountNode = CoOpGlobal.xmlFindOrCreateChild(xmlParameters, root, "SpamMessageCount", "3");

            prefixCharacter = Convert.ToChar(prefixNode.InnerText);
            spamTimer = int.Parse(spamTimerNode.InnerText);
            spamMessageCount = int.Parse(spamMessageCountNode.InnerText);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    };
};
