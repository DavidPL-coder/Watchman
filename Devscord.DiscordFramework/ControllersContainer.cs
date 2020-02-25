﻿using Devscord.DiscordFramework.Commons.Extensions;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Devscord.DiscordFramework
{
    internal class ControllersContainer
    {
        public IReadOnlyList<ControllerInfo> WithReadAlways { get; private set; }
        public IReadOnlyList<ControllerInfo> WithDiscordCommand { get; private set; }

        public ControllersContainer(IReadOnlyList<ControllerInfo> controllers)
        {
            this.WithReadAlways = controllers
                .Select(x => new ControllerInfo(x.Controller, x.Methods.Where(m => m.HasAttribute<ReadAlways>())))
                .Where(x => x.Methods.Any()).ToList();

            this.WithDiscordCommand = controllers
                .Select(x => new ControllerInfo(x.Controller, x.Methods.Where(m => m.HasAttribute<DiscordCommand>())))
                .Where(x => x.Methods.Any()).ToList();
        }
    }
}
