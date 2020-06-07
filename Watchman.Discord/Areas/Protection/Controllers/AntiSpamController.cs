﻿using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.AntiSpam.Models;
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
        private readonly ServerMessagesCacheService _serverMessagesCacheService;

        public AntiSpamController(AntiSpamService antiSpamService, UserMessagesCountService userMessagesCountService)
        {
            this._antiSpamService = antiSpamService;
            _userMessagesCountService = userMessagesCountService;
            this._serverMessagesCacheService = new ServerMessagesCacheService();
        }

        [ReadAlways]
        public Task Scan(DiscordRequest request, Contexts contexts)
        {
            Log.Information("Started scanning the message");
            _serverMessagesCacheService.AddMessage(request, contexts);

            Log.Information("Scanned");
            return Task.CompletedTask;
        }
    }
}
