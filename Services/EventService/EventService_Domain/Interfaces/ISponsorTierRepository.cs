using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISponsorTierRepository
{
    Task<SponsorTier?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<SponsorTier>> GetAllSponsorTiersAsync();
    Task AddAsync(SponsorTier entity);
    Task UpdateAsync(SponsorTier entity);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
