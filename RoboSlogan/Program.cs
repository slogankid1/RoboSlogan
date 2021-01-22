using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace RoboSlogan
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IAppSettings _appSettings;
        public Serilog.Core.Logger _logger;

        static void Main(string[] args) 
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var logConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Serilog")
                .CreateLogger();

            logConfig.Information("Hello, world!");

            //var log = new SerilogTraceListener.SerilogTraceListener();
            //Trace.Listeners.Add(log);
            //var logConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            ////logConfig.WriteTo.Console();
            //Log.Logger = logConfig.CreateLogger();

            try
            {
                new Program().RunBotAsync(config).GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                Log.Information(e.Message);
                Console.WriteLine(e.Message);
            }
        }

        public async Task RunBotAsync(
            IConfigurationRoot config
            )
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _appSettings = new AppSettings(config);
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton( _appSettings)
                .BuildServiceProvider();

            string token = LoadToken();//_appSettings.Token;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1); //keep bot awake
        }

        private string LoadToken()
        {
            if (!string.IsNullOrEmpty(_appSettings.Token.Trim())) 
                return _appSettings.Token;

            return System.IO.File.ReadAllText(@"token.txt");

        }

        private Task _client_Log(LogMessage arg)
        {
            Log.Information(arg.ToString());
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return; //if a bot sends a message, our bot doesn't respond

            int argPos = 0; //where the special char lies in message
            if (message.HasStringPrefix(_appSettings.SpecialChar.ToString(), ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    Console.WriteLine($"{DateTime.Now} Error: {result.ErrorReason}");
                    Console.WriteLine($"\t{message.Author}: {message}");


                    //await ReplyAsync($"Error: {result.ErrorReason}");
                }
            }
        }
    }
}
