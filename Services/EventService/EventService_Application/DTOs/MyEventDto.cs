using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public record MyEventDto
(
    Guid EventId,
    string Title,
    string? CoverImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    string EventMode,
    int Status,
    string? Location,
    string MyRole,       // "Organizer", "CoOrganizer", "Staff", "Attendee"
    DateTime JoinedAt
);
public record MyEventQueryParams(
    EventRole? Role = null, 
    int? Status = null, 
    string? Search = null, 
    int Page = 1,
    int PageSize = 10
    );