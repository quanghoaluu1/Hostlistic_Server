using Common;
using NotificationService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

public interface INotificationCrudService
{
    Task<ApiResponse<NotificationDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<NotificationDto>>> GetAllAsync();
    Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationRequest request);
    Task<ApiResponse<NotificationDto>> UpdateAsync(Guid id, UpdateNotificationRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
