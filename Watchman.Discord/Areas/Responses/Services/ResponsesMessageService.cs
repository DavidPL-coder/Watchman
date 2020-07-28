﻿using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watchman.Discord.Areas.Responses.Services
{
    public class ResponsesMessageService
    {
        private const string DESCRIPTION = "dokumentacja:\nhttps://watchman.readthedocs.io/pl/latest/135-services-in-framework/";
        private readonly ResponsesGetterService _responsesDatabase;
        private readonly EmbedMessageSplittingService _embedMessageSplittingService;

        public ResponsesMessageService(ResponsesGetterService responsesDatabase, EmbedMessageSplittingService embedMessageSplittingService)
        {
            this._responsesDatabase = responsesDatabase;
            this._embedMessageSplittingService = embedMessageSplittingService;
        }

        public async Task PrintResponses(string typeOfResponses, Contexts contexts)
        {
            switch (typeOfResponses)
            {
                case "default":
                    await _embedMessageSplittingService.SendEmbedSplitMessage("Domyślne responses:", DESCRIPTION, GetDefaultResponses(), contexts);
                    break;
                case "custom":
                    await _embedMessageSplittingService.SendEmbedSplitMessage("Nadpisane responses:", DESCRIPTION, GetCustomResponses(contexts.Server.Id), contexts);
                    break;
                default:
                    await _embedMessageSplittingService.SendEmbedSplitMessage("Wszystkie responses:", DESCRIPTION, GetAllResponses(), contexts);
                    break;
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetDefaultResponses()
        {
            return this._responsesDatabase.GetResponsesFromBase()
                .Where(x => x.IsDefault)
                .Select(x => new KeyValuePair<string, string>(x.OnEvent, this.GetRawMessage(x.Message)));
        }

        private IEnumerable<KeyValuePair<string, string>> GetCustomResponses(ulong serverId)
        {
            var responses = this._responsesDatabase.GetResponsesFromBase()
                .Where(x => x.ServerId == serverId)
                .Select(x => new KeyValuePair<string, string>(x.OnEvent, this.GetRawMessage(x.Message)));

            if (!responses.Any())
            {
                responses = responses.Append(new KeyValuePair<string, string>("-------------------------------", "```Obecnie jest brak nadpisanych responses!```"));
            }
            return responses;
        }

        private IEnumerable<KeyValuePair<string, string>> GetAllResponses()
        {
            return this._responsesDatabase.GetResponsesFromBase()
                .Select(x => new KeyValuePair<string, string>(x.OnEvent, this.GetRawMessage(x.Message)));
        }

        private string GetRawMessage(string message)
        {
            return message.Replace("`", @"\`").Replace("*", @"\*");
        }
    }
}
