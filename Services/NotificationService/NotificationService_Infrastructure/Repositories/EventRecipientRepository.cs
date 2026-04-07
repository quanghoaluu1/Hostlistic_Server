using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Infrastructure.Repositories;

public class EventRecipientRepository(NotificationServiceDbContext dbContext) : IEventRecipientRepository
{
    public async Task UpsertAsync(EventRecipient recipient)
    {
        // Idempotent: check by (EventId, UserId, TicketTypeId) composite
        var existing = await dbContext.EventRecipients
            .FirstOrDefaultAsync(r =>
                r.EventId == recipient.EventId &&
                r.UserId == recipient.UserId &&
                r.TicketTypeId == recipient.TicketTypeId);
 
        if (existing is not null)
        {
            // Update mutable fields only
            existing.Email = recipient.Email;
            existing.FullName = recipient.FullName;
            existing.TicketTypeName = recipient.TicketTypeName;
            existing.SyncedAt = DateTime.UtcNow;
        }
        else
        {
            recipient.Id = Guid.CreateVersion7();
            await dbContext.EventRecipients.AddAsync(recipient);
        }
    }
 
    public async Task<List<EventRecipient>> GetRecipientsAsync(
        Guid eventId,
        RecipientGroup group,
        EmailTargetFilter? filter)
    {
        var query = BuildQuery(eventId, group, filter);
        return await query.AsNoTracking().ToListAsync();
    }
 
    public async Task<int> CountRecipientsAsync(
        Guid eventId,
        RecipientGroup group,
        EmailTargetFilter? filter)
    {
        // Use a simple distinct-user count instead of the full BuildQuery (which uses
        // GroupBy + g.First() for deduplication — that translation can fail in some
        // EF Core versions). Counting distinct UserIds is equivalent and always translates.
        IQueryable<EventRecipient> query = dbContext.EventRecipients
            .Where(r => r.EventId == eventId);

        switch (group)
        {
            case RecipientGroup.SpecificTicketType:
                if (filter?.TicketTypeIds is { Count: > 0 })
                    query = query.Where(r => filter.TicketTypeIds.Contains(r.TicketTypeId!.Value));
                break;

            case RecipientGroup.NotCheckedIn:
                query = query.Where(r => !r.IsCheckedIn);
                break;

            case RecipientGroup.ManualList:
                if (filter?.SpecificUserIds is { Count: > 0 })
                    query = query.Where(r => filter.SpecificUserIds.Contains(r.UserId));
                break;
        }

        if (filter?.PurchasedAfter.HasValue == true)
            query = query.Where(r => r.BookingConfirmedAt >= filter.PurchasedAfter.Value);

        return await query.Select(r => r.UserId).Distinct().CountAsync();
    }
 
    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
 
    /// <summary>
    /// Builds the recipient query based on RecipientGroup + EmailTargetFilter.
    /// All filtering happens at database level via IQueryable → SQL translation.
    /// 
    /// Thesis rationale: "Push computation to the data" — never load all recipients
    /// into memory then filter with LINQ-to-Objects. EF Core translates this to
    /// SQL WHERE clauses with proper index usage.
    /// </summary>
    private IQueryable<EventRecipient> BuildQuery(
        Guid eventId,
        RecipientGroup group,
        EmailTargetFilter? filter)
    {
        IQueryable<EventRecipient> query = dbContext.EventRecipients
            .Where(r => r.EventId == eventId);
 
        switch (group)
        {
            case RecipientGroup.AllTicketHolders:
                // No additional filter — all confirmed bookings for this event
                break;
 
            case RecipientGroup.SpecificTicketType:
                if (filter?.TicketTypeIds is { Count: > 0 })
                    query = query.Where(r => filter.TicketTypeIds.Contains(r.TicketTypeId!.Value));
                break;
 
            case RecipientGroup.NotCheckedIn:
                query = query.Where(r => !r.IsCheckedIn);
                break;
 
            case RecipientGroup.ManualList:
                if (filter?.SpecificUserIds is { Count: > 0 })
                    query = query.Where(r => filter.SpecificUserIds.Contains(r.UserId));
                break;
        }
 
        // Apply optional PurchasedAfter filter (works with any RecipientGroup)
        if (filter?.PurchasedAfter.HasValue == true)
            query = query.Where(r => r.BookingConfirmedAt >= filter.PurchasedAfter.Value);
 
        // Deduplicate by UserId — a user may have multiple ticket types
        // but should only receive one email per campaign
        query = query
            .GroupBy(r => new { r.UserId, r.Email, r.FullName })
            .Select(g => g.First());
 
        return query;
    }
}