using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharp_CustomBot;
using static CSharp_CustomBot.Program;
using Discord.Commands;

namespace CSharp_CustomBot.Modules
{
    public class Utils
    {

        public string GetUptime()
        => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        public string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private string SettingsFile = @"Settings.json";
        public async Task SendEmededMessage(SocketTextChannel channel, string text, string description, Discord.Color color)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = color;
            eb.Description = description;
            eb.Title = text;

            await channel.SendMessageAsync("", false, eb.Build());
        }
        public EmbedBuilder GetEmededMessage(string title)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.Orange;
            eb.Title = title;
            return eb;
        }
        public async Task SendEmededMessage(SocketTextChannel channel, string text)
        {
            await SendEmededMessage(channel, text, "", Color.Orange);
        }
        public async Task SendEmededMessage(SocketTextChannel channel, string text, Color color)
        {
            await SendEmededMessage(channel, text, "", color);
        }
        public async Task SendEmededMessage(SocketTextChannel channel, string text, string description)
        {
            await SendEmededMessage(channel, text, description, Color.Orange);
        }

        internal void RoleToChannelCheck(SocketCommandContext context)
        {
            var guildData = GetSettings(context.Guild.Id);

            if (!context.User.IsBot)
            {
                if (guildData.RolesToChannelList.Any(x => x.ChannelID == context.Channel.Id))
                {
                    var item = guildData.RolesToChannelList.FirstOrDefault(x => x.ChannelID == context.Channel.Id);
                    if (context.Guild.Roles.Any(x => x.Id == item.RoleID))
                    {
                        var role = context.Guild.Roles.FirstOrDefault(x => x.Id == item.RoleID);
                        if (!role.Members.Any(x => x.Id == context.User.Id))
                        {
                            (context.User as SocketGuildUser).AddRoleAsync(role);
                            Program.Log($@"User {context.User.Username} have been added to role {role.Name}",ConsoleColor.Green);
                        }
                    }
                }
            }
        }

        public bool DoesGuildHaveSettings(ulong id)
        {
            if (Program.botData.GuildData.Exists(x => x.GuildID == id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal GuildData GetSettings(ulong id)
        {
            if (DoesGuildHaveSettings(id))
            {
                var returnitem = Program.botData.GuildData.SingleOrDefault(x => x.GuildID == id);
                if (returnitem.RolesToChannelList == null)
                {
                    returnitem.RolesToChannelList = new List<RoleToChannel>();
                }
                return returnitem;
            }
            else
            {
                GuildData guildSettings = new GuildData()
                {
                    GuildID = id,
                    RolesToChannelList = new List<RoleToChannel>()
                };
                Program.botData.GuildData.Add(guildSettings);
                return Program.botData.GuildData.SingleOrDefault(x => x.GuildID == id);

            }

        }
        public int MinSince(DateTime time)
        {
            DateTime when = time;
            TimeSpan ts = DateTime.Now.Subtract(when);
            return (int)ts.TotalMinutes;
        }
        public void SaveSettings()
        {
            try
            {
                File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(Program.botData, Formatting.Indented));
            }
            catch (Exception)
            {
            }
        }
        public void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                Program.botData = JsonConvert.DeserializeObject<BotData>(File.ReadAllText(SettingsFile));
            }
            else
            {
                Program.Log("Data file not found!", ConsoleColor.Red);
                BotData sample = new BotData();
                sample.token = "YOUR BOT TOKEN HERE";
                sample.GuildData = new List<GuildData>();
                sample.prefix = '.';
                Program.botData = sample;
                SaveSettings();
                Program.Log("A sample data file have been created!", ConsoleColor.Red);
                Program.Log("Please add your token to it and restart the bot", ConsoleColor.Red);

                throw new Exception("Data file not found");
            }
        }
        public async Task SetGame(string text, DiscordSocketClient client)
        {
            await client.SetGameAsync(text);
        }
    }
}
