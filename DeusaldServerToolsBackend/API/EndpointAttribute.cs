namespace DeusaldServerToolsBackend;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EndpointAttribute(HttpMethodType method, string route) : Attribute
{
    public HttpMethodType Method { get; } = method;
    public string     Route  { get; } = route;
}