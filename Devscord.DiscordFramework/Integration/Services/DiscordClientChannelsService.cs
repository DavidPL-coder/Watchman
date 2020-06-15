﻿using Devscord.DiscordFramework.Middlewares.Contexts;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework.Commands.Parsing;
using Devscord.DiscordFramework.Middlewares.Factories;
using Devscord.DiscordFramework.Services.Models;
using Discord.Rest;
using Serilog;
using Devscord.DiscordFramework.Integration.Services.Interfaces;

namespace Devscord.DiscordFramework.Integration.Services
{
    internal class DiscordClientChannelsService : IDiscordClientChannelsService
    {
        private DiscordSocketRestClient _restClient => _client.Rest;
        private readonly DiscordSocketClient _client;
        private readonly IDiscordClientUsersService _discordClientUsersService;

        public DiscordClientChannelsService(DiscordSocketClient client, IDiscordClientUsersService discordClientUsersService)
        {
            this._client = client;
            this._discordClientUsersService = discordClientUsersService;
        }

        public async Task SendDirectMessage(ulong userId, string message)
        {
            var user = await _discordClientUsersService.GetUser(userId);
            await user.SendMessageAsync(message);
        }

        public async Task SendDirectEmbedMessage(ulong userId, Embed embed)
        {
            var user = await _discordClientUsersService.GetUser(userId);
            await user.SendMessageAsync(embed: embed);
        }

        public async Task<IChannel> GetChannel(ulong channelId, RestGuild guild = null)
        {
            if (guild != null)
            {
                return await guild.GetChannelAsync(channelId);
            }

            IChannel channel;
            try
            {
                channel = await _restClient.GetChannelAsync(channelId);
            }
            catch
            {
                Log.Warning($"RestClient couldn't get channel: {channelId}");
                channel = _client.GetChannel(channelId);
            }
            return channel;
        }

        public async Task<IEnumerable<Message>> GetMessages(DiscordServerContext server, ChannelContext channel, int limit, ulong fromMessageId = 0, bool goBefore = true)
        {
            var textChannel = (ITextChannel)this.GetChannel(channel.Id).Result;
            if (!this.CanBotReadTheChannel(textChannel))
            {
                return new List<Message>();
            }

            IEnumerable<IMessage> channelMessages;
            if (fromMessageId == 0)
            {
                channelMessages = await textChannel.GetMessagesAsync(limit).FlattenAsync();
            }
            else
            {
                channelMessages = await textChannel.GetMessagesAsync(fromMessageId, goBefore ? Direction.Before : Direction.After, limit).FlattenAsync();
            }

            var userFactory = new UserContextsFactory();
            var messages = channelMessages.Select(message =>
            {
                var user = userFactory.Create(message.Author);
                var contexts = new Contexts();
                contexts.SetContext(server);
                contexts.SetContext(channel);
                contexts.SetContext(user);

                var commandParser = new CommandParser();
                var request = commandParser.Parse(message.Content, message.Timestamp.UtcDateTime);
                return new Message(message.Id, request, contexts);
            });
            return messages;
        }

        public bool CanBotReadTheChannel(IMessageChannel textChannel)
        {
            try
            {
                textChannel.GetMessagesAsync(limit: 1).FlattenAsync().Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}