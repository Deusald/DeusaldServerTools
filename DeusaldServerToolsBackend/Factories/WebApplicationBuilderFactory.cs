using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public static class WebApplicationBuilderFactory
{
    public static WebApplication Create(string[] args, bool external, bool extendWebRootPath, bool portB, Type startupClassType, 
                                        Action<WebApplicationBuilder, bool> configureServices, Action<WebApplication> configureApp)
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
        
        configureServices(builder, portB);
        WebApplication app = builder.Build();
        configureApp(app);
        return app;
    }
}