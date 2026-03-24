using Common;

namespace EventService_Application.DTOs;

public record CreateEventTypeDto(string Name);
public record UpdateEventTypeDto(string? Name, bool? IsActive);
public record EventTypeResponse(Guid Id, string Name, bool IsActive);
public record EventTypeRequest : BaseQueryParams
{
    public string? Name { get; init; }
}

