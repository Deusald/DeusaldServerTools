using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;

namespace DeusaldServerToolsBackend;

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