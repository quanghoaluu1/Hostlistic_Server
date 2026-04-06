using EventService_Test.Helpers.TestDataBuilders;

namespace EventService_Test;

public class EventServiceTest
{
    private readonly IEventRepository _eventRepository;
    private readonly ITrackService _trackService;
    private readonly ISessionService _sessionService;
    private readonly IEventTeamMemberRepository _eventTeamMemberRepository;
    private readonly IUserPlanServiceClient _userPlanServiceClient;

    //System under test
    private readonly EventService _sut;

    public EventServiceTest()
    {
        _eventRepository = Substitute.For<IEventRepository>();
        _trackService = Substitute.For<ITrackService>();
        _sessionService = Substitute.For<ISessionService>();
        _eventTeamMemberRepository = Substitute.For<IEventTeamMemberRepository>();
        _userPlanServiceClient = Substitute.For<IUserPlanServiceClient>();

        _sut = new EventService(
            _eventRepository,
            _trackService,
            _sessionService,
            _eventTeamMemberRepository,
            _userPlanServiceClient);
    }

    #region CreateEventAsync

    [Fact]
    public async Task CreateEventAsync_WithValidRequest_ReturnsSuccess()
    {
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();
        
        _userPlanServiceClient.GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());
        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());
        var result = await _sut.CreateEventAsync(request, organizerId);
        
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Event created successfully");
        
        _eventRepository.Received(1).AddEventAsync(Arg.Any<Event>());
        await _eventRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task CreateEventAsync_WithValidRequest_CreatesDefaultTrackAndSession()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest(title: "My Conference");
        Event? capturedEvent = null;

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());

        // Capture the event entity passed to AddEventAsync
        _eventRepository
            .AddEventAsync(Arg.Do<Event>(e => capturedEvent = e));

        // Act
        await _sut.CreateEventAsync(request, organizerId);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.Tracks.Should().HaveCount(1);
        capturedEvent.Tracks.First().Name.Should().Be("Main Track");
        capturedEvent.Tracks.First().Sessions.Should().HaveCount(1);
        capturedEvent.Tracks.First().Sessions.First().Title.Should().Be("Main Session");
    }

    [Fact]
    public async Task CreateEventAsync_WithValidRequest_AddsOrganizerAsTeamMember()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();
        Event? capturedEvent = null;

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());
        _eventRepository.AddEventAsync(Arg.Do<Event>(e => capturedEvent = e));

        // Act
        await _sut.CreateEventAsync(request, organizerId);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.EventTeamMembers.Should().HaveCount(1);

        var member = capturedEvent.EventTeamMembers.First();
        member.UserId.Should().Be(organizerId);
        member.Role.Should().Be(EventRole.Organizer);
        member.Status.Should().Be(EventMemberStatus.Active);
        member.Permissions.Should().ContainKey("can_edit_event")
            .WhoseValue.Should().BeTrue();
    }

    [Fact]
    public async Task CreateEventAsync_WhenNoActivePlan_ReturnsFail403()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.NoPlanResult());

        // Act
        var result = await _sut.CreateEventAsync(request, organizerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("No active subscription plan");

        // Verify repository was NEVER called
        _eventRepository.DidNotReceive().AddEventAsync(Arg.Any<Event>());
    }

    [Fact]
    public async Task CreateEventAsync_WhenPlanServiceFails_ReturnsFail403()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.FailedPlanResult("IdentityService returned 500"));

        // Act
        var result = await _sut.CreateEventAsync(request, organizerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        _eventRepository.DidNotReceive().AddEventAsync(Arg.Any<Event>());
    }

    [Fact]
    public async Task CreateEventAsync_WhenMaxEventsReached_ReturnsFail403()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult(maxEvents: 2));

        // Simulate 2 existing events (limit is 2)
        var existingEvents = new List<Event>
        {
            EventBuilder.CreateEvent(organizerId: organizerId, status: EventStatus.Draft),
            EventBuilder.CreateEvent(organizerId: organizerId, status: EventStatus.Published)
        };
        _eventRepository.GetQueryable().Returns(existingEvents.AsQueryable());

        // Act
        var result = await _sut.CreateEventAsync(request, organizerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("Plan limit reached: max events");
    }

    [Fact]
    public async Task CreateEventAsync_WhenCapacityExceedsPlanLimit_ReturnsFail403()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest(totalCapacity: 1000);

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult(maxAttendeesPerEvent: 500));

        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());

        // Act
        var result = await _sut.CreateEventAsync(request, organizerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("max attendees per event");
    }

    [Fact]
    public async Task CreateEventAsync_CancelledEventsNotCountedTowardLimit()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var request = EventBuilder.CreateEventRequest();

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult(maxEvents: 1));

        // 1 cancelled event — should NOT count toward limit
        var existingEvents = new List<Event>
        {
            EventBuilder.CreateEvent(organizerId: organizerId, status: EventStatus.Cancelled)
        };
        _eventRepository.GetQueryable().Returns(existingEvents.AsQueryable());

        // Act
        var result = await _sut.CreateEventAsync(request, organizerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateEventAsync_SetsDateTimeToUtc()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var startDate = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Unspecified);
        var endDate = new DateTime(2026, 6, 16, 18, 0, 0, DateTimeKind.Unspecified);
        var request = EventBuilder.CreateEventRequest(startDate: startDate, endDate: endDate);
        Event? capturedEvent = null;

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());
        _eventRepository.AddEventAsync(Arg.Do<Event>(e => capturedEvent = e));

        // Act
        await _sut.CreateEventAsync(request, organizerId);

        // Assert — DateTime.Kind should be UTC
        capturedEvent.Should().NotBeNull();
        capturedEvent!.StartDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        capturedEvent.EndDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task CreateEventAsync_AlwaysSetsStatusToDraft()
    {
        // Arrange — even if request says Published, service should override to Draft
        var organizerId = Guid.NewGuid();
        var request = new EventRequestDto(
            Title: "Sneaky Published Event",
            EventStatus: EventStatus.Published
        );
        Event? capturedEvent = null;

        _userPlanServiceClient
            .GetByUserIdAsync(organizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        _eventRepository.GetQueryable().Returns(new List<Event>().AsQueryable());
        _eventRepository.AddEventAsync(Arg.Do<Event>(e => capturedEvent = e));

        // Act
        await _sut.CreateEventAsync(request, organizerId);

        // Assert — should always be Draft on creation
        capturedEvent!.EventStatus.Should().Be(EventStatus.Draft);
        capturedEvent.IsPublic.Should().BeFalse();
    }

    #endregion
    
    #region GetEventByIdAsync

    [Fact]
    public async Task GetEventByIdAsync_WhenEventExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = EventBuilder.CreateEvent(id: eventId, title: "My Event");
        eventEntity.EventType = EventTypeBuilder.CreateEventType();

        _eventRepository.GetEventByIdAsync(eventId).Returns(eventEntity);

        // Act
        var result = await _sut.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns((Event?)null);

        // Act
        var result = await _sut.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    #endregion
    
    #region UpdateEventAsync

    [Fact]
    public async Task UpdateEventAsync_WhenEventExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = EventBuilder.CreateEvent(id: eventId);
        existingEvent.EventType = EventTypeBuilder.CreateEventType();
        var updateRequest = new EventRequestDto(Title: "Updated Title");

        _eventRepository.GetEventByIdAsync(eventId).Returns(existingEvent);
        _userPlanServiceClient
            .GetByUserIdAsync(existingEvent.OrganizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        // Act
        var result = await _sut.UpdateEventAsync(eventId, updateRequest, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _eventRepository.Received(1).UpdateEventAsync(Arg.Any<Event>());
        await _eventRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateEventAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.UpdateEventAsync(
            Guid.NewGuid(),
            new EventRequestDto(Title: "X"),
            null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        _eventRepository.DidNotReceive().UpdateEventAsync(Arg.Any<Event>());
    }

    [Fact]
    public async Task UpdateEventAsync_WhenCapacityExceedsPlanLimit_ReturnsFail403()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var existingEvent = EventBuilder.CreateEvent(id: eventId);
        var updateRequest = new EventRequestDto(TotalCapacity: 9999);

        _eventRepository.GetEventByIdAsync(eventId).Returns(existingEvent);
        _userPlanServiceClient
            .GetByUserIdAsync(existingEvent.OrganizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult(maxAttendeesPerEvent: 500));

        // Act
        var result = await _sut.UpdateEventAsync(eventId, updateRequest, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateEventAsync_OnlyUpdatesProvidedFields()
    {
        // Arrange — only Title is provided, Location should remain unchanged
        var eventId = Guid.NewGuid();
        var existingEvent = EventBuilder.CreateEvent(id: eventId);
        existingEvent.Title = "Original Title";
        existingEvent.Location = "Original Location";
        existingEvent.EventType = EventTypeBuilder.CreateEventType();

        var updateRequest = new EventRequestDto(Title: "New Title");

        _eventRepository.GetEventByIdAsync(eventId).Returns(existingEvent);
        _userPlanServiceClient
            .GetByUserIdAsync(existingEvent.OrganizerId, true)
            .Returns(UserPlanBuilder.ActivePlanResult());

        // Act
        await _sut.UpdateEventAsync(eventId, updateRequest, null);

        // Assert — Title changed, Location preserved
        existingEvent.Title.Should().Be("New Title");
        existingEvent.Location.Should().Be("Original Location");
    }

    #endregion

}