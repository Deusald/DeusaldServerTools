using DeusaldServerToolsClient;
using DeusaldSharp;
using Microsoft.AspNetCore.SignalR;

namespace DeusaldServerToolsBackend;

public static class HubExtensions
{
    public static Task SubscribeAsync<THub, TClient, TSub>(this IHubContext<THub, TClient> hubContext, Guid guid, TSub hubSubscriptionType, string connectionId)
        where THub : Hub<TClient>
        where TClient : class
        where TSub : struct, Enum
    {
        return hubContext.Groups.AddToGroupAsync(connectionId, GetGroupName(guid, hubSubscriptionType));
    }

    public static async Task UnsubscribeAsync<THub, TClient, TSub>(this IHubContext<THub, TClient> hubContext, Guid guid, TSub hubSubscriptionType, string connectionId)
        where THub : Hub<TClient>
        where TClient : class
        where TSub : struct, Enum
    {
        await hubContext.Groups.RemoveFromGroupAsync(connectionId, GetGroupName(guid, hubSubscriptionType));
    }

    public static TClient GetSubscriptionGroup<THub, TClient, TSub>(this IHubContext<THub, TClient> hubContext, Guid guid, TSub hubSubscriptionType, string? except = null)
        where THub : Hub<TClient>
        where TClient : class
        where TSub : struct, Enum
    {
        if (except == null) return hubContext.Clients.Group(GetGroupName(guid, hubSubscriptionType));
        return hubContext.Clients.GroupExcept(GetGroupName(guid, hubSubscriptionType), except);
    }

    public static async Task SendMsgAsync<TMsg>(this IHubClient client, TMsg message) where TMsg : ProtoMsg<TMsg>, IHubMsg, new()
    {
        await client.OnMessageFromServerAsync(RequestData.GetHubMsg(typeof(TMsg)), message.Serialize());
    }
    
    private static string GetGroupName<TSub>(Guid guid, TSub hubSubscriptionType)
        where TSub : struct, Enum
        => $"sub_{hubSubscriptionType.ToString().ToSnakeCase()}_{guid:N}";
}