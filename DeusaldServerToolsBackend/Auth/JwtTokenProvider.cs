using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DeusaldServerToolsBackend;

public class JwtTokenProvider(string secretKey, string issuer, string audience) : ITokenProvider
{
    private readonly SymmetricSecurityKey _SigningKey = new(Encoding.ASCII.GetBytes(secretKey));

    private const string _ALGORITHM = SecurityAlgorithms.HmacSha512Signature;

    public string CreateAccessToken(ClaimsIdentity subject, DateTime expiresAt)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = subject,
            Issuer             = issuer,
            Audience           = audience,
            IssuedAt           = DateTime.UtcNow,
            NotBefore          = DateTime.UtcNow,
            Expires            = expiresAt,
            SigningCredentials = new SigningCredentials(_SigningKey, _ALGORITHM)
        };

        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    public TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            RequireSignedTokens   = true,
            RequireExpirationTime = true,

            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = _SigningKey,
            ValidateIssuerSigningKey = true,
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,

            ClockSkew = TimeSpan.FromSeconds(30),
        };
    }
}