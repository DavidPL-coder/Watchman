﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Watchman.Discord.Contexts;
using Watchman.Discord.Framework.Architecture.Middlewares;

namespace Watchman.Discord.Middlewares
{
    public class ChannelMiddleware : IMiddleware<ChannelContext>
    {
        public Task<IDiscordContext> Process(SocketMessage data)
        {
            throw new NotImplementedException();
        }
    }
}
