using EventService_Test.Helpers.TestDataBuilders;

namespace EventService_Test;

public class SessionBookingServiceTest
{
    private readonly ISessionBookingRepository _bookingRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly SessionBookingService _sut;

    public SessionBookingServiceTest()
    {
        _bookingRepository = Substitute.For<ISessionBookingRepository>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _sut = new SessionBookingService(_bookingRepository, _sessionRepository);
    }

    // ── BookSessionAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task BookSessionAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        _sessionRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Session?)null);

        // Act
        var result = await _sut.BookSessionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task BookSessionAsync_WhenSessionIsCancelled_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, status: SessionStatus.Cancelled);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Cannot book a cancelled session");
    }

    [Fact]
    public async Task BookSessionAsync_WhenSessionIsCompleted_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, status: SessionStatus.Completed);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Cannot book a completed session");
    }

    [Fact]
    public async Task BookSessionAsync_WhenUserAlreadyBooked_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _bookingRepository.UserHasBookingForSessionAsync(userId, sessionId).Returns(true);

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already booked");
    }

    [Fact]
    public async Task BookSessionAsync_WhenSessionIsFull_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId, totalCapacity: 10);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _bookingRepository.UserHasBookingForSessionAsync(userId, sessionId).Returns(false);
        _sessionRepository.GetBookedCountAsync(sessionId).Returns(10);

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Session is full");
    }

    [Fact]
    public async Task BookSessionAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _bookingRepository.UserHasBookingForSessionAsync(userId, sessionId).Returns(false);
        _bookingRepository.GetConflictingSessionsAsync(userId, eventId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<Guid?>())
            .Returns(new List<Session>());

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(BookingStatus.Confirmed);
        await _bookingRepository.Received(1).AddSessionBookingAsync(Arg.Any<SessionBooking>());
    }

    [Fact]
    public async Task BookSessionAsync_WithConflictingSession_StillSucceedsWithWarnings()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId,
            startTime: DateTime.UtcNow.AddDays(7).AddHours(9),
            endTime: DateTime.UtcNow.AddDays(7).AddHours(10));
        var conflicting = SessionBuilder.CreateEntity(eventId: eventId);
        _sessionRepository.GetByIdWithinEventAsync(eventId, sessionId).Returns(session);
        _bookingRepository.UserHasBookingForSessionAsync(userId, sessionId).Returns(false);
        _bookingRepository.GetConflictingSessionsAsync(userId, eventId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<Guid?>())
            .Returns(new List<Session> { conflicting });

        // Act
        var result = await _sut.BookSessionAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.Warnings.Should().NotBeNullOrEmpty();
    }

    // ── CancelBookingAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CancelBookingAsync_WhenBookingNotFound_ReturnsFail404()
    {
        // Arrange
        _bookingRepository.GetByUserAndSessionAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((SessionBooking?)null);

        // Act
        var result = await _sut.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingIsAlreadyCancelled_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        var booking = new SessionBooking
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            Status = BookingStatus.Cancelled,
            BookingDate = DateTime.UtcNow,
            Session = session
        };
        _bookingRepository.GetByUserAndSessionAsync(userId, sessionId).Returns(booking);

        // Act
        var result = await _sut.CancelBookingAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Cannot cancel a");
    }

    [Fact]
    public async Task CancelBookingAsync_WithValidBooking_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(id: sessionId, eventId: eventId);
        var booking = new SessionBooking
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            Status = BookingStatus.Confirmed,
            BookingDate = DateTime.UtcNow,
            Session = session
        };
        _bookingRepository.GetByUserAndSessionAsync(userId, sessionId).Returns(booking);

        // Act
        var result = await _sut.CancelBookingAsync(eventId, sessionId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    // ── GetMyScheduleAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMyScheduleAsync_ReturnsSuccess200WithBookings()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(eventId: eventId);
        var booking = new SessionBooking
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            UserId = userId,
            Status = BookingStatus.Confirmed,
            BookingDate = DateTime.UtcNow,
            Session = session
        };
        _bookingRepository.GetByUserAndEventAsync(userId, eventId)
            .Returns(new List<SessionBooking> { booking });

        // Act
        var result = await _sut.GetMyScheduleAsync(eventId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.TotalBookings.Should().Be(1);
        result.Data.Bookings.Should().HaveCount(1);
    }
}
