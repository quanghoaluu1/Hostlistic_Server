using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISponsorInteractionRepository
{
    Task<SponsorInteraction?> GetByIdAsync(Guid id);
    Task<PagedResult<SponsorInteraction>> GetBySponsorIdAsync(Guid sponsorId, BaseQueryParams request);
    Task<PagedResult<SponsorInteraction>> GetByUserIdAsync(Guid userId, BaseQueryParams request);
    Task AddAsync(SponsorInteraction entity);
    Task SaveChangesAsync();
}
