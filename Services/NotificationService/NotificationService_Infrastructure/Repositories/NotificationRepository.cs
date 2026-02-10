using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Infrastructure.Repositories;

public class NotificationRepository(NotificationServiceDbContext dbContext) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await dbContext.Notifications
            .Include(n => n.UserNotifications)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await dbContext.Notifications
            .OrderByDescending(n => n.ScheduledDate)
            .ToListAsync();
    }

    public async Task AddAsync(Notification notification)
    {
        await dbContext.Notifications.AddAsync(notification);
    }

    public Task UpdateAsync(Notification notification)
    {
        dbContext.Notifications.Update(notification);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Notification notification)
    {
        dbContext.Notifications.Remove(notification);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
