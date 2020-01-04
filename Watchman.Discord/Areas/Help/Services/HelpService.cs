﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services;
using Devscord.DiscordFramework.Services.Models;
using Newtonsoft.Json;
using Watchman.Cqrs;
using Watchman.Discord.Areas.Help.Factories;
using Watchman.DomainModel.Help.Models;
using Watchman.DomainModel.Help.Queries;
using Watchman.Integrations.MongoDB;

namespace Watchman.Discord.Areas.Help.Services
{
    public class HelpService : IService
    {
        private readonly IQueryBus _queryBus;
        private readonly HelpInformationFactory _helpInformationFactory;
        private readonly ISession _session;

        public HelpService(ISessionFactory sessionFactory, IQueryBus queryBus, HelpInformationFactory helpInformationFactory)
        {
            _queryBus = queryBus;
            _helpInformationFactory = helpInformationFactory;
            _session = sessionFactory.Create();
        }

        //todo: tutaj myślę czy nie wydzielić generateHelp i generateJsonHelp do osobnej klasy
        public string GenerateHelp(Contexts contexts)
        {
            var result = this._queryBus.Execute(new GetHelpInformationQuery(contexts.Server.Id));

            var messageBuilder = new StringBuilder();
            foreach (var helpInfo in result.HelpInformations)
            {
                var line = new StringBuilder("-" + helpInfo.Names.Aggregate((x, y) => $"{x} / -{y}"));
                line.Append(" => ");
                line.Append(helpInfo.Descriptions.First(x => x.Name == helpInfo.DefaultDescriptionName).Details);
                messageBuilder.AppendLine(line.ToString());
            }

            return messageBuilder.ToString();
        }

        public IEnumerable<string> GenerateJsonHelp(Contexts contexts)
        {
            var result = this._queryBus.Execute(new GetHelpInformationQuery(contexts.Server.Id));
            var serialized = JsonConvert.SerializeObject(result.HelpInformations, Formatting.Indented);
            var smallerMessages = SplitJsonToSmallerMessages(serialized);

            foreach (var sm in smallerMessages)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("```json");
                messageBuilder.AppendLine(sm);
                messageBuilder.AppendLine("```");
                yield return messageBuilder.ToString();
            }
        }

        private IEnumerable<string> SplitJsonToSmallerMessages(string message)
        {
            const int MAX_MESSAGE_LENGTH = 1998; // for safety reason I made it smaller

            while (message.Length > MAX_MESSAGE_LENGTH)
            {
                var cutIndex = message.Substring(0, MAX_MESSAGE_LENGTH).LastIndexOf("},", StringComparison.Ordinal) + 2;
                yield return message.Substring(0, cutIndex);
                message = message.Remove(0, cutIndex);
            }
            yield return message;
        }

        public void FillDatabaseWithNewMethods(IEnumerable<CommandInfo> commandInfosFromAssembly)
        {
            var commandInfosFromAssemblyList = commandInfosFromAssembly.ToList(); // for not multiple enumerating
            var helpInfosInDb = _session.Get<HelpInformation>().ToList();

            var newCommands = FindNewCommands(commandInfosFromAssemblyList, helpInfosInDb).ToList();
            var oldCommands = FindOldHelps(commandInfosFromAssemblyList, helpInfosInDb).ToList();
            var changedCommands = FindChangedHelps(commandInfosFromAssemblyList, helpInfosInDb).ToList();

            newCommands.ForEach(x => _session.Add(this._helpInformationFactory.Create(x)));
            oldCommands.ForEach(x => _session.Delete(x));
            changedCommands.ForEach(x => _session.Update(UpdateHelpInfo(x, commandInfosFromAssemblyList)));
        }

        private IEnumerable<CommandInfo> FindNewCommands(IEnumerable<CommandInfo> commandInfosFromAssembly, IEnumerable<HelpInformation> helpInfosInDb)
        {
            var defaultHelpInfosInDb = helpInfosInDb.Where(x => x.IsDefault).ToList(); // for optimize checking only defaults
            return commandInfosFromAssembly.Where(x => defaultHelpInfosInDb.All(h => h.MethodName != x.MethodName));
        }

        private IEnumerable<HelpInformation> FindOldHelps(IEnumerable<CommandInfo> commandInfosFromAssembly, IEnumerable<HelpInformation> helpInfosInDb)
        {
            return helpInfosInDb.Where(x => commandInfosFromAssembly.All(c => c.MethodName != x.MethodName));
        }

        private IEnumerable<HelpInformation> FindChangedHelps(IEnumerable<CommandInfo> commandInfosFromAssembly, IEnumerable<HelpInformation> helpInfosInDb)
        {
            var allCommandsNamesInAssembly = commandInfosFromAssembly.SelectMany(x => x.Names);
            return helpInfosInDb.Where(x =>
            {
                return !x.Names.All(n => allCommandsNamesInAssembly.Contains(n))
                    || !allCommandsNamesInAssembly.All(n => x.Names.Contains(n));
            });
        }

        private HelpInformation UpdateHelpInfo(HelpInformation help, IEnumerable<CommandInfo> commandInfosFromAssembly)
        {
            help.Names = commandInfosFromAssembly.First(x => x.MethodName == help.MethodName).Names;
            return help;
        }
    }
}
