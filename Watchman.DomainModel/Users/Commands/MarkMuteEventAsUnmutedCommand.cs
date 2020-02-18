﻿using System;
using Watchman.Cqrs;

namespace Watchman.DomainModel.Users.Commands
{
    public class MarkMuteEventAsUnmutedCommand : ICommand
    {
        public Guid MuteEventGuid { get; }

        public MarkMuteEventAsUnmutedCommand(Guid muteEventGuid)
        {
            MuteEventGuid = muteEventGuid;
        }
    }
}
