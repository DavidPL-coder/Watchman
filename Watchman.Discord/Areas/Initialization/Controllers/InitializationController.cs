﻿using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Initialization.Services;
using Watchman.DomainModel.Responses.Commands;
using Watchman.DomainModel.Responses.Queries;

namespace Watchman.Discord.Areas.Initialization.Controllers
{
    public class InitializationController : IController
    {
        private readonly IQueryBus _queryBus;
        private readonly ICommandBus _commandBus;
        private readonly MutedRoleInitService _mutedRoleInitService;

        public InitializationController(IQueryBus queryBus, ICommandBus commandBus, MutedRoleInitService mutedRoleInitService)
        {
            this._queryBus = queryBus;
            this._commandBus = commandBus;
            _mutedRoleInitService = mutedRoleInitService;
        }

        [AdminCommand]
        [DiscordCommand("init")]
        //[IgnoreForHelp] TODO
        public void Init(DiscordRequest request, Contexts contexts)
        {
            ResponsesInit();
            _mutedRoleInitService.InitForServer(contexts);
        }

        private void ResponsesInit()
        {
            var query = new GetResponsesQuery();
            var responsesInBase = _queryBus.Execute(query).Responses;

            if (!responsesInBase.Any())
            {
                var fileContent = File.ReadAllText(@"Framework/Commands/Responses/responses-configuration.json");
                var responsesToAdd = JsonConvert.DeserializeObject<IEnumerable<DomainModel.Responses.Response>>(fileContent);
                var command = new AddResponsesCommand(responsesToAdd);
                _commandBus.ExecuteAsync(command);
            }
        }
    }
}
