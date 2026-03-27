namespace NotificationService_Application.Interfaces;

public interface INotificationPushService
{
    Task PushToUserAsync(Guid userId, object payload);
}
