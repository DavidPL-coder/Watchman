﻿using System.Linq;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Discord.Rest;
using Discord.WebSocket;

namespace Devscord.DiscordFramework.Middlewares.Factories
{
    public class DiscordServerContextFactory : IContextFactory<RestGuild, DiscordServerContext>
    {
        public DiscordServerContext Create(RestGuild restGuild)
        {
            var userFactory = new UserContextsFactory();
            var channelFactory = new ChannelContextFactory();

            var owner = userFactory.Create(restGuild.GetOwnerAsync().Result);
            var systemChannel = channelFactory.Create(restGuild.GetSystemChannelAsync().Result);
            var textChannels = restGuild.GetTextChannelsAsync().Result.Select(x => channelFactory.Create(x));

            return new DiscordServerContext(restGuild.Id, restGuild.Name, owner, systemChannel, textChannels);
        }
    }
}
