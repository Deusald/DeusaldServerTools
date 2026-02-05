using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public interface IEndpointResolver
{
    Task HandleAsync(HttpContext context, CancellationToken ct);
}