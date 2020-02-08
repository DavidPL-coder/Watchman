﻿using Devscord.DiscordFramework.Framework;
using Devscord.DiscordFramework.Middlewares;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Services;
using Watchman.Integrations.MongoDB;
using Watchman.Discord.Ioc;
using Devscord.DiscordFramework;
using Autofac;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Watchman.Discord.Areas.Help.Services;
using System.Text;
using System.Collections.Generic;
using Devscord.DiscordFramework.Commons.Extensions;
using Watchman.Common.Exceptions;
using System.Linq;

namespace Watchman.Discord
{
    public class WatchmanBot : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly Workflow _workflow;
        private readonly DiscordConfiguration _configuration;
        private readonly IContainer _container;

        public WatchmanBot(DiscordConfiguration configuration)
        {
            this._configuration = configuration;
            this._client = new DiscordSocketClient(new DiscordSocketConfig
            {
                TotalShards = 1
            });
            this._client.MessageReceived += this.MessageReceived;

            this._container = GetAutofacContainer(configuration);
            this._workflow = GetWorkflow(configuration, _container);

            var dataCollector = _container.Resolve<HelpDataCollectorService>();
            var helpService = _container.Resolve<HelpDBGeneratorService>();

            helpService.FillDatabase(dataCollector.GetCommandsInfo(typeof(WatchmanBot).Assembly));
        }

        public async Task Start()
        {
            MongoConfiguration.Initialize();
            ServerInitializer.Initialize(this._client);

            await _client.LoginAsync(TokenType.Bot, this._configuration.Token);
            await _client.StartAsync();

            var unmutingService = _container.Resolve<UnmutingExpiredMuteEventsService>();
            var serversService = _container.Resolve<DiscordServersService>();
            var servers = await serversService.GetDiscordServers();
            servers.ToList().ForEach(x => unmutingService.UnmuteUsersInit(x));

            await Task.Delay(-1);
        }

        private Task MessageReceived(SocketMessage message)
        {
#if DEBUG
            if (!message.Channel.Name.Contains("test"))
                return Task.CompletedTask;
#endif
            return message.Author.IsBot ? Task.CompletedTask : this._workflow.Run(message);
        }

        private Workflow GetWorkflow(DiscordConfiguration configuration, IContainer context)
        {
            var workflow = new Workflow(typeof(WatchmanBot).Assembly, context);
            workflow.AddMiddleware<ChannelMiddleware>()
                .AddMiddleware<ServerMiddleware>()
                .AddMiddleware<UserMiddleware>();
            workflow.WorkflowException += this.LogException;
#if DEBUG
            workflow.WorkflowException += this.PrintDebugExceptionInfo;
#endif
            return workflow;
        }

        private void LogException(Exception e, Contexts contexts)
        {
            var messagesService = _container.Resolve<MessagesServiceFactory>().Create(contexts);

            switch (e)
            {
                case NotAdminPermissionsException _:
                    messagesService.SendResponse(x => x.UserIsNotAdmin(), contexts);
                    break;
                case RoleNotFoundException roleExc:
                    messagesService.SendResponse(x => x.RoleNotFound(roleExc.RoleName), contexts);
                    break;
                case UserDidntMentionedAnyUserToMuteException _:
                    messagesService.SendResponse(x => x.UserDidntMentionedAnyUserToMute(), contexts);
                    break;
                case UserNotFoundException notFoundExc:
                    messagesService.SendResponse(x => x.UserNotFound(notFoundExc.Mention), contexts);
                    break;
                default:
                messagesService.SendMessage("Wystąpił nieznany wyjątek");
                    break;
            }
        }

        private void PrintDebugExceptionInfo(Exception e, Contexts contexts)
        {
            var lines = new Dictionary<string, string>()
            {
                { "Message", e.Message },
                { "InnerException message", e.InnerException?.Message },
                { "InnerException2 message", e.InnerException?.InnerException?.Message }
            };

            var exceptionMessageBuilder = new StringBuilder();
            exceptionMessageBuilder.PrintManyLines(lines);

            var messagesService = _container.Resolve<MessagesServiceFactory>().Create(contexts);
            messagesService.SendMessage(exceptionMessageBuilder.ToString());
            Console.WriteLine(exceptionMessageBuilder.ToString());
        }

        private IContainer GetAutofacContainer(DiscordConfiguration configuration)
        {
            return new ContainerModule(configuration)
                .GetBuilder()
                .Build();
        }

        public void Dispose()
        {
            this._client.Dispose();
        }
    }
}
