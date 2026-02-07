// MIT License

// DeusaldServerTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Net;
using DeusaldServerToolsClient;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace DeusaldServerToolsBackend;

[PublicAPI]
public abstract class GameHubBase(HubResolverRegistry registry, IServiceProvider sp, IRequestLogContext logCtx) : Hub<IHubClient>
{
    public virtual async Task<byte[]> OnRequestFromClient(string requestId, byte[] request)
    {
        CancellationToken ct = Context.ConnectionAborted;

        try
        {
            Type                resolverType = registry.Get(requestId);
            IHubRequestResolver resolver     = (IHubRequestResolver)sp.GetRequiredService(resolverType);

            HubRequestContext ctx = new HubRequestContext
            {
                Caller  = Context,
                Clients = Clients,
                Groups  = Groups
            };
            
            return await resolver.HandleAsync(ctx, request, ct);
        }
        catch (ServerException serverException)
        {
            ServerResponse<ErrorResponse> serverResponse = new ServerResponse<ErrorResponse>(HttpStatusCode.BadRequest,
                serverException.ErrorCode, serverException.MessageToClient, serverException.ErrorId);
            logCtx.SetErrorId(serverResponse.ServerErrorId.ToString());
            logCtx.SetErrorType(serverResponse.ErrorCode.ToString());
            return serverResponse.Serialize();
        }
        catch (Exception e)
        {
            Guid errorId = Guid.NewGuid();
            ServerResponse<ErrorResponse> serverResponse = new ServerResponse<ErrorResponse>(HttpStatusCode.InternalServerError,
                ErrorCode.ServerInternalError, "Internal server error.", errorId);
            logCtx.SetErrorId(errorId.ToString());
            logCtx.SetErrorType(nameof(ErrorCode.ServerInternalError));
            LogManager.GetCurrentClassLogger().Error(e);
            return serverResponse.Serialize();
        }
    }
}