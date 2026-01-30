using Common;

namespace EventService_Domain.Entities;

public class EventTemplate : BaseClass
{
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventTemplateConfig Config { get; set; } = new EventTemplateConfig();
}

public class EventTemplateConfig
{
    public string ThemeColor { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; } = string.Empty;
    public List<TemplateTicket> DefaultTickets { get; set; } = [];
}

public class TemplateTicket
{
    public string Name { get; set; } = string.Empty;
    public float Price { get; set; }
}