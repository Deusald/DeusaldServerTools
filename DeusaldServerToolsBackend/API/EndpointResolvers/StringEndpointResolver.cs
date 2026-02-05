using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public abstract class StringEndpointResolver : IEndpointResolver
{
    protected abstract int    RequestMaxCharactersCount { get; }
    protected abstract string ResponseContentType       { get; }

    public async Task HandleAsync(HttpContext context, CancellationToken ct)
    {
        try
        {
            string response = await HandleAsync(RequestMaxCharactersCount == 0 ? string.Empty : await TextHttp.ReadRequestTextAsync(context, RequestMaxCharactersCount), ct);
            await TextHttp.WriteResponseTextAsync(context, response, $"{ResponseContentType}; charset=utf-8", ct);
        }
        catch (BadHttpRequestException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(ex.Message, ct);
        }
    }

    protected abstract Task<string> HandleAsync(string request, CancellationToken ct);
}