﻿using System;
using System.Collections.Generic;
using System.Text;
using Watchman.Discord.Framework.Architecture.Middlewares;

namespace Watchman.Discord.Contexts
{
    public class DiscordServerContext : IDiscordContext
    {
        public ulong Id { get; private set; }
        public string Name { get; private set; }

        public DiscordServerContext(ulong id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
