﻿using Devscord.DiscordFramework.Commons.Exceptions;
using Devscord.DiscordFramework.Framework.Commands.Properties;
using Devscord.DiscordFramework.Integration;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Devscord.DiscordFramework.Framework.Commands.Services
{
    public class BotCommandsPropertyConversionService
    {
        private readonly Regex _exTime = new Regex(@"(?<Value>\d+)(?<Unit>(ms|d|h|m|s))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex _exMention = new Regex(@"\d+", RegexOptions.Compiled);

        public object ConvertType(string value, BotCommandPropertyType type)
        {
            try
            {
                return type switch
                {
                    BotCommandPropertyType.Time => this.ToTimeSpan(value),
                    BotCommandPropertyType.Number => int.Parse(value),
                    BotCommandPropertyType.Bool => bool.Parse(value),
                    BotCommandPropertyType.UserMention => this.ParseToUserId(value),
                    BotCommandPropertyType.ChannelMention => this.ParseToChannelId(value),
                    BotCommandPropertyType.SingleWord => !value.Any(char.IsWhiteSpace) ? value : throw new InvalidArgumentsException(),
                    _ => value
                };
            }
            catch
            {
                throw new InvalidArgumentsException();
            }  
        }

        private TimeSpan ToTimeSpan(string value)
        {
            var match = this._exTime.Match(value);
            var unit = match.Groups["Unit"].Value.ToLowerInvariant();
            var timeValue = uint.Parse(match.Groups["Value"].Value);
            return unit switch
            {
                "d" => TimeSpan.FromDays(timeValue),
                "h" => TimeSpan.FromHours(timeValue),
                "m" => TimeSpan.FromMinutes(timeValue),
                "s" => TimeSpan.FromSeconds(timeValue),
                "ms" => TimeSpan.FromMilliseconds(timeValue),
                _ => throw new ArgumentNullException()
            };
        }

        private ulong ParseToUserId(string value)
        {
            var id = ulong.Parse(_exMention.Match(value).Value);
            var user = Server.GetUser(id).Result;
            return user != null ? id : throw new InvalidArgumentsException();
        }

        private ulong ParseToChannelId(string value)
        {
            var id = ulong.Parse(_exMention.Match(value).Value);
            var channel = Server.GetChannel(id).Result;
            return channel != null ? id : throw new InvalidArgumentsException();
        }
    }
}
