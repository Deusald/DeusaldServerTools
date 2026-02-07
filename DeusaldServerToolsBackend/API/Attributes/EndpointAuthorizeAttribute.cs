namespace DeusaldServerToolsBackend;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EndpointAuthorizeAttribute(string policy) : Attribute
{
    public string Policy { get; } = policy;
}