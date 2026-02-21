using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Infrastructure.Repositories;

public class EmailCampaignRepository(NotificationServiceDbContext dbContext) : IEmailCampaignRepository
{
    public async Task<EmailCampaign?> GetByIdAsync(Guid id)
    {
        return await dbContext.EmailCampaigns
            .Include(ec => ec.EmailLogs)
            .FirstOrDefaultAsync(ec => ec.Id == id);
    }

    public async Task<List<EmailCampaign>> GetAllAsync()
    {
        return await dbContext.EmailCampaigns
            .OrderByDescending(ec => ec.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(EmailCampaign emailCampaign)
    {
        await dbContext.EmailCampaigns.AddAsync(emailCampaign);
    }

    public Task UpdateAsync(EmailCampaign emailCampaign)
    {
        dbContext.EmailCampaigns.Update(emailCampaign);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(EmailCampaign emailCampaign)
    {
        dbContext.EmailCampaigns.Remove(emailCampaign);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
