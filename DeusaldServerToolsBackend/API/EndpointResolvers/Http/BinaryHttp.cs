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

using System.Buffers;
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public static class BinaryHttp
{
    public static async Task<byte[]> ReadRequestBytesAsync(HttpContext ctx, int maxBytes, CancellationToken ct)
    {
        HttpRequest req = ctx.Request;
        
        string ctHeader = req.ContentType ?? "";
        if (!ctHeader.StartsWith(System.Net.Mime.MediaTypeNames.Application.Octet, StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("Unsupported Content-Type.", StatusCodes.Status415UnsupportedMediaType);
        
        if (req.ContentLength is { } len && len > maxBytes)
            throw new BadHttpRequestException("Payload too large.", StatusCodes.Status413PayloadTooLarge);
        
        using MemoryStream ms = req.ContentLength is { } known and >= 0 && known <= maxBytes
                                    ? new MemoryStream((int)known)
                                    : new MemoryStream();

        byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        try
        {
            int total = 0;
            while (true)
            {
                int read = await req.Body.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
                if (read == 0) break;

                total += read;
                if (total > maxBytes)
                    throw new BadHttpRequestException("Payload too large.", StatusCodes.Status413PayloadTooLarge);

                ms.Write(buffer, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return ms.ToArray();
    }

    public static async Task WriteResponseBytesAsync(HttpContext ctx, byte[] bytes, CancellationToken ct)
    {
        ctx.Response.StatusCode    = StatusCodes.Status200OK;
        ctx.Response.ContentType   = System.Net.Mime.MediaTypeNames.Application.Octet;
        ctx.Response.ContentLength = bytes.Length;
        await ctx.Response.Body.WriteAsync(bytes, ct);
    }
}