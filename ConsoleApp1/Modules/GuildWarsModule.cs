using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Procurios.Public;
using System.Text.RegularExpressions;

namespace CoOpBot.Modules.GuildWars
{
    [Group("gw")]
    [Name("Guild Wars 2")]
    public class GuildWarsModule : ModuleBase
    {
        XmlDocument xmlDatabase = new XmlDocument();
        XmlNode root;
        XmlNode usersNode;
        XmlNode guildWarsNode;
        string apiPrefix;
        XmlNode guildIDNode;
        XmlNode guildAccessTokenNode;
        string guildId;
        string guildAccessToken;
        XmlNode accountBoundItemsNode;

        public GuildWarsModule()
        {
            apiPrefix = "https://api.guildwars2.com/v2";

            xmlDatabase.Load(FileLocations.xmlDatabase());
            root = xmlDatabase.DocumentElement;

            // Users
            usersNode = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, root, "Users");

            // GW specific settings
            guildWarsNode = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, root, "GuildWars");

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

            // GW non tradeable items
            accountBoundItemsNode = CoOpGlobal.XML.findOrCreateChild(xmlDatabase, guildWarsNode, "AccountBoundItems");
        }

        #region Commands

        [Command("RegisterKey")]
        [Alias("rk")]
        [Summary("Registers your guild wars API access token with the bot")]
        private async Task RegisterTokenCommand(string key)
        {
            XmlElement userDetails;
            userDetails = CoOpGlobal.XML.findOrCreateNodeFromAttribute(xmlDatabase, usersNode, "id", this.Context.Message.Author.Id.ToString(), "User");

            CoOpGlobal.XML.updateOrCreateChildNode(xmlDatabase, userDetails, "gwAPIKey", key);

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
                guildIDNode = xmlDatabase.CreateElement("GuildId");
                guildAccessTokenNode = xmlDatabase.CreateElement("GuildAccessToken");

                guildWarsNode.AppendChild(guildIDNode);
                guildWarsNode.AppendChild(guildAccessTokenNode);
            }

            guildIDNode.InnerText = gid;
            guildAccessTokenNode.InnerText = gat;

            xmlDatabase.Save(FileLocations.xmlDatabase());

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

            apiKey = await getUserAPIKey(user);

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
        [Summary("Gets the guilds message of the day")]
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
        private async Task UsernameCommand(IUser user)
        {
            string apiKey;
            string url;
            Hashtable accountInfo;
            Array apiResponse;
            string gw2Username;

            if (user == null)
            {
                user = this.Context.Message.Author;
            }

            apiKey = await getUserAPIKey(user);
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
            Dictionary<string, Dictionary<int, int>> userDonatedItems = new Dictionary<string, Dictionary<int, int>>();
            int rank = 1;
            string upgradeListUrl = apiPrefix + "/guild/upgrades/";
            ArrayList upgradeList = getAPIResponse(upgradeListUrl, true).GetValue(0) as ArrayList;

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
                else if (curTransaction["type"].ToString() == "stash")
                {
                    int directionMultiplier;

                    directionMultiplier = curTransaction["operation"].ToString() == "deposit" ? 1 : -1;

                    if (!userDonatedItems.ContainsKey(curTransaction["user"].ToString()))
                    {
                        Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();


                        if (int.Parse(curTransaction["item_id"].ToString()) == 0)
                        {
                            itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), directionMultiplier * int.Parse(curTransaction["coins"].ToString()));
                        }
                        else
                        {
                            itemDictionaryElemet.Add(int.Parse(curTransaction["item_id"].ToString()), directionMultiplier * int.Parse(curTransaction["count"].ToString()));
                        }
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
                else if (curTransaction["type"].ToString() == "upgrade" && curTransaction["action"].ToString() == "queued" && curTransaction.ContainsKey("user") && upgradeList.Contains(curTransaction["upgrade_id"]))
                {
                    string upgradeId = curTransaction["upgrade_id"].ToString();
                    string upgradeUrl = apiPrefix + "/guild/upgrades/" + upgradeId;
                    Array upgradeAPIResponse;

                    upgradeAPIResponse = getAPIResponse(upgradeUrl, true);

                    if (upgradeAPIResponse != null)
                    {

                        Hashtable upgradeInfo = upgradeAPIResponse.GetValue(0) as Hashtable;
                        ArrayList upgradeCosts = upgradeInfo["costs"] as ArrayList;

                        foreach (Hashtable costItem in upgradeCosts)
                        {
                            if (costItem["type"].ToString().ToLower() == "coins")
                            {
                                if (!userDonatedItems.ContainsKey(curTransaction["user"].ToString()))
                                {
                                    Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();
                                    itemDictionaryElemet.Add(0, int.Parse(costItem["count"].ToString()));

                                    userDonatedItems.Add(curTransaction["user"].ToString(), itemDictionaryElemet);
                                }
                                else
                                {
                                    Dictionary<int, int> itemDictionaryElemet = new Dictionary<int, int>();

                                    itemDictionaryElemet = userDonatedItems[curTransaction["user"].ToString()];


                                    if (!itemDictionaryElemet.ContainsKey(0))
                                    {
                                        itemDictionaryElemet.Add(0, int.Parse(costItem["count"].ToString()));
                                    }
                                    else
                                    {
                                        itemDictionaryElemet[0] += int.Parse(costItem["count"].ToString());
                                    }

                                    userDonatedItems[curTransaction["user"].ToString()] = itemDictionaryElemet;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            /**********************************************************
             * 
             * Add on gold cost for upgrades which aren't showing a user
             * 
            **********************************************************/

            Dictionary<int, int> additionItemDictionaryElemet = new Dictionary<int, int>();
            int coinAmountToAdd;

            additionItemDictionaryElemet = userDonatedItems["Poppins.2053"];

            coinAmountToAdd = 0;
            // 20 gold for Repair Anvil
            coinAmountToAdd += 200000;

            if (!additionItemDictionaryElemet.ContainsKey(0))
            {
                additionItemDictionaryElemet.Add(0, coinAmountToAdd);
            }
            else
            {
                // 20 gold for Repair Anvil
                additionItemDictionaryElemet[0] += coinAmountToAdd;
            }

            userDonatedItems["Poppins.2053"] = additionItemDictionaryElemet;

            additionItemDictionaryElemet = userDonatedItems["Xutos.4632"];

            coinAmountToAdd = 0;
            // 100 gold for Guild Hall access
            coinAmountToAdd += 1000000;

            if (!additionItemDictionaryElemet.ContainsKey(0))
            {
                additionItemDictionaryElemet.Add(0, coinAmountToAdd);
            }
            else
            {
                // 20 gold for Repair Anvil
                additionItemDictionaryElemet[0] += coinAmountToAdd;
            }

            userDonatedItems["Xutos.4632"] = additionItemDictionaryElemet;
            /**********************************************************
             * 
             * End
             * 
            **********************************************************/

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
                        if (isItemAccountBount(curItem))
                        {
                            itemValue = 0;
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
                                XmlElement newAccountBoundItemNode;
                                Hashtable itemDetails;
                                
                                url = apiPrefix + "/items?id=" + curItem;
                                apiResponse = getAPIResponse(url, true);

                                itemDetails = apiResponse.GetValue(0) as Hashtable;

                                newAccountBoundItemNode = xmlDatabase.CreateElement("Item");
                                newAccountBoundItemNode.SetAttribute("id", $"{curItem}");
                                newAccountBoundItemNode.InnerText = itemDetails["name"].ToString();

                                newAccountBoundItemNode = accountBoundItemsNode.AppendChild(newAccountBoundItemNode) as XmlElement;

                                xmlDatabase.Save(FileLocations.xmlDatabase());

                                itemValue = 0;
                            }
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
                    if (gold + silver + copper < 0)
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

        [Command("AmountStored")]
        [Alias("as", "stored")]
        [Summary("Finds the amount of a material the specified user has in their material storage")]
        private async Task AmountStoredStringCommand(IUser user, params string[] itemNameQuery)
        {
            List<itemResult> itemSearchResults = new List<itemResult>();
            string queryStr = "";
            itemResult closestMatch;
            int amountStored = 0;
            int itemId;

            foreach (string s in itemNameQuery)
            {
                queryStr += $"{s}";
            }

            itemSearchResults = itemSearch(queryStr).OrderBy(o => o.similarity).ToList();
            closestMatch = itemSearchResults[0];
            itemId = int.Parse(closestMatch.id);

            amountStored = await getAmountStored(itemId, user);

            await ReplyAsync($"{user.Username} has {amountStored} of item \"{itemNameFromId(itemId)}\" in their material storage");
        }

        [Command("AmountStored")]
        [Alias("as", "stored")]
        [Summary("Finds the amount of a material you have in your material storage")]
        private async Task AmountStoredStringCommand(params string[] itemNameQuery)
        {
            List<itemResult> itemSearchResults = new List<itemResult>();
            IUser user;

            user = this.Context.Message.Author;

            await AmountStoredStringCommand(user, itemNameQuery);
        }

        [Command("StoredAll")]
        [Alias("sa", "totalstored")]
        [Summary("Finds the amount of a material every user has in their material storage")]
        private async Task StoredAllCommand(params string[] itemNameQuery)
        {
            List<itemResult> itemSearchResults = new List<itemResult>();
            string queryStr = "";
            itemResult closestMatch;
            int userStored = 0;
            int totalStored = 0;
            int itemId;
            string output = "";
            string itemName = "";
            IUser curUser;
            IEnumerator usersEnumerator = usersNode.GetEnumerator();

            foreach (string s in itemNameQuery)
            {
                queryStr += $"{s}";
            }

            itemSearchResults = itemSearch(queryStr).OrderBy(o => o.similarity).ToList();
            closestMatch = itemSearchResults[0];
            itemId = int.Parse(closestMatch.id);
            itemName = itemNameFromId(itemId);

            while (usersEnumerator.MoveNext())
            {
                XmlElement curUserNode = usersEnumerator.Current as XmlElement;
                XmlNode userAPIKeyNode = curUserNode.SelectSingleNode("descendant::gwAPIKey");

                if (userAPIKeyNode == null)
                {
                    continue;
                }

                curUser = this.Context.Guild.GetUserAsync(ulong.Parse(curUserNode.GetAttribute("id"))).Result as IUser;

                if (curUser == null)
                {
                    continue;
                }

                userStored = await getAmountStored(itemId, curUser);
                totalStored += userStored;

                output += $"\n{curUser.Username} - {userStored}";
            }

            output = $"Total {itemName} stored = {totalStored}:{output}";

            await ReplyAsync(output);
        }

        [Command("ItemSearch")]
        [Alias("is")]
        [Summary("Finds the items close to the query string entered")]
        private async Task ItemSearchCommand(params string[] query)
        {
            List<itemResult> itemSearchResults = new List<itemResult>();
            string queryStr = "";

            foreach (string s in query)
            {
                queryStr += $"{s}";
            }

            itemSearchResults = itemSearch(queryStr).OrderBy(o => o.similarity).ToList();

            await ReplyAsync($"Closest match - {itemSearchResults[0].Name}");
        }

        [Command("price")]
        [Summary("Finds the sell price for the given item")]
        private async Task PriceCommand(params string[] query)
        {
            List<itemResult> itemSearchResults = new List<itemResult>();
            string queryStr = "";
            itemResult item;
            int itemValue;
            Hashtable buyInfo;
            string url;
            Dictionary<string, int> donationValueArray = new Dictionary<string, int>();
            Dictionary<int, int> itemValueArray = new Dictionary<int, int>();
            Array apiResponse;
            Hashtable itemPrices;

            foreach (string s in query)
            {
                queryStr += $"{s}";
            }

            itemSearchResults = itemSearch(queryStr).OrderBy(o => o.similarity).ToList();

            item = itemSearchResults[0];

            itemValue = 0;

            if (isItemAccountBount(int.Parse(item.id)))
            {
                itemValue = 0;
            }
            else
            {
                url = $"{apiPrefix}/commerce/prices?id={item.id}";

                try
                {
                    apiResponse = getAPIResponse(url, true);

                    itemPrices = apiResponse.GetValue(0) as Hashtable;

                    buyInfo = itemPrices["buys"] as Hashtable;

                    itemValue = int.Parse(buyInfo["unit_price"].ToString());
                }
                catch
                {
                    XmlElement newAccountBoundItemNode;
                    Hashtable itemDetails;

                    url = $"{apiPrefix}/items?id={item.id}";
                    apiResponse = getAPIResponse(url, true);

                    itemDetails = apiResponse.GetValue(0) as Hashtable;

                    newAccountBoundItemNode = xmlDatabase.CreateElement("Item");
                    newAccountBoundItemNode.SetAttribute("id", $"{item.id}");
                    newAccountBoundItemNode.InnerText = itemDetails["name"].ToString();

                    newAccountBoundItemNode = accountBoundItemsNode.AppendChild(newAccountBoundItemNode) as XmlElement;

                    xmlDatabase.Save(FileLocations.xmlDatabase());

                    itemValue = 0;
                }
            }
            
            await ReplyAsync($"1 x {itemSearchResults[0].Name} = {num2currency(itemValue)}");
        }
        
        [Command("daily")]
        [Alias("dailies")]
        [Summary("Gets information about the daily achievements. Options are pve, pvp, wvw or fractals")]
        private async Task DailyCommand(string type = "pve")
        {
            string url;
            Array apiResponse;
            Hashtable dailiesInfo;
            ArrayList pveDailies;
            string dailyIds = "";
            Boolean firstId = true;
            
            // Find the IDs of the 4 daily achievements for level 80 characters with HoT
            url = apiPrefix + "/achievements/daily";
            
            apiResponse = getAPIResponse(url, true);

            dailiesInfo = apiResponse.GetValue(0) as Hashtable;

            pveDailies = dailiesInfo[type] as ArrayList;

            foreach (Hashtable daily in pveDailies)
            {
                Hashtable levelInfo;
                ArrayList accessInfo;

                levelInfo = daily["level"] as Hashtable;
                accessInfo = daily["required_access"] as ArrayList;

                if (levelInfo["max"].ToString() == "80" && accessInfo.Contains("HeartOfThorns"))
                {
                    // Don't add a comma before the first item in the list
                    if (!firstId)
                    {
                        dailyIds += ",";
                    }
                    else
                    {
                        firstId = false;
                    }

                    dailyIds += daily["id"].ToString();
                }
            }

            url = apiPrefix + "/achievements?ids=" + dailyIds;

            apiResponse = getAPIResponse(url);
            
            // Build the response
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
            };

            foreach (Hashtable daily in apiResponse)
            {

                builder.AddField(x =>
                {
                    x.Name = daily["name"].ToString();
                    x.Value = daily["requirement"].ToString();
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
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

        private async Task<string> getUserAPIKey(IUser user)
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
                await ReplyAsync($"API key not found for {user.Username}. \n Go to https://account.arena.net/applications to get a key, then use the command gw registerkey [KEY] to register it with the bot");
            }

            apiElement = userDetails.SelectSingleNode("descendant::gwAPIKey");

            if (apiElement == null)
            {
                await ReplyAsync($"API key not found for {user.Username}. \n Go to https://account.arena.net/applications to get a key, then use the command gw registerkey [KEY] to register it with the bot");
            }

            apiKey = apiElement.InnerText;
            return apiKey;
        }

        private Boolean isItemAccountBount(int itemId)
        {
            IEnumerator accountBoundItemsEnumerator = accountBoundItemsNode.GetEnumerator();
            Boolean itemNodeExists = false;

            while (accountBoundItemsEnumerator.MoveNext())
            {
                XmlElement curNode = accountBoundItemsEnumerator.Current as XmlElement;

                if (int.Parse(curNode.GetAttribute("id")) == itemId)
                {
                    itemNodeExists = true;
                }
            }

            return itemNodeExists;
        }

        private string itemNameFromId(int itemId)
        {
            XmlDocument gwItemsDocument = new XmlDocument();
            gwItemsDocument.Load(FileLocations.gwItemNames());
            XmlElement gwRoot = gwItemsDocument.DocumentElement;
            IEnumerator itemsEnumerator = gwRoot.GetEnumerator();
            XmlNode itemNode = null;

            while (itemsEnumerator.MoveNext())
            {
                XmlElement curNode = itemsEnumerator.Current as XmlElement;

                if (curNode.GetAttribute("id") == itemId.ToString())
                {
                    itemNode = curNode;
                    break;
                }
            }

            return itemNode.InnerText;
        }

        public class itemResult
        {
            public string Name { get; set; }
            public string id { get; set; }
            public int similarity { get; set; }
        }

        private List<itemResult> itemSearch(string queryStr)
        {
            string itemName;
            Hashtable itemList = new Hashtable();
            int matchcount = 0;
            List<itemResult> results = new List<itemResult>();

            XmlDocument gwItemsDocument = new XmlDocument();
            gwItemsDocument.Load(FileLocations.gwItemNames());
            XmlElement gwRoot = gwItemsDocument.DocumentElement;
            IEnumerator itemsEnumerator = gwRoot.GetEnumerator();

            while (itemsEnumerator.MoveNext())
            {
                XmlElement curNode = itemsEnumerator.Current as XmlElement;

                itemList.Add(curNode.GetAttribute("id"), curNode.InnerText);
            }


            foreach (DictionaryEntry curItem in itemList)
            {
                //curGame = games[i] as Hashtable;
                itemName = Regex.Replace(curItem.Value.ToString().ToLower(), @"\s+", "");
                if (itemName.Contains(queryStr))
                {
                    matchcount++;
                    int similarity = LevenshteinDistance.Compute(itemName, queryStr);
                    itemResult resultClass = new itemResult
                    {
                        Name = curItem.Value.ToString(),
                        id = curItem.Key.ToString(),
                        similarity = similarity
                    };
                    results.Add(resultClass);
                }
            }

            if (results.Count == 0)
            {
                foreach (DictionaryEntry curItem in itemList)
                {
                    //curGame = games[i] as Hashtable;
                    itemName = Regex.Replace(curItem.Value.ToString().ToLower(), @"\s+", "");
                    matchcount++;
                    int similarity = LevenshteinDistance.Compute(itemName, queryStr);
                    if (similarity < 15)
                    {
                        itemResult resultClass = new itemResult
                        {
                            Name = curItem.Value.ToString(),
                            id = curItem.Key.ToString(),
                            similarity = similarity
                        };
                        results.Add(resultClass);
                    }
                }
            }
            return results;

        }

        private async Task<int> getAmountStored(int itemId, IUser user)
        {
            string apiKey;
            string url;
            Array materialStorage;
            int materialCount = 0;
            int amountStored = 0;

            if (user == null)
            {
                user = this.Context.Message.Author;
            }

            apiKey = await getUserAPIKey(user);
            url = apiPrefix + "/account/materials?access_token=" + apiKey;
            materialStorage = getAPIResponse(url);

            materialCount = materialStorage.Length;

            XmlDocument gwItemsDocument = new XmlDocument();
            gwItemsDocument.Load(FileLocations.gwItemNames());
            XmlElement gwRoot = gwItemsDocument.DocumentElement;
            Boolean fileIsNew = gwRoot.HasChildNodes;
            Boolean saveFile = false;

            for (int i = 0; i < materialCount; i++)
            {
                Hashtable curMaterial = materialStorage.GetValue(i) as Hashtable;
                XmlElement gwItem = null;

                if (fileIsNew)
                {
                    IEnumerator filteredEnumerator = gwItemsDocument.SelectNodes($"//Item[@id={curMaterial["id"].ToString()}]").GetEnumerator();
                    filteredEnumerator.MoveNext();
                    gwItem = filteredEnumerator.Current as XmlElement;
                }

                if (gwItem == null)
                {
                    string itemurl = apiPrefix + "/items?id=" + curMaterial["id"].ToString();
                    Hashtable itemDetails = getAPIResponse(itemurl, true).GetValue(0) as Hashtable;

                    gwItem = gwItemsDocument.CreateElement($"Item");
                    gwItem.SetAttribute("id", $"{curMaterial["id"].ToString()}");
                    gwItem.InnerText = itemDetails["name"].ToString();
                    gwRoot.AppendChild(gwItem);

                    saveFile = true;
                }

                if (int.Parse(curMaterial["id"].ToString()) == itemId)
                {
                    amountStored = int.Parse(curMaterial["count"].ToString());
                }
            }

            if (saveFile)
            {
                gwItemsDocument.Save(FileLocations.gwItemNames());
            }

            return amountStored;
        }

        private string num2currency(int rawAmount)
        {
            string currencyOutput = "0c";
            int gold = 0;
            int silver = 0;
            int copper = 0;
            
            copper = rawAmount % 100;
            rawAmount -= copper;

            silver = (rawAmount % 10000) / 100;
            rawAmount -= (silver * 100);

            gold = rawAmount / 10000;

            if (gold != 0)
            {
                currencyOutput = $"{gold}g{silver}s{copper}c";
            }
            else if (silver != 0)
            {
                currencyOutput = $"{silver}s{copper}c";
            }
            else
            {
                currencyOutput = $"{copper}c";
            }

            return currencyOutput;
        }
        #endregion

    }
}
