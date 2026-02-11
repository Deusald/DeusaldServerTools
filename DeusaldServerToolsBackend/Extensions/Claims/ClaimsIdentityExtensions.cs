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
using DeusaldSharp;
using JetBrains.Annotations;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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