using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Procurios.Public;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace CoOpBot.Modules.Steam
{
    [Group("steam")]
    public class SteamModule : ModuleBase
    {
        XmlDocument xmlParameters = new XmlDocument();
        XmlNode root;
        XmlNode usersNode;
        string apiPrefix;
        XmlNode steamKeyNode;
        string steamKey;

        public SteamModule()
        {
            apiPrefix = "https://api.steampowered.com";

            xmlParameters.Load(FileLocations.xmlParameters());
            root = xmlParameters.DocumentElement;
            usersNode = root.SelectSingleNode("descendant::Users");
            
            usersNode = root.SelectSingleNode("descendant::Users");

            if (usersNode == null)
            {
                usersNode = xmlParameters.CreateElement("Users");
                root.AppendChild(usersNode);
            }

            steamKeyNode = root.SelectSingleNode("SteamToken");
            if (steamKeyNode == null)
            {
                steamKeyNode = xmlParameters.CreateElement("SteamToken");
                root.AppendChild(steamKeyNode);
            }
            steamKey = steamKeyNode.InnerText;
        }

        [Command("key")]
        [Summary("Shows the steam key")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task RegistersteamKeyCommand()
        {
            await ReplyAsync(steamKey); return;
        }

        [Command("RegisterKey")]
        [Alias("rk")]
        [Summary("Registers your steam ID with the bot")]
        private async Task RegisterTokenCommand(string key)
        {
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            Boolean userNodeExists = false;
            XmlElement userDetails = null;

            while (usersEnumerator.MoveNext())
            {
                XmlElement curNode = usersEnumerator.Current as XmlElement;

                if (curNode.GetAttribute("id") == this.Context.Message.Author.Id.ToString())
                {
                    userDetails = curNode;
                    userNodeExists = true;
                }


            }

            if (!userNodeExists)
            {
                XmlElement newUserNode;

                newUserNode = xmlParameters.CreateElement("User");
                newUserNode.SetAttribute("id", this.Context.Message.Author.Id.ToString());

                userDetails = usersNode.AppendChild(newUserNode) as XmlElement;

            }

            XmlNode apiElement;

            apiElement = userDetails.SelectSingleNode("descendant::steamID");

            if (apiElement == null)
            {
                apiElement = xmlParameters.CreateElement("steamID");
                apiElement.InnerText = key;
                userDetails.AppendChild(apiElement);
            }
            else
            {
                apiElement.InnerText = key;
            }

            xmlParameters.Save(FileLocations.xmlParameters());

            await ReplyAsync("Your Steam profile has been updated");
        }


        [Command("hasgame")]
        [Summary("Shows which members have a game")]
        private async Task RegisterhasgameCommand(params string[] appName)
        {
            string queryStr = "";

            foreach (string s in appName)
            {
                queryStr += $"{s}";
            }

            string appid = queryStr;
            await ReplyAsync("Searching...");

            string url = apiPrefix + "/IPlayerService/GetOwnedGames/v1/?key=" + steamKey + "&steamid=";
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            //Boolean userNodeExists = false;
            XmlElement userDetails = null;
            string output = "";
            string usernamesOutput = "";

            if (!Regex.IsMatch(appid, @"^\d+$"))
            {
                List<gameResult> result;
                result = getGameId(appid);
                List<gameResult> resultOrdered = result.OrderBy(o => o.similarity).ToList();
                if (resultOrdered[0].similarity == 0) // if top result is an exact match
                {
                    appid = resultOrdered[0].id;
                } else
                {
                    string response ="Multiple games found \n";
                    for (var i = 0; i < 5; i++)
                    {
                        response += $"{resultOrdered[i].Name} - SteamID = {resultOrdered[i].id} \n";
                    }
                    response += "\n\nPlease try again using the SteamID of the game.";
                    await ReplyAsync(response); return;
                }

            }

            while (usersEnumerator.MoveNext())
            {
                XmlElement curNode = usersEnumerator.Current as XmlElement;
                //Console.WriteLine(curNode.SelectSingleNode("descendant::steamID").InnerText.Length);

                if (curNode.SelectSingleNode("descendant::steamID") != null)
                {
                    Hashtable userGamesInfo;
                    int numberOfGames = 0;
                    ArrayList games;
                    string username;

                    userDetails = curNode;
                    username = await getDiscordUsername(userDetails.GetAttribute("id"));
                    userGamesInfo = getAPIResponse(url + userDetails.SelectSingleNode("descendant::steamID").InnerText, "response", true);
                    games = userGamesInfo["games"] as ArrayList;
                    numberOfGames = games.Count;

                    for (int i = 0; i < numberOfGames; i++)
                    {
                        Hashtable curGame;
                        curGame = games[i] as Hashtable;

                        if (curGame["appid"].ToString() == appid)
                        {
                            usernamesOutput += $"\n{username}";
                            break;
                        }
                    }
                }
            }

            if (usernamesOutput != "")
            {
                output = $"Users with {gameName(appid)}:{usernamesOutput}";
            }
            else
            {
                output = $"No one owns {gameName(appid)}";
            }
            await ReplyAsync(output); return;
        }

        /* https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=431240
        [Command("latestnews")]
        [Summary("Shows Latest news article for a game")]
        private async Task RegisterlatestnewsCommand(string appid)
        {
            string url = apiPrefix + "/IPlayerService/GetOwnedGames/v1/?key=" + steamKey + "&steamid=";
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            //Boolean userNodeExists = false;
            XmlElement userDetails = null;
            string output = "";
            string usernamesOutput = "";

            while (usersEnumerator.MoveNext())
            {
                XmlElement curNode = usersEnumerator.Current as XmlElement;
                //Console.WriteLine(curNode.SelectSingleNode("descendant::steamID").InnerText.Length);

                if (curNode.SelectSingleNode("descendant::steamID") != null)
                {
                    Hashtable userGamesInfo;
                    int numberOfGames = 0;
                    ArrayList games;
                    string username;

                    userDetails = curNode;
                    username = await getDiscordUsername(userDetails.GetAttribute("id"));
                    userGamesInfo = getAPIResponse(url + userDetails.SelectSingleNode("descendant::steamID").InnerText, "response", true);
                    games = userGamesInfo["games"] as ArrayList;
                    numberOfGames = games.Count;

                    for (int i = 0; i < numberOfGames; i++)
                    {
                        Hashtable curGame;
                        curGame = games[i] as Hashtable;

                        if (curGame["appid"].ToString() == appid)
                        {
                            usernamesOutput += $"\n{username}";
                            break;
                        }
                    }
                }
            }

            if (usernamesOutput != "")
            {
                output = $"Users with {gameName(appid)}:{usernamesOutput}";
            }
            else
            {
                output = $"No one owns {gameName(appid)}";
            }
            await ReplyAsync(output); return;
        }
        */

        public class gameResult
        {
            public string Name { get; set; }
            public string id { get; set; }
            public int similarity { get; set; }
        }


        [Command("gameID")]
        [Summary("Searches for a Steam App ID")]
        private async Task RegistergameIDCommand(params string[] appName)
        {
            string queryStr = "";

            foreach (string s in appName)
            {
                queryStr += $"{s}";
            }

            string appid = queryStr;
            await ReplyAsync("Searching...");
            string response = "Did you mean? \n";
            List<gameResult> result;
            result = getGameId(appid);
            List<gameResult> resultOrdered = result.OrderBy(o => o.similarity).ToList();
            for (var i = 0; i < 5; i++)
            {
                response += $"{resultOrdered[i].Name} - SteamID = {resultOrdered[i].id} \n";
            }
            await ReplyAsync(response); return;
        }

        private Hashtable getAPIResponse(string url, string responseContainerString, Boolean addSquareBrackets = false)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    var jsonResponse = wc.DownloadString(url);

                    if (addSquareBrackets)
                    {
                        jsonResponse = "[" + jsonResponse + "]";
                    }

                    ArrayList decoded = JSON.JsonDecode(jsonResponse) as ArrayList;
                    Array decodedArray = decoded.ToArray();

                    //steam specific part - go 1 layer into the array becasue there is a single node that holds all the info we want
                    Hashtable response = decodedArray.GetValue(0) as Hashtable;
                    Hashtable returnArray = response[responseContainerString] as Hashtable;

                    return returnArray;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        private List<gameResult> getGameId(string queryStr)
        {
            string url;
            string gameName;
            Hashtable gameList;

            url = $"{apiPrefix}/ISteamApps/GetAppList/v2/?key={steamKey}";
            gameList = getAPIResponse(url, "applist", true);

            Hashtable steamGamesInfo;
            int numberOfGames = 0;
            //int matchcount = 0;
            ArrayList games;
            List<gameResult> results = new List<gameResult>();

            steamGamesInfo = getAPIResponse(url, "applist", true);
            games = steamGamesInfo["apps"] as ArrayList;
            numberOfGames = games.Count;

            for (int i = 0; i < numberOfGames; i++)
            {
                Hashtable curGame;
                curGame = games[i] as Hashtable;
                gameName = Regex.Replace(curGame["name"].ToString().ToLower(), @"\s+", "");
                int similarity = LevenshteinDistance.Compute(gameName, queryStr);
                if (similarity > 15) continue;
                gameResult resultClass = new gameResult
                {
                    Name = curGame["name"].ToString(),
                    id = curGame["appid"].ToString(),
                    similarity = similarity
                };
                results.Add(resultClass);
                if (similarity == 0) return results;
            }
            return results;

        }

        private string gameName(string appId)
        {
            string url;
            Hashtable gameInfo;
            string gameName;

            url = $"{apiPrefix}/ISteamUserStats/GetSchemaForGame/v2/?key={steamKey}&appid={appId}";
            //Console.WriteLine(url);
            gameInfo = getAPIResponse(url, "game", true);
            gameName = gameInfo["gameName"].ToString();

            return gameName;
        }

        private async Task<string> getDiscordUsername(string discordId)
        {
            string username = "";
            IEnumerable<IGuildUser> users;
            IGuildUser curUser;

            users = await this.Context.Guild.GetUsersAsync();

            for (int i = 0; i < users.Count(); i++)
            {
                curUser = users.ElementAt(i);

                if ($"{curUser.Id}" == discordId)
                {
                    if (curUser.Nickname != null)
                    {
                        username = curUser.Nickname;
                    }
                    else
                    {
                        username = curUser.Username;
                    }
                }
            }

            return username;
        }
    }
}
