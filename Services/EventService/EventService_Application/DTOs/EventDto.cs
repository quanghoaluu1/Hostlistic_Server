using EventService_Domain.Entities;
using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public record EventRequestDto(

    string? Title = null,
    RichTextContent? Description = null,
    EventMode? EventMode = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Location = null,
    Guid? EventTypeId = null,
    string? CoverImageUrl = null,
    int? TotalCapacity = null,
    Guid? VenueId = null,
    bool? IsPublic = null,
    EventStatus? EventStatus = null
);

public record EventResponseDto(
    Guid Id, 
    string Title, 
    RichTextContent Description, 
    EventMode EventMode, 
    string EventTypeName,
    Guid EventTypeId,
    DateTime StartDate, 
    DateTime EndDate, 
    string Location, 
    string CoverImageUrl, 
    int TotalCapacity,
    bool IsPublic,
    VenueResponseDto? Venue, 
    List<TrackResponseDto> Tracks,
    EventStatus EventStatus
);
public record VenueResponseDto(Guid Id, string Name, string Address, string Capacity);

public record SessionResponseDto(Guid Id, string Title, DateTime StartTime, DateTime EndTime, TalentDetailDto[] Talents);

public record TrackResponseDto(Guid Id, string Name, string Description, List<SessionResponseDto> Sessions);

public record TalentDetailDto(Guid Id, string Name, string Type);
    
    
    