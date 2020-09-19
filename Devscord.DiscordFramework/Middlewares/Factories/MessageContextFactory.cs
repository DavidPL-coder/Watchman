﻿using Autofac;
using Devscord.DiscordFramework.Framework.Commands.Parsing;
using Devscord.DiscordFramework.Middlewares.Contexts;
using System;

namespace Devscord.DiscordFramework.Middlewares.Factories
{
    internal class MessageContextFactory
    {
        private CommandParser _commandParser;

        public MessageContextFactory(CommandParser commandParser)
        {
            this._commandParser = commandParser;
        }

        public MessageContext Create(DateTime sentAt, string message)
        {
            bool IsBotCommand = this._commandParser.Parse(message, sentAt).IsCommandForBot;

            return new MessageContext(sentAt, IsBotCommand);
        }
    }
}
