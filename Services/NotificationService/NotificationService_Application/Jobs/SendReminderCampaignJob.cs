using Hangfire;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos.ServiceClientDtos;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;
using Resend;

namespace NotificationService_Application.Jobs;

public class SendReminderCampaignJob(
    IEmailCampaignRepository campaignRepository,
    IBookingServiceClient bookingServiceClient,
    IResend resend,
    ILogger<SendReminderCampaignJob> logger)
{
    private const int BatchSize = 100;


    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    [Queue("reminders")]
    public async Task ExecuteAsync(Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
        {
            logger.LogWarning("Campaign {CampaignId} not found, skipping job", campaignId);
            return;
        }

        if (campaign.Status == EmailCampaignStatus.Cancelled)
        {
            logger.LogInformation("Campaign {CampaignId} is cancelled, skipping job", campaignId);
            return;
        }

        // Mark Sending
        campaign.Status        = EmailCampaignStatus.Sending;
        campaign.SendStartedAt = DateTime.UtcNow;
        await campaignRepository.SaveChangesAsync();
        
        try
        {
            // Resolve recipients
            var recipients = await bookingServiceClient.GetEmailRecipientsAsync(
                eventId:         campaign.EventId!.Value,
                recipientGroup:  (int)campaign.RecipientGroup,
                ticketTypeIds:   campaign.TargetFilter?.TicketTypeIds,
                specificUserIds: campaign.TargetFilter?.SpecificUserIds,
                ct:              ct);

            if (recipients.Count == 0)
            {
                logger.LogWarning("Campaign {CampaignId} has 0 recipients", campaignId);
                await MarkCompletedAsync(campaign, 0, 0, ct);
                return;
            }

            campaign.TotalRecipients = recipients.Count;
            await campaignRepository.SaveChangesAsync();

            // Batch send
            var sentCount   = 0;
            var failedCount = 0;

            foreach (var batch in recipients.Chunk(BatchSize))
            {
                var messages = batch.Select(r => new EmailMessage
                {
                    From     = "Hostlistic <noreply@hostlistic.tech>",
                    To       = r.Email,
                    Subject  = campaign.Name,
                    HtmlBody = Personalize(campaign.Content, r)
                }).ToList();

                try
                {
                    await resend.EmailBatchAsync(messages, ct);
                    sentCount += messages.Count;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Batch send failed for campaign {CampaignId}, batch size {Count}",
                        campaignId, messages.Count);
                    failedCount += messages.Count;
                }
            }

            await MarkCompletedAsync(campaign, sentCount, failedCount, ct);

            logger.LogInformation(
                "Campaign {CampaignId} done: sent={Sent}, failed={Failed}",
                campaignId, sentCount, failedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in campaign job {CampaignId}", campaignId);
            campaign.Status = EmailCampaignStatus.Failed;
            await campaignRepository.SaveChangesAsync();
            throw; // Hangfire sẽ retry theo AutomaticRetry config
        }
    }
    
    private async Task MarkCompletedAsync(
        EmailCampaign campaign, int sent, int failed, CancellationToken ct)
    {
        campaign.SentCount       = sent;
        campaign.FailedCount     = failed;
        campaign.SendCompletedAt = DateTime.UtcNow;
        campaign.Status          = failed > 0 && sent == 0
            ? EmailCampaignStatus.Failed
            : EmailCampaignStatus.Completed;

        await campaignRepository.SaveChangesAsync();
    }

    private static string Personalize(string html, EmailRecipientDto recipient) =>
        html.Replace("{{name}}", recipient.FullName,         StringComparison.OrdinalIgnoreCase)
            .Replace("{{ticketType}}", recipient.TicketTypeName, StringComparison.OrdinalIgnoreCase);
}