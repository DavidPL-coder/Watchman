﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Framework;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Middlewares.Factories;
using Devscord.DiscordFramework.Services.Factories;
using Discord.WebSocket;

namespace Devscord.DiscordFramework.Services
{
    public class UsersService
    {
        private readonly MessagesServiceFactory _messagesServiceFactory;

        public UsersService(MessagesServiceFactory messagesServiceFactory)
        {
            _messagesServiceFactory = messagesServiceFactory;
        }

        public Task AddRole(UserRole role, UserContext user, DiscordServerContext server)
        {
            var socketUser = GetUser(user, server);
            var socketRole = GetRole(role.Id, server);
            return socketUser.AddRoleAsync(socketRole);
        }

        public Task RemoveRole(UserRole role, UserContext user, DiscordServerContext server)
        {
            var socketUser = GetUser(user, server);
            var socketRole = GetRole(role.Id, server);
            return socketUser.RemoveRoleAsync(socketRole);
        }

        public IEnumerable<UserContext> GetUsers(DiscordServerContext server)
        {
            var guildUsers = Server.GetGuildUsers(server.Id);

            var userContextFactory = new UserContextsFactory();
            var userContexts = guildUsers.Select(x => userContextFactory.Create(x));
            return userContexts;
        }

        public Task WelcomeUser(Contexts contexts)
        {
            var messagesService = _messagesServiceFactory.Create(contexts);
            messagesService.SendResponse(x => x.NewUserArrived(contexts), contexts);
            return Task.CompletedTask;
        }

        private SocketGuildUser GetUser(UserContext user, DiscordServerContext server)
        {
            return Server.GetGuildUser(user.Id, server.Id);
        }

        private SocketRole GetRole(ulong roleId, DiscordServerContext server)
        {
            return Server.GetSocketRoles(server.Id).First(x => x.Id == roleId);
        }
    }
}
