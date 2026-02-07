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

using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;
using JetBrains.Annotations;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public static class ClaimsPrincipalExtensions
{
    public static ClaimsIdentity GetClaimsIdentity(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Identity as ClaimsIdentity ?? throw new ServerException(
                   ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                   "Could not retrieve claims identity from claims principal");
    }

    public static Guid GetGuid(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return Guid.TryParse(rawId, out Guid guid)
                   ? guid
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static DateTime GetDate(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawTime = claimsPrincipal.FindFirst(claimName)?.Value;
        return long.TryParse(rawTime, out long unixTime)
                   ? DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static short GetShort(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return short.TryParse(rawId, out short data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static ushort GetUshort(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return ushort.TryParse(rawId, out ushort data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static uint GetUint(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return uint.TryParse(rawId, out uint data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static int GetInt(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return int.TryParse(rawId, out int data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static bool GetBool(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return bool.TryParse(rawId, out bool data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static Version GetVersion(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return Version.TryParse(rawId, out Version? data)
                   ? data
                   : throw new ServerException(
                         ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                         $"Couldn't find {claimName} in claims principal.");
    }

    public static void InjectVersion(this ClaimsPrincipal claimsPrincipal, Version version, string claimName)
    {
        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        if (Version.TryParse(rawId, out Version? _)) return;
        ClaimsIdentity? identity = claimsPrincipal.Identity as ClaimsIdentity;
        if (identity == null)
        {
            identity = new ClaimsIdentity();
            claimsPrincipal.AddIdentity(identity);
        }

        identity.AddClaim(new Claim(claimName, version.ToString()));
    }

    public static string GetString(this ClaimsPrincipal? claimsPrincipal, string claimName)
    {
        if (claimsPrincipal == null)
            throw new ServerException(
                ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                $"Couldn't find {claimName} in claims principal.");

        string? rawId = claimsPrincipal.FindFirst(claimName)?.Value;
        return rawId ?? throw new ServerException(
                   ErrorCode.Unauthorized, LogLevel.Error, "Unauthorized",
                   $"Couldn't find {claimName} in claims principal.");
    }
}