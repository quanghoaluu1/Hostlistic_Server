using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;

namespace NotificationService_Domain.Interfaces;

public interface IEventRecipientRepository
{
    Task UpsertAsync(EventRecipient recipient);
    Task<List<EventRecipient>> GetRecipientsAsync(
        Guid eventId,
        RecipientGroup group,
        EmailTargetFilter? filter);
    Task<int> CountRecipientsAsync(
        Guid eventId,
        RecipientGroup group,
        EmailTargetFilter? filter);
 
    Task SaveChangesAsync();
}