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

using System.Reflection;
using DeusaldServerToolsClient;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public sealed class HubResolverRegistry
{
    private readonly Dictionary<string, Type> _Map = new();
    
    public Type Get(string requestId)
        => _Map.TryGetValue(requestId, out Type? t)
               ? t
               : throw new ServerException(ErrorCode.WrongRequestId, $"Unknown requestId '{requestId}'.");
    
    public bool TryAdd(string requestId, Type resolverType) => _Map.TryAdd(requestId, resolverType);
}

[PublicAPI]
public static class ResolverEndpointMappingExtensions
{
    public static IServiceCollection AddEndpointResolvers(this IServiceCollection services, params Assembly[] assemblies)
    {
        List<Type> resolverTypes =
            assemblies.SelectMany(a => a.GetTypes())
                      .Where(t => typeof(IEndpointResolver).IsAssignableFrom(t) || typeof(IHubRequestResolver).IsAssignableFrom(t))
                      .Where(t => t is { IsAbstract: false, IsInterface: false })
                      .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                      .ToList();

        foreach (Type t in resolverTypes) services.AddScoped(t);
        services.AddSingleton(new HubResolverRegistry());
        return services;
    }

    public static void MapEndpointResolvers(this IEndpointRouteBuilder endpoints, params Assembly[] assemblies)
    {
        var resolverTypes =
            assemblies.SelectMany(a => a.GetTypes())
                      .Where(t => typeof(IEndpointResolver).IsAssignableFrom(t) || typeof(IHubRequestResolver).IsAssignableFrom(t))
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

        HubResolverRegistry hubResolverRegistry = endpoints.ServiceProvider.GetRequiredService<HubResolverRegistry>();
        
        foreach (var r in resolverTypes)
        {
            string          address = r.Endpoint!.Address;
            SendMethodType? method  = r.Endpoint.Method;

            if (method == null) // We need to find route and method from request type
            {
                Type? match = r.Type
                               .GetInterfaces()
                               .FirstOrDefault(i =>
                                    i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IPairEndpointResolver<,>));

                if (match == null) throw new Exception("Parameterless Endpoint has to implement IPairEndpointResolver<,> so address and method can be extracted from Request type");

                Type requestType = match.GetGenericArguments()[0];
                if (Activator.CreateInstance(requestType) is not IRequestMetadata meta) throw new Exception("Parameterless Request type of Endpoint has to implement IRequestMetadata");
                address = meta.Address;
                method  = meta.SendMethod;
            }

            if (method == SendMethodType.SignalR_Hub)
            {
                if (!hubResolverRegistry.TryAdd(address, r.Type)) throw new InvalidOperationException($"Duplicate HubRequestId '{address}'.");
            }
            else
            {
                IEndpointConventionBuilder builder = endpoints.MapMethods(address, [method.Value.ToHttpMethod().Method.ToUpper()], async context =>
                {
                    CancellationToken ct       = context.RequestAborted;
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
}