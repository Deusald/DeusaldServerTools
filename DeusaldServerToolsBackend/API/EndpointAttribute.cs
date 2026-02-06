using DeusaldServerToolsClient;

namespace DeusaldServerToolsBackend;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EndpointAttribute(HttpMethodType method, string route) : Attribute
{
    public HttpMethodType Method { get; } = method;
    public string         Route  { get; } = route;

    public EndpointAttribute(Type requestType) : this(RequestData.GetHttpMethodType(requestType), RequestData.GetUrl(requestType)) { }
}