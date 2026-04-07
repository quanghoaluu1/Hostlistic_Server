using Common;
using NotificationService_Application.Dtos;

namespace NotificationService_Application.Interfaces;

public interface ICampaignSendService
{
    Task<ApiResponse<CampaignSendResponse>> TriggerSendAsync(Guid campaignId, Guid requestedBy);
    Task<ApiResponse<CampaignPreviewResponse>> PreviewAsync(Guid campaignId);
    Task<ApiResponse<CampaignStatusResponse>> GetStatusAsync(Guid campaignId);

}