using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class FeedbackServiceTest
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly FeedbackService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public FeedbackServiceTest()
    {
        _feedbackRepository = Substitute.For<IFeedbackRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sut = new FeedbackService(_feedbackRepository, _eventRepository);

        TypeAdapterConfig<Feedback, FeedbackDto>.NewConfig();
    }

    // ── AddFeedbackAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddFeedbackAsync_WhenEventNotFound_ReturnsFail404()
    {
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);
        var dto = FeedbackBuilder.CreateDto();

        var result = await _sut.AddFeedbackAsync(dto, _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task AddFeedbackAsync_WhenDuplicateExists_ReturnsFail409()
    {
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _feedbackRepository.GetFeedbackByEventAndUserAsync(eventId, _userId)
            .Returns(FeedbackBuilder.CreateEntity(eventId: eventId, userId: _userId));

        var dto = FeedbackBuilder.CreateDto(eventId: eventId);

        var result = await _sut.AddFeedbackAsync(dto, _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already submitted");
    }

    [Fact]
    public async Task AddFeedbackAsync_WithValidDto_ReturnsSuccess201()
    {
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _feedbackRepository.GetFeedbackByEventAndUserAsync(eventId, _userId).Returns((Feedback?)null);

        var dto = FeedbackBuilder.CreateDto(eventId: eventId, rating: 5);

        var result = await _sut.AddFeedbackAsync(dto, _userId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _feedbackRepository.Received(1).AddFeedbackAsync(Arg.Any<Feedback>());
    }

    // ── GetFeedbackByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetFeedbackByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _feedbackRepository.GetFeedbackByIdAsync(Arg.Any<Guid>()).Returns((Feedback?)null);

        var result = await _sut.GetFeedbackByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetFeedbackByIdAsync_WhenExists_ReturnsSuccess200()
    {
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id);
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        var result = await _sut.GetFeedbackByIdAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── GetMyFeedbackForEventAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetMyFeedbackForEventAsync_WhenNotFound_Returns204()
    {
        var eventId = Guid.NewGuid();
        _feedbackRepository.GetFeedbackByEventAndUserAsync(eventId, _userId).Returns((Feedback?)null);

        var result = await _sut.GetMyFeedbackForEventAsync(eventId, _userId);

        result.StatusCode.Should().Be(204);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetMyFeedbackForEventAsync_WhenExists_Returns200()
    {
        var eventId = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(eventId: eventId, userId: _userId);
        _feedbackRepository.GetFeedbackByEventAndUserAsync(eventId, _userId).Returns(feedback);

        var result = await _sut.GetMyFeedbackForEventAsync(eventId, _userId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
    }

    // ── UpdateFeedbackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateFeedbackAsync_WhenNotFound_ReturnsFail404()
    {
        _feedbackRepository.GetFeedbackByIdAsync(Arg.Any<Guid>()).Returns((Feedback?)null);

        var result = await _sut.UpdateFeedbackAsync(Guid.NewGuid(), FeedbackBuilder.UpdateRequest(), _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_WhenNotOwner_ReturnsFail403()
    {
        var id = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _feedbackRepository.GetFeedbackByIdAsync(id)
            .Returns(FeedbackBuilder.CreateEntity(id: id, userId: otherUserId));

        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(), _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_WithValidRequest_ReturnsSuccess200()
    {
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id, userId: _userId, comment: "Old comment");
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(comment: "New comment"), _userId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _feedbackRepository.Received(1).UpdateFeedbackAsync(Arg.Any<Feedback>());
    }

    [Fact]
    public async Task UpdateFeedbackAsync_UpdatesRating()
    {
        var id = Guid.NewGuid();
        var feedback = FeedbackBuilder.CreateEntity(id: id, userId: _userId, rating: 2);
        _feedbackRepository.GetFeedbackByIdAsync(id).Returns(feedback);

        var result = await _sut.UpdateFeedbackAsync(id, FeedbackBuilder.UpdateRequest(rating: 5, comment: "x"), _userId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Rating.Should().Be(5);
        await _feedbackRepository.Received(1).UpdateFeedbackAsync(Arg.Is<Feedback>(f => f.Rating == 5));
    }

    // ── DeleteFeedbackAsync ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteFeedbackAsync_WhenNotFound_ReturnsFail404()
    {
        _feedbackRepository.GetFeedbackByIdAsync(Arg.Any<Guid>()).Returns((Feedback?)null);

        var result = await _sut.DeleteFeedbackAsync(Guid.NewGuid(), _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteFeedbackAsync_WhenNotOwner_ReturnsFail403()
    {
        var id = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _feedbackRepository.GetFeedbackByIdAsync(id)
            .Returns(FeedbackBuilder.CreateEntity(id: id, userId: otherUserId));

        var result = await _sut.DeleteFeedbackAsync(id, _userId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteFeedbackAsync_WhenExists_ReturnsSuccess200()
    {
        var id = Guid.NewGuid();
        _feedbackRepository.GetFeedbackByIdAsync(id)
            .Returns(FeedbackBuilder.CreateEntity(id: id, userId: _userId));
        _feedbackRepository.DeleteFeedbackAsync(id).Returns(true);

        var result = await _sut.DeleteFeedbackAsync(id, _userId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
