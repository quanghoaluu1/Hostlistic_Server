using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IVenueService
    {
        Task<ApiResponse<VenueResponse>> CreateAsync(Guid eventId, CreateVenueRequest request);
        Task<ApiResponse<VenueResponse>> GetByIdAsync(Guid eventId, Guid venueId);
        Task<ApiResponse<IReadOnlyList<VenueResponse>>> GetByEventIdAsync(Guid eventId);
        Task<ApiResponse<VenueResponse>> UpdateAsync(Guid eventId, Guid venueId, UpdateVenueRequest request);
        Task<ApiResponse<bool>> DeleteAsync(Guid eventId, Guid venueId);
    }
}
