using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class VenueServiceTest
{
    private readonly IVenueRepository _venueRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IPhotoService _photoService;
    private readonly VenueService _sut;

    public VenueServiceTest()
    {
        _venueRepository = Substitute.For<IVenueRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _photoService = Substitute.For<IPhotoService>();
        _sut = new VenueService(_venueRepository, _eventRepository, _photoService);

        // Mapster config for record type
        TypeAdapterConfig<Venue, VenueResponse>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.EventId, src => src.EventId)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Capacity, src => src.Capacity)
            .Map(dest => dest.LayoutUrl, src => src.LayoutUrl);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.EventExistsAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.CreateAsync(Guid.NewGuid(), VenueBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Event not found");
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateVenueName_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.EventExistsAsync(eventId).Returns(true);
        _venueRepository.ExistsByNameAsync(eventId, Arg.Any<string>()).Returns(true);

        // Act
        var result = await _sut.CreateAsync(eventId, VenueBuilder.CreateRequest(name: "Main Hall"));

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
        _eventRepository.EventExistsAsync(eventId).Returns(true);
        _venueRepository.ExistsByNameAsync(eventId, Arg.Any<string>()).Returns(false);

        // Act
        var result = await _sut.CreateAsync(eventId, VenueBuilder.CreateRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Contain("Venue created successfully");
        await _venueRepository.Received(1).AddVenueAsync(Arg.Any<Venue>());
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenVenueNotFound_ReturnsFail404()
    {
        // Arrange
        _venueRepository.GetByIdWithinEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Venue?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVenueExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var venue = VenueBuilder.CreateEntity(id: venueId, eventId: eventId);
        _venueRepository.GetByIdWithinEventAsync(eventId, venueId).Returns(venue);

        // Act
        var result = await _sut.GetByIdAsync(eventId, venueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenVenueNotFound_ReturnsFail404()
    {
        // Arrange
        _venueRepository.GetByIdWithinEventForUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Venue?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), VenueBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateName_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var venue = VenueBuilder.CreateEntity(id: venueId, eventId: eventId, name: "Old Hall");
        _venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId).Returns(venue);
        _venueRepository.ExistsByNameAsync(eventId, Arg.Any<string>(), Arg.Any<Guid?>()).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(eventId, venueId, VenueBuilder.UpdateRequest(name: "Taken Hall"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var venue = VenueBuilder.CreateEntity(id: venueId, eventId: eventId);
        _venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId).Returns(venue);
        _venueRepository.ExistsByNameAsync(eventId, Arg.Any<string>(), Arg.Any<Guid?>()).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(eventId, venueId, VenueBuilder.UpdateRequest(name: "Updated Hall"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        venue.Name.Should().Be("Updated Hall");
        await _venueRepository.Received(1).UpdateVenueAsync(Arg.Any<Venue>());
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenVenueNotFound_ReturnsFail404()
    {
        // Arrange
        _venueRepository.GetByIdWithinEventForUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((Venue?)null);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAsync_WhenVenueExists_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var venue = VenueBuilder.CreateEntity(id: venueId, eventId: eventId);
        _venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId).Returns(venue);

        // Act
        var result = await _sut.DeleteAsync(eventId, venueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _venueRepository.Received(1).DeleteVenueAsync(venueId);
    }

    [Fact]
    public async Task DeleteAsync_WhenVenueHasLayout_DeletesPhotoFirst()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var venue = VenueBuilder.CreateEntity(id: venueId, eventId: eventId, layoutPublicId: "pub123");
        _venueRepository.GetByIdWithinEventForUpdateAsync(eventId, venueId).Returns(venue);

        // Act
        var result = await _sut.DeleteAsync(eventId, venueId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _photoService.Received(1).DeletePhotoAsync("pub123");
    }
}
