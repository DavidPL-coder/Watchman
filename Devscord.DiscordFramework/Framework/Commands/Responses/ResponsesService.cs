﻿using Devscord.DiscordFramework.Middlewares.Contexts;
using Devscord.DiscordFramework.Services;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Devscord.DiscordFramework.Framework.Commands.Responses
{
    public class ResponsesService
    {
        public IEnumerable<Response> Responses { get; set; }
        private readonly ResponsesParser _parser;

        public Func<Contexts, IEnumerable<Response>> GetResponsesFunc { get; set; } = x => throw new NotImplementedException();

        public ResponsesService()
        {
            this._parser = new ResponsesParser();
        }

        public void RefreshResponses(Contexts contexts)
        {
            try
            {
                this.Responses = this.GetResponsesFunc(contexts);
            }
            catch { }
        }

        public Response GetResponse(string name)
        {
            try
            {
                var res = Responses.SingleOrDefault(x => x.OnEvent == name);
            return Responses.SingleOrDefault(x => x.OnEvent == name);
            }
            catch { }


            return new Response();
        }

        public string ProcessResponse(string response, params KeyValuePair<string, string>[] values)
        {
            return ProcessResponse(GetResponse(response), values);
        }

        public string ProcessResponse(string response, Contexts contexts, params KeyValuePair<string, string>[] values)
        {
            return ProcessResponse(GetResponse(response), contexts, values);
        }

        public string ProcessResponse(Response response, params KeyValuePair<string, string>[] values)
        {
            try
            {

            if(values.Length != response.GetFields().Count())
            {
                throw new ArgumentException($"Cannot process response {response.OnEvent}. Values must be equal to required.");
            }
            Log.Debug($"Start parsing response {response} with values {values}");
            }
            catch { }
            return _parser.Parse(response, values);
        }

        public string ProcessResponse(Response response, Contexts contexts, params KeyValuePair<string, string>[] values)
        {
            var fields = contexts.ConvertToResponseFields(response.GetFields()).ToList();
            Log.Debug($"Found fields {fields} in response {response}");
            fields.AddRange(values);
            Log.Debug($"Start parsing response {response} with values {values}");
            return _parser.Parse(response, fields);
        }
    }
}
