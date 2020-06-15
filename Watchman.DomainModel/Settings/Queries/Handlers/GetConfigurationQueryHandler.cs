﻿using System.Linq;
using Watchman.Cqrs;
using Watchman.Integrations.MongoDB;

namespace Watchman.DomainModel.Settings.Queries.Handlers
{
    public class GetConfigurationQueryHandler : IQueryHandler<GetConfigurationQuery, GetConfigurationQueryResult>
    {
        private readonly ISessionFactory _sessionFactory;

        public GetConfigurationQueryHandler(ISessionFactory sessionFactory)
        {
            this._sessionFactory = sessionFactory;
        }

        public GetConfigurationQueryResult Handle(GetConfigurationQuery query)
        {
            using var session = this._sessionFactory.Create();
            var configuration = session.Get<Configuration>().FirstOrDefault() ?? Configuration.Default;
            return new GetConfigurationQueryResult(configuration);
        }
    }
}
