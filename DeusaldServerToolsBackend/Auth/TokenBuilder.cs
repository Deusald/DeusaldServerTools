using System.Security.Claims;
using JetBrains.Annotations;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public static class TokenBuilder
{
    public static double       ExpireTimeDays { get; set; }
    public static List<string> RegisteredKeys { get; set; } = new();
    
    public static string CreateToken(ITokenProvider tokenProvider, ITokenData tokenData, out DateTime expirationTime)
    {
        ClaimsIdentity identity = CreateIdentity(tokenData);
        expirationTime = DateTime.UtcNow.AddDays(ExpireTimeDays);
        return tokenProvider.CreateAccessToken(identity, expirationTime);
    }

    public static string RefreshToken(ITokenProvider tokenProvider, ClaimsPrincipal claimsPrincipal, out DateTime expirationTime)
    {
        ClaimsIdentity identity = new ClaimsIdentity();
        foreach (string key in RegisteredKeys) identity.AddClaim(new Claim(key, claimsPrincipal.GetString(key)));
        expirationTime = DateTime.UtcNow.AddDays(ExpireTimeDays);
        return tokenProvider.CreateAccessToken(identity, expirationTime);
    }

    private static ClaimsIdentity CreateIdentity(ITokenData tokenData)
    {
        ClaimsIdentity identity = new ClaimsIdentity();
        foreach (KeyValuePair<string, string> kv in tokenData.GetData()) identity.AddClaim(new Claim(kv.Key, kv.Value));
        return identity;
    }
}