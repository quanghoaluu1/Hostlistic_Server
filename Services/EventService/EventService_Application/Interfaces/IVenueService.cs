using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IVenueService
    {
        Task<ApiResponse<VenueDto>> CreateVenueAsync(CreateVenueDto createVenueDto);
        Task<ApiResponse<VenueDto>> GetVenueByIdAsync(Guid id);
        Task<ApiResponse<PagedResult<VenueDto>>> GetAllVenuesAsync(BaseQueryParams request);
        Task<ApiResponse<VenueDto>> UpdateVenueAsync(Guid id, CreateVenueDto updateVenueDto);
        Task<ApiResponse<bool>> DeleteVenueAsync(Guid id);
    }
}
