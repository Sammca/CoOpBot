using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot.Modules.Bloodbowl
{

    [Name("BloodBowl")]
    [Group("bb")]
    public class BloodbowlModule : ModuleBase
    {

        XmlDocument xmlDatabase = new XmlDocument();
        XmlNode root;
        XmlNode bloodbowlNode;
        XmlNode playersNode;
        XmlNode racesNode;
        XmlNode tournamentRacesNode;
        XmlNode tournamentSizeNode;
        int tournamentSize;
        Random rng;

        public BloodbowlModule()
        {
            xmlDatabase.Load(FileLocations.xmlDatabase());
            root = xmlDatabase.DocumentElement;
            rng = new Random();

            // Bloodbowl section of XML
            bloodbowlNode = CoOpGlobal.xmlFindOrCreateChild(xmlDatabase, root, "Bloodbowl");

            // List of all available races
            racesNode = bloodbowlNode.SelectSingleNode("descendant::Races");

            if (racesNode == null)
            {
                XmlNode raceNode;
                racesNode = xmlDatabase.CreateElement("Races");
                bloodbowlNode.AppendChild(racesNode);

                #region add all races
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Humans";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Orcs";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Dwarves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Skaven";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "High Elves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Dark Elves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Brettonians";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Chaos";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Wood Elves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Lizardmen";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Norse";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Undead";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Necromantic";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Nurgle";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Khemri";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Chaos Dwarves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Ogres";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Halflings";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Vampires";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Amazons";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Kislev";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Pro Elves";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Underworld";
                racesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Goblins";
                racesNode.AppendChild(raceNode);
                #endregion

                xmlDatabase.Save(FileLocations.xmlDatabase());
            }

            // Node that stores the total number of players in tournament
            tournamentSizeNode = CoOpGlobal.xmlFindOrCreateChild(xmlDatabase, bloodbowlNode, "TournamentSize", "16");

            tournamentSize = int.Parse(tournamentSizeNode.InnerText);

            // Players registered for the tournament
            playersNode = CoOpGlobal.xmlFindOrCreateChild(xmlDatabase, bloodbowlNode, "Players");

            // List of races being available for random distribution in the current tournament
            tournamentRacesNode = bloodbowlNode.SelectSingleNode("descendant::TournamentRacesNode");

            if (tournamentRacesNode == null)
            {
                XmlNode raceNode;
                tournamentRacesNode = xmlDatabase.CreateElement("TournamentRacesNode");
                bloodbowlNode.AppendChild(tournamentRacesNode);

                #region add all races
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Humans";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Orcs";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Dwarves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Skaven";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "High Elves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Dark Elves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Brettonians";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Chaos";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Wood Elves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Lizardmen";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Norse";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Undead";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Necromantic";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Nurgle";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Khemri";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Chaos Dwarves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Ogres";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Halflings";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Vampires";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Amazons";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Kislev";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Pro Elves";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Underworld";
                tournamentRacesNode.AppendChild(raceNode);
                raceNode = xmlDatabase.CreateElement("Race");
                raceNode.InnerText = "Goblins";
                tournamentRacesNode.AppendChild(raceNode);
                #endregion

                xmlDatabase.Save(FileLocations.xmlDatabase());
            }
        }

        #region Commands

        [Command("Players")]
        [Summary("Lists the players who are signed up for the bloodbowl tournament.")]
        private async Task PlayersCommand()
        {
            try
            {
                string output = "";
                string outputWithTeams = "";
                IEnumerator playersEnumerator = playersNode.GetEnumerator();
                int aiCount = tournamentSize;
                Boolean teamsAssigned = true;
                IUser playerUser;
                
                while (playersEnumerator.MoveNext())
                {
                    XmlElement playerNode = playersEnumerator.Current as XmlElement;

                    if (teamsAssigned && playerNode.InnerText == "")
                    {
                        teamsAssigned = false;
                    }

                    // check if node is for an AI
                    if (playerNode.GetAttribute("id") != "0")
                    {
                        playerUser = await Context.Guild.GetUserAsync(ulong.Parse(playerNode.GetAttribute("id")));

                        output += $"{playerUser.Username} \r\n";
                        outputWithTeams += $"{playerUser.Username} - {playerNode.InnerText} \r\n";
                    }
                    else
                    {
                        output += $"{playerNode.GetAttribute("name")} \r\n";
                        outputWithTeams += $"{playerNode.GetAttribute("name")} - {playerNode.InnerText} \r\n";
                    }

                    aiCount--;
                }

                if (aiCount > 0)
                {
                    teamsAssigned = false;
                    output += $"AI x{aiCount}";
                }

                if (teamsAssigned)
                {
                    output = outputWithTeams;
                }


                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("Register")]
        [Summary("Register yourself for the bloodbowl tournament")]
        private async Task RegisterCommand()
        {
            try
            {
                string output = "";
                IEnumerator playersEnumerator = playersNode.GetEnumerator();
                Boolean alreadyRegistered = false;

                while (playersEnumerator.MoveNext())
                {
                    XmlElement playerNode = playersEnumerator.Current as XmlElement;

                    if (ulong.Parse(playerNode.GetAttribute("id")) == Context.Message.Author.Id)
                    {
                        output = "You have already registered for the tournament!";
                        alreadyRegistered = true;
                    }
                }

                if (!alreadyRegistered)
                {
                    XmlElement newPlayerNode;

                    newPlayerNode = xmlDatabase.CreateElement("Player");
                    newPlayerNode.SetAttribute("id", Context.Message.Author.Id.ToString());
                    playersNode.AppendChild(newPlayerNode);

                    xmlDatabase.Save(FileLocations.xmlDatabase());

                    output = "You have successfully registered for the tournament";
                }

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        [Command("Register")]
        [Summary("Register other users for the bloodbowl tournament")]
        private async Task RegisterCommand(params IUser[] users)
        {
            try
            {
                string output = "";

                foreach (IUser curUser in users)
                {
                    IEnumerator playersEnumerator = playersNode.GetEnumerator();
                    Boolean alreadyRegistered = false;

                    while (playersEnumerator.MoveNext())
                    {
                        XmlElement playerNode = playersEnumerator.Current as XmlElement;

                        if (ulong.Parse(playerNode.GetAttribute("id")) == curUser.Id)
                        {
                            alreadyRegistered = true;
                        }
                    }

                    if (!alreadyRegistered)
                    {
                        XmlElement newPlayerNode;

                        newPlayerNode = xmlDatabase.CreateElement("Player");
                        newPlayerNode.SetAttribute("id", curUser.Id.ToString());
                        playersNode.AppendChild(newPlayerNode);

                        xmlDatabase.Save(FileLocations.xmlDatabase());
                    }
                }

                output += "Users successfully added to the tournament";

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("AssignTeams")]
        [Alias("CreateLeague")]
        [Summary("Assigns teams to the registered players. Parameter sets how many teams can be of the same race")]
        private async Task AssignTeamsCommand(int maxTeamsPerRace = 1)
        {
            try
            {
                string output = "";
                IEnumerator playersEnumerator = playersNode.GetEnumerator();
                IEnumerator tournamentRacesEnumerator = tournamentRacesNode.GetEnumerator();
                List<string> availableRaces = new List<string>();
                int aiCount = tournamentSize;

                while (tournamentRacesEnumerator.MoveNext())
                {
                    XmlElement raceNode = tournamentRacesEnumerator.Current as XmlElement;

                    for (int j = 1; j <= maxTeamsPerRace; j++)
                    {
                        availableRaces.Add(raceNode.InnerText);
                    }
                }

                while (playersEnumerator.MoveNext())
                {
                    XmlElement playerNode = playersEnumerator.Current as XmlElement;
                    string race;
                    int randomPosition = rng.Next(availableRaces.Count); // Don't add 1 to the exclusive Max becuase lists are 0 based
                    IUser playerUser = await Context.Guild.GetUserAsync(ulong.Parse(playerNode.GetAttribute("id")));

                    race = availableRaces[randomPosition];
                    availableRaces.RemoveAt(randomPosition);

                    playerNode.InnerText = race;

                    output += $"{playerUser.Username} - {race} \r\n";

                    aiCount--;
                }

                for (int i = 1; i <= aiCount; i++)
                {
                    XmlElement newAINode;
                    string race;
                    int randomPosition = rng.Next(availableRaces.Count); // Don't add 1 to the exclusive Max becuase lists are 0 based

                    race = availableRaces[randomPosition];
                    availableRaces.RemoveAt(randomPosition);

                    newAINode = xmlDatabase.CreateElement("Player");
                    newAINode.SetAttribute("id", "0");
                    newAINode.SetAttribute("name", $"AI {i}");
                    newAINode.InnerText = race;
                    playersNode.AppendChild(newAINode);

                    output += $"{$"AI {i}"} - {race} \r\n";
                }

                xmlDatabase.Save(FileLocations.xmlDatabase());

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("Reset")]
        [Summary("Resets the league player information")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task ResetCommand()
        {
            try
            {
                string output = "";
                playersNode.RemoveAll();
                xmlDatabase.Save(FileLocations.xmlDatabase());

                output += "League data reset";

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("SetSize")]
        [Summary("Sets the number of teams in the tournament")]
        private async Task SetSizeCommand(int teamCount)
        {
            try
            {
                string output = "";

                tournamentSize = teamCount;
                tournamentSizeNode.InnerText = tournamentSize.ToString();
                xmlDatabase.Save(FileLocations.xmlDatabase());

                output += $"League size has been updated to {tournamentSize} teams";

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("Size")]
        [Summary("Gets the number of teams in the tournament")]
        private async Task SizeCommand()
        {
            try
            {
                string output = "";

                output += $"League size is {tournamentSize} teams";

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

