// MIT License

// DeusaldServerTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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