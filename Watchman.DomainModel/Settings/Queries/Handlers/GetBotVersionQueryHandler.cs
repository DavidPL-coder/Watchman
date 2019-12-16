﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watchman.Cqrs;

namespace Watchman.DomainModel.Settings.Queries.Handlers
{
    public class GetBotVersionQueryHandler : IQueryHandler<GetBotVersionQuery, GetBotVersionQueryResult>
    {
        private const string _fileName = "version.txt";

        public GetBotVersionQueryResult Handle(GetBotVersionQuery query)
        {
            try
            {
                var version = File.ReadAllText(_fileName);
                return new GetBotVersionQueryResult(version);
            }
            catch
            {
                return new GetBotVersionQueryResult("Plik version.txt nie został utworzony");
            }
        }
    }
}
