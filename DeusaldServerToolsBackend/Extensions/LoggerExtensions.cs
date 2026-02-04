using Microsoft.Extensions.Logging;

namespace DeusaldServerToolsBackend;

public static class LoggerExtensions
{
    public static NLog.LogLevel ToNLog(this DeusaldSharp.LogLevel level)
    {
        return level switch
        {
            DeusaldSharp.LogLevel.Off   => NLog.LogLevel.Off,
            DeusaldSharp.LogLevel.Trace => NLog.LogLevel.Trace,
            DeusaldSharp.LogLevel.Debug => NLog.LogLevel.Debug,
            DeusaldSharp.LogLevel.Info  => NLog.LogLevel.Info,
            DeusaldSharp.LogLevel.Warn  => NLog.LogLevel.Warn,
            DeusaldSharp.LogLevel.Error => NLog.LogLevel.Error,
            DeusaldSharp.LogLevel.Fatal => NLog.LogLevel.Fatal,
            _                           => NLog.LogLevel.Off
        };
    }

    public static LogLevel ToMicrosoftLog(this DeusaldSharp.LogLevel level)
    {
        return level switch
        {
            DeusaldSharp.LogLevel.Off   => LogLevel.None,
            DeusaldSharp.LogLevel.Trace => LogLevel.Trace,
            DeusaldSharp.LogLevel.Debug => LogLevel.Debug,
            DeusaldSharp.LogLevel.Info  => LogLevel.Information,
            DeusaldSharp.LogLevel.Warn  => LogLevel.Warning,
            DeusaldSharp.LogLevel.Error => LogLevel.Error,
            DeusaldSharp.LogLevel.Fatal => LogLevel.Critical,
            _                           => LogLevel.None
        };
    }
}