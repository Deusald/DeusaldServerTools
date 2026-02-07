using System.Threading.Tasks;
using DeusaldSharp;

namespace DeusaldServerToolsClient
{
    public static class RequestExtensions
    {
        public static Task<TResponse?> SendRESTAsync<TRequest, TResponse>(
            this IAPIRequest<TRequest, TResponse> request,
            APIClient apiClient,
            bool ignoreError = false)
            where TRequest : ProtoMsg<TRequest>, IAPIRequest<TRequest, TResponse>, new()
            where TResponse : ProtoMsg<TResponse>, IResponse, new()
            => apiClient.MakeAPIRequestAsync<TRequest, TResponse>((TRequest)request, ignoreError);
        
        public static Task<TResponse?> SendHubAsync<TRequest, TResponse>(
            this IHubRequest<TRequest, TResponse> request,
            APIClient apiClient,
            bool ignoreError = false)
            where TRequest : ProtoMsg<TRequest>, IHubRequest<TRequest, TResponse>, new()
            where TResponse : ProtoMsg<TResponse>, IResponse, new()
            => apiClient.MakeHubRequestAsync<TRequest, TResponse>((TRequest)request, ignoreError);
    }
}