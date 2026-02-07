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

using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public static class WebHostEnvironmentExtensions
{
    public static bool IsManual(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Manual");
    }

    public static bool IsLocal(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Local");
    }

    public static bool IsDevOrLower(this IWebHostEnvironment environment)
    {
        return environment.IsManual() || environment.IsLocal() || environment.IsDevelopment();
    }

    public static Env GetEnv(this IWebHostEnvironment environment)
    {
        if (environment.IsManual()) return Env.Manual;
        if (environment.IsLocal()) return Env.Local;
        if (environment.IsDevelopment()) return Env.Development;
        if (environment.IsStaging()) return Env.Staging;
        if (environment.IsProduction()) return Env.Production;
        return Env.Manual;
    }
}