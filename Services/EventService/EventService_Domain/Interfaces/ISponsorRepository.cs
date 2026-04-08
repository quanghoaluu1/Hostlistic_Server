using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISponsorRepository
{
    Task<IReadOnlyList<Sponsor>> GetByTierIdAsync(Guid tierId);
    Task<Sponsor?> GetByIdAsync(Guid id);
    Task AddAsync(Sponsor entity);
    Task UpdateAsync(Sponsor entity);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();

    // Additional methods specific to Sponsor
    Task<IEnumerable<Sponsor>> GetByEventIdAsync(Guid eventId);
    Task<Sponsor?> GetByIdWithInteractionsAsync(Guid sponsorId);
    Task<bool> ExistsAsync(Guid sponsorId);
}
