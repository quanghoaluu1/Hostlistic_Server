using Microsoft.AspNetCore.Http;

namespace EventService_Application.DTOs
{
    public class VenueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? LayoutUrl { get; set; } = string.Empty;
    }

    public class CreateVenueDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public IFormFile? LayoutUrl { get; set; }
    }
}
