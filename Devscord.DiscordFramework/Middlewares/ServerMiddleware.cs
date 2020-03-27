﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework;
using Devscord.DiscordFramework.Framework.Architecture.Middlewares;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Middlewares.Factories;
using Discord.Rest;
using Discord.WebSocket;

namespace Devscord.DiscordFramework.Middlewares
{
    public class ServerMiddleware : IMiddleware<DiscordServerContext>
    {
        private readonly DiscordServerContextFactory discordServerContextsFactory;

        public ServerMiddleware()
        {
            this.discordServerContextsFactory = new DiscordServerContextFactory();
        }

        public DiscordServerContext Process(SocketMessage data)
        {
            var serverInfo = ((SocketGuildChannel)data.Channel).Guild;
            var guild = Server.GetGuild(serverInfo.Id).Result;
            return this.discordServerContextsFactory.Create(guild);
        }
    }
}
