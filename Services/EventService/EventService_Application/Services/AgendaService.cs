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

        // Map raw query result to response DTOs — flat tracks (backward compatible)
        var trackDtos = queryResult.Tracks.Select(t => new AgendaTrackDto
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
        }).ToList();

        // Build day-grouped structure
        // Index EventDay entities by date for O(1) lookup
        var eventDaysByDate = queryResult.EventDays
            .ToDictionary(d => d.Date);

        // Collect all unique dates from sessions
        var sessionDateGroups = queryResult.Tracks
            .SelectMany(t => t.Sessions)
            .Where(s => s.StartTime.HasValue)
            .GroupBy(s => DateOnly.FromDateTime(s.StartTime!.Value))
            .ToDictionary(g => g.Key, g => g.Select(s => s.Id).ToHashSet());

        // Union: dates from EventDay entities + dates from sessions
        var allDates = eventDaysByDate.Keys
            .Union(sessionDateGroups.Keys)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var dayDtos = new List<AgendaDayDto>();
        var dayCounter = 1;

        foreach (var date in allDates)
        {
            eventDaysByDate.TryGetValue(date, out var eventDay);
            sessionDateGroups.TryGetValue(date, out var sessionIdsOnDay);

            // Build tracks with only sessions that start on this date
            var tracksForDay = queryResult.Tracks
                .Select(t => new AgendaTrackDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ColorHex = t.ColorHex,
                    SortOrder = t.SortOrder,
                    Sessions = t.Sessions
                        .Where(s => s.StartTime.HasValue
                            && DateOnly.FromDateTime(s.StartTime.Value) == date)
                        .Select(s => new AgendaSessionDto
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
                })
                .Where(t => t.Sessions.Count > 0)
                .ToList();

            dayDtos.Add(new AgendaDayDto
            {
                EventDayId = eventDay?.Id,
                DayNumber = eventDay?.DayNumber ?? dayCounter,
                Date = date,
                Title = eventDay?.Title,
                Theme = eventDay?.Theme,
                Tracks = tracksForDay
            });

            dayCounter++;
        }

        var response = new AgendaResponse
        {
            EventId = queryResult.EventId,
            EventStartDate = queryResult.EventStartDate,
            EventEndDate = queryResult.EventEndDate,
            Tracks = trackDtos,
            Days = dayDtos
        };

        return ApiResponse<AgendaResponse>.Success(200, "Agenda retrieved", response);
    }
}
