using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;
using Resend;

namespace NotificationService_Application.Consumers;

/// <summary>
/// Message-driven bulk email consumer. Processes SendBulkEmailCommand from RabbitMQ.
///
/// Sending strategy:
/// 1. Load campaign + all Pending EmailLogs from DB
/// 2. Batch into groups of 10 (configurable)
/// 3. Per batch: check Redis quota → send via Resend API → update EmailLog status
/// 4. If quota exhausted mid-campaign: pause campaign, remaining logs stay Pending
/// 5. On completion: update campaign to Completed/Failed
///
/// Why batch of 10:
/// - Resend free tier has per-second rate limits beyond the daily cap
/// - Small batches allow graceful pause on quota exhaustion
/// - Each batch is a "checkpoint" — if consumer crashes, only current batch is lost
///
/// Fault tolerance:
/// - MassTransit retry policy handles transient RabbitMQ/Resend failures
/// - Individual email failures don't stop the campaign (logged per-EmailLog)
/// - Campaign status accurately reflects partial success (SentCount + FailedCount)
///
/// Thesis note: This implements the "Competing Consumer" pattern — in production,
/// multiple instances can process different campaigns concurrently. The Redis rate
/// limiter provides global coordination across instances.
/// </summary>
public class BulkEmailConsumer(
    IEmailCampaignRepository emailCampaignRepository,
    IEmailLogRepository emailLogRepository,
    IEmailRateLimiter rateLimiter,
    IResend resend,
    ILogger<BulkEmailConsumer> logger) : IConsumer<SendBulkEmailCommand>
{
    private const int BatchSize = 10;
    private static readonly TimeSpan DelayBetweenBatches = TimeSpan.FromSeconds(2);

    public async Task Consume(ConsumeContext<SendBulkEmailCommand> context)
    {
        var campaignId = context.Message.CampaignId;

        logger.LogInformation("BulkEmailConsumer started for Campaign {CampaignId}", campaignId);

        // 1. Load campaign with EmailLogs
        var campaign = await emailCampaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
        {
            logger.LogError("Campaign {CampaignId} not found, discarding message", campaignId);
            return; // Don't throw — don't retry a missing campaign
        }

        if (campaign.Status is not EmailCampaignStatus.Sending)
        {
            logger.LogWarning(
                "Campaign {CampaignId} status is {Status}, expected Sending. Skipping.",
                campaignId, campaign.Status);
            return;
        }

        // 2. Get pending logs (created by CampaignSendService)
        var pendingLogs = campaign.EmailLogs
            .Where(l => l.Status == DeliveryStatus.Pending)
            .ToList();

        if (pendingLogs.Count == 0)
        {
            logger.LogInformation("No pending emails for Campaign {CampaignId}", campaignId);
            await CompleteCampaignAsync(campaign);
            return;
        }

        logger.LogInformation(
            "Processing {Count} pending emails for Campaign {CampaignId}",
            pendingLogs.Count, campaignId);

        // 3. Process in batches
        var batches = pendingLogs.Chunk(BatchSize);
        var totalSent = 0;
        var totalFailed = 0;
        var quotaExhausted = false;

        foreach (var batch in batches)
        {
            // 3a. Check rate limit before each batch
            var consumed = await rateLimiter.TryConsumeAsync(batch.Length);
            if (consumed == 0)
            {
                logger.LogWarning(
                    "Daily quota exhausted for Campaign {CampaignId}. " +
                    "Pausing with {Remaining} emails pending.",
                    campaignId, pendingLogs.Count - totalSent - totalFailed);

                quotaExhausted = true;
                break;
            }

            // 3b. Send each email in the batch (up to consumed count)
            var emailsToSend = batch.Take(consumed);
            foreach (var emailLog in emailsToSend)
            {
                try
                {
                    var message = new EmailMessage
                    {
                        From = "Hostlistic <noreply@hostlistic.tech>",
                        To = emailLog.RecipientEmail,
                        Subject = campaign.Name,
                        HtmlBody = campaign.Content
                    };

                    await resend.EmailSendAsync(message);

                    emailLog.Status = DeliveryStatus.Sent;
                    emailLog.SentAt = DateTime.UtcNow;
                    totalSent++;

                    logger.LogDebug(
                        "Email sent to {Email} for Campaign {CampaignId}",
                        emailLog.RecipientEmail, campaignId);
                }
                catch (Exception ex)
                {
                    emailLog.Status = DeliveryStatus.Failed;
                    emailLog.ErrorMessage = ex.Message.Length > 500
                        ? ex.Message[..500]
                        : ex.Message;
                    totalFailed++;

                    logger.LogError(ex,
                        "Failed to send email to {Email} for Campaign {CampaignId}",
                        emailLog.RecipientEmail, campaignId);
                }
            }

            // 3c. Persist batch results
            await emailLogRepository.SaveChangesAsync();

            // 3d. Update campaign counters (checkpoint)
            campaign.SentCount += totalSent;
            campaign.FailedCount += totalFailed;
            campaign.UpdatedAt = DateTime.UtcNow;
            await emailCampaignRepository.UpdateAsync(campaign);
            await emailCampaignRepository.SaveChangesAsync();

            // Reset batch counters
            totalSent = 0;
            totalFailed = 0;

            // 3e. Delay between batches to respect Resend rate limits
            if (!context.CancellationToken.IsCancellationRequested)
                await Task.Delay(DelayBetweenBatches, context.CancellationToken);
        }

        // 4. Final campaign status update
        if (quotaExhausted)
        {
            campaign.Status = EmailCampaignStatus.Paused;
            campaign.UpdatedAt = DateTime.UtcNow;
            await emailCampaignRepository.UpdateAsync(campaign);
            await emailCampaignRepository.SaveChangesAsync();

            logger.LogInformation(
                "Campaign {CampaignId} paused — daily quota exhausted. " +
                "Sent: {Sent}, Failed: {Failed}, Pending: {Pending}",
                campaignId, campaign.SentCount, campaign.FailedCount,
                campaign.TotalRecipients - campaign.SentCount - campaign.FailedCount);
        }
        else
        {
            await CompleteCampaignAsync(campaign);
        }
    }
    
    private async Task CompleteCampaignAsync(EmailCampaign campaign)
    {
        campaign.Status = campaign.FailedCount > 0 && campaign.SentCount == 0
            ? EmailCampaignStatus.Failed
            : EmailCampaignStatus.Completed;
 
        campaign.SendCompletedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;
        await emailCampaignRepository.UpdateAsync(campaign);
        await emailCampaignRepository.SaveChangesAsync();
 
        logger.LogInformation(
            "Campaign {CampaignId} {Status}. Sent: {Sent}, Failed: {Failed}, Total: {Total}",
            campaign.Id, campaign.Status, campaign.SentCount,
            campaign.FailedCount, campaign.TotalRecipients);
    }

}