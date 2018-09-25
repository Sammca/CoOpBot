using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot.Modules.HelpModule
{
    [Name("Help")]
    public class HelpModule : ModuleBase
    {
        private CommandService _service;
        private string prefix;

        public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
        {
            XmlDocument xmlParameters = new XmlDocument();
            xmlParameters.Load(FileLocations.xmlParameters());
            XmlNode root = xmlParameters.DocumentElement;
            XmlNode prefixNode = root.SelectSingleNode("descendant::PrefixChar");
            
            _service = service;
            prefix = prefixNode.InnerText;
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    int parameterCount = 0;
                    int parameterNumber = 1;
                    if (result.IsSuccess)
                    {
                        description += $"{prefix}{cmd.Aliases.First()} ";

                        parameterCount = cmd.Parameters.Count;

                        if (parameterCount > 0)
                        {
                            description += "(";
                        }

                        foreach (ParameterInfo paramInfo in cmd.Parameters)
                        {
                            string parameterOutput = "";

                            parameterOutput = paramInfo.Name;

                            if (paramInfo.IsMultiple == true)
                            {
                                parameterOutput += "[]";
                            }

                            if (paramInfo.IsOptional == true)
                            {
                                parameterOutput = "[" + parameterOutput + "]";
                            }

                            if (parameterCount > parameterNumber)
                            {
                                parameterOutput += ", ";
                            }

                            description += parameterOutput;
                            parameterNumber++;
                        }

                        if (parameterCount > 0)
                        {
                            description += ")";
                        }

                        description += "\n";
                    }

                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        public async Task HelpAsync(params string[] command)
        {
            string commandString = "";
            string output = "";
            bool inputIsModule = false;
            bool breakStatemenet = false;
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218)
            };

            commandString = command.Length > 1 ? command[0] + " " + command[1] : command[0];

            foreach (var module in _service.Modules)
            {

                if (module.Aliases[0] == commandString)
                {
                    inputIsModule = true;
                    breakStatemenet = true;
                    foreach (var cmd in module.Commands)
                    {
                        string description = null;
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        int parameterCount = 0;
                        int parameterNumber = 1;

                        if (result.IsSuccess)
                        {
                            description += $"{prefix}{cmd.Aliases.First()} ";

                            parameterCount = cmd.Parameters.Count;

                            if (parameterCount > 0)
                            {
                                description += "(";
                            }

                            foreach (ParameterInfo paramInfo in cmd.Parameters)
                            {
                                string parameterOutput = "";

                                parameterOutput = paramInfo.Name;

                                if (paramInfo.IsMultiple == true)
                                {
                                    parameterOutput += "[]";
                                }

                                if (paramInfo.IsOptional == true)
                                {
                                    parameterOutput = "[" + parameterOutput + "]";
                                }

                                if (parameterCount > parameterNumber)
                                {
                                    parameterOutput += ", ";
                                }

                                description += parameterOutput;
                                parameterNumber++;
                            }

                            if (parameterCount > 0)
                            {
                                description += ")";
                            }

                            description += "\n";
                        }
                        output += description;

                    }
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = output;
                        x.IsInline = false;
                    });
                }
                if (breakStatemenet)
                {
                    break;
                }
            }

            breakStatemenet = false;

            if (!inputIsModule)
            {
                foreach (var module in _service.Modules)
                {
                    string description = null;
                    
                    foreach (var cmd in module.Commands)
                    {
                        if (cmd.Aliases.Contains(commandString, StringComparer.OrdinalIgnoreCase))
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context);
                            int parameterCount = 0;
                            int parameterNumber = 1;
                            int aliasCount = 0;
                            int aliasNumber = 1;
                            breakStatemenet = true;
                            string commandStr = "";

                            if (result.IsSuccess)
                            {
                                description += $"{prefix}{cmd.Aliases.First()} ";
                                commandStr = $"{cmd.Aliases.First()}";

                                parameterCount = cmd.Parameters.Count;
                                aliasCount = cmd.Aliases.Count;

                                if (parameterCount > 0)
                                {
                                    description += "(";
                                }

                                foreach (ParameterInfo paramInfo in cmd.Parameters)
                                {
                                    string parameterOutput = "";

                                    parameterOutput = paramInfo.Name;

                                    if (paramInfo.IsMultiple == true)
                                    {
                                        parameterOutput += "[]";
                                    }

                                    if (paramInfo.IsOptional == true)
                                    {
                                        parameterOutput = "[" + parameterOutput + "]";
                                    }

                                    if (parameterCount > parameterNumber)
                                    {
                                        parameterOutput += ", ";
                                    }

                                    description += parameterOutput;
                                    parameterNumber++;
                                }

                                if (parameterCount > 0)
                                {
                                    description += ")";
                                }

                                if (aliasCount > 1)
                                {
                                    description += "\n";
                                    description += "Other names: ";
                                }
                                foreach (string alias in cmd.Aliases)
                                {
                                    if (alias != cmd.Aliases.First())
                                    {
                                        description += alias;
                                        if (aliasCount > aliasNumber)
                                        {
                                            description += ", ";
                                        }
                                    }
                                    aliasNumber++;
                                }

                                description += "\n";

                                description += "*" + cmd.Summary + "*";
                            }

                            output = description;

                            builder.AddField(x =>
                            {
                                x.Name = commandStr;
                                x.Value = description;
                                x.IsInline = false;
                            });
                        }
                        if (breakStatemenet)
                        {
                            break;
                        }
                    }
                    if (breakStatemenet)
                    {
                        break;
                    }
                }
            }
            
            await ReplyAsync("", false, builder.Build());
        }

        [Command("infoHelp")]
        public async Task InfoHelpAsync()
        {
            string infoHelpString = "";

            infoHelpString = $@"Information can be added to my database to help people know more about you and to make it easier to add friends on Steam, Origin and Battle Net

Adding details to your user info card (all optional - this is what people see when using the '{CoOpGlobal.prefixCharacter}user info' command for you): 
{CoOpGlobal.prefixCharacter}user title [TITLE TEXT] - Sets the title text, this can also have a URL attached (see next command).
{CoOpGlobal.prefixCharacter}user URL [URL] - Sets the URL that your title text links to. Please keep it sensible!
{CoOpGlobal.prefixCharacter}user description [DESCRIPTION TEXT] - Sets a description to go under the title.
{CoOpGlobal.prefixCharacter}user footer [FOOTER TEXT] - Sets footer text, this is in a smaller font at the bottom of the output.
{CoOpGlobal.prefixCharacter}user icon [ICON URL] - Adds a small icon in your footer next to the footer text.

Adding your usernames (These all help people find you to add friends easier):
{CoOpGlobal.prefixCharacter}steam RegisterKey [YOUR STEAM ID NUMBER HERE] - Links your Steam account.
{CoOpGlobal.prefixCharacter}user OriginName [ORIGIN USERNAME] - Links your Origin account name.
{CoOpGlobal.prefixCharacter}user BattleNetName [BATTLE NET USERNAME] - Links your Battle Net account name (needs the # and numbers at the end).
{CoOpGlobal.prefixCharacter}user Twitch [TWITCH USERNAME] - Links your Twitch account name.
{CoOpGlobal.prefixCharacter}user Youtube [YOUTUBE CHANNEL URL] - Links your YouTube channel.
";

            await ReplyAsync(infoHelpString);
        }
    }
}