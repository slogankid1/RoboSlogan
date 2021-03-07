using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoboSlogan
{
    class RoboSlogan
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IAppSettings _appSettings;
        private string _token;

        public RoboSlogan(IConfigurationRoot config)
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _appSettings = new AppSettings(config);
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_appSettings)
                .BuildServiceProvider();

            _token = LoadToken();
            _client.Log += _client_Log;
        }

        public async Task RunBotAsync()
        {
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1); //keep bot awake. TODO: Use Cancellation Token to stop bot w/ StopBot() method: Task.Delay(-1, cancellationToken).
        }

        private string LoadToken()
        {
            //Prioritises appsettings.json Token value
            if (!string.IsNullOrEmpty(_appSettings.Token.Trim()))
                return _appSettings.Token;

            return System.IO.File.ReadAllText(@"token.txt");
        }

        private Task _client_Log(LogMessage arg)
        {
            Log.Information(arg.ToString());
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
                    Log.Error($"Error: {result.ErrorReason} >> {message.Author}: {message}");
                }
            }
        }
    }
}
