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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DeusaldSharp;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DeusaldServerToolsClient
{
    [PublicAPI]
    public class APIClient
    {
        private struct Message
        {
            public string MsgId { get; set; }
            public byte[] Data  { get; set; }
        }

        public delegate void OnRequestError(string requestType, HttpStatusCode statusCode, ErrorCode errorCode, string errorMessage, Guid serverErrorId);

        public event Action<string>?     OnConnectedToHub;
        public event Action<Exception?>? OnDisconnectedFromHub;
        public event Action<Exception>?  OnExceptionOnMessage;
        public event Action?             OnUnauthorizedError;

        public HubConnectionState ConnectionState => _HubConnection?.State ?? HubConnectionState.Disconnected;
        public string             JwtToken        { get; private set; } = string.Empty;

        private HubConnection? _HubConnection;
        private string         _BaseUrl;

        private readonly Dictionary<string, List<Delegate>>        _Callbacks           = new();
        private readonly Dictionary<string, Func<byte[], IHubMsg>> _MsgIdToDeserializer = new();
        private readonly ConcurrentQueue<Message>                  _OnMessageFromHubQueue;
        private readonly ConcurrentQueue<Exception?>               _OnDisconnectedFromHubMessagesQueue;
        private readonly bool                                      _WithDispatcher;
        private readonly Dictionary<BackendAddress, string>        _Addresses = new();
        private readonly ILoggerProvider                           _LoggerProvider;
        private readonly ILogger                                   _Logger;
        private readonly HttpClient                                _HttpClient;
        private readonly OnRequestError                            _OnRequestError;

        public APIClient(Dictionary<BackendAddress, string> addresses, bool withDispatcher, OnRequestError onRequestError, ILoggerProvider loggerProvider, Version clientVersion)
        {
            foreach (KeyValuePair<BackendAddress, string> kv in addresses) _Addresses[kv.Key] = kv.Value;
            _WithDispatcher                     = withDispatcher;
            _LoggerProvider                     = loggerProvider;
            _Logger                             = loggerProvider.CreateLogger(GetType().Name);
            _BaseUrl                            = "";
            _OnDisconnectedFromHubMessagesQueue = new ConcurrentQueue<Exception?>();
            _OnMessageFromHubQueue              = new ConcurrentQueue<Message>();
            _OnRequestError                     = onRequestError;
            _HttpClient                         = new HttpClient();
            _HttpClient.DefaultRequestHeaders.Add(BaseClaimNames.CLIENT_VERSION, clientVersion.ToString());
            _Logger.LogInformation("Create Standard Backend Client");
        }

        public void UpdateCustomAddress(string newCustomAddress)
        {
            _Addresses[BackendAddress.Custom] = newCustomAddress;
        }

        public void SetBaseAddress(BackendAddress backendAddress)
        {
            _BaseUrl                = _Addresses[backendAddress];
            _HttpClient.BaseAddress = new Uri(_BaseUrl);
            _Logger.LogInformation("Set Backend Url to: {BaseUrl}", _BaseUrl);
        }

        public void UpdateJwtToken(string jwtToken)
        {
            JwtToken                                        = jwtToken;
            _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        public async Task<Exception?> ConnectToHubAsync(string hubUrl)
        {
            await DisconnectAsync();

            string url = _BaseUrl + hubUrl;

            try
            {
                _HubConnection = new HubConnectionBuilder()
                                .WithUrl(url, options =>
                                 {
                                     options.AccessTokenProvider = () => Task.FromResult(JwtToken)!;
                                     options.SkipNegotiation     = true;
                                     options.Transports          = HttpTransportType.WebSockets;
                                 }).ConfigureLogging(options =>
                                 {
                                     options.SetMinimumLevel(LogLevel.Information);
                                     options.AddProvider(_LoggerProvider);
                                 }).Build();

                _HubConnection.On<string, byte[]>(nameof(OnMessageFromServerAsync), OnMessageFromServerAsync);
                await _HubConnection.StartAsync();

                _HubConnection.Closed += ConnectionOnClosedAsync;
                OnConnectedToHub?.Invoke(url);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_HubConnection == null) return;
            if (_HubConnection.State is not HubConnectionState.Connected and not HubConnectionState.Connecting) return;
            await _HubConnection.StopAsync();
            _HubConnection = null;
        }

        public void Dispatch()
        {
            while (_OnMessageFromHubQueue.Count != 0)
            {
                bool success = _OnMessageFromHubQueue.TryDequeue(out Message msg);
                if (!success) continue;
                ExecuteHubMessage(msg.MsgId, msg.Data);
            }

            while (_OnDisconnectedFromHubMessagesQueue.Count != 0)
            {
                bool success = _OnDisconnectedFromHubMessagesQueue.TryDequeue(out Exception? exception);
                if (!success) continue;
                OnDisconnectedFromHub?.Invoke(exception);
            }
        }

        public void RegisterToHubMsg<T>(Action<T> callback) where T : ProtoMsg<T>, IHubMsg, new()
        {
            string msgId = RequestData.GetHubMsg(typeof(T));

            _MsgIdToDeserializer.TryAdd(msgId, ProtoMsg<T>.Deserialize);

            if (_Callbacks.TryGetValue(msgId, out List<Delegate>? callbackType)) callbackType.Add(callback);
            else _Callbacks.Add(msgId, new List<Delegate> { callback });
        }

        public void UnregisterFromHubMsg<T>(Action<T> callback) where T : ProtoMsg<T>, IHubMsg, new()
        {
            string msgId = RequestData.GetHubMsg(typeof(T));
            _Callbacks[msgId].Remove(callback);
        }

        public async Task<TResponse?> MakeAPIRequestAsync<TRequest, TResponse>(TRequest req, bool ignoreError = false)
            where TRequest : ProtoMsg<TRequest>, IRequest, new()
            where TResponse : ProtoMsg<TResponse>, IResponse, new()
        {
            try
            {
                HttpMethod         httpMethod     = new HttpMethod(RequestData.GetHttpMethodType<TRequest>().ToString());
                string             url            = RequestData.GetUrl<TRequest>();
                HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, url);
                HttpContent        content        = new ByteArrayContent(req.Serialize());
                content.Headers.ContentType = new MediaTypeHeaderValue(System.Net.Mime.MediaTypeNames.Application.Octet);
                requestMessage.Content      = content;
                HttpResponseMessage result = await _HttpClient.SendAsync(requestMessage);

                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == HttpStatusCode.Unauthorized) OnUnauthorizedError?.Invoke();
                    if (ignoreError) return null;
                    _OnRequestError(typeof(TRequest).Name, result.StatusCode, ErrorCode.HttpError, "", Guid.Empty);
                    return null;
                }

                byte[]                    responseDataBytes = await result.Content.ReadAsByteArrayAsync();
                ServerResponse<TResponse> response          = ServerResponse<TResponse>.Deserialize(responseDataBytes);
                if (response.ErrorCode == ErrorCode.Unauthorized) OnUnauthorizedError?.Invoke();

                if (response.Success) return response.Data;
                if (ignoreError) return null;
                _OnRequestError(typeof(TRequest).Name, response.StatusCode, response.ErrorCode, response.ErrorMessage, response.ServerErrorId);
                return null;
            }
            catch (Exception e)
            {
                if (ignoreError) return null;
                _OnRequestError(typeof(TRequest).Name, HttpStatusCode.BadRequest, ErrorCode.ClientError, e.Message, Guid.Empty);
                return null;
            }
        }

        public async Task<TResponse?> MakeHubRequestAsync<TRequest, TResponse>(TRequest req, bool ignoreError = false)
            where TRequest : ProtoMsg<TRequest>, IRequest, new()
            where TResponse : ProtoMsg<TResponse>, IResponse, new()
        {
            if (ConnectionState != HubConnectionState.Connected)
            {
                if (ignoreError) return null;
                _OnRequestError(typeof(TRequest).Name, HttpStatusCode.BadRequest, ErrorCode.ClientError, "Client is not connected to hub.", Guid.Empty);
                return null;
            }

            try
            {
                byte[]                    responseSerialized = await _HubConnection!.InvokeAsync<byte[]>("OnRequestFromClient", RequestData.GetHubUrl<TRequest>(), req.Serialize());
                ServerResponse<TResponse> response           = ServerResponse<TResponse>.Deserialize(responseSerialized);
                if (response.ErrorCode == ErrorCode.Unauthorized) OnUnauthorizedError?.Invoke();

                if (response.Success) return response.Data;
                if (ignoreError) return null;
                _OnRequestError(typeof(TRequest).Name, response.StatusCode, response.ErrorCode, response.ErrorMessage, response.ServerErrorId);
                return null;
            }
            catch (Exception e)
            {
                if (ignoreError) return null;
                _OnRequestError(typeof(TRequest).Name, HttpStatusCode.BadRequest, ErrorCode.ClientError, e.Message, Guid.Empty);
                return null;
            }
        }

        private Task OnMessageFromServerAsync(string msgId, byte[] data)
        {
            try
            {
                if (_WithDispatcher)
                {
                    _OnMessageFromHubQueue.Enqueue(new Message { MsgId = msgId, Data = data });
                }
                else
                {
                    ExecuteHubMessage(msgId, data);
                }
            }
            catch (Exception e)
            {
                OnExceptionOnMessage?.Invoke(e);
            }

            return Task.CompletedTask;
        }

        private void ExecuteHubMessage(string msgId, byte[] data)
        {
            if (!_MsgIdToDeserializer.TryGetValue(msgId, out Func<byte[], IHubMsg>? deserializer)) return;
            if (!_Callbacks.TryGetValue(msgId, out List<Delegate>? callbacks)) return;
            foreach (Delegate callback in callbacks) callback.DynamicInvoke(deserializer(data));
        }

        private Task ConnectionOnClosedAsync(Exception? exception)
        {
            if (_WithDispatcher)
            {
                _OnDisconnectedFromHubMessagesQueue.Enqueue(exception);
            }
            else
            {
                OnDisconnectedFromHub?.Invoke(exception);
            }

            return Task.CompletedTask;
        }
    }
}