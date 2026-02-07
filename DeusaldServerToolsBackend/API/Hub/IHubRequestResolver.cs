namespace DeusaldServerToolsBackend;

public interface IHubRequestResolver
{
    public Task<byte[]> HandleAsync(HubRequestContext ctx, byte[] request, CancellationToken ct);
}