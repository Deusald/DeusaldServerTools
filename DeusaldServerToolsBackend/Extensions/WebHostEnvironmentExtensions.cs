using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DeusaldServerToolsBackend;

public static class WebHostEnvironmentExtensions
{
    public static bool IsManual(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Manual");
    }

    public static bool IsLocal(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Local");
    }

    public static bool IsDevOrLower(this IWebHostEnvironment environment)
    {
        return environment.IsManual() || environment.IsLocal() || environment.IsDevelopment();
    }

    public static Env GetEnv(this IWebHostEnvironment environment)
    {
        if (environment.IsManual()) return Env.Manual;
        if (environment.IsLocal()) return Env.Local;
        if (environment.IsDevelopment()) return Env.Development;
        if (environment.IsStaging()) return Env.Staging;
        if (environment.IsProduction()) return Env.Production;
        return Env.Manual;
    }
}