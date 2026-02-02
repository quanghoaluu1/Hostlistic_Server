namespace EventService_Application.DTOs;

public record CreateEventTypeDto(string Name);
public record UpdateEventTypeDto(EventTypeDto EventType);
public record EventTypeDto(string? Name, bool? IsActive);
public record EventTypeResponse(Guid Id, string Name, bool IsActive);

