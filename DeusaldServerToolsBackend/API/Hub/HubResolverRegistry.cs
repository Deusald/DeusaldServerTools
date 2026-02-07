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
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

public sealed class HubResolverRegistry(Dictionary<string, Type> map)
{
    public Type Get(string requestId)
        => map.TryGetValue(requestId, out Type? t)
               ? t
               : throw new ServerException(ErrorCode.WrongRequestId ,$"Unknown requestId '{requestId}'.");
}

[PublicAPI]
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