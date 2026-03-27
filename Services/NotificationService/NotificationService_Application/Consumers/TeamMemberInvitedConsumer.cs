using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

public class TeamMemberInvitedConsumer(
    IEmailService emailService,
    INotificationRepository notificationRepository,
    IUserNotificationRepository userNotificationRepository,
    INotificationPushService pushService,
    ILogger<TeamMemberInvitedConsumer> logger) : IConsumer<TeamMemberInvitedEvent>
{
    public async Task Consume(ConsumeContext<TeamMemberInvitedEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Processing team invite for user {UserId} to event {EventId} as {Role}",
            msg.InvitedUserId, msg.EventId, msg.Role);

        // 1. Send invite email (best-effort)
        await SendInviteEmailAsync(msg);

        // 2. Create in-app notification (critical — MassTransit will retry on failure)
        var notificationId = await CreateInAppNotificationAsync(msg);

        // 3. Push via SignalR (best-effort)
        await PushRealTimeNotificationAsync(msg, notificationId);
    }

    private async Task SendInviteEmailAsync(TeamMemberInvitedEvent msg)
    {
        try
        {
            await emailService.SendTeamMemberInviteEmailAsync(new InviteMemberEmailRequest(
                EventId: msg.EventId,
                EventTitle: msg.EventTitle,
                InvitedUserEmail: msg.InvitedUserEmail,
                InvitedUserName: msg.InvitedUserName,
                Role: msg.Role,
                CustomTitle: msg.CustomTitle,
                InviteToken: msg.InviteToken,
                InviteTokenExpiry: msg.InviteTokenExpiry,
                InvitedByUserName: msg.InvitedByUserName));

            logger.LogInformation("Invite email sent to {Email}", msg.InvitedUserEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send invite email to {Email}", msg.InvitedUserEmail);
        }
    }

    private async Task<Guid> CreateInAppNotificationAsync(TeamMemberInvitedEvent msg)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventId = msg.EventId,
            Title = $"Team invitation: {msg.EventTitle}",
            Content = $"{msg.InvitedByUserName} invited you to join as {msg.CustomTitle ?? msg.Role}",
            Type = NotificationType.TeamInvitation,
            RecipientType = RecipientType.SpecificUsers,
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow,
        };

        await notificationRepository.AddAsync(notification);

        var userNotification = new UserNotification
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = msg.InvitedUserId,
            IsRead = false,
        };

        await userNotificationRepository.AddAsync(userNotification);
        await notificationRepository.SaveChangesAsync();

        return notification.Id;
    }

    private async Task PushRealTimeNotificationAsync(TeamMemberInvitedEvent msg, Guid notificationId)
    {
        try
        {
            await pushService.PushToUserAsync(msg.InvitedUserId, new
            {
                Id = notificationId,
                Type = "TeamInvitation",
                Title = $"Team invitation: {msg.EventTitle}",
                Body = $"{msg.InvitedByUserName} invited you to join as {msg.CustomTitle ?? msg.Role}",
                EventId = msg.EventId,
                Role = msg.Role,
                InviteToken = msg.InviteToken,
                CreatedAt = DateTime.UtcNow,
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR push failed for user {UserId}", msg.InvitedUserId);
        }
    }

}
