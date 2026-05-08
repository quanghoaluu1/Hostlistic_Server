using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Models;

public record AdminEventQueryParams : BaseQueryParams
{
    public string? Search { get; init; }
    public List<EventMode>? EventModes { get; init; }
    public List<Guid>? EventTypeIds { get; init; }
    public List<EventStatus>? Statuses { get; init; }
}
