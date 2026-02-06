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
    public delegate Task<TResponse> ServerResponseDataDelegate<in TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, string? connectionId, TRequest verifiedRequest);

    private readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    public async Task<byte[]> ServerErrorAPIRequestHandledByteResponseAsync<TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, HttpRequest request,
                                                                                                 byte[] binaryRequest, ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate,
                                                                                                 bool checkMaintenanceMode)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        claimsPrincipal ??= new ClaimsPrincipal();
        claimsPrincipal.InjectVersion(request.ExtractVersion(), BaseClaimNames.CLIENT_VERSION);
        return await ServerErrorHandledByteResponseAsync(claimsPrincipal, null, binaryRequest, serverResponseDelegate, checkMaintenanceMode, false);
    }

    public async Task<byte[]> ServerErrorInternalHandledByteResponseAsync<TRequest, TResponse>(byte[] binaryRequest, ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        ClaimsPrincipal claimsPrincipal = new();
        claimsPrincipal.InjectVersion(Versions.MaxVersion, BaseClaimNames.CLIENT_VERSION);
        return await ServerErrorHandledByteResponseAsync(claimsPrincipal, null, binaryRequest, serverResponseDelegate, false, false);
    }

    private async Task<byte[]> ServerErrorHandledByteResponseAsync<TRequest, TResponse>(ClaimsPrincipal claimsPrincipal, string? connectionId, byte[] binaryRequest,
                                                                                        ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate, bool checkMaintenanceMode,
                                                                                        bool heartBeat)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        ServerResponse<TResponse> serverResponse;
        Guid                      errorId = Guid.Empty;

        if (claimsPrincipal.FindFirst(BaseClaimNames.USER_ID) != null)
        {
            logCtx.SetUserId(claimsPrincipal.GetUserId().ToString());
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
                TResponse responseData = await serverResponseDelegate(claimsPrincipal, connectionId, request);
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
                errorId = Guid.NewGuid();
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