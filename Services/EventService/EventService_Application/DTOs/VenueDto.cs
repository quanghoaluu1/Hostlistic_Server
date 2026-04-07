using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventService_Application.DTOs
{
    public record CreateVenueRequest(
        [Required, StringLength(200, MinimumLength = 1)]
        string Name,

        [StringLength(1000)]
        string? Description,

        [Range(1, 100_000)]
        int Capacity,

        IFormFile? LayoutImage
    );

    public record UpdateVenueRequest(
        [StringLength(200, MinimumLength = 1)]
        string? Name,

        [StringLength(1000)]
        string? Description,

        [Range(1, 100_000)]
        int? Capacity,

        IFormFile? LayoutImage,

        bool RemoveLayout = false  // explicit flag to remove existing image
    );

// === Response DTO ===
    public record VenueResponse(
        Guid Id,
        Guid EventId,
        string Name,
        string? Description,
        int Capacity,
        string? LayoutUrl
    );
}
