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