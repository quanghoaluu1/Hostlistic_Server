using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

public class EventPostponedConsumer(
    IBookingServiceClient bookingServiceClient,
    IEmailService emailService,
    INotificationRepository notificationRepository,
    IUserNotificationRepository userNotificationRepository,
    INotificationPushService pushService,
    ILogger<EventPostponedConsumer> logger) : IConsumer<EventPostponedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventPostponedIntegrationEvent> context)
    {
        var msg = context.Message;
        
        logger.LogInformation("Processing Event Postponement for Event {EventId}: {EventName}", msg.EventId, msg.EventName);
        
        // 1. Resolve recipients (All confirmed ticket holders)
        var recipients = await bookingServiceClient.GetEmailRecipientsAsync(msg.EventId, (int)RecipientGroup.AllTicketHolders);
        
        if (recipients == null || !recipients.Any())
        {
            logger.LogInformation("No recipients found for postponed event {EventId}", msg.EventId);
            return;
        }
        
        // 2. Create in-app notification
        var notificationId = await CreateInAppNotificationsAsync(msg, recipients.Select(r => r.UserId).Distinct().ToList());
        
        // 3. Push real-time notification
        await PushRealTimeNotificationsAsync(msg, notificationId, recipients.Select(r => r.UserId).Distinct().ToList());
        
        // 4. Send emails
        await SendPostponementEmailsAsync(msg, recipients);
    }
    
    private async Task<Guid> CreateInAppNotificationsAsync(EventPostponedIntegrationEvent msg, List<Guid> userIds)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventId = msg.EventId,
            Title = $"Event Postponed: {msg.EventName}",
            Content = $"The event '{msg.EventName}' has been postponed. New Start Time: {(msg.NewStartTime?.ToString("MMM dd, yyyy HH:mm") ?? "TBA")}. New End Time: {(msg.NewEndTime?.ToString("MMM dd, yyyy HH:mm") ?? "TBA")}. Please check your dashboard to accept or request a refund.",
            Type = NotificationType.EventPostponement,
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
    
    private async Task PushRealTimeNotificationsAsync(EventPostponedIntegrationEvent msg, Guid notificationId, List<Guid> userIds)
    {
        var payload = new
        {
            Id = notificationId,
            Type = "EventPostponement",
            Title = $"Event Postponed: {msg.EventName}",
            Body = $"The event '{msg.EventName}' has been postponed. Please check your dashboard.",
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
    
    private async Task SendPostponementEmailsAsync(EventPostponedIntegrationEvent msg, IEnumerable<Dtos.ServiceClientDtos.EmailRecipientDto> recipients)
    {
        var dashboardUrl = $"https://hostlistic.tech/events/{msg.EventId}/dashboard"; // Update base URL logic as needed
        var subject = $"[Important] Event Postponed: {msg.EventName}";
        
        foreach (var recipient in recipients)
        {
            try
            {
                var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #e74c3c;'>Event Postponed</h2>
                        <p>Hi {recipient.FullName},</p>
                        <p>We are writing to inform you that the event <strong>{msg.EventName}</strong> has been postponed.</p>
                        
                        <div style='background-color: #f8f9fa; border-left: 4px solid #e74c3c; padding: 15px; margin: 20px 0;'>
                            <p><strong>Reason:</strong> {msg.Reason}</p>
                            <p><strong>New Start Time:</strong> {(msg.NewStartTime?.ToString("MMM dd, yyyy HH:mm") ?? "To be announced")}</p>
                            <p><strong>New End Time:</strong> {(msg.NewEndTime?.ToString("MMM dd, yyyy HH:mm") ?? "To be announced")}</p>
                        </div>
                        
                        <p><strong>Action Required:</strong></p>
                        <p>Please visit your dashboard to either accept the new date or request a refund for your ticket.</p>
                        
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
                logger.LogInformation("Sent postponement email to {Email}", recipient.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send postponement email to {Email}", recipient.Email);
            }
        }
    }
}
