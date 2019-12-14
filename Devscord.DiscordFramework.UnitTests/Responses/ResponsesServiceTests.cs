﻿using Devscord.DiscordFramework.Framework.Commands.Responses;
using NUnit.Framework;

namespace Devscord.DiscordFramework.UnitTests.Responses
{
    [TestFixture]
    public class ResponsesServiceTests
    {
        [Test]
        public void ShouldFoundManyResponsesInFile(int a)
        {
            //Arrange
            var responsesService = new ResponsesService();

            //Act
            var responses = responsesService.Responses;
            var result = responses.Count();

            //Assert
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}
