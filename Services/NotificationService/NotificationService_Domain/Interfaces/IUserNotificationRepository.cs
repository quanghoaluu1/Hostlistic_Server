using NotificationService_Domain.Entities;

namespace NotificationService_Domain.Interfaces;

public interface IUserNotificationRepository
{
    Task<UserNotification?> GetByIdAsync(Guid id);
    Task<List<UserNotification>> GetAllAsync();
    Task<List<UserNotification>> GetByUserIdAsync(Guid userId);
    Task<List<UserNotification>> GetByNotificationIdAsync(Guid notificationId);
    Task AddAsync(UserNotification userNotification);
    Task UpdateAsync(UserNotification userNotification);
    Task DeleteAsync(UserNotification userNotification);
    Task SaveChangesAsync();
}
