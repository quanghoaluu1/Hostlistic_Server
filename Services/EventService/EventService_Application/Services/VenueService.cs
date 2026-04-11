using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class VenueService(IVenueRepository venueRepository, IEventRepository eventRepository, IPhotoService photoService)
        : IVenueService
    {
        public async Task<ApiResponse<VenueResponse>> CreateAsync(
        Guid eventId, CreateVenueRequest request)
        {
            // 1. Verify event exists
            var eventExists = await eventRepository.EventExistsAsync(eventId);
            if (!eventExists)
                return ApiResponse<VenueResponse>.Fail(404, "Event not found.");

            // 2. Check duplicate name within event
            var nameExists = await venueRepository.ExistsByNameAsync(eventId, request.Name);
            if (nameExists)
                return ApiResponse<VenueResponse>.Fail(409,
                    $"A venue named '{request.Name}' already exists in this event.");

            // 3. Create via domain factory
            var venue = Venue.Create(eventId, request.Name, request.Description, request.Capacity);

            // 4. Upload layout image if provided
            if (request.LayoutImage is not null)
            {
                var uploadResult = await photoService.UploadPhotoAsync(
                    request.LayoutImage, $"events/{eventId}/venues/");

                if (uploadResult.Error is not null)
                    return ApiResponse<VenueResponse>.Fail(500, "Failed to upload layout image.");

            venue.LayoutUrl = uploadResult.SecureUrl.AbsoluteUri;
            venue.LayoutPublicId = uploadResult.PublicId;
        }

            // 5. Persist
            await venueRepository.AddVenueAsync(venue);

            var response = venue.Adapt<VenueResponse>();
            return ApiResponse<VenueResponse>.Success(201, "Venue created successfully.", response);
        }

        public async Task<ApiResponse<VenueResponse>> GetByIdAsync(Guid eventId, Guid venueId)
        {
            var venue = await venueRepository.GetByIdWithinEventAsync(eventId, venueId);
            if (venue is null)
                return ApiResponse<VenueResponse>.Fail(404, "Venue not found.");

            return ApiResponse<VenueResponse>.Success(200, "Retrieved venue.", venue.Adapt<VenueResponse>());
        }

        public async Task<ApiResponse<PagedResult<VenueResponse>>> GetByEventIdAsync(Guid eventId, BaseQueryParams? queryParams = null)
        {
            // No pagination — room count per event is inherently low (2–20).
            // Thesis rationale: "Pagination adds query overhead without meaningful benefit
            // when cardinality is bounded by physical/virtual room constraints."
            var venues = await venueRepository.GetByEventIdAsync(eventId, queryParams);
            var response = venues.Items.Adapt<PagedResult<VenueResponse>>();
            return ApiResponse<PagedResult<VenueResponse>>.Success(200, "Retrieved venues.", response);
        }

        public async Task<ApiResponse<VenueResponse>> UpdateAsync(
            Guid eventId, Guid venueId, UpdateVenueRequest request)
        {
            // 1. Fetch with tracking (need to persist changes)
            var venue = await venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId);
            if (venue is null)
                return ApiResponse<VenueResponse>.Fail(404, "Venue not found.");

            // 2. Duplicate name check (exclude self)
            if (request.Name is not null)
            {
                var nameExists = await venueRepository.ExistsByNameAsync(
                    eventId, request.Name, excludeVenueId: venueId);
                if (nameExists)
                    return ApiResponse<VenueResponse>.Fail(409,
                        $"A venue named '{request.Name}' already exists in this event.");

                venue.Name = request.Name;
            }

            // 3. Apply partial updates
            if (request.Description is not null)
                venue.Description = request.Description;
            if (request.Capacity.HasValue)
                venue.Capacity = request.Capacity.Value;

            // 4. Handle layout image
            if (request.RemoveLayout && venue.LayoutPublicId is not null)
            {
                await photoService.DeletePhotoAsync(venue.LayoutPublicId);
                venue.LayoutUrl = null;
                venue.LayoutPublicId = null;
            }
            else if (request.LayoutImage is not null)
            {
                // Delete old image first
                if (venue.LayoutPublicId is not null)
                    await photoService.DeletePhotoAsync(venue.LayoutPublicId);

                var uploadResult = await photoService.UploadPhotoAsync(
                    request.LayoutImage, $"events/{eventId}/venues/");

                if (uploadResult.Error is not null)
                    return ApiResponse<VenueResponse>.Fail(500, "Failed to upload layout image.");

                venue.LayoutUrl = uploadResult.Url.AbsoluteUri;
                venue.LayoutPublicId = uploadResult.PublicId;
            }

            // 5. Persist
            await venueRepository.UpdateVenueAsync(venue);

            return ApiResponse<VenueResponse>.Success(200, "Venue updated.", venue.Adapt<VenueResponse>());
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid eventId, Guid venueId)
        {
            var venue = await venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId);
            if (venue is null)
                return ApiResponse<bool>.Fail(404, "Venue not found.");

            // Clean up Cloudinary
            if (venue.LayoutPublicId is not null)
                await photoService.DeletePhotoAsync(venue.LayoutPublicId);

            await venueRepository.DeleteVenueAsync(venueId);

            return ApiResponse<bool>.Success(200, "Venue deleted.", true);
        }

        public async Task<ApiResponse<object>> GetDashboardAsync(Guid? eventId)
        {
            var dashboardData = await venueRepository.GetVenueDashboardAsync(eventId);
            return ApiResponse<object>.Success(200, "Retrieved venue dashboard data.", dashboardData);
        }
    }
}
