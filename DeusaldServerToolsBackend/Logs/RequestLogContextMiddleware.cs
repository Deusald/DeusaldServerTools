using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public class RequestLogContextMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx, IRequestLogContext logCtx, RequestLogContext concrete)
    {
        string? userId = ctx.User.FindFirst(BaseClaimNames.USER_ID)?.Value;
        if (!string.IsNullOrEmpty(userId)) logCtx.SetUserId(userId);

        try
        {
            await next(ctx);
        }
        finally
        {
            concrete.Dispose();
        }
    }
}