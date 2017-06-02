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

            xmlParameters.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml");
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

            xmlParameters.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml");

            await ReplyAsync("Your API key has been updated");
        }

        [Command("RegisterGuild")]
        [Summary("Registers the guild ID with the bot")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task RegisterGuildCommand(string gid, string gat)
        {
            IEnumerator gwEnumerator = guildWarsNode.GetEnumerator();

            if (guildIDNode == null && guildAccessTokenNode == null)
            {
                guildIDNode = xmlParameters.CreateElement("GuildId");
                guildAccessTokenNode = xmlParameters.CreateElement("GuildAccessToken");

                guildWarsNode.AppendChild(guildIDNode);
                guildWarsNode.AppendChild(guildAccessTokenNode);
            }

            guildIDNode.InnerText = gid;
            guildAccessTokenNode.InnerText = gat;

            xmlParameters.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml");

            await ReplyAsync("Guild API key has been updated");
        }

        [Command("GoldCount")]
        [Alias("gc")]
        [Summary("Finds the amount of gold the mentioned user has. Defaults to message author if no user is mentioned")]
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
            Dictionary<string, string> rankMembersArray = new Dictionary<string, string>();
            string output;
            Array apiResponse;
            Hashtable curRank;
            Hashtable guildMember;

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

            url = apiPrefix + "/guild/" + guildId + "/members?access_token=" + guildAccessToken;

            apiResponse = getAPIResponse(url);
            for (int i = 0; i < apiResponse.Length; i++)
            {
                guildMember = apiResponse.GetValue(i) as Hashtable;
                if (!rankMembersArray.ContainsKey(guildMember["rank"].ToString()))
                {
                    rankMembersArray.Add(guildMember["rank"].ToString(), guildMember["name"].ToString());
                }
                else
                {
                    rankMembersArray[guildMember["rank"].ToString()] += ", " + guildMember["name"].ToString();
                }
            }

            for (int i = 1; i <= rankArray.Count; i++)
            {
                if (rankMembersArray.ContainsKey(rankArray[i]))
                {
                    output += "**" + rankArray[i] + " (" + rankMembersArray[rankArray[i]].Split(',').Length + ")**\r\n";
                    output += "*" + rankMembersArray[rankArray[i]] + "*\r\n";
                }
                else
                {
                    output += "**" + rankArray[i] + "**\r\n";
                }
            }

            await ReplyAsync(string.Format("{0}", output));
        }


        [Command("Username")]
        [Alias("un")]
        [Summary("Finds the username of the mentioned user. Defaults to message author if no user is mentioned")]
        private async Task UsernameCommand(params IUser[] users)
        {
            string apiKey;
            string url;
            Hashtable accountInfo;
            Array apiResponse;
            IUser user;
            string gw2Username;

            if (users.Length == 0)
            {
                user = this.Context.Message.Author;
            }
            else
            {
                user = users.GetValue(0) as IUser;
            }

            apiKey = getUserAPIKey(user);
            url = apiPrefix + "/account?access_token=" + apiKey;
            apiResponse = getAPIResponse(url, true);
            accountInfo = apiResponse.GetValue(0) as Hashtable;
            gw2Username = accountInfo["name"].ToString();
            
            await ReplyAsync(string.Format("{0}'s GW2 usernme is {1}", user.Username, gw2Username));
        }

        [Command("Donations")]
        [Summary("Computes the cost of guild donations per person")]
        private async Task DonationsCommand()
        {
            string url;
            Dictionary<string, int> donationValueArray = new Dictionary<string, int>();
            Dictionary<int, int> itemValueArray = new Dictionary<int, int>();
            string output;
            Array apiResponse;
            Hashtable curTransaction;
            Hashtable itemPrices;
            Hashtable guildMember;
            Dictionary<string, Dictionary<int, int>> userDonatedItems = new Dictionary<string, Dictionary<int, int>>();
            int rank = 1;

            output = "";

            itemValueArray.Add(0, 1);

            if (guildId == null)
            {
                await ReplyAsync("Guild not found in config file");
            }

            url = apiPrefix + "/guild/" + guildId + "/log?access_token=" + guildAccessToken;

            apiResponse = getAPIResponse(url);
            for (int i = 0; i < apiResponse.Length; i++)
            {
                curTransaction = apiResponse.GetValue(i) as Hashtable;

                if (curTransaction["type"].ToString() == "treasury")
                {
                    if (!userDonatedItems.ContainsKey(curTransaction["user"].ToString()))
                    {
                        Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();
                        itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), int.Parse(curTransaction["count"].ToString()));

                        userDonatedItems.Add(curTransaction["user"].ToString(), itemDictionaryElemet);
                    }
                    else
                    {
                        Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();

                        itemDictionaryElemet = userDonatedItems[curTransaction["user"].ToString()];


                        if (!itemDictionaryElemet.ContainsKey(int.Parse(curTransaction["item_id"].ToString())))
                        {
                            itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), int.Parse(curTransaction["count"].ToString()));
                        }
                        else
                        {
                            itemDictionaryElemet[int.Parse(curTransaction["item_id"].ToString())] += int.Parse(curTransaction["count"].ToString());
                        }

                        userDonatedItems[curTransaction["user"].ToString()] = itemDictionaryElemet;
                    }
                }
                if (curTransaction["type"].ToString() == "stash")
                {
                    int directionMultiplier;

                    directionMultiplier = curTransaction["operation"].ToString() == "deposit" ? 1 : -1;

                    if (!userDonatedItems.ContainsKey(curTransaction["user"].ToString()))
                    {
                        Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();
                        itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), directionMultiplier * int.Parse(curTransaction["count"].ToString()));

                        userDonatedItems.Add(curTransaction["user"].ToString(), itemDictionaryElemet);
                    }
                    else
                    {
                        Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();

                        itemDictionaryElemet = userDonatedItems[curTransaction["user"].ToString()];


                        if (!itemDictionaryElemet.ContainsKey(int.Parse(curTransaction["item_id"].ToString())))
                        {
                            if (int.Parse(curTransaction["item_id"].ToString()) == 0)
                            {
                                itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), directionMultiplier * int.Parse(curTransaction["coins"].ToString()));
                            }
                            else
                            {
                                itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), directionMultiplier * int.Parse(curTransaction["count"].ToString()));
                            }
                        }
                        else
                        {
                            if (int.Parse(curTransaction["item_id"].ToString()) == 0)
                            {
                                itemDictionaryElemet[int.Parse(curTransaction["item_id"].ToString())] += (directionMultiplier * int.Parse(curTransaction["coins"].ToString()));
                            }
                            else
                            {
                                itemDictionaryElemet[int.Parse(curTransaction["item_id"].ToString())] += (directionMultiplier * int.Parse(curTransaction["count"].ToString()));
                            }
                        }

                        userDonatedItems[curTransaction["user"].ToString()] = itemDictionaryElemet;
                    }
                }
            }
            
            foreach (KeyValuePair<string, Dictionary<int, int>> entry in userDonatedItems)
            {
                Dictionary<int, int> memberDonationArray = new Dictionary<int, int>();
                string curGuildMember;

                curGuildMember = entry.Key;
                memberDonationArray = entry.Value;

                foreach (KeyValuePair<int, int> donationItemEntry in memberDonationArray)
                {
                    int curItem;
                    int donationCount;
                    int itemValue;
                    Hashtable buyInfo;

                    itemValue = 0;

                    curItem = donationItemEntry.Key;
                    donationCount = donationItemEntry.Value;

                    if (itemValueArray.ContainsKey(curItem))
                    {
                        itemValue = itemValueArray[curItem];
                    }
                    else
                    {
                        url = apiPrefix + "/commerce/prices?id=" + curItem;

                        try
                        {
                            apiResponse = getAPIResponse(url, true);

                            itemPrices = apiResponse.GetValue(0) as Hashtable;

                            buyInfo = itemPrices["buys"] as Hashtable;

                            itemValue = int.Parse(buyInfo["unit_price"].ToString());
                        }
                        catch
                        {
                            itemValue = 0;
                        }
                        itemValueArray.Add(curItem, itemValue);
                    }

                    if (!donationValueArray.ContainsKey(curGuildMember))
                    {
                        donationValueArray.Add(curGuildMember, (itemValue * donationCount));
                    }
                    else
                    {
                        donationValueArray[curGuildMember] += (itemValue * donationCount);
                    }
                }
            }

            var sortedDict = from entry in donationValueArray orderby entry.Value descending select entry;

            Dictionary<string, int> donationValueOrdered = sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (KeyValuePair<string, int> entry in donationValueOrdered)
            {
                int gold, silver, copper, rawAmount;

                rawAmount = entry.Value;

                copper = rawAmount % 100;
                rawAmount -= copper;

                silver = (rawAmount % 10000) / 100;
                rawAmount -= (silver * 100);

                gold = rawAmount / 10000;

                if (rank <= 3)
                {
                    output += string.Format("**{0}. {1}: {2}g{3}s{4}c** \r\n", rank, entry.Key, gold, silver, copper);
                }
                else
                {
                    if (rawAmount < 0)
                    {
                        output += string.Format("*{0}. {1}: {2}g{3}s{4}c* \r\n", rank, entry.Key, gold, silver, copper);
                    }
                    else
                    {
                        output += string.Format("{0}. {1}: {2}g{3}s{4}c \r\n", rank, entry.Key, gold, silver, copper);
                    }
                }
                rank++;
            }

            await ReplyAsync(output);
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
