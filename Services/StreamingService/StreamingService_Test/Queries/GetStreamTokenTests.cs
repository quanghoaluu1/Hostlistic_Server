using StreamingService_Test.Helpers.TestDataBuilders;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using FluentAssertions;

namespace StreamingService_Test.Queries;

public class GetStreamTokenTests
{
    private readonly IStreamingServiceDbContext _dbContext = StreamRoomBuilder.CreateInMemoryDbContext();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    
    private readonly GetStreamTokenQueryHandler _sut;

    public GetStreamTokenTests()
    {
        _sut = new GetStreamTokenQueryHandler(_dbContext, _tokenGenerator, _eventServiceClient);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsToken()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = StreamRoomBuilder.CreateRoom(id: roomId, status: StreamRoomStatus.Live);
        var query = new GetStreamTokenQuery(roomId, userId, "user-identity", ParticipantRole.Attendee);

        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _eventServiceClient.VerifyStreamAccessAsync(room.EventId, userId)
            .Returns(new StreamAuthResponseDto { IsAllowed = true, Role = "Attendee" });
            
        _tokenGenerator.GenerateLiveKitToken(room.LiveKitRoomName, query.Identity, ParticipantRole.Attendee)
            .Returns("valid-token");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be("valid-token");
    }

    [Fact]
    public async Task Handle_WhenRoomNotFound_ThrowsException()
    {
        // Arrange
        var query = new GetStreamTokenQuery(Guid.NewGuid(), Guid.NewGuid(), "id", ParticipantRole.Attendee);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRoomEnded_ThrowsException()
    {
        // Arrange
        var room = StreamRoomBuilder.CreateRoom(Guid.NewGuid(), "room-ended");
        room.Status = StreamRoomStatus.Ended;
        var query = new GetStreamTokenQuery(room.Id, Guid.NewGuid(), "Guest-123", ParticipantRole.Attendee);

        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act & Assert
        await _sut.Invoking(s => s.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Stream room has already ended");
    }

    [Fact]
    public async Task Handle_WhenUserUnauthorized_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var room = StreamRoomBuilder.CreateRoom(Guid.NewGuid(), "room-1");
        var query = new GetStreamTokenQuery(room.Id, Guid.NewGuid(), "User-X", ParticipantRole.Attendee);

        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _eventServiceClient.VerifyStreamAccessAsync(room.EventId, query.UserId)
            .Returns(new StreamAuthResponseDto { IsAllowed = false, ErrorMessage = "Access Denied" });

        // Act & Assert
        await _sut.Invoking(s => s.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Access Denied");
    }
}
