using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdWithinEventAsync(Guid eventId, Guid venueId);
        Task<Venue?> GetByIdWithinEventForUpdateAsync(Guid eventId, Guid venueId);
        Task<PagedResult<Venue>> GetByEventIdAsync(Guid eventId, BaseQueryParams request);
        Task<bool> ExistsByNameAsync(Guid eventId, string name, Guid? excludeVenueId = null);
        Task<Venue> AddVenueAsync(Venue venue);
        Task<Venue> GetVenueByIdAsync(Guid id);
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<Venue> UpdateVenueAsync(Venue venue);
        Task<bool> DeleteVenueAsync(Guid id);
        Task<object> GetVenueDashboardAsync(Guid? eventId = null);
    }
}
