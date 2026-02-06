using Microsoft.Extensions.Logging;

namespace DeusaldServerToolsClient
{
    public class ConsoleLogProvider : ILoggerProvider
    {
        private readonly LogLevel _MinLogLevel;

        public ConsoleLogProvider(LogLevel minLogLevel)
        {
            _MinLogLevel = minLogLevel;
        }

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(_MinLogLevel);
        }
    }
}