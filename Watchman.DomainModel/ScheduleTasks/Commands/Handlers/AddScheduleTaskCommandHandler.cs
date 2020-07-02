﻿using System.Threading.Tasks;
using Watchman.Cqrs;
using Watchman.Integrations.MongoDB;

namespace Watchman.DomainModel.ScheduleTasks.Commands.Handlers
{
    public class AddScheduleTaskCommandHandler : ICommandHandler<AddScheduleTaskCommand>
    {
        private readonly ISessionFactory _sessionFactory;

        public AddScheduleTaskCommandHandler(ISessionFactory sessionFactory)
        {
            this._sessionFactory = sessionFactory;
        }

        public async Task HandleAsync(AddScheduleTaskCommand command)
        {
            var scheduleTask = new ScheduleTask(command.CommandName, command.Arguments, command.ExecutionDate);
            using var session = this._sessionFactory.Create();
            await session.AddAsync(scheduleTask);
        }
    }
}
