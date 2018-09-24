using CoOpBot.Database;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoOpBot.Modules
{
    [Group("User")]
    public class UserModule : ModuleBase
    {
        [Command("Info")]
        [Summary("Gets information about a user")]
        private async Task InfoCommand(IGuildUser user = null)
        {
            EmbedBuilder builder = new EmbedBuilder();
            User userRecord = new User();
            IEnumerable<IGuildUser> usersList = null;
            string userID;
            string name = "";


            if (user == null)
            {
                userID = this.Context.Message.Author.Id.ToString();

                usersList = await this.Context.Guild.GetUsersAsync();
                foreach (IGuildUser curUser in usersList)
                {
                    if (curUser.Id.ToString() == userID)
                    {
                        user = curUser;
                        break;
                    }
                }
            }
            else
            {
                userID = user.Id.ToString();
            }

            userRecord = userRecord.find(userID) as User;

            if (userRecord != null)
            {
                name = (user.Nickname == "" || user.Nickname == null) ? $"{user.Username}#{user.Discriminator}" : $"{user.Nickname} ({user.Username}#{user.Discriminator})";

                builder.Author = new EmbedAuthorBuilder()
                {
                    Name = $"{name}"
                };
                builder.ThumbnailUrl = user.GetAvatarUrl();

                if (userRecord.description != null && userRecord.description != "")
                {
                    builder.Description = userRecord.description;
                }

                if (userRecord.footerText != null && userRecord.footerText != "")
                {
                    if (userRecord.footerIconURL != null && userRecord.footerIconURL != "")
                    {
                        builder.Footer = new EmbedFooterBuilder()
                        {
                            Text = userRecord.footerText,
                            IconUrl = userRecord.footerIconURL
                        };
                    }
                    else
                    {
                        builder.Footer = new EmbedFooterBuilder()
                        {
                            Text = userRecord.footerText
                        };
                    }
                }

                if (userRecord.titleText != null && userRecord.titleText != "")
                {
                    builder.Title = userRecord.titleText;
                    if (userRecord.titleURL != null && userRecord.titleURL != "")
                    {
                        builder.Url = userRecord.titleURL;
                    }
                }

                if (userRecord.name != null && userRecord.name != "")
                {
                    builder.AddField("Name", userRecord.name);
                }
                if (userRecord.steamID != null && userRecord.steamID != "")
                {
                    builder.AddField("Steam Profile", $"{Steam.SteamModule.DisplayNameFromID(user.Id)}: https://steamcommunity.com/profiles/{userRecord.steamID}");
                }
                if (userRecord.OriginName != null && userRecord.OriginName != "")
                {
                    builder.AddField("Origin Profile", userRecord.OriginName);
                }
                if (userRecord.gwAPIKey != null && userRecord.gwAPIKey != "")
                {
                    builder.AddField("Guild Wars API Key", userRecord.gwAPIKey);
                }
            }
            else
            {
                await ReplyAsync($"User not found");
                return;
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("Name")]
        [Summary("Sets a name against your user record")]
        private async Task NameCommand(params string[] name)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.name = string.Join(" ", name);
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.name = string.Join(" ", name);
                user.insert();
            }

            await ReplyAsync("Name set");
        }

        [Command("Description")]
        [Summary("Sets a description against your user record")]
        private async Task DescriptionCommand(params string[] description)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.description = string.Join(" ", description);
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.description = string.Join(" ", description);
                user.insert();
            }

            await ReplyAsync("Description set");
        }

        [Command("Title")]
        [Summary("Sets title text against your user record")]
        private async Task TitleCommand(params string[] title)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.titleText = string.Join(" ", title);
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.description = string.Join(" ", title);
                user.insert();
            }

            await ReplyAsync("Title text set");
        }

        [Command("URL")]
        [Summary("Sets a title URL against your user record")]
        private async Task URLCommand(string url)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.titleURL = url;
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.titleURL = url;
                user.insert();
            }

            await ReplyAsync("Title URL set");
        }

        [Command("footer")]
        [Summary("Sets footer text against your user record")]
        private async Task FooterCommand(params string[] footerText)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.footerText = string.Join(" ", footerText);
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.footerText = string.Join(" ", footerText);
                user.insert();
            }

            await ReplyAsync("Footer text set");
        }

        [Command("Icon")]
        [Summary("Sets a footer icon URL against your user record")]
        private async Task IconCommand(string iconURL)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.footerIconURL = string.Join(" ", iconURL);
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.footerIconURL = string.Join(" ", iconURL);
                user.insert();
            }

            await ReplyAsync("Description set");
        }

        [Command("OriginName")]
        [Alias("Origin")]
        [Summary("Sets an Origin account name against your user record")]
        private async Task OriginNameCommand(string name)
        {
            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user.OriginName = name;
                user.update();
            }
            else
            {
                user = new User();
                user.userID = this.Context.Message.Author.Id;
                user.OriginName = name;
                user.insert();
            }

            await ReplyAsync("Origin account name set");
        }

        [Command("BattleNetName")]
        [Alias("BattleNet", "bnet")]
        [Summary("Sets a Battle Net account name against your user record")]
        private async Task BattleNetNameCommand(string name)
        {
            if (name.IndexOf('#') == -1)
            {
                await ReplyAsync("Please include the code at the end of your account (e.g AccoutnName#1234, not just AccountName)");
                return;
            }

            User user = new User();

            user = user.find(this.Context.Message.Author.Id.ToString()) as User;

            if (user != null)
            {
                user = new User();
                user.battleNetName = name;
                user.update();
            }
            else
            {
                user.userID = this.Context.Message.Author.Id;
                user.battleNetName = name;
                user.insert();
            }

            await ReplyAsync("Battle Net account name set");
        }
    }
}
