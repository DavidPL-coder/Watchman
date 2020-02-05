﻿using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Protection.Factories;
using Watchman.DomainModel.Mute.Commands;

namespace Watchman.Discord.Areas.Protection.Controllers
{
    public class MuteUserController : IController
    {
        private readonly ICommandBus _commandBus;
        private readonly MessagesServiceFactory _messagesServiceFactory;
        private readonly MuteServiceFactory _muteServiceFactory;

        public MuteUserController(ICommandBus commandBus, MessagesServiceFactory messagesServiceFactory, MuteServiceFactory muteServiceFactory)
        {
            this._commandBus = commandBus;
            this._messagesServiceFactory = messagesServiceFactory;
            this._muteServiceFactory = muteServiceFactory;
        }

        [DiscordCommand("mute")]
        public void MuteUser(DiscordRequest request, Contexts contexts)
        {
            var messagesService = _messagesServiceFactory.Create(contexts);
            var muteService = _muteServiceFactory.Create(contexts);

            muteService.MuteUser(request);
            _commandBus.ExecuteAsync(new AddMuteInfoToDbCommand(muteService.MuteEvent));
            messagesService.SendResponse(x => x.MutedUser(muteService.MutedUser, muteService.MuteEvent.TimeRange.End), contexts);

            //todo:delay
            muteService.UnmuteUser();
            messagesService.SendResponse(x => x.UnmutedUser(contexts.User), contexts);
        }
    }
}
