using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DeusaldSharp;
using Microsoft.Extensions.Logging;

namespace DeusaldServerToolsClient
{
    public class APIClient
    {
        public delegate void OnRequestError(string requestType, HttpStatusCode statusCode, ErrorCode errorCode, string errorMessage, Guid serverErrorId);

        public event Action? OnUnauthorizedError;

        private string _BaseUrl;

        private readonly Dictionary<BackendAddress, string> _Addresses = new();
        private readonly ILoggerProvider                    _LoggerProvider;
        private readonly ILogger                            _Logger;
        private readonly HttpClient                         _HttpClient;
        private readonly OnRequestError                     _OnRequestError;

        public APIClient(Dictionary<BackendAddress, string> addresses, OnRequestError onRequestError, ILoggerProvider loggerProvider, Version clientVersion)
        {
            foreach (KeyValuePair<BackendAddress, string> kv in addresses) _Addresses[kv.Key] = kv.Value;
            _LoggerProvider = loggerProvider;
            _Logger         = loggerProvider.CreateLogger(GetType().Name);
            _BaseUrl        = "";
            _OnRequestError = onRequestError;
            _HttpClient     = new HttpClient();
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
    }
}