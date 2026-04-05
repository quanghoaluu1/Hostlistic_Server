using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class SponsorServiceTest
{
    private readonly ISponsorRepository _repository;
    private readonly ISponsorTierRepository _sponsorTierRepository;
    private readonly IEventRepository _eventRepository;
    private readonly SponsorService _sut;

    public SponsorServiceTest()
    {
        _repository = Substitute.For<ISponsorRepository>();
        _sponsorTierRepository = Substitute.For<ISponsorTierRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _sut = new SponsorService(_repository, _sponsorTierRepository, _eventRepository);

        TypeAdapterConfig<Sponsor, SponsorDto>.NewConfig();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var dto = SponsorBuilder.CreateRequest(name: "");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyEventId_ReturnsFail400()
    {
        // Arrange
        var dto = SponsorBuilder.CreateRequest(eventId: Guid.Empty);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WhenEventNotFound_ReturnsFail400()
    {
        // Arrange
        var dto = SponsorBuilder.CreateRequest();
        _eventRepository.GetEventByIdAsync(dto.EventId).Returns((Event?)null);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Sự kiện không tồn tại");
    }

    [Fact]
    public async Task CreateAsync_WhenTierNotFound_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var dto = SponsorBuilder.CreateRequest(eventId: eventId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sponsorTierRepository.GetByIdAsync(dto.TierId).Returns((SponsorTier?)null);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Tier không tồn tại");
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var dto = SponsorBuilder.CreateRequest(eventId: eventId, tierId: tierId);
        var tier = new SponsorTier { Id = tierId, Name = "Gold", Priority = 1 };

        _eventRepository.GetEventByIdAsync(eventId).Returns(EventBuilder.CreateEvent(id: eventId));
        _sponsorTierRepository.GetByIdAsync(tierId).Returns(tier);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _repository.Received(1).AddAsync(Arg.Any<Sponsor>());
        await _repository.Received(1).SaveChangesAsync();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Sponsor?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var sponsor = SponsorBuilder.CreateEntity(id: sponsorId, name: "TechCo");
        _repository.GetByIdAsync(sponsorId).Returns(sponsor);

        // Act
        var result = await _sut.GetByIdAsync(sponsorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Sponsor?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), SponsorBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_WhenTierNotFound_ReturnsFail400()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var newTierId = Guid.NewGuid();
        var sponsor = SponsorBuilder.CreateEntity(id: sponsorId);
        _repository.GetByIdAsync(sponsorId).Returns(sponsor);
        _sponsorTierRepository.GetByIdAsync(newTierId).Returns((SponsorTier?)null);

        // Act
        var result = await _sut.UpdateAsync(sponsorId, SponsorBuilder.UpdateRequest(tierId: newTierId));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var sponsor = SponsorBuilder.CreateEntity(id: sponsorId, name: "Old Name");
        _repository.GetByIdAsync(sponsorId).Returns(sponsor);

        // Act
        var result = await _sut.UpdateAsync(sponsorId, SponsorBuilder.UpdateRequest(name: "New Name"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        sponsor.Name.Should().Be("New Name");
        await _repository.Received(1).UpdateAsync(Arg.Any<Sponsor>());
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.DeleteAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _repository.Received(1).SaveChangesAsync();
    }
}
