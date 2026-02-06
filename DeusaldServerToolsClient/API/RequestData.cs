using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DeusaldServerToolsClient
{
    public static class RequestData
    {
        private static readonly ConcurrentDictionary<Type, string>         _UrlCache            = new();
        private static readonly ConcurrentDictionary<Type, HttpMethodType> _HttpMethodTypeCache = new();

        public static string GetUrl(Type requestType)                     => InnerGetUrl(requestType);
        public static string GetUrl(IRequest request)                     => InnerGetUrl(request.GetType());
        public static string GetUrl<TRequest>() where TRequest : IRequest => InnerGetUrl(typeof(TRequest));
        
        public static HttpMethodType GetHttpMethodType(Type requestType)                     => InnerGetHttpMethodType(requestType);
        public static HttpMethodType GetHttpMethodType(IRequest request)                     => InnerGetHttpMethodType(request.GetType());
        public static HttpMethodType GetHttpMethodType<TRequest>() where TRequest : IRequest => InnerGetHttpMethodType(typeof(TRequest));

        private static string InnerGetUrl(Type t)
        {
            return _UrlCache.GetOrAdd(t, static type =>
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                FieldInfo? field = type.GetField("URL", flags);
                if (field is null || field.FieldType != typeof(string)) throw new InvalidOperationException($"Type {type.FullName} must define `public const string URL`.");

                string? value = (string?)field.GetRawConstantValue();
                return value ?? throw new InvalidOperationException($"Type {type.FullName}.URL is null.");
            });
        }

        private static HttpMethodType InnerGetHttpMethodType(Type t)
        {
            return _HttpMethodTypeCache.GetOrAdd(t, static type =>
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                FieldInfo? field = type.GetField("HTTP_METHOD_TYPE", flags);
                if (field is null || field.FieldType != typeof(HttpMethodType)) throw new InvalidOperationException($"Type {type.FullName} must define `public const HttpMethodType HTTP_METHOD_TYPE`.");

                HttpMethodType value = (HttpMethodType)field.GetRawConstantValue();
                return value;
            });
        }
    }
}