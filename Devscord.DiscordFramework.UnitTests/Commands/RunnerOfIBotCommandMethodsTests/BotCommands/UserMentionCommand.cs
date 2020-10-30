﻿using Devscord.DiscordFramework.Framework.Commands;
using Devscord.DiscordFramework.Framework.Commands.PropertyAttributes;

namespace Devscord.DiscordFramework.UnitTests.Commands.RunnerOfIBotCommandMethodsTests.BotCommands
{
    class UserMentionCommand : IBotCommand
    {
        [UserMention]
        public ulong TestUserMention { get; set; }
    }
}
