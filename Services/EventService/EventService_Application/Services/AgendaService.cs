using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Infrastructure.Interfaces;

namespace EventService_Application.Services;

public class AgendaService(IAgendaRepository agendaRepository) : IAgendaService
{
    public async Task<ApiResponse<AgendaResponse>> GetAgendaAsync(Guid eventId, Guid? currentUserId)
    {
        var queryResult = await agendaRepository.GetAgendaAsync(eventId, currentUserId);
 
        if (queryResult is null)
            return ApiResponse<AgendaResponse>.Fail(404, "Event not found");
 
        // Map raw query result to response DTOs
        var response = new AgendaResponse
        {
            EventId = queryResult.EventId,
            EventStartDate = queryResult.EventStartDate,
            EventEndDate = queryResult.EventEndDate,
            Tracks = queryResult.Tracks.Select(t => new AgendaTrackDto
            {
                Id = t.Id,
                Name = t.Name,
                ColorHex = t.ColorHex,
                SortOrder = t.SortOrder,
                Sessions = t.Sessions.Select(s => new AgendaSessionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    TotalCapacity = s.TotalCapacity,
                    BookedCount = s.BookedCount,
                    IsFull = s.TotalCapacity.HasValue && s.BookedCount >= s.TotalCapacity.Value,
                    VenueName = s.VenueName,
                    Status = s.Status,
                    IsBookedByCurrentUser = s.IsBookedByCurrentUser,
                    Speakers = s.Speakers.Select(sp => new SpeakerBriefDto
                    {
                        TalentId = sp.TalentId,
                        Name = sp.Name,
                        AvatarUrl = sp.AvatarUrl
                    }).ToList()
                }).ToList()
            }).ToList()
        };
 
        return ApiResponse<AgendaResponse>.Success(200, "Agenda retrieved", response);
    }
}