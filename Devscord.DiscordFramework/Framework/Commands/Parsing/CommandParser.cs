﻿using Devscord.DiscordFramework.Commons.Extensions;
using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devscord.DiscordFramework.Framework.Commands.Parsing
{
    public class CommandParser
    {
        private readonly string[] possiblePrefixes = new string[] { "!", "--", "-", "^", "$", "%" };

        public DiscordRequest Parse(string message)
        {
            var prefix = this.GetPrefix(message);
            message = message.CutStart(prefix);

            var name = this.GetName(message);
            message = message.CutStart(name);

            var arguments = this.GetArguments(message);

            return new DiscordRequest
            {
                Prefix = prefix,
                Name = name,
                ArgumentsPrefix = arguments?.FirstOrDefault()?.Prefix,
                Arguments = arguments
            };
        }

        private string GetPrefix(string message)
        {
            var withoutWhitespaces = message.Trim();
            return possiblePrefixes.FirstOrDefault(x => withoutWhitespaces.StartsWith(x));
        }

        private string GetName(string message)
        {
            return message.Split(' ').First();
        }

        private IEnumerable<DiscordRequestArgument> GetArguments(string message)
        {
            var prefix = this.GetPrefix(message);
            var splitted = message.Split(prefix)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim());
            return splitted.Select(x => this.GetArgument(x, prefix));
        }

        private DiscordRequestArgument GetArgument(string message, string prefix)
        {
            var prefixExists = !string.IsNullOrWhiteSpace(prefix);
            var splitted = message.Split(' ');
            var parameter = prefixExists ? splitted.First() : null;
            var values = splitted.Skip(prefixExists ? 1 : 0);

            return new DiscordRequestArgument
            {
                Name = parameter,
                Prefix = prefixExists ? prefix : null,
                Values = values
            };
        }
    }
}
