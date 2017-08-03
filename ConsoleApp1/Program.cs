using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.IO;

namespace CoOpBot
{
    public class Program
    {
        // Define & initialise variables
        private DiscordSocketClient client;
        private CommandService commands = new CommandService();
        XmlDocument xmlParameters = new XmlDocument();
        char prefixCharacter;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();


        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            
            client.Log += Log;

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
            XmlNode prefixNode = root.SelectSingleNode("descendant::PrefixChar");
            
            if (prefixNode == null)
            {
                prefixNode = xmlParameters.CreateElement("PrefixChar");
                prefixNode.InnerText = "!";
                root.AppendChild(prefixNode);

                xmlParameters.Save(FileLocations.xmlParameters());
            }

            prefixCharacter = Convert.ToChar(prefixNode.InnerText);
            
            await InstallCommands();
            
            string token = myNode.InnerText;
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
        
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    };
};
