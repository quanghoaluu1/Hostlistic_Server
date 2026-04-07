using NotificationService_Domain.Entities;

namespace NotificationService_Domain.Interfaces;

public interface IEmailLogRepository
{
    Task<EmailLog?> GetByIdAsync(Guid id);
    Task<List<EmailLog>> GetAllAsync();
    Task<List<EmailLog>> GetByCampaignIdAsync(Guid campaignId);
    Task AddAsync(EmailLog emailLog);
    Task AddRangeAsync(IEnumerable<EmailLog> emailLogs);
    Task UpdateAsync(EmailLog emailLog);
    Task DeleteAsync(EmailLog emailLog);
    Task SaveChangesAsync();
}
