using Microsoft.EntityFrameworkCore;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;
using StreamingService_Application.UseCases.Streams.Commands.CreateStreamRoom;
using StreamingService_Infrastructure.Data;

namespace StreamingService_Test.Helpers.TestDataBuilders;

public class StreamRoomBuilder
{
    public static IStreamingServiceDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StreamingServiceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StreamingServiceDbContext(options);
    }

    public static StreamRoom CreateRoom(
        Guid? id = null,
        string roomName = "test-room",
        Guid? eventId = null,
        StreamRoomStatus status = StreamRoomStatus.Scheduled)
    {
        return new StreamRoom
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            TrackId = Guid.NewGuid(),
            LiveKitRoomName = roomName,
            LiveKitRoomSid = Guid.NewGuid().ToString(),
            Status = status,
            MaxParticipants = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }

    public static CreateStreamRoomCommand CreateCommand(Guid? eventId = null)
    {
        return new CreateStreamRoomCommand
        {
            EventId = eventId ?? Guid.NewGuid(),
            TrackId = Guid.NewGuid(),
            Title = "Live Stream",
            MaxParticipants = 100,
            CreatedBy = Guid.NewGuid(),
            IsRecordEnabled = true
        };
    }
}
