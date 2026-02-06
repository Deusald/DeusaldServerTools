using DeusaldServerToolsClient;
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public static class RequestExtensions
{
    public static Version ExtractVersion(this HttpRequest request)
    {
        if (request.Headers.ContainsKey(BaseClaimNames.CLIENT_VERSION)) return Version.Parse(request.Headers[BaseClaimNames.CLIENT_VERSION]!);
        return new Version(0, 0, 0);
    }
}