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

using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public static class TextHttp
{
    public static async Task<string> ReadRequestTextAsync(HttpContext ctx, int maxChars)
    {
        HttpRequest req = ctx.Request;
        
        string contentType = req.ContentType ?? "";
        
        if (!contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            && !contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            && !contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadHttpRequestException(
                "Unsupported Content-Type.",
                StatusCodes.Status415UnsupportedMediaType);
        }

        // If Content-Length known, reject early
        if (req.ContentLength is { } len && len > maxChars * 4L)
            throw new BadHttpRequestException(
                "Payload too large.",
                StatusCodes.Status413PayloadTooLarge);

        // Use UTF-8 explicitly
        using StreamReader reader = new StreamReader(
            req.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        StringBuilder sb         = new StringBuilder();
        char[]        buffer     = new char[1024];
        int           totalChars = 0;

        while (true)
        {
            int read = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0) break;

            totalChars += read;
            if (totalChars > maxChars)
                throw new BadHttpRequestException(
                    "Payload too large.",
                    StatusCodes.Status413PayloadTooLarge);

            sb.Append(buffer, 0, read);
        }

        return sb.ToString();
    }

    public static async Task WriteResponseTextAsync(HttpContext ctx, string text, string contentType, CancellationToken ct)
    {
        ctx.Response.StatusCode = StatusCodes.Status200OK;
        ctx.Response.ContentType = contentType;
        ctx.Response.ContentLength = Encoding.UTF8.GetByteCount(text);
        await ctx.Response.WriteAsync(text, ct);
    }
}