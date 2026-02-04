using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;

namespace DeusaldServerToolsBackend;

public static class ClaimsIdentityExtensions
{
    public static Guid GetGuidFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return Guid.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                               ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                               $"Could not retrieve {claimId} claim")).Value);
    }

    public static string GetStringFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return (claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                    ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                    $"Could not retrieve {claimId} claim")).Value;
    }

    public static int GetIntFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return int.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                              ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                              $"Could not retrieve {claimId} claim")).Value);
    }

    public static uint GetUIntFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return uint.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                               ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                               $"Could not retrieve {claimId} claim")).Value);
    }

    public static ushort GetUshortFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return ushort.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                                 ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                                 $"Could not retrieve {claimId} claim")).Value);
    }

    public static bool GetBoolFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return bool.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                               ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                               $"Could not retrieve {claimId} claim")).Value);
    }

    public static Version GetVersionFromClaimsIdentity(this ClaimsIdentity claimsIdentity, string claimId)
    {
        return Version.Parse((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                                  ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                                  $"Could not retrieve {claimId} claim")).Value);
    }

    public static T GetEnumFromClaimsIdentity<T>(this ClaimsIdentity claimsIdentity, string claimId) where T : struct, Enum
    {
        return Enum.Parse<T>((claimsIdentity.FindFirst(claimId) ?? throw new ServerException(
                                  ErrorCode.Unauthorized, LogLevel.Error, "Refresh failed",
                                  $"Could not retrieve {claimId} claim")).Value);
    }
}