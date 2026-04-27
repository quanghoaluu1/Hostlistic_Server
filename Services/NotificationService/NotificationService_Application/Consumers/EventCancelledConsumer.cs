using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

public class EventCancelledConsumer(
    IBookingServiceClient bookingServiceClient,
    IEmailService emailService,
    INotificationRepository notificationRepository,
    IUserNotificationRepository userNotificationRepository,
    INotificationPushService pushService,
    ILogger<EventCancelledConsumer> logger) : IConsumer<EventCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventCancelledIntegrationEvent> context)
    {
        var msg = context.Message;
        
        logger.LogInformation("Processing Event Cancellation for Event {EventId}: {EventName}", msg.EventId, msg.Title);
        
        // 1. Resolve recipients (All confirmed ticket holders)
        var recipients = await bookingServiceClient.GetEmailRecipientsAsync(msg.EventId, (int)RecipientGroup.AllTicketHolders);
        
        if (recipients == null || !recipients.Any())
        {
            logger.LogInformation("No recipients found for cancelled event {EventId}", msg.EventId);
            return;
        }
        
        // 2. Create in-app notification
        var notificationId = await CreateInAppNotificationsAsync(msg, recipients.Select(r => r.UserId).Distinct().ToList());
        
        // 3. Push real-time notification
        await PushRealTimeNotificationsAsync(msg, notificationId, recipients.Select(r => r.UserId).Distinct().ToList());
        
        // 4. Send emails
        await SendCancellationEmailsAsync(msg, recipients);
    }
    
    private async Task<Guid> CreateInAppNotificationsAsync(EventCancelledIntegrationEvent msg, List<Guid> userIds)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventId = msg.EventId,
            Title = $"Event Cancelled: {msg.Title}",
            Content = $"The event '{msg.Title}' has been cancelled. Reason: {msg.Reason}. A full refund has been credited to your internal wallet.",
            Type = NotificationType.EventCancellation,
            RecipientType = RecipientType.Attendees,
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow,
        };

        await notificationRepository.AddAsync(notification);

        foreach (var userId in userIds)
        {
            var userNotification = new UserNotification
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = userId,
                IsRead = false,
            };
            await userNotificationRepository.AddAsync(userNotification);
        }

        await notificationRepository.SaveChangesAsync();
        return notification.Id;
    }
    
    private async Task PushRealTimeNotificationsAsync(EventCancelledIntegrationEvent msg, Guid notificationId, List<Guid> userIds)
    {
        var payload = new
        {
            Id = notificationId,
            Type = "EventCancellation",
            Title = $"Event Cancelled: {msg.Title}",
            Body = $"The event '{msg.Title}' has been cancelled. A full refund has been credited to your internal wallet.",
            EventId = msg.EventId,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var userId in userIds)
        {
            try
            {
                await pushService.PushToUserAsync(userId, payload);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "SignalR push failed for user {UserId}", userId);
            }
        }
    }
    
    private async Task SendCancellationEmailsAsync(EventCancelledIntegrationEvent msg, IEnumerable<Dtos.ServiceClientDtos.EmailRecipientDto> recipients)
    {
        var dashboardUrl = $"https://hostlistic.tech/events/{msg.EventId}/dashboard";
        var subject = $"[Important] Event Cancelled: {msg.Title}";
        
        foreach (var recipient in recipients)
        {
            try
            {
                var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>Event Cancelled</h2>
                        <p>Hi {recipient.FullName},</p>
                        <p>We regret to inform you that the event <strong>{msg.Title}</strong> has been cancelled.</p>
                        
                        <div style='background-color: #f8f9fa; border-left: 4px solid #e74c3c; padding: 15px; margin: 20px 0;'>
                            <p><strong>Reason:</strong> {msg.Reason}</p>
                            <p><strong>Cancelled On:</strong> {msg.CancelledAt.ToString("MMM dd, yyyy HH:mm")}</p>
                        </div>
                        
                        <p><strong>Refund Information:</strong></p>
                        <p>A full refund for your ticket purchase has been automatically processed and credited to your internal wallet. You can use these funds for future events or withdraw them from your dashboard.</p>
                        
                        <div style='margin-top: 30px;'>
                            <a href='{dashboardUrl}' style='background-color: #3498db; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Go to Dashboard</a>
                        </div>
                        
                        <p style='margin-top: 40px; font-size: 12px; color: #7f8c8d;'>
                            Thank you for your understanding.<br>
                            The Hostlistic Team
                        </p>
                    </div>
                </body>
                </html>";

                await emailService.SendEmailAsync(recipient.Email, subject, htmlBody);
                logger.LogInformation("Sent cancellation email to {Email}", recipient.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send cancellation email to {Email}", recipient.Email);
            }
        }
    }
}
