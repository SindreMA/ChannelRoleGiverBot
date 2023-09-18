using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using CSharp_CustomBot.Modules;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Globalization;
using Discord.Commands;
using System.Net.Http;
using System.Collections.Generic;

namespace CSharp_CustomBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();
        private CommandHandler _handler;
        private DiscordSocketClient _client;
        ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
        public async Task StartAsync()
        {
            try
            {
                Log("Getting bot data from file...", ConsoleColor.Green);
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                var _utils = new Utils();
                _utils.LoadSettings();

                using (ServiceProvider services = ConfigureServices())
                {
                    var client = services.GetRequiredService<DiscordSocketClient>();
                    Log("Logging in...", ConsoleColor.Green);
                    await client.LoginAsync(TokenType.Bot, botData.token);
                    Log("Starting bot...", ConsoleColor.Green);
                    await client.StartAsync();
                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await Task.Delay(-1);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR LOGGING BOT IN, CHECK YOUR TOKEN KEY", ConsoleColor.Red);
                Console.ReadLine();
            }
        }
        public class RoleToChannel
        {
            public ulong RoleID { get; set; }
            public ulong ChannelID { get; set; }
        }
        public class GuildData
        {
            public ulong GuildID { get; set; }
            public List<RoleToChannel> RolesToChannelList { get; set; }
        }
        public class BotData
        {
            public char prefix { get; set; }
            public string token { get; set; }
            public List<GuildData> GuildData { get; set; }
        }
        public static BotData botData = new BotData();


        public static async Task _client_LeftGuild(SocketGuild arg)
        {
            if (Program.botData.GuildData.Any(x => x.GuildID == arg.Id))
            {
                var settings = Program.botData.GuildData.Where(x => x.GuildID == arg.Id);
                foreach (var setting in settings)
                {
                    Program.botData.GuildData.Remove(setting);
                }
                var _util = new Utils();
                _util.SaveSettings();
            }
        }

        public static async Task _client_JoinedGuild(SocketGuild arg)
        {
            var _utils = new Utils();
            var settings = _utils.GetSettings(arg.Id);
            _utils.SaveSettings();
        }

        public static async Task _client_GuildAvailable(SocketGuild arg)
        {
                await Log(arg.Name + " Connected!", ConsoleColor.Green);
        }
        public static async Task Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(DateTime.Now +" : " + message, color);
            FileInfo f = null;
            if (File.Exists("Log.txt"))
            {
                f = new FileInfo("Log.txt");
            }
            if ( f != null && f.Length > 25052672)
            {
                System.IO.File.Move("Log.txt", "Log - "+ DateTime.Now.ToShortDateString()+".txt");
                System.Threading.Thread.Sleep(2000);
                File.AppendAllText("Log.txt", DateTime.Now + " : " + message + Environment.NewLine);
            }
            else
            {
                File.AppendAllText("Log.txt", DateTime.Now + " : " + message + Environment.NewLine);
            }
            Console.ResetColor();
        }
    }
}