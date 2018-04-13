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
using System.Timers;
using CoOpBot.Database;

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
        string token;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();


        public async Task MainAsync()
        {
            // Setup repeated method call
            // 300000 = every 5 minutes
            Timer timer = new Timer(300000);
            // Not sure if AutoReset and Enabled are needed... but it works like this so I'm leaving it
            // Fight me
            timer.AutoReset = true;
            timer.Enabled = true;

            client = new DiscordSocketClient();
            
            client.Log += Log;

            loadXMLParameters();
            
            await InstallCommands();
            
            //string token = botTokenNode.InnerText;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Set the method that runs on the timer
            timer.Elapsed += async (sender, e) => await HandleTimer(client);
            timer.Start();

            //await client.SetGameAsync("TestGameName");

            // Set global variables
            CoOpGlobal.bootupDateTime = DateTime.UtcNow;
            CoOpGlobal.rng = new Random();
            
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
                SocketGuild guild;

                messageSender = message.Author as SocketGuildUser;
                channel = message.Channel;
                guild = (message.Channel as IGuildChannel).Guild as SocketGuild;
                xmlParameters.Load(FileLocations.xmlParameters());
                XmlNode root = xmlParameters.DocumentElement;
                XmlNode spamTimerNode = CoOpGlobal.XML.findOrCreateChild(xmlParameters, root, "SpamTimer", "8");
                spamTimer = int.Parse(spamTimerNode.InnerText);

                // Check to make sure that a bot is not the author
                if (!messageSender.IsBot)
                {
                    // Increment the counter by 1
                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, guild, channel, 1); });

                    // Decrese the counter by 1 after parameteriesed number of seconds (default 8)
                    await Task.Factory.StartNew(async () => { await CountMessage(messageSender, guild, channel, -1, spamTimer); });
                }
            };
        }

        private async static Task<bool> HandleTimer(DiscordSocketClient client)
        {

            try
            {
                IReadOnlyCollection<SocketGuild> guilds;
                
                guilds = client.Guilds;

                foreach (SocketGuild curGuild in guilds)
                {
                    IReadOnlyCollection<SocketGuildUser> users;
                    users = curGuild.Users;
                    foreach (SocketGuildUser curUser in users)
                    {
                        if (curUser.Game.HasValue && !curUser.IsBot)
                        {
                            IRole gameRole;
                            string gameName;
                            IReadOnlyCollection<SocketRole> userRoles;
                            CoOpBot.Modules.Admin.RolesModule roleModule = new Modules.Admin.RolesModule();
                            Boolean userHasRole = false;
                            List<IRole> roleList = new List<IRole>();
                            List<ulong> userList = new List<ulong>();
                            RoleTranslations rt = new RoleTranslations();

                            gameName = curUser.Game.Value.Name;

                            rt = rt.find(gameName) as RoleTranslations;

                            if (rt != null)
                            {
                                gameName = rt.translateTo;
                            }

                            gameRole = roleModule.FindRoleFromName(gameName, curGuild);

                            if (gameRole == null)
                            {
                                return true;
                                /*
                                await roleModule.RoleCreate(curGuild, gameName);

                                gameRole = roleModule.FindRoleFromName(gameName, curGuild);
                                */
                            }

                            userRoles = curUser.Roles;

                            foreach (SocketRole curRole in userRoles)
                            {
                                if (curRole.Name == gameRole.Name)
                                {
                                    userHasRole = true;
                                    break;
                                }
                            }

                            if (!userHasRole)
                            {
                                roleList.Add(gameRole);
                                userList.Add(curUser.Id);
                                await roleModule.RoleAddUsers(curGuild, userList, roleList);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return true;
            }

            return true;
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
                        case "you":
                            responseText = "No u";
                            break;
                        case "good bot":
                            List<string> goodBotResponseList = new List<string>();

                            goodBotResponseList.Add("Good human");
                            goodBotResponseList.Add("No u");
                            goodBotResponseList.Add("Why thank you!");
                            goodBotResponseList.Add("(◠﹏◠✿)");
                            goodBotResponseList.Add("ｖ(◠ｏ◠)ｖ");
                            goodBotResponseList.Add("( ͡° ͜ʖ ͡°)");
                            goodBotResponseList.Add("Beep Boop");
                            goodBotResponseList.Add($"Yes {userSentBy.Username}, good bot indeed");
                            goodBotResponseList.Add("More like Gu'd bot");
                            goodBotResponseList.Add("Bot is the Cakeob!");

                            responseText = goodBotResponseList[CoOpGlobal.rng.Next(goodBotResponseList.Count)];
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

        private async Task<int> CountMessage(SocketGuildUser messageSender, SocketGuild guild, ISocketMessageChannel channel, int changeAmount, int delaySeconds = 0)
        {
            int messageCount;

            await Task.Delay(delaySeconds * 1000);

            if (userRecentMessageCounter[messageSender.Username] == null)
            {
                userRecentMessageCounter[messageSender.Username] = 0.ToString();
            }

            messageCount = int.Parse(userRecentMessageCounter[messageSender.Username]) + changeAmount;
            userRecentMessageCounter[messageSender.Username] = messageCount.ToString();
            
            if (changeAmount == 1)
            {
                xmlParameters.Load(FileLocations.xmlParameters());
                XmlNode root = xmlParameters.DocumentElement;
                XmlNode spamMessageCountNode = CoOpGlobal.XML.findOrCreateChild(xmlParameters, root, "SpamMessageCount", "3");
                spamMessageCount = int.Parse(spamMessageCountNode.InnerText);

                if (messageCount == spamMessageCount)
                {
                    CoOpBot.Modules.Admin.RolesModule roleModule = new Modules.Admin.RolesModule();

                    await channel.SendMessageAsync("#StopCamSpam");

                    List<IRole> roleList = new List<IRole>();
                    List<ulong> userList = new List<ulong>();

                    // Check if the Muted role exists (case sensitve [I think])
                    foreach (IRole curRole in guild.Roles)
                    {
                        if (curRole.Name == "Muted")
                        {
                            roleList.Add(curRole);
                        }
                    }
                    
                    userList.Add(messageSender.Id);
                    // Mute the user
                    if (roleList.Count == 1 && userList.Count == 1)
                    {
                        try
                        {
                            await roleModule.RoleAddUsers(guild, userList, roleList);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            else if (changeAmount == -1 && messageCount == 0)
            {
                CoOpBot.Modules.Admin.RolesModule roleModule = new Modules.Admin.RolesModule();

                List<IRole> roleList = new List<IRole>();
                List<ulong> userList = new List<ulong>();

                // Check if the Muted role exists (case sensitve [I think])
                foreach (IRole curRole in guild.Roles)
                {
                    if (curRole.Name == "Muted")
                    {
                        roleList.Add(curRole);
                    }
                }

                userList.Add(messageSender.Id);
                // Unmute the user
                if (roleList.Count == 1 && userList.Count == 1)
                {
                    try
                    {
                        await roleModule.RoleRemoveUsers(guild, userList, roleList);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
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

            prefixNode = CoOpGlobal.XML.findOrCreateChild(xmlParameters, root, "PrefixChar", "!");
            spamTimerNode = CoOpGlobal.XML.findOrCreateChild(xmlParameters, root, "SpamTimer", "8");
            spamMessageCountNode = CoOpGlobal.XML.findOrCreateChild(xmlParameters, root, "SpamMessageCount", "3");

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
