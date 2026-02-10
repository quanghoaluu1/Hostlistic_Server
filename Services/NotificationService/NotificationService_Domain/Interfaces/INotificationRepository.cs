using NotificationService_Domain.Entities;

namespace NotificationService_Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<List<Notification>> GetAllAsync();
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(Notification notification);
    Task SaveChangesAsync();
}
