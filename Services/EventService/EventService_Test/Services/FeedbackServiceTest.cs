using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class FeedbackServiceTest
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly FeedbackService _sut;

    public FeedbackServiceTest()
    {
        _feedbackRepository = Substitute.For<IFeedbackRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sessionRepository = Substitute.For<ISessionRepository>();
        _sut = new FeedbackService(_feedbackRepository, _eventRepository, _sessionRepository);

        TypeAdapterConfig<Feedback, FeedbackDto>.NewConfig();
    }

    // ── AddFeedbackAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddFeedbackAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);
        var dto = FeedbackBuilder.CreateDto();

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task AddFeedbackAsync_WhenSessionNotInEvent_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var otherEventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(sessionId).Returns(SessionBuilder.CreateEntity(id: sessionId, eventId: otherEventId));
        var dto = FeedbackBuilder.CreateDto(eventId: eventId, sessionId: sessionId);

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("does not belong");
    }

    [Fact]
    public async Task AddFeedbackAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(Arg.Any<Guid>()).Returns((Session?)null);
        var dto = FeedbackBuilder.CreateDto(eventId: eventId);

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Session not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task AddFeedbackAsync_WithInvalidRating_ReturnsFail400(int rating)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(sessionId).Returns(SessionBuilder.CreateEntity(id: sessionId, eventId: eventId));

        var dto = FeedbackBuilder.CreateDto(eventId: eventId, sessionId: sessionId, rating: rating);

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Rating must be between 1 and 5");
    }

    [Fact]
    public async Task AddFeedbackAsync_WithEmptyUserId_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(sessionId).Returns(SessionBuilder.CreateEntity(id: sessionId, eventId: eventId));

        var dto = FeedbackBuilder.CreateDto(eventId: eventId, sessionId: sessionId, userId: Guid.Empty);

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("UserId is required");
    }

    [Fact]
    public async Task AddFeedbackAsync_WithEmptyComment_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(sessionId).Returns(SessionBuilder.CreateEntity(id: sessionId, eventId: eventId));

        var dto = FeedbackBuilder.CreateDto(eventId: eventId, sessionId: sessionId);
        dto.Comment = "";

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Comment is required");
    }

    [Fact]
    public async Task AddFeedbackAsync_WithValidDto_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sessionRepository.GetSessionByIdAsync(sessionId).Returns(SessionBuilder.CreateEntity(id: sessionId, eventId: eventId));

        var dto = FeedbackBuilder.CreateDto(eventId: eventId, sessionId: sessionId, rating: 5);

        // Act
        var result = await _sut.AddFeedbackAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _feedbackRepository.Received(1).AddFeedbackAsync(Arg.Any<Feedback>());
    }

    // ── GetFeedbackByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetFeedbackByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _feedbackRepository.GetFeedbackByIdAsync(Arg.Any<Guid>()).Returns((Feedback?)null);

        // Act
        var result = await _sut.GetFeedbackByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetFeedbackByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id);
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        // Act
        var result = await _sut.GetFeedbackByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── UpdateFeedbackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateFeedbackAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _feedbackRepository.GetFeedbackByIdAsync(Arg.Any<Guid>()).Returns((Feedback?)null);

        // Act
        var result = await _sut.UpdateFeedbackAsync(Guid.NewGuid(), FeedbackBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task UpdateFeedbackAsync_WithInvalidRating_ReturnsFail400(int rating)
    {
        // Arrange
        var id = Guid.NewGuid();
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(FeedbackBuilder.CreateEntity(id: id));

        // Act
        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(rating: rating));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id, comment: "Old comment");
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        // Act
        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(comment: "New comment"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _feedbackRepository.Received(1).UpdateFeedbackAsync(Arg.Any<Feedback>());
    }

    [Fact]
    public async Task UpdateFeedbackAsync_UpdatesRating()
    {
        // Arrange
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id, rating: 2);
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        // Act
        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(rating: 5, comment: "x"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Rating.Should().Be(5);
        await _feedbackRepository.Received(1).UpdateFeedbackAsync(Arg.Is<Feedback>(f => f.Rating == 5));
    }

    // ── DeleteFeedbackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteFeedbackAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _feedbackRepository.DeleteFeedbackAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteFeedbackAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteFeedbackAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        _feedbackRepository.DeleteFeedbackAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteFeedbackAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
