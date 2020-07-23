﻿using Devscord.DiscordFramework.Commons.Exceptions;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Watchman.Discord.Areas.Commons;
using Watchman.Discord.Areas.Protection.BotCommands;
using Watchman.DomainModel.Users;

namespace Watchman.Discord.Areas.Protection.Services
{
    public class MuteRequestParser
    {
        private readonly UsersService _usersService;
        private readonly Contexts _contexts;
        private readonly ParserTime _parserTime;

        public MuteRequestParser(UsersService usersService, Contexts contexts)
        {
            this._usersService = usersService;
            this._contexts = contexts;
            this._parserTime = new ParserTime();
        }

        public async Task<UserContext> GetUser(string mention)
        {
            if (mention == null || !mention.StartsWith("<@!") || !mention.EndsWith('>'))
            {
                throw new UserDidntMentionAnyUserException();
            }
            var userToMute = await this._usersService.GetUserByMentionAsync(this._contexts.Server, mention);
            if (userToMute == null)
            {
                throw new UserNotFoundException(mention);
            }
            return userToMute;
        }

        public MuteEvent GetMuteEvent(ulong userId, Contexts contexts, string reason, string timeAsString)
        {
            if (reason == null)
            {
                throw new NotEnoughArgumentsException();
            }
            var timeRange = this._parserTime.GetFutureTimeRange(timeAsString, defaultTime: TimeSpan.FromHours(1));
            return new MuteEvent(userId, timeRange, reason, contexts.Server.Id, contexts.Channel.Id);
        }
    }
}
