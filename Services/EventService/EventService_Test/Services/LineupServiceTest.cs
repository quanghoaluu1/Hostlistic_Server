using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class LineupServiceTest
{
    private readonly ILineupRepository _lineupRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ITalentRepository _talentRepository;
    private readonly LineupService _sut;

    public LineupServiceTest()
    {
        _lineupRepository = Substitute.For<ILineupRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _talentRepository = Substitute.For<ITalentRepository>();
        _sut = new LineupService(_lineupRepository, _eventRepository, _talentRepository);

        TypeAdapterConfig<Talent, TalentDto>.NewConfig();
    }

    // ── CreateLineupAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateLineupAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);
        var request = new CreateLineupsRequest
        {
            EventId = Guid.NewGuid(),
            TalentIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _sut.CreateLineupAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task CreateLineupAsync_WithEmptyTalentList_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        var request = new CreateLineupsRequest
        {
            EventId = eventId,
            TalentIds = new List<Guid>()
        };

        // Act
        var result = await _sut.CreateLineupAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Talent list is empty");
    }

    [Fact]
    public async Task CreateLineupAsync_WhenNoTalentsFound_ReturnsFail404()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _talentRepository.GetTalentByIdAsync(Arg.Any<List<Guid>>()).Returns(new List<Talent>());

        var request = new CreateLineupsRequest
        {
            EventId = eventId,
            TalentIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var result = await _sut.CreateLineupAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("No talents found");
    }

    [Fact]
    public async Task CreateLineupAsync_WithValidRequest_CreatesLineupsAndReturnsSuccess()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _talentRepository.GetTalentByIdAsync(Arg.Any<List<Guid>>()).Returns(new List<Talent> { talent });
        _lineupRepository.GetLineupsByEventAndTalentsAsync(eventId, Arg.Any<Guid?>(), Arg.Any<List<Guid>>())
            .Returns(new List<Lineup>());

        var request = new CreateLineupsRequest
        {
            EventId = eventId,
            TalentIds = new List<Guid> { talentId }
        };

        // Act
        var result = await _sut.CreateLineupAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Created.Should().HaveCount(1);
        result.Data.SkippedTalentIds.Should().BeEmpty();
        await _lineupRepository.Received(1).AddLineupAsync(Arg.Any<Lineup>());
    }

    [Fact]
    public async Task CreateLineupAsync_WhenTalentAlreadyInLineup_SkipsAndReportsIt()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId);
        var existingLineup = new Lineup { Id = Guid.NewGuid(), EventId = eventId, TalentId = talentId, Talent = talent };

        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _talentRepository.GetTalentByIdAsync(Arg.Any<List<Guid>>()).Returns(new List<Talent> { talent });
        _lineupRepository.GetLineupsByEventAndTalentsAsync(eventId, Arg.Any<Guid?>(), Arg.Any<List<Guid>>())
            .Returns(new List<Lineup> { existingLineup });

        var request = new CreateLineupsRequest
        {
            EventId = eventId,
            TalentIds = new List<Guid> { talentId }
        };

        // Act
        var result = await _sut.CreateLineupAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Created.Should().BeEmpty();
        result.Data.SkippedTalentIds.Should().Contain(talentId);
        _lineupRepository.DidNotReceive().AddLineupAsync(Arg.Any<Lineup>());
    }

    // ── GetLineupsByEventIdAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetLineupsByEventIdAsync_ReturnsSuccess200WithLineups()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity();
        var lineup = new Lineup { Id = Guid.NewGuid(), EventId = eventId, TalentId = talent.Id, Talent = talent };
        _lineupRepository.GetLineupsByEventIdAsync(eventId).Returns(new List<Lineup> { lineup });

        // Act
        var result = await _sut.GetLineupsByEventIdAsync(eventId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(1);
    }

    // ── GetLineupById ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetLineupById_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _lineupRepository.GetLineupByIdAsync(Arg.Any<Guid>()).Returns((Lineup?)null);

        // Act
        var result = await _sut.GetLineupById(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    // ── DeleteLineupAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteLineupAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _lineupRepository.GetLineupByIdAsync(Arg.Any<Guid>()).Returns((Lineup?)null);

        // Act
        var result = await _sut.DeleteLineupAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteLineupAsync_WhenSessionIsOnGoing_ReturnsFail400()
    {
        // Arrange
        var lineupId = Guid.NewGuid();
        var session = SessionBuilder.CreateEntity(status: SessionStatus.OnGoing);
        var lineup = new Lineup
        {
            Id = lineupId,
            EventId = Guid.NewGuid(),
            TalentId = Guid.NewGuid(),
            Session = session,
            Talent = TalentBuilder.CreateEntity()
        };
        _lineupRepository.GetLineupByIdAsync(lineupId).Returns(lineup);

        // Act
        var result = await _sut.DeleteLineupAsync(lineupId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Cannot remove talent from an ongoing session");
    }

    [Fact]
    public async Task DeleteLineupAsync_WithValidLineup_ReturnsSuccess200()
    {
        // Arrange
        var lineupId = Guid.NewGuid();
        var lineup = new Lineup
        {
            Id = lineupId,
            EventId = Guid.NewGuid(),
            TalentId = Guid.NewGuid(),
            Session = null,
            Talent = TalentBuilder.CreateEntity()
        };
        _lineupRepository.GetLineupByIdAsync(lineupId).Returns(lineup);
        _lineupRepository.DeleteLineupAsync(lineupId).Returns(true);

        // Act
        var result = await _sut.DeleteLineupAsync(lineupId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
