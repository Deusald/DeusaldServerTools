// MIT License

// DeusaldServerTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DeusaldServerToolsClient;
using DeusaldSharp;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;

namespace DeusaldServerToolsBackend;

[PublicAPI]
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