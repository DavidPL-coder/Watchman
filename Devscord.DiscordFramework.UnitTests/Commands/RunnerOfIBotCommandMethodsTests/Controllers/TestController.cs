﻿using System;
using System.Collections.Generic;
using Devscord.DiscordFramework.UnitTests.Commands.RunnerOfIBotCommandMethodsTests.BotCommands;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Middlewares.Contexts;

namespace Devscord.DiscordFramework.UnitTests.Commands.RunnerOfIBotCommandMethodsTests.Controllers
{
    class TestController : IController
    { 
        public string ValueOfTestText { get; private set; }
        public bool ValueOfTestBool { get; private set; }
        public string ValueOfTestSingleWord { get; private set; }
        public List<string> ValueOfTestList { get; private set; }
        public TimeSpan ValueOfTestTime { get; private set; }
        public ulong ValueOfTestUserMention { get; private set; }
        public ulong ValueOfTestChannelMention { get; private set; }
        public object GeneralValue { get; private set; }
        public OptionalArgsCommand OptionalArgs { get; private set; }

        public void TestOptionalArgs(OptionalArgsCommand command, Contexts contexts)
        {
            if (command.TestNullableInt != null)
            {
                this.GeneralValue = command.TestNullableInt;
            }
            else if (command.TestTime != null)
            {
                this.GeneralValue = command.TestTime;
            }
            else if (command.TestUserMention != null)
            {
                this.GeneralValue = command.TestUserMention;
            }
            else if (command.TestChannelMention != null)
            {
                this.GeneralValue = command.TestChannelMention;
            }
            else if (command.TestList != null)
            {
                this.GeneralValue = command.TestList;
            }
            this.OptionalArgs = command;  
        }

        public void TestText(TextCommand command, Contexts contexts)
        {
            this.ValueOfTestText = command.TestText;
        }

        public void TestBool(BoolCommand command, Contexts contexts)
        {
            this.ValueOfTestBool = command.TestBool;
            this.GeneralValue = command.TestBool;
        }

        public void TestSingleWord(SingleWordCommand command, Contexts contexts)
        {
            this.ValueOfTestSingleWord = command.TestSingleWord;
            this.GeneralValue = command.TestSingleWord;
        }

        public void TestList(ListCommand command, Contexts contexts)
        {
            this.ValueOfTestList = command.TestList;
            this.GeneralValue = command.TestList;
        }

        public void TestTime(TimeCommand command, Contexts contexts)
        {
            this.ValueOfTestTime = command.TestTime;
        }

        public void TestUserMention(UserMentionCommand command, Contexts contexts)
        {
            this.ValueOfTestUserMention = command.TestUserMention;
        }

        public void TestChannelMention(ChannelMentionCommand command, Contexts contexts)
        {
            this.ValueOfTestChannelMention = command.TestChannelMention;
        }
    }
}
