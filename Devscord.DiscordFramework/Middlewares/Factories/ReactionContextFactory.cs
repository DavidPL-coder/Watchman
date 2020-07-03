﻿using System;
using Discord;
using Discord.WebSocket;
using Devscord.DiscordFramework.Middlewares.Contexts;

namespace Devscord.DiscordFramework.Middlewares.Factories
{
    internal class ReactionContextFactory : IContextFactory<(SocketReaction socketReaction, IUserMessage userMessage), ReactionContext>
    {
        private readonly ChannelContextFactory _channelContextFactory;
        private readonly UserContextsFactory _userContextsFactory;
        private readonly MessageContextFactory _messageContextFactory;
        private readonly RestGuildGetterService _restGuildGetterService;
        private readonly DiscordServerContextFactory _discordServerContextsFactory;

        public ReactionContextFactory(ChannelContextFactory channelContextFactory, UserContextsFactory userContextsFactory, MessageContextFactory messageContextFactory, RestGuildGetterService restGuildGetterService, DiscordServerContextFactory discordServerContextsFactory)
        {
            this._channelContextFactory = channelContextFactory;
            this._userContextsFactory = userContextsFactory;
            this._messageContextFactory = messageContextFactory;
            this._restGuildGetterService = restGuildGetterService;
            this._discordServerContextsFactory = discordServerContextsFactory;
        }

        public ReactionContext Create((SocketReaction socketReaction, IUserMessage userMessage) reactionAndUserMessage)
        {
            var guild = _restGuildGetterService.GetRestGuild(reactionAndUserMessage.socketReaction.Channel);
            var discordServerContext = this._discordServerContextsFactory.Create(guild);
            var channelContext = this._channelContextFactory.Create(reactionAndUserMessage.socketReaction.Channel);
            var userContext = this._userContextsFactory.Create(reactionAndUserMessage.socketReaction.User.Value);

            var contexts = new Contexts.Contexts();
            contexts.SetContext(discordServerContext);
            contexts.SetContext(channelContext);
            contexts.SetContext(userContext);

            var messageContext = this._messageContextFactory.Create(reactionAndUserMessage.userMessage);
            
            return new ReactionContext(contexts, messageContext, reactionAndUserMessage.socketReaction.Emote.Name, reactedAt: DateTime.Now);
        }
    }
}
