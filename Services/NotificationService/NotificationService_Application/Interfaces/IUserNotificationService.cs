using Common;
using NotificationService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

public interface IUserNotificationService
{
    Task<ApiResponse<UserNotificationDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<List<UserNotificationDto>>> GetAllAsync();
    Task<ApiResponse<List<UserNotificationDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<List<UserNotificationDto>>> GetUnreadByUserIdAsync(Guid userId);
    Task<ApiResponse<UserNotificationDto>> MarkAsReadAsync(Guid id, Guid userId);
    Task<ApiResponse<List<UserNotificationDto>>> GetByNotificationIdAsync(Guid notificationId);
    Task<ApiResponse<UserNotificationDto>> CreateAsync(CreateUserNotificationRequest request);
    Task<ApiResponse<UserNotificationDto>> UpdateAsync(Guid id, UpdateUserNotificationRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
