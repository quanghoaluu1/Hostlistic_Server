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
    /// <summary>Marks all EventRecipient rows for a given (EventId, UserId) as checked-in.</summary>
    Task MarkCheckedInAsync(Guid eventId, Guid userId);
 
    Task SaveChangesAsync();
}