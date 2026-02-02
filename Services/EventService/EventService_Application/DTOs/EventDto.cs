using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public record CreateEventDto(

    string Title,
    string Description,
    EventMode EventMode,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    Guid? EventTypeId,
    string CoverImageUrl,
    int TotalCapacity,
    Guid? VenueId
);
public record EventResponseDto(
    Guid Id, 
    string Title, 
    string Description, 
    EventMode EventMode, 
    DateTime StartDate, 
    DateTime EndDate, 
    string Location, 
    string CoverImageUrl, 
    int TotalCapacity,
    VenueResponseDto? Venue, 
    List<TrackResponseDto> Tracks
);
public record VenueResponseDto(Guid Id, string Name, string Address, string Capacity);

public record SessionResponseDto(Guid Id, string Title, DateTime StartTime, DateTime EndTime, string SpeakerName);

public record TrackResponseDto(Guid Id, string Name, string Description, List<SessionResponseDto> Sessions);

    
    
    