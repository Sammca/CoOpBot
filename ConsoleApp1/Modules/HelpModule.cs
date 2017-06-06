using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoOpBot.Modules.HelpModule
{
    [Name("Help")]
    public class HelpModule : ModuleBase
    {
        private CommandService _service;
        private string prefix;

        public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
            prefix = "!";
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

                            if (result.IsSuccess)
                            {
                                description += $"{prefix}{cmd.Aliases.First()} ";

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

                                if (aliasCount > 0)
                                {
                                    description += "\n";
                                    description += "Other names: ";
                                }
                                foreach (string alias in cmd.Aliases)
                                {
                                    description += alias;
                                    if (aliasCount > aliasNumber)
                                    {
                                        description += ", ";
                                    }
                                    aliasNumber++;
                                }

                                description += "\n";

                                description += "*" + cmd.Summary + "*";
                            }

                            output = description;
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

            await ReplyAsync(output);
        }
    }
}