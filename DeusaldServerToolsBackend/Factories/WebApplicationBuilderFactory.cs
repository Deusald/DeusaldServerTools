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
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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