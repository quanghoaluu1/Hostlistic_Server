using Common;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;
using Resend;

namespace NotificationService_Application.Services;

/// <summary>
/// Sends Thank-You emails directly via Resend (synchronous, no RabbitMQ dependency).
/// Called by the EventEmailController which is invoked via HTTP from EventService
/// after an event is completed.
/// </summary>
public class ThankYouEmailService(
    IEventRecipientRepository recipientRepository,
    IEmailCampaignRepository campaignRepository,
    IEmailLogRepository emailLogRepository,
    IResend resend,
    ILogger<ThankYouEmailService> logger) : IThankYouEmailService
{
    public async Task<ApiResponse<ThankYouEmailResult>> SendThankYouEmailsAsync(
        Guid eventId,
        string eventTitle,
        Guid organizerId,
        DateTime completedAt,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "ThankYouEmailService: START — Event {EventId} '{EventTitle}', Organizer {OrganizerId}.",
            eventId, eventTitle, organizerId);

        // ── Idempotency guard ────────────────────────────────────────────────
        var alreadyExists = await campaignRepository.ExistsAutoReminderAsync(eventId, ct);
        if (alreadyExists)
        {
            logger.LogWarning(
                "ThankYouEmailService: Thank-You campaign already exists for Event {EventId}. Skipping.",
                eventId);
            return ApiResponse<ThankYouEmailResult>.Success(200,
                "Thank-You campaign already sent for this event.",
                new ThankYouEmailResult(Guid.Empty, 0, 0, 0, "Already sent"));
        }

        // ── 1. Resolve all ticket holders ──────────────────────────────────
        var allRecipients = await recipientRepository.GetRecipientsAsync(
            eventId, RecipientGroup.AllTicketHolders, filter: null);

        logger.LogInformation(
            "ThankYouEmailService: Event {EventId} — {Total} total ticket holders in EventRecipient table.",
            eventId, allRecipients.Count);

        // ── 2. Resolve checked-in attendees ─────────────────────────────────
        var recipients = await recipientRepository.GetRecipientsAsync(
            eventId, RecipientGroup.CheckedIn, filter: null);

        logger.LogInformation(
            "ThankYouEmailService: Event {EventId} — {Count} checked-in recipients found.",
            eventId, recipients.Count);

        if (recipients.Count == 0)
        {
            logger.LogWarning(
                "ThankYouEmailService: No checked-in attendees for Event {EventId} " +
                "(total in DB: {Total}). " +
                "If Total > 0 it means IsCheckedIn was never set — CheckInSyncConsumer may have failed. " +
                "Sending to ALL ticket holders as fallback.",
                eventId, allRecipients.Count);

            // Fallback: if nobody has IsCheckedIn=true but there ARE ticket holders,
            // send to all ticket holders so the email is never silently skipped.
            recipients = allRecipients;

            if (recipients.Count == 0)
            {
                logger.LogWarning(
                    "ThankYouEmailService: No ticket holders at all for Event {EventId}. Aborting.",
                    eventId);
                return ApiResponse<ThankYouEmailResult>.Success(200,
                    "No attendees found for this event.",
                    new ThankYouEmailResult(Guid.Empty, 0, 0, 0, "No attendees"));
            }
        }

        // ── 3. Create the campaign record ──────────────────────────────────
        var campaign = new EmailCampaign
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            CreatedBy = organizerId,
            Name = $"Thank You — {eventTitle}",
            Content = BuildThankYouHtml(eventTitle, completedAt),
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
            "ThankYouEmailService: Campaign {CampaignId} created for Event {EventId} — {Count} recipient(s).",
            campaign.Id, eventId, recipients.Count);

        // ── 4. Create Pending email log entries ───────────────────────────
        var emailLogs = recipients.Select(r => new EmailLog
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaign.Id,
            SentTo = r.UserId,
            RecipientEmail = r.Email,
            SentAt = DateTime.UtcNow,
            Status = DeliveryStatus.Pending
        }).ToList();

        await emailLogRepository.AddRangeAsync(emailLogs);
        await emailLogRepository.SaveChangesAsync();

        // ── 5. Send emails directly via Resend ────────────────────────────
        var totalSent = 0;
        var totalFailed = 0;

        foreach (var log in emailLogs)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var message = new EmailMessage
                {
                    From = "Hostlistic <noreply@hostlistic.tech>",
                    To = log.RecipientEmail,
                    Subject = $"Thank You for Attending {eventTitle}!",
                    HtmlBody = campaign.Content
                };

                await resend.EmailSendAsync(message);

                log.Status = DeliveryStatus.Sent;
                log.SentAt = DateTime.UtcNow;
                totalSent++;

                logger.LogDebug(
                    "ThankYouEmailService: Sent Thank-You email to {Email} (Campaign {CampaignId}).",
                    log.RecipientEmail, campaign.Id);
            }
            catch (Exception ex)
            {
                log.Status = DeliveryStatus.Failed;
                log.ErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
                totalFailed++;

                logger.LogError(ex,
                    "ThankYouEmailService: Failed to send email to {Email} (Campaign {CampaignId}).",
                    log.RecipientEmail, campaign.Id);
            }
        }

        // ── 6. Persist final statuses ─────────────────────────────────────
        await emailLogRepository.SaveChangesAsync();

        campaign.SentCount = totalSent;
        campaign.FailedCount = totalFailed;
        campaign.Status = totalFailed > 0 && totalSent == 0
            ? EmailCampaignStatus.Failed
            : EmailCampaignStatus.Completed;
        campaign.SendCompletedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;

        await campaignRepository.UpdateAsync(campaign);
        await campaignRepository.SaveChangesAsync();

        logger.LogInformation(
            "ThankYouEmailService: DONE — Campaign {CampaignId}, Event {EventId}. " +
            "Sent: {Sent}, Failed: {Failed}, Total: {Total}.",
            campaign.Id, eventId, totalSent, totalFailed, recipients.Count);

        return ApiResponse<ThankYouEmailResult>.Success(200, "Thank-You emails processed.",
            new ThankYouEmailResult(
                CampaignId: campaign.Id,
                TotalRecipients: recipients.Count,
                Sent: totalSent,
                Failed: totalFailed,
                Message: $"Sent {totalSent}/{recipients.Count}, Failed {totalFailed}"));
    }

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
