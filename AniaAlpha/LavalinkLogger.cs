using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace AniaAlpha
{
    class LavalinkLogger
    {
        private readonly Lavalink _lavalink;

        // CTOR injection
        public LavalinkLogger(Lavalink lavalink)
        {
            _lavalink = lavalink;
            lavalink.Log += OnLog;
        }

        // Configure logging
        private Task OnLog(LogMessage log)
        {
            Console.WriteLine($"{log}");
            return Task.CompletedTask;
        }
    }
}
