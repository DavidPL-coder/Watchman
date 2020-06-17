﻿using Devscord.DiscordFramework.Commons;
using Devscord.DiscordFramework.Framework.Commands.Responses;
using Devscord.DiscordFramework.Integration;
using Devscord.DiscordFramework.Middlewares.Contexts;
using Discord.Rest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Devscord.DiscordFramework.Commons.Exceptions;
using System.Linq;

namespace Devscord.DiscordFramework.Services
{
    public class MessagesService
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }

        private static readonly Dictionary<ulong, IEnumerable<Response>> _serversResponses;
        private static ResponsesService _responsesService;
        private readonly MessageSplittingService _splittingService;
        private readonly EmbedMessagesService _embedMessagesService;

        static MessagesService()
        {
            _serversResponses = new Dictionary<ulong, IEnumerable<Response>>();
            RefreshResponsesCyclic();
        }

        public MessagesService(ResponsesService responsesService, MessageSplittingService splittingService, EmbedMessagesService embedMessagesService)
        {
            _responsesService = responsesService;
            this._splittingService = splittingService;
            this._embedMessagesService = embedMessagesService;
        }

        public Task SendMessage(string message, MessageType messageType = MessageType.NormalText)
        {
            var channel = this.GetChannel();
            foreach (var mess in this._splittingService.SplitMessage(message, messageType))
            {
                channel.SendMessageAsync(mess);
                Log.Information("Bot sent message {splitted} {message}", mess, messageType != MessageType.NormalText ? "splitted" : string.Empty);
            }

            return Task.CompletedTask;
        }

        public Task SendEmbedMessage(string title, string description, IEnumerable<KeyValuePair<string, string>> values)
        {
            var channel = this.GetChannel();
            var embed = this._embedMessagesService.Generate(title, description, values);
            channel.SendMessageAsync(embed: embed);
            return Task.CompletedTask;
        }

        public Task SendResponse(Func<ResponsesService, string> response)
        {
            _responsesService.Responses = _serversResponses.GetValueOrDefault(this.GuildId) ?? GetResponsesForNewServer(this.GuildId);
            var message = response.Invoke(_responsesService);
            return this.SendMessage(message);
        }

        public async Task SendFile(string filePath)
        {
            var channel = (IRestMessageChannel)await Server.GetChannel(this.ChannelId);
            await channel.SendFileAsync(filePath);
        }

        public async Task SendExceptionResponse(BotException botException, Contexts contexts)
        {
            var responseName = botException.GetType().Name.Replace("Exception", "");
            var responseManagerMethod = typeof(ResponsesManager).GetMethod(responseName);
            if (responseManagerMethod == null)
            {
                Log.Error("{name} doesn't exists as a response", responseName);
                await this.SendMessage($"{responseName} doesn't exists as a response"); // message typed into code, bcs it's called only when there is a problem with responses
                return;
            }
            await this.SendResponse(x =>
            {
                var arg = new object[] { x };
                if (botException.Value != null)
                {
                    arg = arg.Append(botException.Value).ToArray();
                }
                return (string)responseManagerMethod.Invoke(null, arg);
            });
        }

        private static async void RefreshResponsesCyclic()
        {
            while (true)
            {
                foreach (var serverId in _serversResponses.Keys.ToList())
                {
                    var responses = _responsesService.GetResponsesFunc(serverId);
                    _serversResponses[serverId] = responses;
                }
                await Task.Delay(10 * 60 * 1000);
            }
        }

        private static IEnumerable<Response> GetResponsesForNewServer(ulong serverId)
        {
            var responses = _responsesService.GetResponsesFunc(serverId).ToList();
            _serversResponses.Add(serverId, responses);
            return responses;
        }

        private IRestMessageChannel GetChannel()
        {
            RestGuild guild = null;
            if (this.GuildId != default)
                guild = Server.GetGuild(this.GuildId).Result;
            var channel = (IRestMessageChannel)Server.GetChannel(this.ChannelId, guild).Result;
            return channel;
        }
    }
}
