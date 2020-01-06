﻿using System;
using System.Collections.Generic;
using System.Text;
using Devscord.DiscordFramework.Framework.Commands.Parsing;
using NUnit.Framework;

namespace Devscord.DiscordFramework.UnitTests.Commands.Parsing.Models
{
    [TestFixture]
    public class DiscordRequestTests
    {
        [Test]
        [TestCase("-help -json", "json", true)]
        [TestCase("-help", "json", false)]
        [TestCase("-add", "role", false)]
        [TestCase("-add role", "role", false)]
        [TestCase("-add -role", "role", true)]
        public void ShouldFoundName(string message, string name, bool shouldTrue)
        {
            //Arrange
            var commandParser = new CommandParser();

            //Act
            var result = commandParser.Parse(message);

            //Assert
            Assert.AreEqual(shouldTrue, result.HasArgument(name));
        }

        [Test]
        [TestCase("-help json", "", "json", true)]
        [TestCase("-help -json xml", "json", "xml", true)] 
        [TestCase("-help -json json", "json", "json", true)]
        [TestCase("-add tester", null, "tester", true)]
        [TestCase("-add tester", null, "csharp", false)]
        [TestCase("-help xml format", "format", "xml", false)]
        [TestCase("-help format xml", "format", "json", false)]
        public void ShouldFoundNameAndValue(string message, string name, string value, bool shouldTrue)
        {
            //Arrange
            var commandParser = new CommandParser();

            //Act
            var result = commandParser.Parse(message);

            //Assert
            Assert.AreEqual(shouldTrue, result.HasArgument(name, value));
        }
    }
}
