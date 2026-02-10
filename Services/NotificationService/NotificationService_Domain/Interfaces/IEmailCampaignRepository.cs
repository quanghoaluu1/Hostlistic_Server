using NotificationService_Domain.Entities;

namespace NotificationService_Domain.Interfaces;

public interface IEmailCampaignRepository
{
    Task<EmailCampaign?> GetByIdAsync(Guid id);
    Task<List<EmailCampaign>> GetAllAsync();
    Task AddAsync(EmailCampaign emailCampaign);
    Task UpdateAsync(EmailCampaign emailCampaign);
    Task DeleteAsync(EmailCampaign emailCampaign);
    Task SaveChangesAsync();
}
