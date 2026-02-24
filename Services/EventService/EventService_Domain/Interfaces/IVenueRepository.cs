using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IVenueRepository
    {
        Task<Venue> AddVenueAsync(Venue venue);
        Task<Venue> GetVenueByIdAsync(Guid id);
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<Venue> UpdateVenueAsync(Venue venue);
        Task<bool> DeleteVenueAsync(Guid id);
    }
}
