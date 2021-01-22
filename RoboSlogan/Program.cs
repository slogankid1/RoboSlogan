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

        static void Main(string[] args) 
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Serilog")
                .CreateLogger();

            try
            {
                new RoboSlogan(config).RunBotAsync().GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                Log.Information(e.Message);
            }
        }


    }
}
