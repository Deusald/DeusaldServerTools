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

using System;
using System.Net;
using DeusaldSharp;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace DeusaldServerToolsClient
{
    [PublicAPI]
    public class ServerResponse<T> : ProtoMsg<ServerResponse<T>> where T : ProtoMsg<T>, IResponse, new()
    {
        public bool           Success;
        public HttpStatusCode StatusCode;
        public ErrorCode      ErrorCode;
        public string         ErrorMessage;
        public Guid           ServerErrorId;
        public T?             Data;

        static ServerResponse()
        {
            _model = new ProtoModel<ServerResponse<T>>(
                ProtoField.Bool(1, static (ref ServerResponse<T> o) => ref o.Success),
                ProtoField.HttpStatusCode(2, static (ref ServerResponse<T> o) => ref o.StatusCode),
                ProtoField.SerializableEnum(3, static (ref ServerResponse<T> o) => ref o.ErrorCode),
                ProtoField.String(4, static (ref ServerResponse<T> o) => ref o.ErrorMessage),
                ProtoField.Guid(5, static (ref ServerResponse<T> o) => ref o.ServerErrorId),
                ProtoField.NullableObject(6, static (ref ServerResponse<T> o) => ref o.Data)
            );
        }

        public ServerResponse()
        {
            ErrorMessage = "";
        }

        public ServerResponse(T data)
        {
            Success       = true;
            StatusCode    = HttpStatusCode.OK;
            ErrorCode     = ErrorCode.Success;
            ErrorMessage  = "";
            ServerErrorId = Guid.Empty;
            Data          = data;
        }

        public ServerResponse(HttpStatusCode httpStatusCode, ErrorCode errorCode, string errorMessage, Guid errorId)
        {
            Success       = false;
            StatusCode    = httpStatusCode;
            ErrorCode     = errorCode;
            ErrorMessage  = errorMessage;
            ServerErrorId = errorId;
            Data          = null;
        }
    }
}