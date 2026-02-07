using System.Net;
using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;
using Microsoft.AspNetCore.Http;
using NLog;
using LogLevel = DeusaldSharp.LogLevel;

namespace DeusaldServerToolsBackend;

public class ServerResponseHelper(MaintenanceData maintenanceData, ISecurityStampChecker securityStampChecker, IRequestLogContext logCtx)
{
    public delegate Task<TResponse> ServerResponse_REST_API_Delegate<in TRequest, TResponse>(ClaimsPrincipal claimsPrincipal, TRequest request);

    public delegate Task<TResponse> ServerResponse_Hub_Delegate<in TRequest, TResponse>(HubRequestContext hubRequestContext, TRequest request);

    private readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    public async Task<byte[]> Handle_REST_API_Request<TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, HttpRequest request,
                                                                           byte[] binaryRequest, ServerResponse_REST_API_Delegate<TRequest, TResponse> serverResponseDelegate,
                                                                           bool checkMaintenanceMode)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        claimsPrincipal ??= new ClaimsPrincipal();
        claimsPrincipal.InjectVersion(request.ExtractVersion(), BaseClaimNames.CLIENT_VERSION);
        return await ServerErrorHandledByteResponseAsync(claimsPrincipal, null, binaryRequest, serverResponseDelegate, null, checkMaintenanceMode, false);
    }

    public async Task<byte[]> Handle_Internal_REST_API_Request<TRequest, TResponse>(byte[] binaryRequest, ServerResponse_REST_API_Delegate<TRequest, TResponse> serverResponseDelegate)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        ClaimsPrincipal claimsPrincipal = new();
        claimsPrincipal.InjectVersion(Versions.MaxVersion, BaseClaimNames.CLIENT_VERSION);
        return await ServerErrorHandledByteResponseAsync(claimsPrincipal, null, binaryRequest, serverResponseDelegate, null, false, false);
    }

    public async Task<byte[]> Handle_Hub_Request<TRequest, TResponse>(HubRequestContext hubRequestContext, byte[] binaryRequest,
                                                                      ServerResponse_Hub_Delegate<TRequest, TResponse> serverResponseDelegate, bool checkMaintenanceMode, bool heartBeat)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        return await ServerErrorHandledByteResponseAsync(hubRequestContext.Caller.User!, hubRequestContext, binaryRequest, null, serverResponseDelegate, checkMaintenanceMode, heartBeat);
    }

    private async Task<byte[]> ServerErrorHandledByteResponseAsync<TRequest, TResponse>(ClaimsPrincipal claimsPrincipal, HubRequestContext? hubContext, byte[] binaryRequest,
                                                                                        ServerResponse_REST_API_Delegate<TRequest, TResponse>? restDelegate,
                                                                                        ServerResponse_Hub_Delegate<TRequest, TResponse>? hubDelegate,
                                                                                        bool checkMaintenanceMode, bool heartBeat)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        ServerResponse<TResponse> serverResponse;
        Guid                      errorId = Guid.Empty;

        if (claimsPrincipal.FindFirst(BaseClaimNames.USER_ID) != null)
        {
            bool unauthorized = claimsPrincipal.GetExpireDate() < DateTime.UtcNow || claimsPrincipal.GetBackendVersion() != BaseEnvVariables.BACKEND_VERSION.GetEnvironmentVariable()!;
            unauthorized |= !heartBeat && await securityStampChecker.IsSecurityStampInvalidAsync(claimsPrincipal.GetSecurityStamp());
            if (unauthorized) return new ServerResponse<TResponse>(HttpStatusCode.Unauthorized, ErrorCode.Unauthorized, "User is not authorized.", Guid.Empty).Serialize();
        }

        if (checkMaintenanceMode && maintenanceData.IsMaintenanceModeOn)
        {
            serverResponse = new ServerResponse<TResponse>(HttpStatusCode.ServiceUnavailable, ErrorCode.MaintenanceModeIsOn, "Maintenance mode is on.", Guid.Empty);
        }
        else
        {
            try
            {
                if (claimsPrincipal.GetClientVersion() < maintenanceData.MinClientVersion)
                    throw new ServerException(ErrorCode.ClientUnsupported, LogLevel.Off, "Client is too old");
                TRequest request = ProtoMsg<TRequest>.Deserialize(binaryRequest);
                request.VerifyData();
                TResponse responseData = restDelegate != null ? await restDelegate(claimsPrincipal, request) : await hubDelegate!(hubContext!, request);
                serverResponse = new ServerResponse<TResponse>(responseData);
            }
            catch (ServerException serverException)
            {
                serverResponse = new ServerResponse<TResponse>(HttpStatusCode.BadRequest, serverException.ErrorCode, serverException.MessageToClient, serverException.ErrorId);
                errorId        = serverException.ErrorId;
                logCtx.SetErrorId(serverException.ErrorId.ToString());
                logCtx.SetErrorType(serverException.ErrorCode.ToString());
                string requestResponseType = $"({typeof(TRequest)}/{typeof(TResponse)}) ";
                _Logger.Log(serverException.ServerLogLevel.ToNLog(), requestResponseType + serverException.Message);
            }
            catch (Exception exception)
            {
                errorId        = Guid.NewGuid();
                serverResponse = new ServerResponse<TResponse>(HttpStatusCode.InternalServerError, ErrorCode.ServerInternalError, "Unexpected server error.", errorId);
                logCtx.SetErrorId(errorId.ToString());
                logCtx.SetErrorType(nameof(ErrorCode.ServerInternalError));
                string requestResponseType = $"({typeof(TRequest)}/{typeof(TResponse)}) ";
                _Logger.Fatal(exception, requestResponseType);
            }
        }

        byte[] binaryResponse;

        try
        {
            binaryResponse = serverResponse.Serialize();
        }
        catch (Exception exception)
        {
            serverResponse = new ServerResponse<TResponse>(HttpStatusCode.InternalServerError, ErrorCode.ServerInternalError, "Unexpected server error.", errorId);
            binaryResponse = serverResponse.Serialize();
            string requestResponseType = $"({typeof(TRequest)}/{typeof(TResponse)}) ";
            _Logger.Fatal(exception, requestResponseType);
        }

        return binaryResponse;
    }
}