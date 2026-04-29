using Common;
using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public record PublicEventDto
(
    Guid Id,
    string Title,
    string? CoverImageUrl,
    DateTime? StartDate,
    DateTime? EndDate,
    string? LocationAddress,
    double? Latitude,
    double? Longitude,
    string EventMode,          // "Online" | "Offline" | "Hybrid"
    string EventStatus,        // "Published" | "OnGoing" | ...
    string? EventTypeName,
    int? TotalCapacity,
    bool IsPublic
    );
public record PublicEventQueryParams : BaseQueryParams
{
    /// <summary>Search by title (case-insensitive contains)</summary>
    public string? Search { get; init; }

    /// <summary>Filter by event mode: 0=Online, 1=Offline, 2=Hybrid</summary>
    public EventMode? EventMode { get; init; }

    /// <summary>Filter by event type ID</summary>
    public Guid? EventTypeId { get; init; }

    /// <summary>Filter by event status (default: only Published)</summary>
    public EventStatus? Status { get; init; }

    /// <summary>Filter by city name substring match on LocationAddress</summary>
    public string? City { get; init; }

    /// <summary>Latitude of the center point for radius-based filtering</summary>
    public double? Lat { get; init; }

    /// <summary>Longitude of the center point for radius-based filtering</summary>
    public double? Lng { get; init; }

    /// <summary>Search radius in kilometers (requires Lat and Lng)</summary>
    public double? RadiusInKm { get; init; }
}