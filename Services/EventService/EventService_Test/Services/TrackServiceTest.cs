using EventService_Test.Helpers.TestDataBuilders;

namespace EventService_Test;

public class TrackServiceTest
{
    private readonly ITrackRepository _trackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly TrackService _sut;

    public TrackServiceTest()
    {
        _trackRepository = Substitute.For<ITrackRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sut = new TrackService(_trackRepository, _eventRepository);
    }

    // ── GetTrackByIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetTrackByIdAsync_WhenTrackExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);

        // Act
        var result = await _sut.GetTrackByIdAsync(eventId, trackId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTrackByIdAsync_WhenTrackNotFound_ReturnsFail404()
    {
        // Arrange
        _trackRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Track?)null);

        // Act
        var result = await _sut.GetTrackByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    // ── GetTracksByEventIdAsync ────────────────────────────────────────────

    // [Fact]
    // public async Task GetTracksByEventIdAsync_WhenEventNotFound_ReturnsFail404()
    // {
    //     // Arrange
    //     _eventRepository.EventExistsAsync(Arg.Any<Guid>()).Returns(false);
    //
    //     // Act
    //     var result = await _sut.GetTracksByEventIdAsync(Guid.NewGuid());
    //
    //     // Assert
    //     result.IsSuccess.Should().BeFalse();
    //     result.StatusCode.Should().Be(404);
    // }

    // [Fact]
    // public async Task GetTracksByEventIdAsync_WhenEventExists_ReturnsSuccess200()
    // {
    //     // Arrange
    //     var eventId = Guid.NewGuid();
    //     _eventRepository.EventExistsAsync(eventId).Returns(true);
    //     _trackRepository.GetTracksByEventIdAsync(eventId)
    //         .Returns(new List<Track> { TrackBuilder.CreateEntity(eventId: eventId) });
    //
    //     // Act
    //     var result = await _sut.GetTracksByEventIdAsync(eventId);
    //
    //     // Assert
    //     result.IsSuccess.Should().BeTrue();
    //     result.StatusCode.Should().Be(200);
    // }

    // ── CreateTrackAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateTrackAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.CreateTrackAsync(Guid.NewGuid(), TrackBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateTrackAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        var request = TrackBuilder.CreateRequest(name: "   ");

        // Act
        var result = await _sut.CreateTrackAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Track name is required");
    }

    [Fact]
    public async Task CreateTrackAsync_WithInvalidColorHex_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        var request = TrackBuilder.CreateRequest(colorHex: "notacolor");

        // Act
        var result = await _sut.CreateTrackAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Invalid color format");
    }

    [Fact]
    public async Task CreateTrackAsync_WithStartTimeAfterEndTime_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);

        var request = TrackBuilder.CreateRequest(
            startTime: DateTime.UtcNow.AddHours(2),
            endTime: DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _sut.CreateTrackAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("start time must be before end time");
    }

    [Fact]
    public async Task CreateTrackAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _trackRepository.GetMaxSortOrderAsync(eventId).Returns(0);

        // Act
        var result = await _sut.CreateTrackAsync(eventId, TrackBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Track created successfully");
        await _trackRepository.Received(1).AddTrackAsync(Arg.Any<Track>());
        await _trackRepository.Received(1).SaveChangesAsync();
    }

    // ── UpdateTrackAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTrackAsync_WhenTrackNotFound_ReturnsFail404()
    {
        // Arrange
        _trackRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Track?)null);

        // Act
        var result = await _sut.UpdateTrackAsync(Guid.NewGuid(), Guid.NewGuid(), TrackBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateTrackAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);

        // Act
        var result = await _sut.UpdateTrackAsync(eventId, trackId, TrackBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _trackRepository.Received(1).UpdateTrackAsync(Arg.Any<Track>());
    }

    // ── DeleteTrackAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTrackAsync_WhenTrackNotFound_ReturnsFail404()
    {
        // Arrange
        _trackRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Track?)null);

        // Act
        var result = await _sut.DeleteTrackAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteTrackAsync_WhenTrackHasSessions_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);
        _trackRepository.HasSessionsAsync(trackId).Returns(true);

        // Act
        var result = await _sut.DeleteTrackAsync(eventId, trackId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Cannot delete track with existing sessions");
    }

    [Fact]
    public async Task DeleteTrackAsync_WhenTrackIsEmpty_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);
        _trackRepository.HasSessionsAsync(trackId).Returns(false);

        // Act
        var result = await _sut.DeleteTrackAsync(eventId, trackId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _trackRepository.Received(1).DeleteTrackAsync(trackId);
    }
}
