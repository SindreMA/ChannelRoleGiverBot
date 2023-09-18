using CSharp_CustomBot.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharp_CustomBot.Modules
{

    public class Commands : ModuleBase<SocketCommandContext>
    {
        Utils _utils = new Utils();

        [Command("ConnectRoleToChannel")]
        public async Task ConnectRoleToChannel( ulong RoleID, ulong ChannelID)
        {
            if (!Context.Guild.Roles.Any(x=> RoleID == x.Id))
            {
                await _utils.SendEmededMessage(Context.Channel as SocketTextChannel, "Cant find any roles with that ID");
            }
            else if (!Context.Guild.TextChannels.Any(x=> x.Id == ChannelID))
            {
                await _utils.SendEmededMessage(Context.Channel as SocketTextChannel, "Cant find any text channels with that ID");
            }
            else
            {
                if (!Program.botData.GuildData.Exists(x => x.GuildID == Context.Guild.Id && x.RolesToChannelList.Exists(c => c.ChannelID == ChannelID && c.RoleID == RoleID)))
                {
                    var settings = _utils.GetSettings(Context.Guild.Id);

                    settings.RolesToChannelList.Add(new Program.RoleToChannel() { ChannelID = ChannelID, RoleID = RoleID });
                    await _utils.SendEmededMessage(Context.Channel as SocketTextChannel, "Connection added!");

                }
                else
                {
                    await _utils.SendEmededMessage(Context.Channel as SocketTextChannel, "Connection already exist!");
                }
            }
        }

        [Command("help")]
        public async Task help()
        {
            var eb = _utils.GetEmededMessage("Rolebot Command list ");
            eb.Description = ("([]) = Optinal   [] = Required");
            eb.AddField($@"{Program.botData.prefix}help", "Shows this message");
            eb.AddField($@"{Program.botData.prefix}ConnectRoleToChannel [RoleID] [ChannelID]","Gives role to user when typing in the channel");
            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }
        [RequireOwner]
        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {Context.User.Username}\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {_utils.GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {_utils.GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }
       
    }
}
