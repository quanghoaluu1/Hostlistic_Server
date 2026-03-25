using Common;
using Common.Messages;
using MassTransit;
using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class CampaignSendService(
    IEmailCampaignRepository campaignRepository,
    IEmailLogRepository emailLogRepository,
    IRecipientResolutionService recipientResolver,
    IEmailRateLimiter rateLimiter,
    IPublishEndpoint publishEndpoint
    ) : ICampaignSendService
{
    public async Task<ApiResponse<CampaignSendResponse>> TriggerSendAsync(
        Guid campaignId, Guid requestedBy)
    {
        // 1. Load and validate campaign
        var campaign = await campaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
            return ApiResponse<CampaignSendResponse>.Fail(404, "Campaign not found");
 
        if (campaign.Status is not (EmailCampaignStatus.Draft or EmailCampaignStatus.Paused))
            return ApiResponse<CampaignSendResponse>.Fail(400,
                $"Campaign cannot be sent from '{campaign.Status}' status. Must be Draft or Paused.");
 
        if (campaign.EventId is null)
            return ApiResponse<CampaignSendResponse>.Fail(400,
                "Campaign must be linked to an event to resolve recipients.");
 
        // 2. Resolve recipients
        var recipients = await recipientResolver.ResolveAsync(campaign);
        if (recipients.Count == 0)
            return ApiResponse<CampaignSendResponse>.Fail(400,
                "No recipients found for the selected targeting criteria.");
 
        // 3. Check daily quota
        var dailyRemaining = await rateLimiter.GetRemainingQuotaAsync();
        if (dailyRemaining <= 0)
            return ApiResponse<CampaignSendResponse>.Fail(429,
                "Daily email quota exceeded. Campaign will be queued for tomorrow.");
 
        // 4. Create EmailLog entries (Pending) — atomic tracking from the start
        var emailLogs = recipients.Select(r => new EmailLog
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            SentTo = r.UserId,
            RecipientEmail = r.Email,
            SentAt = DateTime.UtcNow,
            Status = DeliveryStatus.Pending
        }).ToList();
 
        await emailLogRepository.AddRangeAsync(emailLogs);
 
        // 5. Update campaign status
        campaign.Status = EmailCampaignStatus.Sending;
        campaign.TotalRecipients = recipients.Count;
        campaign.SentCount = 0;
        campaign.FailedCount = 0;
        campaign.SendStartedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;
 
        await campaignRepository.UpdateAsync(campaign);
        await campaignRepository.SaveChangesAsync();
 
        // 6. Publish command for async processing
        await publishEndpoint.Publish(new SendBulkEmailCommand(
            CampaignId: campaignId,
            RequestedBy: requestedBy
        ));
 
        return ApiResponse<CampaignSendResponse>.Success(202, "Campaign send initiated", new CampaignSendResponse
        {
            CampaignId = campaignId,
            RecipientCount = recipients.Count,
            Status = "Sending",
            Message = $"Sending to {recipients.Count} recipients. Track progress via GET /campaigns/{campaignId}/status"
        });
    }
 
    public async Task<ApiResponse<CampaignPreviewResponse>> PreviewAsync(Guid campaignId)
    {
        var campaign = await campaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
            return ApiResponse<CampaignPreviewResponse>.Fail(404, "Campaign not found");
 
        var recipientCount = await recipientResolver.CountAsync(campaign);
        var dailyRemaining = await rateLimiter.GetRemainingQuotaAsync();
 
        var preview = new CampaignPreviewResponse
        {
            CampaignId = campaignId,
            RecipientCount = recipientCount,
            DailyQuotaRemaining = dailyRemaining,
            CanSend = recipientCount > 0 && dailyRemaining > 0,
            Warning = recipientCount > dailyRemaining
                ? $"Only {dailyRemaining} of {recipientCount} emails can be sent today. " +
                  "Remaining will be paused until tomorrow's quota resets."
                : null
        };
 
        return ApiResponse<CampaignPreviewResponse>.Success(200, "Preview generated", preview);
    }
 
    public async Task<ApiResponse<CampaignStatusResponse>> GetStatusAsync(Guid campaignId)
    {
        var campaign = await campaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
            return ApiResponse<CampaignStatusResponse>.Fail(404, "Campaign not found");
 
        var logs = campaign.EmailLogs;
        var status = new CampaignStatusResponse
        {
            CampaignId = campaignId,
            Status = campaign.Status.ToString(),
            TotalRecipients = campaign.TotalRecipients,
            SentCount = logs.Count(l => l.Status == DeliveryStatus.Sent),
            FailedCount = logs.Count(l => l.Status == DeliveryStatus.Failed),
            PendingCount = logs.Count(l => l.Status == DeliveryStatus.Pending),
            StartedAt = campaign.SendStartedAt,
            CompletedAt = campaign.SendCompletedAt
        };
 
        return ApiResponse<CampaignStatusResponse>.Success(200, "Status retrieved", status);
    }
}