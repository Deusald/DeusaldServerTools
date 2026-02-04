using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace DeusaldServerToolsBackend;

public static class JwtTokenValidator
{
    public static ClaimsPrincipal? ValidateToken(string jwtToken, TokenValidationParameters validationParameters, Action<Exception> onException)
    {
        try
        {
            ClaimsPrincipal? principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out _);
            return principal;
        }
        catch (Exception e)
        {
            onException(e);
            return null;
        }
    }
}