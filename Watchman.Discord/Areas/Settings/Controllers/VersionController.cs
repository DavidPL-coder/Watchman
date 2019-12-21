﻿using System.Collections.Generic;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Watchman.Cqrs;
using Watchman.DomainModel.Settings.Queries;

namespace Watchman.Discord.Areas.Settings.Controllers
{
    public class VersionController : IController
    {
        private readonly IQueryBus queryBus;
        private readonly ICommandBus commandBus;
        private readonly MessagesServiceFactory messagesServiceFactory;

        public VersionController(IQueryBus queryBus, ICommandBus commandBus, MessagesServiceFactory messagesServiceFactory)
        {
            this.queryBus = queryBus;
            this.commandBus = commandBus;
            this.messagesServiceFactory = messagesServiceFactory;
        }

        [AdminCommand]
        [DiscordCommand("version")]
        public void PrintVersion(DiscordRequest request, Contexts contexts)
        {
            var version = queryBus.Execute(new GetBotVersionQuery()).Version;
            var messagesService = messagesServiceFactory.Create(contexts);
            messagesService.SendResponse(x => x.CurrentVersion(contexts, version), contexts);
        }
    }
}
