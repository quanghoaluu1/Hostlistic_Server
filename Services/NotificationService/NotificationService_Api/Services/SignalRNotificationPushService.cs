using Microsoft.AspNetCore.SignalR;
using NotificationService_Api.Hubs;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Services;

public class SignalRNotificationPushService(IHubContext<NotificationHub> hubContext) : INotificationPushService
{
    public async Task PushToUserAsync(Guid userId, object payload)
    {
        await hubContext.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", payload);
    }
}
