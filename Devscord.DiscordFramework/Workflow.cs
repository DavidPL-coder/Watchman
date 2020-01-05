﻿using Autofac;
using Devscord.DiscordFramework.Commons.Extensions;
using Devscord.DiscordFramework.Framework.Architecture.Controllers;
using Devscord.DiscordFramework.Framework.Architecture.Middlewares;
using Devscord.DiscordFramework.Framework.Commands.Parsing;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services.Factories;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Services;

namespace Devscord.DiscordFramework
{
    public class Workflow
    {
        public Action<Exception, SocketMessage> WorkflowException { get; set; }

        private readonly List<object> _middlewares;
        private readonly Assembly _botAssembly;
        private readonly IComponentContext _context;
        private readonly CommandParser commandParser;

        public Workflow(Assembly botAssembly, IComponentContext context)
        {
            _middlewares = new List<object>();
            _botAssembly = botAssembly;
            this._context = context;

            this.commandParser = new CommandParser();//todo maybe autofac
        }

        public Workflow AddMiddleware<T>(object configuration = null /*TODO*/)
        {
            if (_middlewares.Any(x => x.GetType().FullName == typeof(T).FullName))
            {
                return this;
            }

            var middleware = Activator.CreateInstance<T>();//todo autofac
            _middlewares.Add(middleware);
            return this;
        }

        public Task Run(SocketMessage data)
        {
            try
            {
                var contexts = this.RunMiddlewares(data);
                this.RunControllers(data.Content, contexts);
            }
            catch (Exception e)
            {
                WorkflowException.Invoke(e, data);
            }
            return Task.CompletedTask;
        }

        private Contexts RunMiddlewares<T>(T data)
        {
            var contexts = new Contexts();
            foreach (var middleware in _middlewares)
            {
                var context = ((dynamic)middleware).Process(data);
                contexts.SetContext(context);
            }
            return contexts;
        }

        private void RunControllers(string message, Contexts contexts)
        {
            var controllers = _botAssembly.GetTypesByInterface<IController>()
                .Select(x => (IController) _context.Resolve(x));

            var discordRequest = commandParser.Parse(message);
            
            if (!discordRequest.IsCommandForBot)
                return;

            foreach (var controller in controllers)
            {
                var methods = controller.GetType().GetMethods();
                var withReadAlways = methods.Where(x => x.HasAttribute<ReadAlways>());
                var withDiscordCommand = methods.Where(x => x.HasAttribute<DiscordCommand>());

                this.RunWithReadAlwaysMethods(controller, discordRequest, contexts, withReadAlways);
                this.RunWithDiscordCommandMethods(controller, discordRequest, contexts, withDiscordCommand);
            }
        }

        private void RunWithReadAlwaysMethods(IController controller, DiscordRequest request, Contexts contexts, IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                var arguments = new object[] { request, contexts };
                method.Invoke(controller, arguments);
            }
        }

        private void RunWithDiscordCommandMethods(IController controller, DiscordRequest request, Contexts contexts, IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                var commandArguments = method.GetCustomAttributesData()
                    .Where(x => x.AttributeType.FullName == typeof(DiscordCommand).FullName)
                    .SelectMany(x => x.ConstructorArguments, (x, arg) => arg.Value).ToArray();

                var commands = commandArguments.Select(x => (DiscordCommand)Activator.CreateInstance(typeof(DiscordCommand), x));

                //todo fix. this version is for usersController but should be changed
                if (commands.Any(x => request.Name == x.Command || request.OriginalMessage.TrimStart(request.Prefix.ToCharArray()).StartsWith(x.Command))) 
                {
                    if (method.HasAttribute<AdminCommand>() && !contexts.User.IsAdmin)
                    {
                        var messageService = new MessagesServiceFactory(new ResponsesService(), new MessageSplittingService()).Create(contexts);
                        messageService.SendMessage("Nie masz wystarczających uprawnień do wywołania tej komendy.");
                        break;
                    }

                    method.Invoke(controller, new object[] { request, contexts });
                    break;
                }
            }
        }
    }
}
