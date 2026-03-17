using Common;
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
public record MyEventQueryParams : BaseQueryParams
{
    public EventRole? Role { get; init; }
    public int? Status { get; init; }
    public string? Search { get; init; }
}