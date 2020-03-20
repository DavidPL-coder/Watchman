﻿using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Newtonsoft.Json;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Statistics.Services;
using Watchman.DomainModel.Messages.Commands;
using Watchman.DomainModel.Messages.Queries;

namespace Watchman.Discord.Areas.Statistics.Controllers
{
    public class StatisticsController : IController
    {
        private readonly ReportsService _reportsService;
        private readonly ChartsService _chartsService;
        private readonly IQueryBus _queryBus;
        private readonly ICommandBus _commandBus;
        private readonly MessagesServiceFactory _messagesServiceFactory;

        public StatisticsController(IQueryBus queryBus, ICommandBus commandBus, MessagesServiceFactory messagesServiceFactory, ReportsService reportsService, ChartsService chartsService)
        {
            this._queryBus = queryBus;
            this._commandBus = commandBus;
            this._messagesServiceFactory = messagesServiceFactory;
            this._reportsService = reportsService;
            this._chartsService = chartsService;
        }

        [ReadAlways]
        public async Task SaveMessageAsync(DiscordRequest request, Contexts contexts)
        {
            //TODO maybe there should be builder... but it doesn't looks very bad
            var command = new AddMessageCommand(request.OriginalMessage,
                contexts.User.Id, contexts.User.Name,
                contexts.Channel.Id, contexts.Channel.Name,
                contexts.Server.Id, contexts.Server.Name,
                contexts.Server.Owner.Id, contexts.Server.Owner.Name,
                request.SentAt);

            await this._commandBus.ExecuteAsync(command);
            Log.Information("Message saved");
        }

        [AdminCommand]
        [DiscordCommand("stats")]
        public void GetStatisticsPerPeriod(DiscordRequest request, Contexts contexts)
        {
            //TODO it doesn't looks clear...
            var period = _reportsService.SelectPeriod(request.OriginalMessage); //TODO use DiscordRequest properties
            var getMessages = new GetMessagesQuery(contexts.Server.Id);
            var messages = this._queryBus.Execute(getMessages).Messages.ToList();
            var report = _reportsService.CreateReport(messages, period);
            Log.Information("Generated statistics for time range {start} {end}", report.TimeRange.Start, report.TimeRange.End);
            var messagesService = _messagesServiceFactory.Create(contexts);
#if DEBUG
            //TODO it should be inside messages service... or responses service
            var dataToMessage = "```json\n" + JsonConvert.SerializeObject(report.StatisticsPerPeriod.Where(x => x.MessagesQuantity > 0), Formatting.Indented) + "\n```";
            Log.Information(dataToMessage);
#endif
            var path = _chartsService.GetImageStatisticsPerPeriod(report);
            messagesService.SendFile(path);
        }
    }
}
