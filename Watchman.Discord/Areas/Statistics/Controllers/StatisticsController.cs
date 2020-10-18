﻿using Devscord.DiscordFramework.Commons.Extensions;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Framework.Commands.PropertyAttributes;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Newtonsoft.Json;
using Serilog;
using Statsman;
using Statsman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchman.Common.Models;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Statistics.BotCommands;
using Watchman.DomainModel.Messages.Commands;
using Watchman.DomainModel.Messages.Queries;
using Watchman.DomainModel.Settings.ConfigurationItems;
using Watchman.DomainModel.Settings.Services;

namespace Watchman.Discord.Areas.Statistics.Controllers
{
    public class StatisticsController : IController
    {
        
        private readonly PeriodStatisticsService _periodStatisticsService;
        private readonly ConfigurationService _configurationService;
        private readonly IQueryBus _queryBus;
        private readonly ICommandBus _commandBus;
        private readonly MessagesServiceFactory _messagesServiceFactory;

        public StatisticsController(IQueryBus queryBus, ICommandBus commandBus, MessagesServiceFactory messagesServiceFactory, ChartsService chartsService, PeriodStatisticsService periodStatisticsService, ConfigurationService configurationService)
        {
            this._queryBus = queryBus;
            this._commandBus = commandBus;
            this._messagesServiceFactory = messagesServiceFactory;
            this._periodStatisticsService = periodStatisticsService;
            this._configurationService = configurationService;
        }

        [ReadAlways]
        public async Task SaveMessageAsync(DiscordRequest request, Contexts contexts)
        {
            Log.Information("Started saving the message");
            var command = new AddMessageCommand(request.OriginalMessage,
                contexts.User.Id, contexts.User.Name,
                contexts.Channel.Id, contexts.Channel.Name,
                contexts.Server.Id, contexts.Server.Name,
                request.SentAt);
            Log.Information("Command created");
            await this._commandBus.ExecuteAsync(command);
            Log.Information("Message saved");
        }

        public async Task Stats(StatsCommand command, Contexts contexts)
        {
            var (chart, message) = await this.GetStatistics(command, contexts);
            if (chart == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            var messagesService = this._messagesServiceFactory.Create(contexts);
            _ = await Task.Run(() => messagesService.SendFile("Statistics.png", chart))
                .ContinueWith(x => messagesService.SendMessage(message));
        }

        private async Task<(Stream Chart, string Message)> GetStatistics(StatsCommand command, Contexts contexts)
        {
            var task = command switch
            {
                _ when command.Minute => this.GetStatisticsPerMinute(command, contexts),
                _ when command.Hour => this.GetStatisticsPerHour(command, contexts),
                _ when command.Day => this.GetStatisticsPerDay(command, contexts),
                _ when command.Week => this.GetStatisticsPerWeek(command, contexts),
                _ when command.Month => this.GetStatisticsPerMonth(command, contexts),
                _ when command.Quarter => this.GetStatisticsPerQuarter(command, contexts),
                _ => this.GetStatisticsPerHour(command, contexts)
            };
            return await task;
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerMinute(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerMinute>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerMinute(request);
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerHour(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerHour>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerHour(request);
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerDay(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerDay>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerDay(request);
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerWeek(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerWeek>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerWeek(request);
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerMonth(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerMonth>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerMonth(request);
        }

        private Task<(Stream Chart, string Message)> GetStatisticsPerQuarter(StatsCommand command, Contexts contexts)
        {
            var time = this._configurationService.GetConfigurationItem<TimeBehindStatisticsPerQuarter>(contexts.Server.Id);
            var request = new StatisticsRequest(contexts.Server.Id, time.Value, command.User, command.Channel);
            return this._periodStatisticsService.PerQuarter(request);
        }
    }
    
}
