﻿using System;
using System.Collections.Generic;
using Devscord.DiscordFramework.Framework.Commands.AntiSpam.Models;
using Devscord.DiscordFramework.Services.Models;
using NUnit.Framework;
using Watchman.Discord.Areas.Protection.Strategies;
using Watchman.DomainModel.Messages.Queries;

namespace Watchman.Discord.UnitTests.AntiSpam
{
    [TestFixture]
    internal class SpamDetectorsStrategiesTests
    {
        [Test]
        [TestCase("some link: https://discord.com abc", 10, SpamProbability.Medium)]
        [TestCase("http://abc.com", 600, SpamProbability.Low)]
        public void LinksDetector_ShouldDetectSpam(string messageContent, int userMessagesCount, SpamProbability exceptedSpamProbability)
        {
            // Arrange
            var spamTestsService = new AntiSpamTestsService();
            var (request, contexts) = spamTestsService.CreateRequestAndContexts(messageContent);
            spamTestsService.UserMessagesCounter
                .Setup(x => x.CountUserMessages(AntiSpamTestsService.DEFAULT_TEST_USER_ID, GetMessagesQuery.GET_ALL_SERVERS))
                .Returns(userMessagesCount);

            var linksDetector = new LinksDetectorStrategy(spamTestsService.UserMessagesCounter.Object);

            // Act
            var spamProbability = linksDetector.GetSpamProbability(spamTestsService.ExampleServerMessages, request, contexts);

            // Assert
            Assert.That(spamProbability, Is.EqualTo(exceptedSpamProbability));
        }

        [Test]
        [TestCase("abcd")]
        [TestCase("rgdrg32http")]
        public void LinksDetector_ShouldNotDetectSpam(string messageContent)
        {
            // Arrange
            var spamTestsService = new AntiSpamTestsService();
            var (request, contexts) = spamTestsService.CreateRequestAndContexts(messageContent);
            var linksDetector = new LinksDetectorStrategy(spamTestsService.UserMessagesCounter.Object);

            // Act
            var spamProbability = linksDetector.GetSpamProbability(spamTestsService.ExampleServerMessages, request, contexts);

            // Assert
            Assert.That(spamProbability, Is.EqualTo(SpamProbability.None));
        }

        [Test]
        [TestCase("spam", "spam", "spam", "spam", 1000, SpamProbability.Medium)]
        [TestCase("spam", "sth else", "spam", "spam", 1000, SpamProbability.Low)]
        [TestCase("spam", "spam2", "spam3", "spamm", 400, SpamProbability.Sure)]
        [TestCase("spam", "spam2", "spam3", "spamm", 1200, SpamProbability.Medium)]
        [TestCase("spam", "spam2", "spam", "spam", 50, SpamProbability.Sure)]
        [TestCase("aaaaaaaaaabbbbb", "https://discord.gg/example", "https://discord.gg/example", "https://discord.gg/example", 20, SpamProbability.Medium)]
        [TestCase("link: https://discord.gg/example", "https://discord.gg/example", "https://discord.gg/example", "https://discord.gg/example", 20, SpamProbability.Sure)]
        public void DuplicatesDetector_ShouldDetectSpam(string messageContent1, string messageContent2, string messageContent3, string messageContent4, int userMessagesCount, SpamProbability exceptedSpamProbability)
        {
            // Arrange
            var spamTestsService = new AntiSpamTestsService();
            spamTestsService.UserMessagesCounter
                .Setup(x => x.CountUserMessages(AntiSpamTestsService.DEFAULT_TEST_USER_ID, GetMessagesQuery.GET_ALL_SERVERS))
                .Returns(userMessagesCount);

            var duplicatesDetector = new DuplicatedMessagesDetectorStrategy(spamTestsService.UserMessagesCounter.Object);

            var (request, contexts) = spamTestsService.CreateRequestAndContexts(messageContent4);
            var serverMessages = new ServerMessagesCacheService();
            serverMessages.OverwriteMessages(new List<SmallMessage>
            {
                new SmallMessage(messageContent1, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now),
                new SmallMessage(messageContent2, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now),
                new SmallMessage(messageContent3, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now)
            });

            // Act
            var spamProbability = duplicatesDetector.GetSpamProbability(serverMessages, request, contexts);

            // Assert
            Assert.That(spamProbability, Is.EqualTo(exceptedSpamProbability));
        }

        [Test]
        [TestCase("not spam", "something else", "something more else", "totally different", 10)]
        [TestCase("hello", "zxy", "afegserg", "egr", 5)]
        [TestCase("hello", "how", "are", "you", 5)]
        [TestCase("hello", "how", "her", "shower", 50)]
        public void DuplicatesDetector_ShouldNotDetectSpam(string messageContent1, string messageContent2, string messageContent3, string messageContent4, int userMessagesCount)
        {
            // Arrange
            var spamTestsService = new AntiSpamTestsService();
            spamTestsService.UserMessagesCounter
                .Setup(x => x.CountUserMessages(AntiSpamTestsService.DEFAULT_TEST_USER_ID, GetMessagesQuery.GET_ALL_SERVERS))
                .Returns(userMessagesCount);

            var duplicatesDetector = new DuplicatedMessagesDetectorStrategy(spamTestsService.UserMessagesCounter.Object);

            var (request, contexts) = spamTestsService.CreateRequestAndContexts(messageContent4);
            var serverMessages = new ServerMessagesCacheService();
            serverMessages.OverwriteMessages(new List<SmallMessage>
            {
                new SmallMessage(messageContent1, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now),
                new SmallMessage(messageContent2, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now),
                new SmallMessage(messageContent3, AntiSpamTestsService.DEFAULT_TEST_USER_ID, DateTime.Now)
            });

            // Act
            var spamProbability = duplicatesDetector.GetSpamProbability(serverMessages, request, contexts);

            // Assert
            Assert.That(spamProbability, Is.EqualTo(SpamProbability.None));
        }
    }
}
