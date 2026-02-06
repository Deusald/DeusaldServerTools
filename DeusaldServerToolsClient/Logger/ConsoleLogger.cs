using System;
using Microsoft.Extensions.Logging;

namespace DeusaldServerToolsClient
{
    public class ConsoleLogger : ILogger
    {
        struct Scope<TState> : IDisposable
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public TState State { get; }
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Guid Id { get; }

            public Scope(TState state, Guid id)
            {
                State = state;
                Id    = id;
            }

            public void Dispose() { }
        }

        private readonly LogLevel _MinLogLevel;

        public ConsoleLogger(LogLevel minLogLevel)
        {
            _MinLogLevel = minLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.None: break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                {
                    Console.WriteLine($"{logLevel} {eventId}, {state}, {exception}");
                    break;
                }
                case LogLevel.Warning:
                {
                    Console.WriteLine($"WARNING: {eventId}, {state}, {exception}");
                    break;
                }
                case LogLevel.Error:
                case LogLevel.Critical:
                {
                    Console.WriteLine($"ERROR: {logLevel} {eventId}, {state}, {exception}");
                    break;
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_MinLogLevel == LogLevel.None) return false;
            return logLevel >= _MinLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return new Scope<TState>(state, Guid.NewGuid());
        }
    }
}