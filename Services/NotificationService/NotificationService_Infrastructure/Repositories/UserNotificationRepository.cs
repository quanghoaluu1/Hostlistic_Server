using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Infrastructure.Repositories;

public class UserNotificationRepository(NotificationServiceDbContext dbContext) : IUserNotificationRepository
{
    public async Task<UserNotification?> GetByIdAsync(Guid id)
    {
        return await dbContext.UserNotifications.FindAsync(id);
    }

    public async Task<List<UserNotification>> GetAllAsync()
    {
        return await dbContext.UserNotifications.ToListAsync();
    }

    public async Task<List<UserNotification>> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.UserNotifications
            .Where(un => un.UserId == userId)
            .OrderByDescending(un => un.ReadAt)
            .ToListAsync();
    }

    public async Task<List<UserNotification>> GetByNotificationIdAsync(Guid notificationId)
    {
        return await dbContext.UserNotifications
            .Where(un => un.NotificationId == notificationId)
            .ToListAsync();
    }

    public async Task AddAsync(UserNotification userNotification)
    {
        await dbContext.UserNotifications.AddAsync(userNotification);
    }

    public Task UpdateAsync(UserNotification userNotification)
    {
        dbContext.UserNotifications.Update(userNotification);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserNotification userNotification)
    {
        dbContext.UserNotifications.Remove(userNotification);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
