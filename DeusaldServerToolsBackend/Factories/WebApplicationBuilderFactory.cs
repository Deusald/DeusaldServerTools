using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public static class WebApplicationBuilderFactory
{
    public static WebApplicationBuilder Create(string[] args, bool external, bool extendWebRootPath, Type startupClassType)
    {
        WebApplicationOptions options = new WebApplicationOptions
        {
            Args = args
        };

        if (external)
        {
            string dictionary = Environment.CurrentDirectory;
            string path       = Path.Combine(dictionary, "..", startupClassType.Namespace!);
            
            options = new WebApplicationOptions
            {
                Args            = args,
                WebRootPath     = extendWebRootPath ? path + "/wwwroot" : "",
                ContentRootPath = path
            };
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

        if (external)
        {
            Assembly assembly = startupClassType.Assembly;
            builder.Services.AddControllersWithViews().AddNewtonsoftJson()
                   .AddApplicationPart(assembly);
        }

        return builder;
    }
}