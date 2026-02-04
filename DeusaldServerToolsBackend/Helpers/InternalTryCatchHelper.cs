using NLog;
using LogLevel = NLog.LogLevel;

namespace DeusaldServerToolsBackend;

public static class InternalTryCatchHelper
{
    public static void LogAllExceptions(Env env)
    {
        if (env == Env.Manual) return;
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
    }
    
    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Log(LogLevel.Fatal, e.ExceptionObject.ToString!);
    }
}