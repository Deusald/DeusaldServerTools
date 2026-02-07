using Microsoft.AspNetCore.SignalR;

namespace DeusaldServerToolsBackend;

public class HubRequestContext
{
    public required HubCallerContext              Caller  { get; init; }
    public required IHubCallerClients<IHubClient> Clients { get; init; }
    public required IGroupManager                 Groups  { get; init; }
}