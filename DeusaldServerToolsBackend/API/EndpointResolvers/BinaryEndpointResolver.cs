using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public abstract class BinaryEndpointResolver : IEndpointResolver
{
    protected abstract int RequestMaxBytesCount { get; }
    
    public async Task HandleAsync(HttpContext context, CancellationToken ct)
    {
        try
        {
            byte[] response = await HandleAsync(RequestMaxBytesCount == 0 ? [] : await BinaryHttp.ReadRequestBytesAsync(context, RequestMaxBytesCount, ct), ct);
            await BinaryHttp.WriteResponseBytesAsync(context, response, ct);
        }
        catch (BadHttpRequestException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(ex.Message, ct);
        }
    }

    protected abstract Task<byte[]> HandleAsync(byte[] request, CancellationToken ct);
}