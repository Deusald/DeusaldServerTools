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
using System.Security.Claims;
using DeusaldServerToolsClient;
using DeusaldSharp;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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