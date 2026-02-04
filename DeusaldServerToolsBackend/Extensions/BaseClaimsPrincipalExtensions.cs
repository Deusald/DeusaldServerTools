using System.Security.Claims;

namespace DeusaldServerToolsBackend;

public static class BaseClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetGuid(BaseClaimNames.USER_ID);
    }

    public static DateTime GetExpireDate(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetDate(BaseClaimNames.EXPIRE);
    }

    public static string GetBackendVersion(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetString(BaseClaimNames.BACKEND_VERSION);
    }
    
    public static Guid GetSecurityStamp(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetGuid(BaseClaimNames.SECURITY_STAMP);
    }
}