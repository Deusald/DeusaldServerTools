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