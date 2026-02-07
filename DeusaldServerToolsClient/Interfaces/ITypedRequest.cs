namespace DeusaldServerToolsClient
{
    public interface IAPIRequest<TRequest, TResponse> : IRequest where TRequest : IAPIRequest<TRequest, TResponse> { }
    public interface IHubRequest<TRequest, TResponse> : IRequest where TRequest : IHubRequest<TRequest, TResponse> { }
}