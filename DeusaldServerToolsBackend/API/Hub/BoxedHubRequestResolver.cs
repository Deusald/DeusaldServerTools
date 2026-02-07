using System.Net;
using DeusaldServerToolsClient;
using DeusaldSharp;

namespace DeusaldServerToolsBackend;

public abstract class BoxedHubRequestResolver<TRequest, TResponse>(ServerResponseHelper serverResponseHelper) : IHubRequestResolver
    where TRequest : ProtoMsg<TRequest>, IRequest, new()
    where TResponse : ProtoMsg<TResponse>, IResponse, new()
{
    protected abstract int  RequestMaxBytesCount { get; }
    protected virtual  bool CheckMaintenanceMode => true;
    protected virtual  bool HeartBeat            => false;

    public async Task<byte[]> HandleAsync(HubRequestContext ctx, byte[] request, CancellationToken ct)
    {
        if (request.Length > RequestMaxBytesCount)
            return new ServerResponse<TResponse>(HttpStatusCode.RequestEntityTooLarge, ErrorCode.CantDeserializeRequest, "Error while processing request.", Guid.Empty).Serialize();

        return await serverResponseHelper.Handle_Hub_Request<TRequest, TResponse>(ctx, request, HandleAsync, CheckMaintenanceMode, HeartBeat);
    }

    protected abstract Task<TResponse> HandleAsync(HubRequestContext ctx, TRequest request);
}