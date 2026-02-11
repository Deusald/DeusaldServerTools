using DeusaldServerToolsClient;
// ReSharper disable UnusedTypeParameter

namespace DeusaldServerToolsBackend;

public interface IPairEndpointResolver<TRequest, TResponse> where TRequest : RequestBase<TResponse> where TResponse : ResponseBase, new();