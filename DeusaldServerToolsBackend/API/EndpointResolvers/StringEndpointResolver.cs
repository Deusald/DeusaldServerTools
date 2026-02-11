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
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public abstract class StringEndpointResolver : IEndpointResolver
{
    protected abstract int    _RequestMaxCharactersCount { get; }
    protected abstract string _ResponseContentType       { get; }

    public async Task HandleAsync(HttpContext context, CancellationToken ct)
    {
        try
        {
            string response = await HandleAsync(_RequestMaxCharactersCount == 0 ? string.Empty : await TextHttp.ReadRequestTextAsync(context, _RequestMaxCharactersCount), ct);
            await TextHttp.WriteResponseTextAsync(context, response, $"{_ResponseContentType}; charset=utf-8", ct);
        }
        catch (BadHttpRequestException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(ex.Message, ct);
        }
    }

    protected abstract Task<string> HandleAsync(string request, CancellationToken ct);
}