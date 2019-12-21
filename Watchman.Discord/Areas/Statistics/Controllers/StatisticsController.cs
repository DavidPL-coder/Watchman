﻿using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Architecture.Middlewares;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchman.Common.Models;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Statistics.Models;
using Watchman.Discord.Areas.Statistics.Services;
using Watchman.Discord.Areas.Statistics.Services.Builders;
using Watchman.Integrations.MongoDB;

namespace Watchman.Discord.Areas.Statistics.Controllers
{
    public class StatisticsController : IController
    {
        private readonly ISession _session;
        private readonly ReportsService _reportsService;
        private readonly ChartsService _chartsService;
        private readonly IQueryBus queryBus;
        private readonly ICommandBus commandBus;
        private readonly MessagesServiceFactory messagesServiceFactory;

        public StatisticsController(IQueryBus queryBus, ICommandBus commandBus, ISessionFactory sessionFactory, MessagesServiceFactory messagesServiceFactory, ReportsService reportsService, ChartsService chartsService)
        {
            this.queryBus = queryBus;
            this.commandBus = commandBus;
            this.messagesServiceFactory = messagesServiceFactory;
            this._session = sessionFactory.Create();
            this._reportsService = reportsService;
            this._chartsService = chartsService;
        }

        [ReadAlways]
        public void SaveMessage(DiscordRequest request, Contexts contexts)
        {
            var messageBuilder = new MessageInformationBuilder(request.OriginalMessage);
            var messageInfo = messageBuilder
                .SetAuthor(contexts.User)
                .SetChannel(contexts.Channel)
                .SetServerInfo(contexts.Server)
                .Build();

            this.SaveToDatabase(messageInfo);
        }

        [AdminCommand]
        [DiscordCommand("stats")]
        public void GetStatisticsPerPeriod(DiscordRequest request, Contexts contexts)
        {
            var period = _reportsService.SelectPeriod(request.OriginalMessage); //TODO use DiscordRequest properties
            var messages = this._session.Get<MessageInformation>().ToList();
            var report = _reportsService.CreateReport(messages, period, contexts.Server);

            var messagesService = messagesServiceFactory.Create(contexts);
#if DEBUG
            var dataToMessage = "```json\n" + JsonConvert.SerializeObject(report.StatisticsPerPeriod.Where(x => x.MessagesQuantity > 0), Formatting.Indented) + "\n```";
            messagesService.SendMessage(dataToMessage);
#endif
            var path = _chartsService.GetImageStatisticsPerPeriod(report);
            messagesService.SendFile(path);
        }

        private Task SaveToDatabase(MessageInformation data)
        {
            Task.Factory.StartNew(() => _session.Add(data));
            return Task.CompletedTask;
        }
    }
}
