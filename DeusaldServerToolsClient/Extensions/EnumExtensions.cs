using System;
using System.Net.Http;

namespace DeusaldServerToolsClient
{
    public static class EnumExtensions
    {
        public static HttpMethod ToHttpMethod(this SendMethodType sendMethodType)
        {
            return sendMethodType switch
            {
                SendMethodType.Http_Get    => HttpMethod.Get,
                SendMethodType.Http_Post   => HttpMethod.Post,
                SendMethodType.Http_Patch  => HttpMethod.Patch,
                SendMethodType.Http_Put    => HttpMethod.Put,
                SendMethodType.Http_Delete => HttpMethod.Delete,
                SendMethodType.Http_Head   => HttpMethod.Head,
                _                          => throw new Exception($"Unsupported send method type to http method: {sendMethodType}")
            };
        }
    }
}