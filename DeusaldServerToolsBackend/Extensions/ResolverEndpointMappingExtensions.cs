using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public static class ResolverEndpointMappingExtensions
{
    public static IServiceCollection AddEndpointResolvers(this IServiceCollection services, params Assembly[] assemblies)
    {
        List<Type> resolverTypes =
            assemblies.SelectMany(a => a.GetTypes())
                      .Where(t => typeof(IEndpointResolver).IsAssignableFrom(t))
                      .Where(t => t is { IsAbstract: false, IsInterface: false })
                      .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                      .ToList();

        foreach (Type t in resolverTypes) services.AddScoped(t);

        return services;
    }

    public static void MapEndpointResolvers(this IEndpointRouteBuilder endpoints, params Assembly[] assemblies)
    {
        var resolverTypes =
            assemblies.SelectMany(a => a.GetTypes())
                      .Where(t => typeof(IEndpointResolver).IsAssignableFrom(t))
                      .Where(t => t is { IsAbstract: false, IsInterface: false })
                      .Select(t => new
                       {
                           Type          = t,
                           Endpoint      = t.GetCustomAttribute<EndpointAttribute>(),
                           Auth          = t.GetCustomAttribute<EndpointAuthorizeAttribute>(),
                           AllowAnonymus = t.GetCustomAttribute<EndpointAllowAnonymousAttribute>()
                       })
                      .Where(x => x.Endpoint != null)
                      .ToList();

        foreach (var r in resolverTypes)
        {
            string     route  = r.Endpoint!.Route;
            HttpMethodType method = r.Endpoint.Method;
            
            IEndpointConventionBuilder builder = endpoints.MapMethods(route, [method.ToString()], async context =>
            {
                CancellationToken ct = context.RequestAborted;
                IEndpointResolver resolver = (IEndpointResolver)context.RequestServices.GetRequiredService(r.Type);
                await resolver.HandleAsync(context, ct);
            });
            
            if (r.Auth != null && r.AllowAnonymus != null) throw new Exception("Method can't be authorized and non authorized at the same time!");
            if (r.AllowAnonymus != null)
            {
                builder.AllowAnonymous();
                continue;
            }
            if (r.Auth == null) builder.RequireAuthorization();
            else builder.RequireAuthorization(r.Auth.Policy);
        }
    }
}