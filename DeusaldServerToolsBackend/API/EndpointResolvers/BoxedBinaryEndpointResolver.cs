using System.Net;
using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

public abstract class BoxedBinaryEndpointResolver<TRequest, TResponse>(ServerResponseHelper serverResponseHelper) : IEndpointResolver
    where TRequest : ProtoMsg<TRequest>, IRequest, new()
    where TResponse : ProtoMsg<TResponse>, IResponse, new()
{
    protected abstract int  RequestMaxBytesCount { get; }
    protected virtual  bool CheckMaintenanceMode => true;
    protected virtual  bool InternalAPI          => false;

    public async Task HandleAsync(HttpContext context, CancellationToken ct)
    {
        try
        {
            byte[] requestInBinary = await BinaryHttp.ReadRequestBytesAsync(context, RequestMaxBytesCount, ct);

            byte[] response = InternalAPI
                                  ? await serverResponseHelper.Handle_Internal_REST_API_Request<TRequest, TResponse>(requestInBinary, HandleAsync)
                                  : await serverResponseHelper.Handle_REST_API_Request<TRequest, TResponse>(context.User, context.Request, requestInBinary, HandleAsync, CheckMaintenanceMode);

            await BinaryHttp.WriteResponseBytesAsync(context, response, ct);
        }
        catch (BadHttpRequestException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await BinaryHttp.WriteResponseBytesAsync(context,
                new ServerResponse<TResponse>((HttpStatusCode)ex.StatusCode, ErrorCode.ServerInternalError, "Error while processing request.", Guid.Empty).Serialize(), ct);
        }
    }

    protected abstract Task<TResponse> HandleAsync(ClaimsPrincipal? claimsPrincipal, TRequest request);
}