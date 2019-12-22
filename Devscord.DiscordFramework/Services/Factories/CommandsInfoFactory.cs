﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devscord.DiscordFramework.Commons.Extensions;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Services.Models;

namespace Devscord.DiscordFramework.Services.Factories
{
    public class CommandsInfoFactory
    {
        public IEnumerable<CommandInfo> Create(Type controller)
        {
            var methods = controller.GetMethods().GetMethodsByAttribute<DiscordCommand>();
            return methods.Select(x => new CommandInfo
            {
                // todo: sprawdzić jeszcze raz
                Prefix = "-",
                Names = x.CustomAttributes.GetAttributes<DiscordCommand>().Select(x => x.ConstructorArguments.First().ToString().Replace("\"", "")),
                MethodName = x.Name,
                CommandArgumentInfos = x.CustomAttributes.Select(x => new CommandArgumentInfo
                {
                    ArgumentPrefix = "",
                    Name = "Default"
                })
            });
        }
    }
}
