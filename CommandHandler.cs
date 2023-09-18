using System;
using Discord;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using UtilityBot.DTO;
using Discord.Rest;
using CSharp_CustomBot.Modules;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CSharp_CustomBot
{

    class CommandHandler
    {
        

        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private DiscordSocketClient _client;
        Utils _utils = new Utils();

        public CommandHandler(DiscordSocketClient client)
        {
            DynamicList.Start(client);
            _client = client;
            _utils.LoadSettings();
            _utils.SetGame(".help for commands", client);

            _client.GuildAvailable += Program._client_GuildAvailable;
            _client.JoinedGuild += Program._client_JoinedGuild;
            _client.LeftGuild += Program._client_LeftGuild;

            _commands = new CommandService();

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(),_services);
            _client.MessageReceived += _client_MessageReceived;

        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);

            //Here i am running the function that checks if user shall get a role
            _utils.RoleToChannelCheck(context);

            int argPost = 0;
            if (msg.HasCharPrefix(Program.botData.prefix, ref argPost))
            {
                var result = _commands.ExecuteAsync(context, argPost,_services);
                if (!result.Result.IsSuccess && result.Result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.Result.ErrorReason);
                }
                if (result.Result.IsSuccess)
                {
                    _utils.SaveSettings();
                }
                await Program.Log("Invoked " + msg + " in " + context.Channel + " with " + result.Result, ConsoleColor.Magenta);
            }
            else
            {
                await Program.Log(context.Channel + "-" + context.User.Username + " : " + msg, ConsoleColor.White);
            }

        }
    }
}

