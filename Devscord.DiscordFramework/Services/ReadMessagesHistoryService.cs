﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;

namespace Devscord.DiscordFramework.Services
{
    public class ReadMessagesHistoryService
    {
        public async Task<IEnumerable<(Contexts contexts, DiscordRequest request, DateTime createdAt)>> ReadMessagesAsync(DiscordServerContext server)
        {
            var messages = new List<IEnumerable<(Contexts, DiscordRequest, DateTime)>>();
            var textChannels = server.TextChannels;

            foreach (var channel in textChannels)
            {
                var channelMessages = await Server.GetMessages(server, channel);
                messages.Add(channelMessages);
            }
            return messages.SelectMany(x => x);
        }
    }
}
