using Common.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService_Application.Consumers;

public class UserSessionOverriddenEventConsumer(IHubContext<AppNotificationHub> hubContext) : IConsumer<UserSessionOverriddenEvent>
{
    public async Task Consume(ConsumeContext<UserSessionOverriddenEvent> context)
    {
        var message = context.Message;
        
        // Send the new SessionId to the specific user. 
        // SubClaimUserIdProvider uses the user's Id as the SignalR User Identifier.
        await hubContext.Clients.User(message.UserId.ToString())
            .SendAsync("ReceiveForceLogout", message.NewSessionId);
    }
}
