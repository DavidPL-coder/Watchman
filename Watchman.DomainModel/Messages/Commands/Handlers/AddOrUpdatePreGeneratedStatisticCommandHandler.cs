﻿using System.Linq;
using System.Threading.Tasks;

using Watchman.Cqrs;
using Watchman.Integrations.Database;

namespace Watchman.DomainModel.Messages.Commands.Handlers
{
    public class AddOrUpdatePreGeneratedStatisticCommandHandler : ICommandHandler<AddOrUpdatePreGeneratedStatisticCommand>
    {
        private readonly ISessionFactory _sessionFactory;

        public AddOrUpdatePreGeneratedStatisticCommandHandler(ISessionFactory sessionFactory)
        {
            this._sessionFactory = sessionFactory;
        }

        public async Task HandleAsync(AddOrUpdatePreGeneratedStatisticCommand command)
        {
            using var session = this._sessionFactory.CreateLite();
            var currentTimeRange = session.Get<PreGeneratedStatistic>()
                .FirstOrDefault(x =>
                x.ServerId == command.PreGeneratedStatistic.ServerId
                && x.UserId == command.PreGeneratedStatistic.UserId
                && x.ChannelId == command.PreGeneratedStatistic.ChannelId
                && x.Period == command.PreGeneratedStatistic.Period
                && x.TimeRange == command.PreGeneratedStatistic.TimeRange);
            if (currentTimeRange == null)
            {
                await session.AddAsync(command.PreGeneratedStatistic);
                return;
            }
            var version = currentTimeRange.Version;
            currentTimeRange.SetCount(command.PreGeneratedStatistic.Count);
            if(version == currentTimeRange.Version)
            {
                return;
            }
            await session.UpdateAsync(currentTimeRange);
        }
    }
}
