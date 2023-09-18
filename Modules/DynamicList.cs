using Discord;
using Discord.Addons.EmojiTools;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CSharp_CustomBot.Modules
{

    public class DynamicList
    {
        private static DiscordSocketClient _client;
        private static List<ListElements> Lists = new List<ListElements>();
        public static void Start(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += _client_ReactionAdded;
        }


        private static async Task _client_ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {


            if (Lists.Exists(x => x.messageID == arg1.Id) && arg3.UserId != _client.CurrentUser.Id && !arg3.User.Value.IsBot)
            {
                var msg = arg2.GetMessageAsync(arg1.Id).Result as IUserMessage;
                await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);

                var item = Lists.Find(x => x.messageID == arg1.Id);
                bool positive = item.from > 0;
                bool NotHigherThanCount = item.to < (item.list.Count());
                if (arg3.Emote.Name == "▶")
                {
                 
                    if (NotHigherThanCount)
                    {
                        item.from = item.from + 20;
                        item.to = item.to + 20;
                        if (item.Embeded)
                        {

                            await msg.ModifyAsync(x => x.Embed =  new EmbedBuilder() { Title = item.Title, Description = CreateList(item.list, item.from, item.to, item.CodeFormat) }.Build());
                        }
                        else
                        {
                            await msg.ModifyAsync(x => x.Content = item.Title + Environment.NewLine + Environment.NewLine + CreateList(item.list, item.from, item.to, item.CodeFormat));
                        }
                    }
                }
                else if (arg3.Emote.Name == "◀")
                {
                  
                    if (positive)
                    {
                        item.from = item.from - 20;
                        item.to = item.to - 20;
                        await msg.ModifyAsync(x => x.Content = item.Title + Environment.NewLine + Environment.NewLine + CreateList(item.list, item.from, item.to,item.CodeFormat));
                    }
                }
            }
        }


        private static string CreateList(List<string> items, int from = 0, int to = 10, bool Codeformat = false)
        {

            string message = "";

            if (Codeformat) message = message + "```";
            int TO = 10;
            if (items.Count < 10) TO = items.Count;
            else if (from + 10 > items.Count) TO = int.Parse(items.Count.ToString().Remove(0, 1));
            int listspot = from + 1;
            foreach (var item in items.GetRange(from, TO))
            {

                message = message + $"[{listspot++}] " + item + Environment.NewLine;
            }
            if (Codeformat) message = message + "```";
            return message;
        }
        public static void NewList(List<string> Items, ISocketMessageChannel channel, string Title, bool Codeformat =false, bool embeded = false)
        {

            var list = new ListElements()
            {
                list = Items,
                msg_channelID = channel.Id,
                from = 0,
                to = 10,
                Title = Title,
                CodeFormat = Codeformat

            };
            RestUserMessage msg = null;
            if (embeded)
            {
                msg = channel.SendMessageAsync("",false , new EmbedBuilder() { Title = Title, Description = CreateList(Items, Codeformat: Codeformat) }.Build()).Result;
            }
            else
            {
                msg = channel.SendMessageAsync(Title + Environment.NewLine + Environment.NewLine + CreateList(Items, Codeformat: Codeformat)).Result;

            }
            if (Items.Count > 10)
            {
                msg.AddReactionAsync(EmojiExtensions.FromText("arrow_backward"));
                msg.AddReactionAsync(EmojiExtensions.FromText("arrow_forward"));
            }
            list.messageID = msg.Id;
            Lists.Add(list);

        }
    }

    public class ListElements
    {
        public string Title { get; set; }
        public ulong messageID { get; set; }
        public ulong msg_channelID { get; set; }
        public List<string> list { get; set; }
        public int from { get; set; }
        public int to { get; set; }
        public bool CodeFormat { get; set; }
        public bool Embeded { get; set; }
    }
}
