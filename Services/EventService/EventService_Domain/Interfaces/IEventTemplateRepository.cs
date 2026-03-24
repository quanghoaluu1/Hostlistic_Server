using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface IEventTemplateRepository
{
    Task<IReadOnlyList<EventTemplate>> GetByCreatorAsync(Guid createdBy);
    Task<EventTemplate?> GetByIdAsync(Guid id);
    Task<PagedResult<EventTemplate>> GetEventTemplateByCreatorAsync(Guid createdBy, int pageNumber, int pageSize, string? sortBy = null);
    Task AddAsync(EventTemplate entity);
    Task UpdateAsync(EventTemplate entity);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
