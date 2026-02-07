using DeusaldServerToolsClient;

namespace DeusaldServerToolsBackend;


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class HubRequestAttribute(Type requestType) : Attribute
{
    public string RequestId { get; } = RequestData.GetHubUrl(requestType);
}