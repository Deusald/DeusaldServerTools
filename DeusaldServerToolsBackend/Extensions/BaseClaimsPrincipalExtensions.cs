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
using JetBrains.Annotations;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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

    public static Version GetClientVersion(this ClaimsPrincipal? claimsPrincipal)
    {
        return claimsPrincipal.GetVersion(BaseClaimNames.CLIENT_VERSION);
    }
}