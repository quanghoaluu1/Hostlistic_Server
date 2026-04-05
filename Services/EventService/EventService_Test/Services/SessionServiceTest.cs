using EventService_Test.Helpers.TestDataBuilders;
using MassTransit;

namespace EventService_Test;

public class SessionServiceTest
{
    private readonly ITrackRepository _trackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly SessionService _sut;

    public SessionServiceTest()
    {
        _trackRepository = Substitute.For<ITrackRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _sut = new SessionService(_trackRepository, _eventRepository, _sessionRepository, _publishEndpoint);
    }

    // ── GetSessionByIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetSessionByIdAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        _sessionRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Session?)null);

        // Act
        var result = await _sut.GetSessionByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetSessionByIdAsync_WhenSessionExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _sessionRepository.GetBookedCountAsync(sessionId).Returns(0);

        // Act
        var result = await _sut.GetSessionByIdAsync(eventId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── GetSessionsByEventIdAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetSessionsByEventIdAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.EventExistsAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.GetSessionsByEventIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    // ── CreateSessionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateSessionAsync_WhenStartTimeAfterEndTime_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = SessionBuilder.CreateRequest(
            startTime: DateTime.UtcNow.AddHours(2),
            endTime: DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _sut.CreateSessionAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Start time must be before end time");
    }

    [Fact]
    public async Task CreateSessionAsync_WithEmptyTitle_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = SessionBuilder.CreateRequest(title: "   ");

        // Act
        var result = await _sut.CreateSessionAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Session title is required");
    }

    [Fact]
    public async Task CreateSessionAsync_WhenTrackNotFound_ReturnsFail404()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _trackRepository.GetByIdWithinEventAsync(eventId, Arg.Any<Guid>()).Returns((Track?)null);
        var request = SessionBuilder.CreateRequest();

        // Act
        var result = await _sut.CreateSessionAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Track not found in this event");
    }

    [Fact]
    public async Task CreateSessionAsync_WhenTrackOverlapExists_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);
        _sessionRepository.HasOverlapInTrackAsync(trackId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(true);

        var request = SessionBuilder.CreateRequest(trackId: trackId);

        // Act
        var result = await _sut.CreateSessionAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Time conflict");
    }

    [Fact]
    public async Task CreateSessionAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var track = TrackBuilder.CreateEntity(id: trackId, eventId: eventId);
        var session = SessionBuilder.CreateEntity(eventId: eventId, trackId: trackId);

        _trackRepository.GetByIdWithinEventAsync(eventId, trackId).Returns(track);
        _sessionRepository.HasOverlapInTrackAsync(trackId, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(false);
        _sessionRepository.GetMaxSortOrderInTrackAsync(trackId).Returns(0);
        _sessionRepository.GetByIdWithinEventAsync(eventId, Arg.Any<Guid>()).Returns(session);
        _sessionRepository.GetBookedCountAsync(Arg.Any<Guid>()).Returns(0);

        var request = SessionBuilder.CreateRequest(trackId: trackId);

        // Act
        var result = await _sut.CreateSessionAsync(eventId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _sessionRepository.Received(1).AddSessionAsync(Arg.Any<Session>());
        await _sessionRepository.Received(1).SaveChangesAsync();
    }

    // ── UpdateSessionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSessionAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        _sessionRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Session?)null);

        // Act
        var result = await _sut.UpdateSessionAsync(Guid.NewGuid(), Guid.NewGuid(), SessionBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateSessionAsync_WhenSessionIsCancelled_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, status: SessionStatus.Cancelled);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);

        // Act
        var result = await _sut.UpdateSessionAsync(eventId, sessionId, SessionBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Cannot edit a");
    }

    [Fact]
    public async Task UpdateSessionAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _sessionRepository.HasOverlapInTrackAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<Guid?>())
            .Returns(false);
        _sessionRepository.GetBookedCountAsync(sessionId).Returns(0);

        // Act
        var result = await _sut.UpdateSessionAsync(eventId, sessionId, SessionBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _sessionRepository.Received(1).UpdateSessionAsync(Arg.Any<Session>());
    }

    // ── DeleteSessionAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSessionAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        _sessionRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Session?)null);

        // Act
        var result = await _sut.DeleteSessionAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenSessionHasActiveBookings_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _sessionRepository.HasBookingsAsync(sessionId).Returns(true);

        // Act
        var result = await _sut.DeleteSessionAsync(eventId, sessionId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Cannot delete session with active bookings");
    }

    [Fact]
    public async Task DeleteSessionAsync_WithNoBookings_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _sessionRepository.HasBookingsAsync(sessionId).Returns(false);

        // Act
        var result = await _sut.DeleteSessionAsync(eventId, sessionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _sessionRepository.Received(1).DeleteSessionAsync(sessionId);
    }

    // ── UpdateSessionStatusAsync ───────────────────────────────────────────

    [Theory]
    [InlineData(SessionStatus.Scheduled, SessionStatus.OnGoing)]
    [InlineData(SessionStatus.Scheduled, SessionStatus.Cancelled)]
    [InlineData(SessionStatus.OnGoing, SessionStatus.Completed)]
    [InlineData(SessionStatus.OnGoing, SessionStatus.Cancelled)]
    public async Task UpdateSessionStatusAsync_ValidTransitions_ReturnsSuccess200(
        SessionStatus from, SessionStatus to)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, status: from);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _sessionRepository.GetBookedCountAsync(sessionId).Returns(0);

        // Act
        var result = await _sut.UpdateSessionStatusAsync(
            eventId, sessionId, new UpdateSessionStatusRequest { Status = to });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        session.Status.Should().Be(to);
    }

    [Fact]
    public async Task UpdateSessionStatusAsync_InvalidTransition_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, status: SessionStatus.Completed);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);

        // Act
        var result = await _sut.UpdateSessionStatusAsync(
            eventId, sessionId, new UpdateSessionStatusRequest { Status = SessionStatus.Scheduled });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Invalid status transition");
    }
}
