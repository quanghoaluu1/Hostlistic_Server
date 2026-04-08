using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventService_Api.Hubs;

[Authorize]
public class EventEngagementHub : Hub
{
    public Task JoinEventGroup(string eventId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildEventGroup(eventId));
    }

    public Task LeaveEventGroup(string eventId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildEventGroup(eventId));
    }

    public static string BuildEventGroup(string eventId) => $"event-engagement:{eventId}";
}
