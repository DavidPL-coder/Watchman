﻿using MongoDB.Driver;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace Watchman.Integrations.Logging
{
    public class SerilogInitializer
    {
        public static ILogger Initialize(IMongoDatabase mongoDatabase)
        {
#if DEBUG
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    outputTemplate: "[{Timestamp:dd-MM-yyyy} - {Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose)
                .CreateLogger();
            return logger;
        }
    }

    public class DiscordSink : ILogEventSink
    {
        private readonly Action<string> _onEmit;

        public DiscordSink(Action<string> onEmit = null)
        {
            _onEmit = onEmit;
        }

        public void Emit(LogEvent logEvent)
        {
            if(_onEmit == null)
            {
                return;
            }

            var message = logEvent.RenderMessage();
            _onEmit.Invoke(message);
        }
    }
}
