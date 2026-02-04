using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DeusaldServerToolsBackend;

public static class JwtBearerEventsFactory
{
    public static JwtBearerEvents Create(string pathOfHub)
    {
        return new JwtBearerEvents
        {
            // We have to hook the OnMessageReceived event in order to
            // allow the JWT authentication handler to read the access
            // token from the query string when a WebSocket or 
            // Server-Sent Events request comes in.

            // Sending the access token in the query string is required due to
            // a limitation in Browser APIs. We restrict it to only calls to the
            // SignalR hub in this code.
            // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
            // for more information about security considerations when using
            // the query string to transmit the access token.
            OnMessageReceived = context =>
            {
                StringValues accessToken = context.Request.Query["access_token"];
                PathString   path        = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(pathOfHub))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    }
}