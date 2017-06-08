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

            xmlParameters.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml");
            root = xmlParameters.DocumentElement;
            usersNode = root.SelectSingleNode("descendant::Users");
            steamKeyNode = root.SelectSingleNode("SteamToken");
            steamKey = steamKeyNode.InnerText;


            if (usersNode == null)
            {
                usersNode = xmlParameters.CreateElement("Users");
                root.AppendChild(usersNode);
            }

            /*guildWarsNode = root.SelectSingleNode("descendant::GuildWars");

            if (guildWarsNode == null)
            {
                guildWarsNode = xmlParameters.CreateElement("GuildWars");
                root.AppendChild(guildWarsNode);
            }
            guildIDNode = guildWarsNode.SelectSingleNode("descendant::GuildId");
            guildAccessTokenNode = guildWarsNode.SelectSingleNode("descendant::GuildAccessToken");

            if (guildIDNode != null && guildAccessTokenNode != null)
            {
                guildId = guildIDNode.InnerText;
                guildAccessToken = guildAccessTokenNode.InnerText;
            }
            else
            {
                guildId = null;
                guildAccessToken = null;
            }*/
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

            xmlParameters.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml");

            await ReplyAsync("Your Steam profile has been updated");
        }


        [Command("hasgame")]
        [Summary("Shows which members have a game")]
        private async Task RegisterhasgameCommand(string appid)
        {
            string url = apiPrefix + "/IPlayerService/GetOwnedGames/v1/?key=" + steamKey + "&steamid =";
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            //Boolean userNodeExists = false;
            XmlElement userDetails = null;

            while (usersEnumerator.MoveNext())
            {
                XmlElement curNode = usersEnumerator.Current as XmlElement;

                if (curNode.SelectSingleNode("descendant::steamID").InnerText.Length > 0)
                {
                    userDetails = curNode;
                    Array result = getAPIResponse(url + userDetails.SelectSingleNode("descendant::steamID").InnerText);
                    Console.WriteLine(result);
                }


            }

            await ReplyAsync(steamKey); return;
        }

        private Array getAPIResponse(string url, Boolean addSquareBrackets = false)
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
                    return decodedArray;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

    }
}
