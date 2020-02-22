﻿using System.Collections.Generic;
using System.Linq;
using Devscord.DiscordFramework.Services.Models;
using Watchman.DomainModel.Help;

namespace Watchman.Discord.Areas.Help.Factories
{
    public class HelpInformationFactory
    {
        private readonly ArgumentInfoFactory _argumentInfoFactory;

        public HelpInformationFactory(ArgumentInfoFactory argumentInfoFactory)
        {
            _argumentInfoFactory = argumentInfoFactory;
        }

        public HelpInformation Create(CommandInfo commandInfo)
        {
            return new HelpInformation
            {
                MethodFullName = commandInfo.MethodFullName,
                Names = commandInfo.Names,
                ArgumentInfos = commandInfo.CommandArgumentInfos.Select(x => _argumentInfoFactory.Create(x)),
                ServerId = 0,
                Descriptions = new List<Description>
                {
                    new Description
                    {
                        Name = "EN",
                        Details = "Empty"
                    }
                }
            };
        }
    }
}
