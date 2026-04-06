using EventService_Infrastructure.Interfaces;

namespace EventService_Test;

public class AgendaServiceTest
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly AgendaService _sut;

    public AgendaServiceTest()
    {
        _agendaRepository = Substitute.For<IAgendaRepository>();
        _sut = new AgendaService(_agendaRepository);
    }

    // ── GetAgendaAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAgendaAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _agendaRepository.GetAgendaAsync(Arg.Any<Guid>(), Arg.Any<Guid?>())
            .Returns((AgendaQueryResult?)null);

        // Act
        var result = await _sut.GetAgendaAsync(Guid.NewGuid(), null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task GetAgendaAsync_WhenEventFound_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            EventStartDate = DateTime.UtcNow.AddDays(7),
            EventEndDate = DateTime.UtcNow.AddDays(8),
            TimeZoneId = "UTC",
            Tracks = new List<AgendaTrackData>(),
            EventDays = new List<AgendaEventDayData>()
        };
        _agendaRepository.GetAgendaAsync(eventId, Arg.Any<Guid?>()).Returns(queryResult);

        // Act
        var result = await _sut.GetAgendaAsync(eventId, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task GetAgendaAsync_WithTracksAndSessions_MapsToFlatTrackList()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionTime = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            TimeZoneId = "UTC",
            Tracks = new List<AgendaTrackData>
            {
                new AgendaTrackData
                {
                    Id = Guid.NewGuid(),
                    Name = "Track A",
                    ColorHex = "#FF0000",
                    SortOrder = 1,
                    Sessions = new List<AgendaSessionData>
                    {
                        new AgendaSessionData
                        {
                            Id = Guid.NewGuid(),
                            Title = "Session 1",
                            StartTime = sessionTime,
                            EndTime = sessionTime.AddHours(1),
                            BookedCount = 5,
                            TotalCapacity = 50,
                            Status = SessionStatus.Scheduled,
                            Speakers = new List<AgendaSpeakerData>()
                        }
                    }
                }
            },
            EventDays = new List<AgendaEventDayData>()
        };
        _agendaRepository.GetAgendaAsync(eventId, Arg.Any<Guid?>()).Returns(queryResult);

        // Act
        var result = await _sut.GetAgendaAsync(eventId, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Tracks.Should().HaveCount(1);
        result.Data.Tracks[0].Name.Should().Be("Track A");
        result.Data.Tracks[0].Sessions.Should().HaveCount(1);
        result.Data.Tracks[0].Sessions[0].Title.Should().Be("Session 1");
    }

    [Fact]
    public async Task GetAgendaAsync_WithEventDays_GroupsSessionsByDay()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var day1Date = new DateOnly(2026, 6, 1);
        var day2Date = new DateOnly(2026, 6, 2);
        var session1Time = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
        var session2Time = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc);
        var trackId = Guid.NewGuid();

        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            TimeZoneId = "UTC",
            Tracks = new List<AgendaTrackData>
            {
                new AgendaTrackData
                {
                    Id = trackId,
                    Name = "Main Track",
                    ColorHex = "#0000FF",
                    SortOrder = 1,
                    Sessions = new List<AgendaSessionData>
                    {
                        new AgendaSessionData { Id = Guid.NewGuid(), Title = "Morning Keynote",
                            StartTime = session1Time, EndTime = session1Time.AddHours(1),
                            Status = SessionStatus.Scheduled, Speakers = [] },
                        new AgendaSessionData { Id = Guid.NewGuid(), Title = "Workshop",
                            StartTime = session2Time, EndTime = session2Time.AddHours(2),
                            Status = SessionStatus.Scheduled, Speakers = [] }
                    }
                }
            },
            EventDays = new List<AgendaEventDayData>
            {
                new AgendaEventDayData { Id = Guid.NewGuid(), DayNumber = 1, Date = day1Date, Title = "Opening" },
                new AgendaEventDayData { Id = Guid.NewGuid(), DayNumber = 2, Date = day2Date, Title = "Deep Dives" }
            }
        };
        _agendaRepository.GetAgendaAsync(eventId, Arg.Any<Guid?>()).Returns(queryResult);

        // Act
        var result = await _sut.GetAgendaAsync(eventId, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Days.Should().HaveCount(2);
        var day1 = result.Data.Days.First(d => d.DayNumber == 1);
        day1.Title.Should().Be("Opening");
        day1.Tracks.Should().HaveCount(1); // has session on day 1
        day1.Tracks[0].Sessions.Should().HaveCount(1);
        day1.Tracks[0].Sessions[0].Title.Should().Be("Morning Keynote");
    }

    [Fact]
    public async Task GetAgendaAsync_WithNoEventDays_DerivesGroupsFromSessionDates()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionTime = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);

        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            TimeZoneId = null,
            Tracks = new List<AgendaTrackData>
            {
                new AgendaTrackData
                {
                    Id = Guid.NewGuid(),
                    Name = "Track 1",
                    ColorHex = "#ABC123",
                    SortOrder = 1,
                    Sessions = new List<AgendaSessionData>
                    {
                        new AgendaSessionData { Id = Guid.NewGuid(), Title = "Talk",
                            StartTime = sessionTime, EndTime = sessionTime.AddHours(1),
                            Status = SessionStatus.Scheduled, Speakers = [] }
                    }
                }
            },
            EventDays = new List<AgendaEventDayData>() // No event days
        };
        _agendaRepository.GetAgendaAsync(eventId, Arg.Any<Guid?>()).Returns(queryResult);

        // Act
        var result = await _sut.GetAgendaAsync(eventId, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Days.Should().HaveCount(1);
        result.Data.Days[0].DayNumber.Should().Be(1);
        result.Data.Days[0].EventDayId.Should().BeNull();
    }

    [Fact]
    public async Task GetAgendaAsync_WithInvalidTimezone_FallsBackToUtc()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            TimeZoneId = "InvalidTimezone/XYZ",
            Tracks = new List<AgendaTrackData>(),
            EventDays = new List<AgendaEventDayData>()
        };
        _agendaRepository.GetAgendaAsync(eventId, Arg.Any<Guid?>()).Returns(queryResult);

        // Act — should not throw, falls back to UTC
        var result = await _sut.GetAgendaAsync(eventId, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetAgendaAsync_WithCurrentUserId_PassesItToRepository()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var queryResult = new AgendaQueryResult
        {
            EventId = eventId,
            Tracks = new List<AgendaTrackData>(),
            EventDays = new List<AgendaEventDayData>()
        };
        _agendaRepository.GetAgendaAsync(eventId, userId).Returns(queryResult);

        // Act
        var result = await _sut.GetAgendaAsync(eventId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _agendaRepository.Received(1).GetAgendaAsync(eventId, userId);
    }
}
