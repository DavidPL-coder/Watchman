﻿using Autofac;
using Devscord.DiscordFramework.Framework;
using Devscord.DiscordFramework.Framework.Architecture.Middlewares;
using Devscord.DiscordFramework.Framework.Commands.Parsing;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Discord.WebSocket;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Middlewares.Factories;
using System.Collections.Generic;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;

namespace Devscord.DiscordFramework
{
    internal class Workflow
    {
        private readonly CommandParser _commandParser = new CommandParser();
        private readonly MiddlewaresService _middlewaresService = new MiddlewaresService();
        private readonly ControllersService _controllersService;

        public List<Func<Task>> OnReady { get; set; } = new List<Func<Task>>();
        public List<Func<Contexts, Task>> OnUserJoined { get; set; } = new List<Func<Contexts, Task>>();
        public List<Func<SocketMessage, Task>> OnMessageReceived { get; set; } = new List<Func<SocketMessage, Task>>();
        public List<Action<Exception, Contexts>> OnWorkflowException { get; set; } = new List<Action<Exception, Contexts>>();

        internal Workflow(Assembly botAssembly, IComponentContext context)
        {
            this._controllersService = new ControllersService(context, botAssembly);
        }

        internal Workflow AddMiddleware<T>() 
            where T : IMiddleware
        {
            this._middlewaresService.AddMiddleware<T>();
            Log.Debug("Added Middleware: {middlewareName}", nameof(T));
            return this;
        }

        internal void Initialize()
        {
            this.OnMessageReceived.Add(message => Task.Run(() => this.MessageReceived(message)));
        }

        internal void MapHandlers(DiscordSocketClient client)
        {
            this.OnReady.ForEach(x => client.Ready += x);
            this.OnMessageReceived.ForEach(x => client.MessageReceived += x);
            Server.UserJoined += CallUserJoined;
        }

        private async void MessageReceived(SocketMessage socketMessage)
        {
            if(ShouldIgnoreMessage(socketMessage))
            {
                return;
            }

            DiscordRequest request;
            Contexts contexts;
            try
            {
                Log.Information("Processing message: {content} from user {user} started", socketMessage.Content, socketMessage.Author);
                request = _commandParser.Parse(socketMessage.Content, socketMessage.Timestamp.UtcDateTime);
                Log.Information("Request parsed");
                contexts = this._middlewaresService.RunMiddlewares(socketMessage);
                Log.Information("Contexts created");
            }
            catch (Exception e)
            {
                Log.Error(e, e.StackTrace);
                return;
            }

            try
            {
                Log.Information("Starting controllers");
                await this._controllersService.Run(request, contexts);
            }
            catch (Exception e)
            {
                Log.Error(e, e.StackTrace);
                OnWorkflowException.ForEach(x => x.Invoke(e, contexts));
            }
        }

        private bool ShouldIgnoreMessage(SocketMessage socketMessage)
        {
#if DEBUG
            if (!socketMessage.Channel.Name.Contains("test"))
            {
                return true;
            }
#endif
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook)
            {
                return true;
            }
            if (socketMessage.Channel.Name.Contains("logs"))
            {
                return true;
            }
                
            return false;
        }

        private async Task CallUserJoined(SocketGuildUser guildUser)
        {
            var userFactory = new UserContextsFactory();
            var serverFactory = new DiscordServerContextFactory();

            var userContext = userFactory.Create(guildUser);
            var guild = await Server.GetGuild(guildUser.Guild.Id);
            var discordServerContext = serverFactory.Create(guild);
            var landingChannel = discordServerContext.LandingChannel;

            var contexts = new Contexts();
            contexts.SetContext(userContext);
            contexts.SetContext(discordServerContext);
            if (landingChannel != null)
            {
                contexts.SetContext(landingChannel);
            }

            OnUserJoined.ForEach(x => x.Invoke(contexts));
        }
    }
}
