using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

/// <summary>
/// Consumes <see cref="EventCompletedMessage"/> published by EventService when an event
/// transitions from OnGoing to Completed.
///
/// Responsibility: auto-create a post-event "Thank You" email campaign targeting all
/// checked-in attendees, then hand off async delivery to <see cref="BulkEmailConsumer"/>
/// via the <see cref="SendBulkEmailCommand"/> message.
///
/// Idempotency: before creating a campaign, the consumer checks whether an auto-reminder
/// campaign already exists for this event to guard against MassTransit redeliveries.
///
/// Clean Architecture note: this consumer sits at the Application layer boundary.
/// It coordinates domain repositories (IEmailCampaignRepository, IEmailLogRepository,
/// IEventRecipientRepository) without containing business logic itself.
/// </summary>
public class EventCompletedConsumer(
    IEmailCampaignRepository campaignRepository,
    IEmailLogRepository emailLogRepository,
    IEventRecipientRepository recipientRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<EventCompletedConsumer> logger) : IConsumer<EventCompletedMessage>
{
    public async Task Consume(ConsumeContext<EventCompletedMessage> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "EventCompletedConsumer: received EventCompletedMessage — Event {EventId} '{EventTitle}', " +
            "Organizer {OrganizerId}, CompletedAt {CompletedAt}.",
            message.EventId, message.EventTitle, message.OrganizerId, message.CompletedAt);

        try
        {
        // ── Idempotency guard ────────────────────────────────────────────────
        // Prevent duplicate campaigns if the message is redelivered by MassTransit.
        var alreadyExists = await campaignRepository.ExistsAutoReminderAsync(
            message.EventId,
            context.CancellationToken);

        if (alreadyExists)
        {
            logger.LogWarning(
                "EventCompletedConsumer: Thank-You campaign already exists for Event {EventId}. Skipping.",
                message.EventId);
            return;
        }

        // ── 1. Resolve checked-in attendees ──────────────────────────────────
        // Diagnostic: also count ALL recipients (regardless of IsCheckedIn) so we can
        // distinguish "nobody booked" from "nobody was marked as checked-in".
        var allRecipients = await recipientRepository.GetRecipientsAsync(
            message.EventId,
            RecipientGroup.AllTicketHolders,
            filter: null);

        logger.LogInformation(
            "EventCompletedConsumer: Event {EventId} has {TotalCount} total ticket holders in EventRecipient table.",
            message.EventId, allRecipients.Count);

        var recipients = await recipientRepository.GetRecipientsAsync(
            message.EventId,
            RecipientGroup.CheckedIn,
            filter: null);

        if (recipients.Count == 0)
        {
            logger.LogWarning(
                "EventCompletedConsumer: No checked-in attendees found for Event {EventId} " +
                "(total recipients in DB: {TotalCount}). " +
                "This means either no attendees have IsCheckedIn=true yet (CheckInSyncConsumer may not have run), " +
                "or the event truly had zero check-ins. Skipping Thank-You campaign.",
                message.EventId, allRecipients.Count);
            return;
        }

        logger.LogInformation(
            "EventCompletedConsumer: {Count} checked-in attendee(s) resolved for Event {EventId}.",
            recipients.Count, message.EventId);

        // ── 2. Create the campaign record ────────────────────────────────────
        var campaign = new EmailCampaign
        {
            Id = Guid.CreateVersion7(),
            EventId = message.EventId,
            CreatedBy = message.OrganizerId,
            Name = $"Thank You — {message.EventTitle}",
            Content = BuildThankYouHtml(message.EventTitle, message.CompletedAt),
            RecipientGroup = RecipientGroup.CheckedIn,
            Status = EmailCampaignStatus.Sending,
            TotalRecipients = recipients.Count,
            SentCount = 0,
            FailedCount = 0,
            SendStartedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsAutoReminder = true
        };

        await campaignRepository.AddAsync(campaign);
        await campaignRepository.SaveChangesAsync();

        logger.LogInformation(
            "EventCompletedConsumer: Campaign {CampaignId} created for Event {EventId}.",
            campaign.Id, message.EventId);

        // ── 3. Create Pending EmailLog entries ───────────────────────────────
        // Each EmailLog represents one pending delivery.  BulkEmailConsumer will
        // load these, send via Resend API, and update their status.
        var emailLogs = recipients.Select(r => new EmailLog
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaign.Id,
            SentTo = r.UserId,
            RecipientEmail = r.Email,
            SentAt = DateTime.UtcNow,
            Status = DeliveryStatus.Pending
        });

        await emailLogRepository.AddRangeAsync(emailLogs);
        await emailLogRepository.SaveChangesAsync();

        // ── 4. Dispatch async send command ───────────────────────────────────
        // BulkEmailConsumer picks this up and processes the EmailLogs in batches,
        // respecting the Redis daily quota and Resend API rate limits.
        await publishEndpoint.Publish(new SendBulkEmailCommand(
            CampaignId: campaign.Id,
            RequestedBy: message.OrganizerId),
            context.CancellationToken);

        logger.LogInformation(
            "EventCompletedConsumer: SendBulkEmailCommand published for Campaign {CampaignId} " +
            "targeting {Count} checked-in attendee(s).",
            campaign.Id, recipients.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "EventCompletedConsumer: unhandled exception processing EventCompletedMessage for Event {EventId}.",
                message.EventId);
            throw; // rethrow so MassTransit retries
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal but well-formed HTML thank-you email body.
    /// Organisers can override this content by editing the campaign in the dashboard.
    /// </summary>
    private static string BuildThankYouHtml(string eventTitle, DateTime completedAt) =>
        $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;color:#1a1a1a;">
          <h1 style="color:#6d28d9;">Thank You for Attending!</h1>
          <p>Dear Attendee,</p>
          <p>
            Thank you for joining us at <strong>{eventTitle}</strong>
            on <strong>{completedAt:MMMM dd, yyyy}</strong>.
            Your presence made the event truly special.
          </p>
          <p>
            We hope you found the experience valuable and look forward to seeing you
            at our future events!
          </p>
          <br/>
          <p style="color:#6b7280;font-size:0.875rem;">
            Warm regards,<br/>
            <strong>The Hostlistic Team</strong>
          </p>
        </div>
        """;
}
