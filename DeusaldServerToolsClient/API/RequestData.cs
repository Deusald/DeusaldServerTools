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
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

namespace DeusaldServerToolsClient
{
    [PublicAPI]
    public static class RequestData
    {
        private static readonly ConcurrentDictionary<Type, string>         _UrlCache            = new();
        private static readonly ConcurrentDictionary<Type, string>         _HubUrlCache         = new();
        private static readonly ConcurrentDictionary<Type, string>         _HubMsgCache         = new();
        private static readonly ConcurrentDictionary<Type, HttpMethodType> _HttpMethodTypeCache = new();

        public static string GetUrl(Type requestType)                     => InnerGetUrl(requestType);
        public static string GetUrl(IRequest request)                     => InnerGetUrl(request.GetType());
        public static string GetUrl<TRequest>() where TRequest : IRequest => InnerGetUrl(typeof(TRequest));

        public static string GetHubUrl(Type requestType)                     => InnerGetHubUrl(requestType);
        public static string GetHubUrl(IRequest request)                     => InnerGetHubUrl(request.GetType());
        public static string GetHubUrl<TRequest>() where TRequest : IRequest => InnerGetHubUrl(typeof(TRequest));
        
        public static string GetHubMsg(Type requestType)                     => InnerGetHubMsg(requestType);
        public static string GetHubMsg(IRequest request)                     => InnerGetHubMsg(request.GetType());
        public static string GetHubMsg<TRequest>() where TRequest : IRequest => InnerGetHubMsg(typeof(TRequest));

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

        private static string InnerGetHubUrl(Type t)
        {
            return _HubUrlCache.GetOrAdd(t, static type =>
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                FieldInfo? field = type.GetField("HUB_URL", flags);
                if (field is null || field.FieldType != typeof(string)) throw new InvalidOperationException($"Type {type.FullName} must define `public const string HUB_URL`.");

                string? value = (string?)field.GetRawConstantValue();
                return value ?? throw new InvalidOperationException($"Type {type.FullName}.HUB_URL is null.");
            });
        }
        
        private static string InnerGetHubMsg(Type t)
        {
            return _HubMsgCache.GetOrAdd(t, static type =>
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                FieldInfo? field = type.GetField("HUB_MSG", flags);
                if (field is null || field.FieldType != typeof(string)) throw new InvalidOperationException($"Type {type.FullName} must define `public const string HUB_MSG`.");

                string? value = (string?)field.GetRawConstantValue();
                return value ?? throw new InvalidOperationException($"Type {type.FullName}.HUB_MSG is null.");
            });
        }

        private static HttpMethodType InnerGetHttpMethodType(Type t)
        {
            return _HttpMethodTypeCache.GetOrAdd(t, static type =>
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                FieldInfo? field = type.GetField("HTTP_METHOD_TYPE", flags);
                if (field is null || field.FieldType != typeof(HttpMethodType))
                    throw new InvalidOperationException($"Type {type.FullName} must define `public const HttpMethodType HTTP_METHOD_TYPE`.");

                // ReSharper disable once RedundantSuppressNullableWarningExpression
                HttpMethodType value = (HttpMethodType)field.GetRawConstantValue()!;
                return value;
            });
        }
    }
}