using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace DeusaldServerToolsBackend;

public interface ITokenProvider
{
    string CreateAccessToken(ClaimsIdentity subject, DateTime expiresAt);

    TokenValidationParameters GetValidationParameters();
}