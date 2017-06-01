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


namespace CoOpBot.Modules.GuildWars
{
    [Group("gw")]
    public class GuildWarsModule : ModuleBase
    {
        XmlDocument xmlParameters = new XmlDocument();
        XmlNode root;
        XmlNode usersNode;
        string apiPrefix;

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
        }

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
        [Summary("Finds the amount of gold your character has")]
        private async Task GoldCountCommand()
        {
            IEnumerator usersEnumerator = usersNode.GetEnumerator();
            Boolean userNodeExists = false;
            XmlElement userDetails = null;
            XmlNode apiElement;
            string apiKey;
            string url;
            int gold, silver, copper, rawAmount;

            gold = 0;
            silver = 0;
            copper = 0;
            rawAmount = 0;

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
                await ReplyAsync("You do not have an API key registered with the bot");
            }


            apiElement = userDetails.SelectSingleNode("descendant::gwAPIKey");

            if (apiElement == null)
            {
                await ReplyAsync("You do not have an API key registered with the bot");
            }


            apiKey = apiElement.InnerText;

            url = apiPrefix + "/account/wallet?access_token=" + apiKey;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    var jsonResponse = wc.DownloadString(url);

                    ArrayList decoded = JSON.JsonDecode(jsonResponse) as ArrayList;

                    Array decodedArray = decoded.ToArray();
                    Hashtable test = decodedArray.GetValue(0) as Hashtable;

                    rawAmount = int.Parse(test["value"].ToString());
                }
                catch (Exception ex)
                {
                    await ReplyAsync("Invalid API key");
                }

            }

            copper = rawAmount % 100;
            rawAmount -= copper;

            silver = (rawAmount % 10000)/100;
            rawAmount -= (silver * 100);

            gold = rawAmount/ 10000;


            await ReplyAsync(string.Format("You have {0}g{1}s{2}c", gold, silver, copper));

        }
    }
}
