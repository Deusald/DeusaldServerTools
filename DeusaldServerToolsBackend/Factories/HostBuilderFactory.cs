using Microsoft.Extensions.Hosting;

namespace DeusaldServerToolsBackend;

public static class HostBuilderFactory
{
    public static IHostBuilder Create(string[] args, bool external, Type startupClassType)
    {
        if (external)
        {
            string dictionary = Environment.CurrentDirectory;
            string path       = Path.Combine(dictionary, "..", startupClassType.Namespace!);
            return Host.CreateDefaultBuilder(args).UseContentRoot(path).UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes  = true;
                options.ValidateOnBuild = true;
            });
        }

        return Host.CreateDefaultBuilder(args);
    }
}