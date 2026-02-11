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

using DeusaldSharp;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public static class WebHostSetupExtensions
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static string HealthPath = "/healthz";

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static string HubPath = "/hub";

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

    public static void DeusaldBaseBuilderConfigure(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<RequestLogContext>();
        builder.Services.AddScoped<IRequestLogContext>(sp => sp.GetRequiredService<RequestLogContext>());

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

    public static void DeusaldAuthBuilderConfigure(this WebApplicationBuilder builder, string issuer, string audience, double expireTimeDays, List<string> registeredClaimNames)
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
            options.Events                    = JwtBearerEventsFactory.Create(HubPath);
        });
        
        TokenBuilder.ExpireTimeDays = expireTimeDays;
        TokenBuilder.RegisteredKeys = registeredClaimNames;
    }

    public static void DeusaldLogsBuilderConfigure(this IServiceCollection services, WebApplicationBuilder? builder, IWebHostEnvironment env, IConfiguration configuration)
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

    public static void DeusaldBaseWebAppConfigure(this WebApplication app, bool emptyFavicon)
    {
        if (app.Environment.IsDevOrLower()) app.UseDeveloperExceptionPage();
        else app.UseHttpsRedirection();

        app.UseRouting();
        if (app.Environment.IsDevOrLower()) app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<RequestLogContextMiddleware>();
        app.MapHealthChecks(HealthPath);

        if (!emptyFavicon) return;
        app.MapGet("/favicon.ico", context =>
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        }).AllowAnonymous();
    }

    public static void DeusaldBaseHubWebAppConfigure<T>(this WebApplication app) where T : Hub
    {
        app.MapHub<T>(HubPath, options =>
        {
            options.CloseOnAuthenticationExpiration = true;
            options.Transports                      = HttpTransportType.WebSockets;
        });
    }
}