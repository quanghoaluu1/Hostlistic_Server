using Common;
using NotificationService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

public interface IEmailLogService
{
    Task<ApiResponse<EmailLogDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<EmailLogDto>>> GetAllAsync();
    Task<ApiResponse<List<EmailLogDto>>> GetByCampaignIdAsync(Guid campaignId);
    Task<ApiResponse<EmailLogDto>> CreateAsync(CreateEmailLogRequest request);
    
    Task<ApiResponse<EmailLogDto>> UpdateAsync(Guid id, UpdateEmailLogRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
