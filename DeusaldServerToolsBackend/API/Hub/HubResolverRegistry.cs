using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public sealed class HubResolverRegistry(Dictionary<string, Type> map)
{
    public Type Get(string requestId)
        => map.TryGetValue(requestId, out Type? t)
               ? t
               : throw new HubException($"Unknown requestId '{requestId}'.");
}

public static class HubResolverRegistration
{
    public static IServiceCollection AddHubResolvers(this IServiceCollection services, params Assembly[] assemblies)
    {
        Dictionary<string, Type> map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (Type t in assemblies.SelectMany(a => a.GetTypes()))
        {
            if (!t.IsClass || t.IsAbstract) continue;
            if (!typeof(IHubRequestResolver).IsAssignableFrom(t)) continue;

            HubRequestAttribute? attr = t.GetCustomAttribute<HubRequestAttribute>();
            if (attr is null) continue;

            if (!map.TryAdd(attr.RequestId, t)) throw new InvalidOperationException($"Duplicate HubRequestId '{attr.RequestId}'.");
            
            services.AddScoped(t);
        }

        services.AddSingleton(new HubResolverRegistry(map));
        return services;
    }
}