﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Watchman.Cqrs;

namespace Watchman.DomainModel.Users.Commands.Handlers
{
    public class RemoveUserRoleCommandHandler : ICommandHandler<RemoveUserRoleCommand>
    {
        public Task HandleAsync(RemoveUserRoleCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
