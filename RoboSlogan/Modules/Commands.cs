using Discord.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboSlogan.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            Log.Information("Recieved Command Ping");
            await ReplyAsync("Pong");
        }
    }
}
