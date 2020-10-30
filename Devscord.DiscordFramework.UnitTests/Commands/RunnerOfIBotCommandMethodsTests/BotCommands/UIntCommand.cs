﻿using Devscord.DiscordFramework.Framework.Commands;
using Devscord.DiscordFramework.Framework.Commands.PropertyAttributes;

namespace Devscord.DiscordFramework.UnitTests.Commands.RunnerOfIBotCommandMethodsTests.BotCommands
{
    class UIntCommand : IBotCommand
    {
        [Number]
        public uint TestUInt { get; set; }
    }
}
