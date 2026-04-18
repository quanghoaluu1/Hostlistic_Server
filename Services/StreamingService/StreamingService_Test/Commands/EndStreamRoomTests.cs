using StreamingService_Application.UseCases.Streams.Commands.EndStreamRoom;
using StreamingService_Test.Helpers.TestDataBuilders;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using FluentAssertions;
using StreamingService_Domain.Enums;
using StreamingService_Domain.Entities;

namespace StreamingService_Test.Commands;

public class EndStreamRoomTests
{
    private readonly IStreamingServiceDbContext _dbContext = StreamRoomBuilder.CreateInMemoryDbContext();
    private readonly ILiveKitService _liveKitService = Substitute.For<ILiveKitService>();
    private readonly EndStreamRoomCommandHandler _sut;

    public EndStreamRoomTests()
    {
        _sut = new EndStreamRoomCommandHandler(_dbContext, _liveKitService);
    }

    [Fact]
    public async Task Handle_WithValidRequest_EndsRoomSuccessfully()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = StreamRoomBuilder.CreateRoom(roomId, "room-1");
        room.Status = StreamRoomStatus.Live;
        room.IsRecordEnabled = true;

        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _liveKitService.EndRoomAsync(room.LiveKitRoomName).Returns(new LiveKitOperationResult(true));

        // Act
        var result = await _sut.Handle(new EndStreamRoomCommand(roomId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        room.Status.Should().Be(StreamRoomStatus.Ended);
        room.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenRoomNotFound_ThrowsException()
    {
        // Act & Assert
        await _sut.Invoking(s => s.Handle(new EndStreamRoomCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Stream room not found");
    }

    [Fact]
    public async Task Handle_WhenLiveKitFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var room = StreamRoomBuilder.CreateRoom(Guid.NewGuid(), "room-1");
        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _liveKitService.EndRoomAsync(Arg.Any<string>()).Returns(new LiveKitOperationResult(false, "Internal Error"));

        // Act & Assert
        await _sut.Invoking(s => s.Handle(new EndStreamRoomCommand(room.Id, Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Internal Error");
    }
}
