using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class EventDayServiceTest
{
    private readonly IEventDayRepository _eventDayRepository;
    private readonly IEventRepository _eventRepository;
    private readonly EventDayService _sut;

    public EventDayServiceTest()
    {
        _eventDayRepository = Substitute.For<IEventDayRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sut = new EventDayService(_eventDayRepository, _eventRepository);

        TypeAdapterConfig<EventDay, EventDayResponse>.NewConfig();
    }

    // ── GetByEventIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByEventIdAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.EventExistsAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.GetByEventIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByEventIdAsync_WhenEventExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.EventExistsAsync(eventId).Returns(true);
        _eventDayRepository.GetByEventIdAsync(eventId).Returns(
            new List<EventDay> { EventDayBuilder.CreateEntity(eventId: eventId) });

        // Act
        var result = await _sut.GetByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(1);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _eventDayRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((EventDay?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var day = EventDayBuilder.CreateEntity(id: dayId, eventId: eventId);
        _eventDayRepository.GetByIdAsync(eventId, dayId).Returns(day);

        // Act
        var result = await _sut.GetByIdAsync(eventId, dayId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── GenerateDaysAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GenerateDaysAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.GenerateDaysAsync(Guid.NewGuid(), EventDayBuilder.GenerateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GenerateDaysAsync_WhenEventHasNoDates_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        ev.StartDate = null; // No start date
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);

        // Act
        var result = await _sut.GenerateDaysAsync(eventId, EventDayBuilder.GenerateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("StartDate and EndDate");
    }

    [Fact]
    public async Task GenerateDaysAsync_WhenDaysAlreadyExist_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _eventDayRepository.AnyExistAsync(eventId).Returns(true);

        // Act
        var result = await _sut.GenerateDaysAsync(eventId, EventDayBuilder.GenerateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already generated");
    }

    [Fact]
    public async Task GenerateDaysAsync_WithInvalidTimezone_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _eventDayRepository.AnyExistAsync(eventId).Returns(false);

        // Act
        var result = await _sut.GenerateDaysAsync(eventId,
            EventDayBuilder.GenerateRequest(timeZoneId: "InvalidTimezone/XYZ"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Unknown timezone");
    }

    [Fact]
    public async Task GenerateDaysAsync_WithValidEvent_GeneratesCorrectNumberOfDays()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        ev.StartDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        ev.EndDate = new DateTime(2026, 6, 3, 23, 59, 59, DateTimeKind.Utc); // 3 days
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _eventDayRepository.AnyExistAsync(eventId).Returns(false);

        IEnumerable<EventDay>? capturedDays = null;
        _eventDayRepository.AddRangeAsync(Arg.Do<IEnumerable<EventDay>>(d => capturedDays = d));

        // Act
        var result = await _sut.GenerateDaysAsync(eventId, EventDayBuilder.GenerateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        capturedDays.Should().HaveCount(3);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.CreateAsync(Guid.NewGuid(), EventDayBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateDate_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        ev.StartDate = null; // no bounds — allow any date
        ev.EndDate = null;
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _eventDayRepository.ExistsAsync(eventId, Arg.Any<DateOnly>()).Returns(true);

        // Act
        var result = await _sut.CreateAsync(eventId, EventDayBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId);
        ev.StartDate = null;
        ev.EndDate = null;
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _eventDayRepository.ExistsAsync(eventId, Arg.Any<DateOnly>()).Returns(false);
        _eventDayRepository.GetByEventIdAsync(eventId).Returns(new List<EventDay>());

        // Act
        var result = await _sut.CreateAsync(eventId, EventDayBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _eventDayRepository.Received(1).AddAsync(Arg.Any<EventDay>());
        await _eventDayRepository.Received(1).SaveChangesAsync();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenDayNotFound_ReturnsFail404()
    {
        // Arrange
        _eventDayRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((EventDay?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), EventDayBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var day = EventDayBuilder.CreateEntity(id: dayId, eventId: eventId, title: "Old Title");
        _eventDayRepository.GetByIdAsync(eventId, dayId).Returns(day);

        // Act
        var result = await _sut.UpdateAsync(eventId, dayId, EventDayBuilder.UpdateRequest(title: "New Title"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        day.Title.Should().Be("New Title");
        _eventDayRepository.Received(1).Update(Arg.Any<EventDay>());
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenDayNotFound_ReturnsFail404()
    {
        // Arrange
        _eventDayRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((EventDay?)null);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAsync_WhenDayExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var day = EventDayBuilder.CreateEntity(id: dayId, eventId: eventId);
        _eventDayRepository.GetByIdAsync(eventId, dayId).Returns(day);

        // Act
        var result = await _sut.DeleteAsync(eventId, dayId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _eventDayRepository.Received(1).Remove(Arg.Any<EventDay>());
        await _eventDayRepository.Received(1).SaveChangesAsync();
    }
}
