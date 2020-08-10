﻿using Devscord.DiscordFramework.Commons.Exceptions;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services;
using Devscord.DiscordFramework.Services.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Watchman.Common.Models;
using Watchman.Discord.Areas.Protection.BotCommands;
using Watchman.Discord.Areas.Protection.Services;
using Watchman.DomainModel.Users;

namespace Watchman.Discord.Areas.Protection.Controllers
{
    public class MuteUserController : IController
    {
        private readonly MutingService _mutingService;
        private readonly UnmutingService _unmutingService;
        private readonly UsersService _usersService;
        private readonly MessagesServiceFactory _messagesServiceFactory;

        public MuteUserController(MutingService mutingService, UnmutingService unmutingService, UsersService usersService, MessagesServiceFactory messagesServiceFactory)
        {
            this._mutingService = mutingService;
            this._unmutingService = unmutingService;
            this._usersService = usersService;
            this._messagesServiceFactory = messagesServiceFactory;
        }

        [AdminCommand]
        public async Task MuteUser(MuteCommand command, Contexts contexts)
        {
            var userToMute = await this._usersService.GetUserByIdAsync(contexts.Server, command.User);
            if (userToMute == null)
            {
                await this._messagesServiceFactory.Create(contexts).SendResponse(x => x.UserNotFound($"<@!{command.User}>"));
                return;
            }
            var timeRange = TimeRange.FromNow(DateTime.UtcNow + command.Time);
            var muteEvent = new MuteEvent(userToMute.Id, timeRange, command.Reason, contexts.Server.Id, contexts.Channel.Id);
            await this._mutingService.MuteUserOrOverwrite(contexts, muteEvent, userToMute);
            this._unmutingService.UnmuteInFuture(contexts, muteEvent, userToMute);
        }

        [AdminCommand]
        public async Task UnmuteUserAsync(UnmuteCommand command, Contexts contexts)
        {
            var userToUnmute = await this._usersService.GetUserByIdAsync(contexts.Server, command.User);
            if (userToUnmute == null)
            {
                await this._messagesServiceFactory.Create(contexts).SendResponse(x => x.InvalidArguments());
                return;
            }
            await this._unmutingService.UnmuteNow(contexts, userToUnmute);
        }
    }
}
