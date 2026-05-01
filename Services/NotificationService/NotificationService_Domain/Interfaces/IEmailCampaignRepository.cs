using NotificationService_Domain.Entities;

namespace NotificationService_Domain.Interfaces;

public interface IEmailCampaignRepository
{
    Task<EmailCampaign?> GetByIdAsync(Guid id);
    Task<List<EmailCampaign>> GetAllAsync();
    Task AddAsync(EmailCampaign emailCampaign);
    Task UpdateAsync(EmailCampaign emailCampaign);
    IQueryable<EmailCampaign> GetQueryable();
    Task DeleteAsync(EmailCampaign emailCampaign);
    Task SaveChangesAsync();

    /// <summary>
    /// Returns true if an auto-generated reminder campaign already exists for
    /// the given event. Used for idempotency checks in event-driven consumers.
    /// </summary>
    Task<bool> ExistsAutoReminderAsync(Guid eventId, CancellationToken ct = default);
}
