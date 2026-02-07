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
using DeusaldSharp;
using JetBrains.Annotations;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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