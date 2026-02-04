using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;

namespace DeusaldServerToolsBackend;

public static class WebHostSetupExtensions
{
    public static void SetUrl(this ConfigureWebHostBuilder builder, int portA, int portB, bool isManual, bool usePortB)
    {
        int port           = 5000;
        if (isManual) port = usePortB ? portB : portA;
        builder.UseUrls($"http://[*]:{port}");
    }

    public static TOptions ConfigureSettings<TOptions>(this IServiceCollection services, IConfiguration configuration,
                                                       Action<TOptions>? configure = null) where TOptions : class, new()
    {
        string settingsName = typeof(TOptions).Name;
        services.Configure<TOptions>(configuration.GetSection(settingsName));
        TOptions settings = new TOptions();
        configuration.Bind(settingsName, settings);
        services.AddTransient(sp => sp.GetService<IOptionsMonitor<TOptions>>()!.CurrentValue);
        if (configure != null)
        {
            services.ConfigureDirect(configure);
        }

        return settings;
    }

    private static void ConfigureDirect<TOptions>(this IServiceCollection services, Action<TOptions>? configure = null)
        where TOptions : class, new()
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddTransient(sp => sp.GetService<IOptions<TOptions>>()!.Value);
    }

    public static void BaseBuilderConfigure(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddEndpointsApiExplorer();

        if (builder.Environment.IsDevOrLower())
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
    }

    public static void AuthBuilderConfigure(this WebApplicationBuilder builder, string issuer, string audience, string hubPath)
    {
        JwtTokenProvider jwtTokenProvider = new JwtTokenProvider(BaseEnvVariables.AUTH_SIGNING_KEY.GetEnvironmentVariable()!, issuer, audience);
        builder.Services.AddSingleton<ITokenProvider>(jwtTokenProvider);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            if (builder.Environment.IsDevOrLower())
            {
                options.RequireHttpsMetadata = false;
            }

            options.TokenValidationParameters = jwtTokenProvider.GetValidationParameters();
            options.SaveToken                 = true;
            options.Events                    = JwtBearerEventsFactory.Create(hubPath);
        });
    }

    public static void LogsBuilderConfigure(this IServiceCollection services, WebApplicationBuilder? builder, IWebHostEnvironment env, IConfiguration configuration)
    {
        GlobalDiagnosticsContext.Set(BaseNLogConsts.VERSION_KEY,     BaseEnvVariables.SERVICE_VERSION.GetEnvironmentVariable()!);
        GlobalDiagnosticsContext.Set(BaseNLogConsts.COMMIT_HASH_KEY, BaseEnvVariables.COMMIT_HASH.GetEnvironmentVariable()!);

        LogManager.Setup()
                  .SetupExtensions(e => e.RegisterLayoutRenderer<LokiFormatter>())
                  .LoadConfiguration(c => c.Configuration = new NLogLoggingConfiguration(configuration.GetSection(BaseNLogConsts.NLOG_CONFIGURATION_SECTION_NAME)));

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddProvider(new NLogLoggerProvider());
        });

        builder?.Logging.ClearProviders();
        builder?.Logging.AddProvider(new NLogLoggerProvider());

        InternalTryCatchHelper.LogAllExceptions(env.GetEnv());
    }
}