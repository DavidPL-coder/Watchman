﻿using System.Collections.Generic;

namespace Devscord.DiscordFramework.Framework.Commands.Parsing.Models
{
    public class DiscordRequestArgument
    {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Values { get; set; }
    }
}
