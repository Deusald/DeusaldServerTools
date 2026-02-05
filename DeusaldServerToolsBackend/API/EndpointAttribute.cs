namespace DeusaldServerToolsBackend;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EndpointAttribute(HttpMethod method, string route) : Attribute
{
    public HttpMethod Method { get; } = method;
    public string     Route  { get; } = route;
}