using System.Net;
using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;
using Microsoft.AspNetCore.Http;
using NLog;
using LogLevel = DeusaldSharp.LogLevel;

namespace DeusaldServerToolsBackend;

public class ServerResponseHelper(MaintenanceData maintenanceData)
{
    public delegate Task<TResponse> ServerResponseDataDelegate<in TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, string? connectionId, TRequest verifiedRequest,
                                                                                       Dictionary<string, string> additionalScopedProperties);

    private readonly Logger _Logger = LogManager.GetCurrentClassLogger();

    public async Task<byte[]> ServerErrorControllerHandledByteResponseAsync<TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, HttpRequest request,
                                                                                                 Version minClientVersion, Func<Guid, Task<bool>> checkInvalidSecurityStampAsync,
                                                                                                 byte[] binaryRequest, ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate,
                                                                                                 bool ignoreClientVersionCheck, bool checkMaintenanceMode)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        return await ServerErrorHandledByteResponseAsync(claimsPrincipal, null, request.ExtractVersion(), minClientVersion, binaryRequest, serverResponseDelegate, checkMaintenanceMode,
                   false,                                                 checkInvalidSecurityStampAsync);
    }

    public async Task<byte[]> ServerErrorInternalHandledByteResponseAsync<TRequest, TResponse>(byte[] binaryRequest, ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        return await ServerErrorHandledByteResponseAsync(null, null, Versions.MaxVersion, Versions.MaxVersion, binaryRequest, serverResponseDelegate, false, false, _ => Task.FromResult(true));
    }

    public async Task<byte[]> ServerErrorHandledByteResponseAsync<TRequest, TResponse>(ClaimsPrincipal? claimsPrincipal, string? connectionId, Version clientVersion, Version minClientVersion,
                                                                                       byte[] binaryRequest,
                                                                                       ServerResponseDataDelegate<TRequest, TResponse> serverResponseDelegate, bool checkMaintenanceMode,
                                                                                       bool heartBeat, Func<Guid, Task<bool>> checkInvalidSecurityStampAsync)
        where TRequest : ProtoMsg<TRequest>, IRequest, new() where TResponse : ProtoMsg<TResponse>, IResponse, new()
    {
        ServerResponse<TResponse>  serverResponse;
        Dictionary<string, string> additionalScopedProperties = new();

        if (claimsPrincipal?.FindFirst(BaseClaimNames.USER_ID) != null)
        {
            additionalScopedProperties.AddToAdditionalScopedProperties(BaseNLogConsts.USER_ID_KEY, claimsPrincipal.GetUserId());
            bool unauthorized = claimsPrincipal.GetExpireDate() < DateTime.UtcNow || claimsPrincipal.GetBackendVersion() != BaseEnvVariables.BACKEND_VERSION.GetEnvironmentVariable()!;
            unauthorized |= !heartBeat && await checkInvalidSecurityStampAsync(claimsPrincipal.GetSecurityStamp());
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
                if (clientVersion < minClientVersion) throw new ServerException(ErrorCode.ClientUnsupported, LogLevel.Off, "Client is too old");
                TRequest request = ProtoMsg<TRequest>.Deserialize(binaryRequest);
                request.VerifyData();
                claimsPrincipal ??= new ClaimsPrincipal();
                claimsPrincipal.InjectVersion(clientVersion, BaseClaimNames.CLIENT_VERSION);
                TResponse responseData = await serverResponseDelegate(claimsPrincipal, connectionId, request, additionalScopedProperties);
                serverResponse = new ServerResponse<TResponse>(responseData);
            }
            catch (ServerException serverException)
            {
                serverResponse = new ServerResponse<TResponse>(HttpStatusCode.BadRequest, serverException.ErrorCode, serverException.MessageToClient, serverException.ErrorId);
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_ID,   serverException.ErrorId);
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_TYPE, serverException.ErrorCode);
                foreach (KeyValuePair<string, string> pair in additionalScopedProperties) ScopeContext.PushProperty(pair.Key, pair.Value);
                string requestResponseType = $"({typeof(TRequest)}/{typeof(TResponse)}) ";
                _Logger.Log(serverException.ServerLogLevel.ToNLog(), requestResponseType + serverException.Message);
            }
            catch (Exception exception)
            {
                Guid errorId = Guid.NewGuid();
                serverResponse = new ServerResponse<TResponse>(HttpStatusCode.InternalServerError, ErrorCode.ServerInternalError, "Unexpected server error.", errorId);
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_ID,   errorId);
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_TYPE, ErrorCode.ServerInternalError);
                foreach (KeyValuePair<string, string> pair in additionalScopedProperties) ScopeContext.PushProperty(pair.Key, pair.Value);
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
            IEnumerable<KeyValuePair<string, object>> scopedProperties = ScopeContext.GetAllProperties()!;

            object? errorIdRaw = scopedProperties.FirstOrDefault(x => x.Key == BaseNLogConsts.ERROR_ID);

            Guid errorId = errorIdRaw == null ? Guid.NewGuid() : Guid.Parse((string)errorIdRaw);

            if (errorIdRaw == null)
            {
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_ID,   errorId);
                ScopeContext.PushProperty(BaseNLogConsts.ERROR_TYPE, ErrorCode.ServerInternalError);
                foreach (KeyValuePair<string, string> pair in additionalScopedProperties) ScopeContext.PushProperty(pair.Key, pair.Value);
            }

            serverResponse = new ServerResponse<TResponse>(HttpStatusCode.InternalServerError, ErrorCode.ServerInternalError, "Unexpected server error.", errorId);
            binaryResponse = serverResponse.Serialize();
            string requestResponseType = $"({typeof(TRequest)}/{typeof(TResponse)}) ";
            _Logger.Fatal(exception, requestResponseType);
        }

        return binaryResponse;
    }
}