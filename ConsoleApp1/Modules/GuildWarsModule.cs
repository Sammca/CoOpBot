﻿using Discord;
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


namespace CoOpBot.Modules.GuildWars
{
    [Group("gw")]
    public class GuildWarsModule : ModuleBase
    {
        XmlDocument xmlParameters = new XmlDocument();
        XmlNode root;
        XmlNode usersNode;
        XmlNode guildWarsNode;
        string apiPrefix;
        XmlNode guildIDNode;
        XmlNode guildAccessTokenNode;
        string guildId;
        string guildAccessToken;

        public GuildWarsModule()
        {
            apiPrefix = "https://api.guildwars2.com/v2";
            xmlParameters.Load("C:\\CoOpBotParameters.xml");
            root = xmlParameters.DocumentElement;
            usersNode = root.SelectSingleNode("descendant::Users");

            if (usersNode == null)
            {
                usersNode = xmlParameters.CreateElement("Users");
                root.AppendChild(usersNode);
            }

            guildWarsNode = root.SelectSingleNode("descendant::GuildWars");

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
            }
        }

        #region Commands

        [Command("RegisterKey")]
        [Alias("rk")]
        [Summary("Registers your guild wars API access token with the bot")]
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

            apiElement = userDetails.SelectSingleNode("descendant::gwAPIKey");

            if (apiElement == null)
            {
                apiElement = xmlParameters.CreateElement("gwAPIKey");
                apiElement.InnerText = key;
                userDetails.AppendChild(apiElement);
            }
            else
            {
                apiElement.InnerText = key;
            }

            xmlParameters.Save("C:\\CoOpBotParameters.xml");

            await ReplyAsync("Your API key has been updated");

        }


        [Command("GoldCount")]
        [Alias("gc")]
        [Summary("Finds the amount of gold the mentioned user has. Defaults to messgae author if no user is mentioned")]
        private async Task GoldCountCommand(params IUser[] users)
        {
            string apiKey;
            string url;
            int gold, silver, copper, rawAmount;
            Hashtable walletInfo;
            Array apiResponse;
            IUser user;

            gold = 0;
            silver = 0;
            copper = 0;
            rawAmount = 0;

            if (users.Length == 0)
            {
                user = this.Context.Message.Author;
            }
            else
            {
                user = users.GetValue(0) as IUser;
            }

            apiKey = getUserAPIKey(user);

            url = apiPrefix + "/account/wallet?access_token=" + apiKey;

            apiResponse = getAPIResponse(url);

            walletInfo = apiResponse.GetValue(0) as Hashtable;

            rawAmount = int.Parse(walletInfo["value"].ToString());
            
            copper = rawAmount % 100;
            rawAmount -= copper;

            silver = (rawAmount % 10000)/100;
            rawAmount -= (silver * 100);

            gold = rawAmount/ 10000;


            await ReplyAsync(string.Format("{0} has {1}g{2}s{3}c", user.Username, gold, silver, copper));

        }

        [Command("MOTD")]
        [Summary("Gets the guilds messgae of the day")]
        private async Task MOTDCommand()
        {
            string url;
            string motd;
            Array apiResponse;
            Hashtable guildInfo;

            motd = "";

            if (guildId == null)
            {
                await ReplyAsync("Guild not found in config file");
            }
            

            url = apiPrefix + "/guild/"+guildId+"?access_token=" + guildAccessToken;


            apiResponse = getAPIResponse(url, true);

            guildInfo = apiResponse.GetValue(0) as Hashtable;

            motd = guildInfo["motd"].ToString();
            
            await ReplyAsync(string.Format("{0}", motd));

        }

        [Command("GuildRanks")]
        [Alias("Ranks", "RankList")]
        [Summary("Lists the guilds ranks")]
        private async Task GuildRanksCommand()
        {
            string url;
            Dictionary<int, string> rankArray = new Dictionary<int, string>();
            string output;
            Array apiResponse;
            Hashtable curRank;

            output = "";
            
            if (guildId == null)
            {
                await ReplyAsync("Guild not found in config file");
            }

            url = apiPrefix + "/guild/" + guildId + "/ranks?access_token=" + guildAccessToken;
            
            apiResponse = getAPIResponse(url);
            for (int i = 0; i < apiResponse.Length; i++)
            {
                curRank = apiResponse.GetValue(i) as Hashtable;
                rankArray.Add(int.Parse(curRank["order"].ToString()), curRank["id"].ToString());
            }

            for (int i = 1; i <= rankArray.Count; i++)
            {
                output += rankArray[i] + "\r\n";
            }

            await ReplyAsync(string.Format("{0}", output));

        }
        #endregion

        #region Functions

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

        private string getUserAPIKey(IUser user)
        {
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            string apiKey;
            Boolean userNodeExists = false;
            XmlElement userDetails = null;
            XmlNode apiElement;

            while (usersEnumerator.MoveNext())
            {
                XmlElement curNode = usersEnumerator.Current as XmlElement;

                if (curNode.GetAttribute("id") == user.Id.ToString())
                {
                    userDetails = curNode;
                    userNodeExists = true;
                }
            }

            if (!userNodeExists)
            {
                throw new Exception(string.Format("API key not found for {0}",user.Username));
            }


            apiElement = userDetails.SelectSingleNode("descendant::gwAPIKey");

            if (apiElement == null)
            {
                throw new Exception(string.Format("API key not found for {0}", user.Username));
            }


            apiKey = apiElement.InnerText;

            return apiKey;
        }

        #endregion

    }
}
