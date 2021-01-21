﻿using Devscord.DiscordFramework.Framework.Commands;
using Devscord.DiscordFramework.Framework.Commands.PropertyAttributes;

namespace Devscord.DiscordFramework.UnitTests.Commands.RunnerOfIBotCommandMethodsTests.BotCommands
{
    public class SingleWordCommand : IBotCommand
    {
        [SingleWord]
        public string TestSingleWord { get; set; }
    }
}