using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISponsorInteractionRepository
{
    Task<SponsorInteraction?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<SponsorInteraction>> GetBySponsorIdAsync(Guid sponsorId);
    Task<IReadOnlyList<SponsorInteraction>> GetByUserIdAsync(Guid userId);
    Task AddAsync(SponsorInteraction entity);
    Task SaveChangesAsync();
}
