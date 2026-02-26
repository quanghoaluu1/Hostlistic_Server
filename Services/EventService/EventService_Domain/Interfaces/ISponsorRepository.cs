using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISponsorRepository
{
    Task<IReadOnlyList<Sponsor>> GetByEventIdAsync(Guid eventId);
    Task<IReadOnlyList<Sponsor>> GetByTierIdAsync(Guid tierId);
    Task<Sponsor?> GetByIdAsync(Guid id);
    Task AddAsync(Sponsor entity);
    Task UpdateAsync(Sponsor entity);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
