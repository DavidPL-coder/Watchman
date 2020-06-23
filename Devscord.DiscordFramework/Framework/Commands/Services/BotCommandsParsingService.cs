﻿using Devscord.DiscordFramework.Framework.Commands.Parsing.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Devscord.DiscordFramework.Framework.Commands.Properties;

namespace Devscord.DiscordFramework.Framework.Commands.Services
{
    public class BotCommandsParsingService
    {
        private readonly BotCommandsPropertyConversionService _botCommandPropertyConversionService;

        public BotCommandsParsingService(BotCommandsPropertyConversionService botCommandPropertyConversionService) => this._botCommandPropertyConversionService = botCommandPropertyConversionService;

        public IBotCommand ParseRequestToCommand(Type commandType, DiscordRequest request, BotCommandTemplate template)
        {
            var result = this.GetFilledInstance(commandType, template, (key, isList, x) =>
            {
                var result = request.Arguments.FirstOrDefault(a => a.Name.ToLowerInvariant() == key.ToLowerInvariant());
                if (result == null)
                {
                    return null;
                }
                var argType = template.Properties.FirstOrDefault(x => x.Name.ToLowerInvariant() == key.ToLowerInvariant())?.Type;
                if (argType.HasValue && argType.Value == BotCommandPropertyType.Bool)
                {
                    return result.Value ?? bool.TrueString;
                }
                if (!isList)
                {
                    return result.Value;
                }
                var argumentsList = request.Arguments.ToList();
                var indexOf = argumentsList.IndexOf(result);
                var nextResults = argumentsList.Skip(indexOf + 1).TakeWhile(x => x.Name == null);
                var list = new List<string>() { result.Value };
                list.AddRange(nextResults.Select(x => x.Value));
                return list;
            });
            return result;
        }

        public IBotCommand ParseCustomTemplate(Type commandType, BotCommandTemplate template, Regex customTemplate, string input)
        {
            var match = customTemplate.Match(input);
            if (!this.CustomTemplateIsValid(match, template))
            {
                Log.Warning("Custom template {customTemplate} is not valid for {commandName}", customTemplate, template.CommandName);
                return null;
            }
            var result = this.GetFilledInstance(commandType, template, (key, isList, template) =>
            {
                if (!match.Groups.ContainsKey(key))
                {
                    return null;
                }
                var value = match.Groups[key].Value;
                if (!isList)
                {
                    return value;
                }
                if (!value.Contains('\"') || value.Count(x => x == '\"') % 2 != 0)
                {
                    return value.Split(' ').ToList();
                }
                var results = value.Split('"')
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith(' ') && !x.EndsWith(' '))
                    .ToList();
                foreach (var toRemove in results)
                {
                    value = value.Replace($"\"{toRemove}\"", string.Empty);
                }
                var otherResults = value.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim());
                results.AddRange(otherResults);
                return results;
            });
            return result;
        }

        private IBotCommand GetFilledInstance(Type commandType, BotCommandTemplate template, Func<string, bool, BotCommandTemplate, object> getValueByName)
        {
            var instance = Activator.CreateInstance(commandType);
            foreach (var property in commandType.GetProperties())
            {
                var propertyType = template.Properties.FirstOrDefault(x => x.Name == property.Name)?.Type;
                var isList = propertyType == BotCommandPropertyType.List;
                var value = getValueByName.Invoke(property.Name, isList, template);
                if (value == null)
                {
                    continue;
                }
                if (isList)
                {
                    property.SetValue(instance, value);
                    continue;
                }
                if (value is string valueString && !string.IsNullOrWhiteSpace(valueString))
                {
                    var convertedType = this._botCommandPropertyConversionService.ConvertType(valueString, propertyType.Value);
                    property.SetValue(instance, convertedType);
                }
            }
            return (IBotCommand) instance;
        }

        private bool CustomTemplateIsValid(Match match, BotCommandTemplate template)
        {
            if (!match.Success)
            {
                return false;
            }
            var requiredProperties = template.Properties.Where(x => !x.IsOptional).ToList();
            if (match.Groups.Count - 1 < requiredProperties.Count)
            {
                return false;
            }
            if (requiredProperties.Any(x => !match.Groups.ContainsKey(x.Name)))
            {
                return false;
            }
            return true;
        }
    }
}
