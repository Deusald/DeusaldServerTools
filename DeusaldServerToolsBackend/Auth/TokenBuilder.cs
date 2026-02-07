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