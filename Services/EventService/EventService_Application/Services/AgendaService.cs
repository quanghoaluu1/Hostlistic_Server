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

        // Resolve event timezone for correct UTC -> local date conversion.
        // EventDay.Date is stored as a local date (generated with the organizer's timezone).
        // Session.StartTime is stored in UTC, so we must convert before extracting DateOnly.
        TimeZoneInfo eventTz = TimeZoneInfo.Utc;
        if (!string.IsNullOrWhiteSpace(queryResult.TimeZoneId))
        {
            try
            {
                eventTz = TimeZoneInfo.FindSystemTimeZoneById(queryResult.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fall back to UTC if timezone ID is invalid
            }
        }

        // Helper: convert UTC DateTime to local DateOnly using event timezone
        DateOnly ToLocalDate(DateTime utcTime) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcTime, eventTz));

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

        var dayDtos = new List<AgendaDayDto>();

        if (queryResult.EventDays.Count > 0)
        {
            // EventDays are authoritative. Only group sessions into known days.
            // Sessions outside any EventDay date are silently excluded (edge case:
            // sessions created outside the event date range).
            foreach (var eventDay in queryResult.EventDays)
            {
                var tracksForDay = queryResult.Tracks
                    .Select(t => new AgendaTrackDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ColorHex = t.ColorHex,
                        SortOrder = t.SortOrder,
                        Sessions = t.Sessions
                            .Where(s => s.StartTime.HasValue
                                && ToLocalDate(s.StartTime!.Value) == eventDay.Date)
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
                    EventDayId = eventDay.Id,
                    DayNumber = eventDay.DayNumber,
                    Date = eventDay.Date,
                    Title = eventDay.Title,
                    Theme = eventDay.Theme,
                    Tracks = tracksForDay
                });
            }
        }
        else
        {
            // No EventDays: derive days from session local dates (fallback path).
            var sessionDateGroups = queryResult.Tracks
                .SelectMany(t => t.Sessions)
                .Where(s => s.StartTime.HasValue)
                .GroupBy(s => ToLocalDate(s.StartTime!.Value))
                .OrderBy(g => g.Key)
                .ToList();

            var dayCounter = 1;
            foreach (var dateGroup in sessionDateGroups)
            {
                var date = dateGroup.Key;
                var sessionIdsOnDay = dateGroup.Select(s => s.Id).ToHashSet();

                var tracksForDay = queryResult.Tracks
                    .Select(t => new AgendaTrackDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ColorHex = t.ColorHex,
                        SortOrder = t.SortOrder,
                        Sessions = t.Sessions
                            .Where(s => sessionIdsOnDay.Contains(s.Id))
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
                    EventDayId = null,
                    DayNumber = dayCounter,
                    Date = date,
                    Title = null,
                    Theme = null,
                    Tracks = tracksForDay
                });

                dayCounter++;
            }
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
