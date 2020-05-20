﻿using Devscord.DiscordFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Watchman.Cqrs;
using Watchman.DomainModel.CustomCommands.Queries;

namespace Watchman.Discord.Integration.DevscordFramework
{
    public class CustomCommandsLoader : ICustomCommandsLoader
    {
        private readonly IQueryBus _queryBus;

        public CustomCommandsLoader(IQueryBus queryBus)
        {
            this._queryBus = queryBus;
        }

        public async Task<List<CustomCommand>> GetCustomCommands()
        {
            var query = new GetCustomCommandsQuery();
            var commands = await this._queryBus.ExecuteAsync(query);
            var mapped = commands.CustomCommands.Select(x => new CustomCommand(x.CommandFullName, new Regex(x.CustomTemplateRegex, RegexOptions.Compiled), x.ServerId));
            return mapped.ToList();
        }
    }
}
