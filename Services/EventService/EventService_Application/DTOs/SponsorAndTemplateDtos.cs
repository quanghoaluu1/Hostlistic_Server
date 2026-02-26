using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class EventTemplateDto
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventTemplateConfigDto Config { get; set; } = new();
}

public class EventTemplateConfigDto
{
    public string ThemeColor { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; } = string.Empty;
    public List<TemplateTicketDto> DefaultTickets { get; set; } = new();
}

public class TemplateTicketDto
{
    public string Name { get; set; } = string.Empty;
    public float Price { get; set; }
}

public class CreateEventTemplateDto
{
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventTemplateConfigDto Config { get; set; } = new();
}

public class UpdateEventTemplateDto
{
    public string? Name { get; set; }
    public EventTemplateConfigDto? Config { get; set; }
}

public class SponsorTierDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class CreateSponsorTierDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class UpdateSponsorTierDto
{
    public string? Name { get; set; }
    public int? Priority { get; set; }
}

public class SponsorDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; } = string.Empty;
    public Guid TierId { get; set; }
}

public class CreateSponsorDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; } = string.Empty;
    public Guid TierId { get; set; }
}

public class UpdateSponsorDto
{
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public Guid? TierId { get; set; }
}

public class SponsorInteractionDto
{
    public Guid Id { get; set; }
    public Guid SponsorId { get; set; }
    public Guid UserId { get; set; }
    public InteractionType InteractionType { get; set; }
    public DateTime InteractionDate { get; set; }
}

public class CreateSponsorInteractionDto
{
    public Guid SponsorId { get; set; }
    public Guid UserId { get; set; }
    public InteractionType InteractionType { get; set; }
}
