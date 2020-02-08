﻿using System.Threading.Tasks;
using Watchman.Cqrs;
using Watchman.Integrations.MongoDB;

namespace Watchman.DomainModel.Mute.Commands.Handlers
{
    public class AddMuteEventToBaseCommandHandler : ICommandHandler<AddMuteEventToBaseCommand>
    {
        private readonly ISessionFactory _sessionFactory;

        public AddMuteEventToBaseCommandHandler(ISessionFactory sessionFactory)
        {
            this._sessionFactory = sessionFactory;
        }

        public async Task HandleAsync(AddMuteEventToBaseCommand command)
        {
            using var session = _sessionFactory.Create();
            await Task.Run(() => session.Add(command.MuteEvent));
        }
    }
}
