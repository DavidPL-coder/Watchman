﻿using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Serilog;
using Watchman.Discord.Areas.Protection.Services;
using Watchman.DomainModel.Users;

namespace Watchman.Discord.Areas.Protection.Controllers
{
    public class AntiSpamController : IController
    {
        private readonly AntiSpamService _antiSpamService;
        private readonly UserMessagesCountService _userMessagesCountService;
        private readonly SpamDetectingStrategy _strategy;

        public AntiSpamController(AntiSpamService antiSpamService, UserMessagesCountService userMessagesCountService)
        {
            this._antiSpamService = antiSpamService;
            _userMessagesCountService = userMessagesCountService;
            this._strategy = new SpamDetectingStrategy();
        }

        [ReadAlways]
        public Task Scan(DiscordRequest request, Contexts contexts)
        {
            this._antiSpamService.AddUserMessage(contexts, request);
            var messagesInShortTime = _antiSpamService.CountUserMessagesShorterTime(contexts.User.Id);
            var messagesInLongTime = _antiSpamService.CountUserMessagesLongerTime(contexts.User.Id);
            var userWarnsInLastFewMinutes = _antiSpamService.CountUserWarnsInShortTime(contexts.User.Id);
            var userWarnsInLastFewHours = _antiSpamService.CountUserWarnsInLongTime(contexts.User.Id);
            var userMutesInLastFewHours = _antiSpamService.CountUserMutesInLongTime(contexts.User.Id);
            var userMessages = _userMessagesCountService.CountMessages(contexts.User.Id, contexts.Server.Id);

            Log.Information($"Warns in few minutes: {userWarnsInLastFewMinutes}; in few hours: {userWarnsInLastFewHours}");
            Log.Information($"Mutes in few hours: {userMutesInLastFewHours}");

            var punishment = _strategy.SelectPunishment(userWarnsInLastFewMinutes, userWarnsInLastFewHours, userMutesInLastFewHours, messagesInShortTime, messagesInLongTime, userMessages);
            _antiSpamService.SetPunishment(contexts, punishment);
            Log.Information("Scanned");
            return Task.CompletedTask;
        }
    }
}
