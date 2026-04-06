using EventService_Domain;
using EventService_Test.Helpers.TestDataBuilders;

namespace EventService_Test;

public class EventTypeServiceTest
{
    private readonly IEventTypeRepository _eventTypeRepository;
    private readonly EventTypeService _sut;

    public EventTypeServiceTest()
    {
        _eventTypeRepository = Substitute.For<IEventTypeRepository>();
        _sut = new EventTypeService(_eventTypeRepository);
    }

    [Fact]
    public async Task CreateEventTypeAsync_WithValidName_ReturnsSuccess201()
    {
        // Arrange
        var request = new CreateEventTypeDto("Conference");

        // Act
        var result = await _sut.CreateEventTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Conference");
        result.Data.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateEventTypeAsync_WithEmptyOrWhitespaceName_ReturnsFail(string name)
    {
        // Arrange
        var request = new CreateEventTypeDto(name);

        // Act
        var result = await _sut.CreateEventTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task GetEventTypeByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _eventTypeRepository.GetEventTypeByIdAsync(Arg.Any<Guid>())
            .Returns((EventType?)null);

        // Act
        var result = await _sut.GetEventTypeByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateEventTypeAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _eventTypeRepository.GetEventTypeByIdAsync(Arg.Any<Guid>())
            .Returns((EventType?)null);

        // Act
        var result = await _sut.UpdateEventTypeAsync(
            Guid.NewGuid(),
            new UpdateEventTypeDto("New Name", true));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateEventTypeAsync_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existing = EventTypeBuilder.CreateEventType(name: "Original", isActive: true);
        _eventTypeRepository.GetEventTypeByIdAsync(existing.Id).Returns(existing);

        var updateDto = new UpdateEventTypeDto(Name: null, IsActive: false);

        // Act
        var result = await _sut.UpdateEventTypeAsync(existing.Id, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existing.Name.Should().Be("Original"); // Name unchanged
        existing.IsActive.Should().BeFalse();   // IsActive updated
    }
}