using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Infrastructure.Repositories;

public class EmailLogRepository(NotificationServiceDbContext dbContext) : IEmailLogRepository
{
    public async Task<EmailLog?> GetByIdAsync(Guid id)
    {
        return await dbContext.EmailLogs.FindAsync(id);
    }

    public async Task<List<EmailLog>> GetAllAsync()
    {
        return await dbContext.EmailLogs
            .OrderByDescending(el => el.SentAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetByCampaignIdAsync(Guid campaignId)
    {
        return await dbContext.EmailLogs
            .Where(el => el.CampaignId == campaignId)
            .OrderByDescending(el => el.SentAt)
            .ToListAsync();
    }

    public async Task AddAsync(EmailLog emailLog)
    {
        await dbContext.EmailLogs.AddAsync(emailLog);
    }

    public async Task AddRangeAsync(IEnumerable<EmailLog> emailLogs)
    {
        await dbContext.EmailLogs.AddRangeAsync(emailLogs);
    }
    public Task UpdateAsync(EmailLog emailLog)
    {
        dbContext.EmailLogs.Update(emailLog);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(EmailLog emailLog)
    {
        dbContext.EmailLogs.Remove(emailLog);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
