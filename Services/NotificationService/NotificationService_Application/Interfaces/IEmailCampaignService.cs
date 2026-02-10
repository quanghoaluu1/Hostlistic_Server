using Common;
using NotificationService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

public interface IEmailCampaignService
{
    Task<ApiResponse<EmailCampaignDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<EmailCampaignDto>>> GetAllAsync();
    Task<ApiResponse<EmailCampaignDto>> CreateAsync(CreateEmailCampaignRequest request);
    Task<ApiResponse<EmailCampaignDto>> UpdateAsync(Guid id, UpdateEmailCampaignRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
