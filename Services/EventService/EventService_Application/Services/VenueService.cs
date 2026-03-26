using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IPhotoService _photoService;
        public VenueService(IVenueRepository venueRepository, IPhotoService photoService)
        {
            _venueRepository = venueRepository;
            _photoService = photoService;
        }

        public async Task<ApiResponse<VenueDto>> CreateVenueAsync(CreateVenueDto createVenueDto)
        {
            if (string.IsNullOrEmpty(createVenueDto.Name) || string.IsNullOrEmpty(createVenueDto.Location) || createVenueDto.Capacity < 0 || createVenueDto.LayoutUrl == null)
                return ApiResponse<VenueDto>.Fail(400, "Invalid venue data.");

            var imageUploadResult = await _photoService.UploadPhotoAsync(createVenueDto.LayoutUrl, "venue-images/");
            if (imageUploadResult.Error != null)
                return ApiResponse<VenueDto>.Fail(500, "Failed to upload venue layout image.");
            var newVenue = new Venue
            {
                Id = Guid.NewGuid(),
                Name = createVenueDto.Name,
                Location = createVenueDto.Location,
                Capacity = createVenueDto.Capacity,
                LayoutUrl = imageUploadResult.Url.AbsoluteUri
            };
            await _venueRepository.AddVenueAsync(newVenue);
            var venueDto = newVenue.Adapt<VenueDto>();
            return ApiResponse<VenueDto>.Success(200, "Created venue successfully", venueDto);
        }

        public async Task<ApiResponse<VenueDto>> GetVenueByIdAsync(Guid id)
        {
            var venue = await _venueRepository.GetVenueByIdAsync(id);
            if (venue == null)
                return ApiResponse<VenueDto>.Fail(404, "Venue not found.");
            var venueDto = venue.Adapt<VenueDto>();
            return ApiResponse<VenueDto>.Success(200, "Retrieved venue successfully", venueDto);
        }

        public async Task<ApiResponse<PagedResult<VenueDto>>> GetAllVenuesAsync(BaseQueryParams request)
        {
            var venues = await _venueRepository.GetAllVenuesAsync(request);
            var venueDtos = venues.Adapt<List<VenueDto>>();
            var result = new PagedResult<VenueDto>
            (
                venueDtos,
                venues.TotalItems,
                venues.CurrentPage,
                venues.PageSize
            );
            return ApiResponse<PagedResult<VenueDto>>.Success(200, "Retrieved venues successfully", result);
        }

        public async Task<ApiResponse<VenueDto>> UpdateVenueAsync(Guid id, CreateVenueDto updateVenueDto)
        {
            var existingVenue = await _venueRepository.GetVenueByIdAsync(id);
            if (existingVenue == null)
                return ApiResponse<VenueDto>.Fail(404, "Venue not found.");

            if (!string.IsNullOrEmpty(updateVenueDto.Name))
                existingVenue.Name = updateVenueDto.Name;
            if (!string.IsNullOrEmpty(updateVenueDto.Location))
                existingVenue.Location = updateVenueDto.Location;
            if (updateVenueDto.Capacity >= 0 && updateVenueDto.Capacity != existingVenue.Capacity)
                existingVenue.Capacity = updateVenueDto.Capacity;
            if (updateVenueDto.LayoutUrl != null)
            {
                var imageUploadResult = await _photoService.UploadPhotoAsync(updateVenueDto.LayoutUrl, "venue-images/");
                if (imageUploadResult.Error != null)
                    return ApiResponse<VenueDto>.Fail(500, "Failed to upload venue layout image.");
                existingVenue.LayoutUrl = imageUploadResult.Url.AbsoluteUri;
            }
            await _venueRepository.UpdateVenueAsync(existingVenue);
            var venueDto = existingVenue.Adapt<VenueDto>();
            return ApiResponse<VenueDto>.Success(200, "Updated venue successfully", venueDto);
        }

        public async Task<ApiResponse<bool>> DeleteVenueAsync(Guid id)
        {
            var success = await _venueRepository.DeleteVenueAsync(id);
            if (!success)
                return ApiResponse<bool>.Fail(404, "Venue not found.");
            return ApiResponse<bool>.Success(200, "Deleted venue successfully", true);
        }
    }
}
