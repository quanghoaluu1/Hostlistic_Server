using Common;

namespace EventService_Application.DTOs
{
    public class TalentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Organization { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;

        // public ICollection<Lineup> Lineups { get; set; } = new List<Lineup>();

    }

    public class CreateTalentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; } = string.Empty;
        public string? AvatarPublicId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Organization { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateTalentDto
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Type { get; set; }
        = string.Empty;
        public string? Organization
        { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
    }

    public record TalentSearchRequest : BaseQueryParams
    {
        public string? Name { get; init; }
    }

}
