using StreamingService_Test.Helpers.TestDataBuilders;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using FluentAssertions;
// Wait, I should check the return type of _eventServiceClient.VerifyStreamAccessAsync

namespace StreamingService_Test.Commands;

public class CreateStreamRoomTests
{
    private readonly IStreamingServiceDbContext _dbContext = StreamRoomBuilder.CreateInMemoryDbContext();
    private readonly ILiveKitService _liveKitService = Substitute.For<ILiveKitService>();
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    
    private readonly CreateStreamRoomCommandHandler _sut;

    public CreateStreamRoomTests()
    {
        _sut = new CreateStreamRoomCommandHandler(_dbContext, _liveKitService, _eventServiceClient);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsRoomId()
    {
        // Arrange
        var command = StreamRoomBuilder.CreateCommand();
        
        _eventServiceClient.VerifyStreamAccessAsync(command.EventId, command.CreatedBy)
            .Returns(Task.FromResult(new StreamAuthResponseDto { IsAllowed = true, Role = "Organizer" }));
            
        _liveKitService.CreateRoomAsync(Arg.Any<string>(), command.MaxParticipants)
            .Returns(Task.FromResult(new LiveKitOperationResult(true)));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAllowed_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = StreamRoomBuilder.CreateCommand();
        
        _eventServiceClient.VerifyStreamAccessAsync(command.EventId, command.CreatedBy)
            .Returns(Task.FromResult(new StreamAuthResponseDto { IsAllowed = false, ErrorMessage = "Not allowed" }));

        // Act & Assert
        await _sut.Invoking(s => s.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Not allowed");
    }

    [Fact]
    public async Task Handle_WhenEventServiceFails_ThrowsException()
    {
        // Arrange
        var command = StreamRoomBuilder.CreateCommand();
        _eventServiceClient.VerifyStreamAccessAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(Task.FromException<StreamAuthResponseDto>(new Exception("Event Service Down")));

        // Act & Assert
        await _sut.Invoking(s => s.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Event Service Down");
    }
}
