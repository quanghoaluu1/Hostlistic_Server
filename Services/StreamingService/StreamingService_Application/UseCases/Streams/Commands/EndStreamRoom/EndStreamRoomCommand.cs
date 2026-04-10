using MediatR;
using Microsoft.EntityFrameworkCore;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;

namespace StreamingService_Application.UseCases.Streams.Commands.EndStreamRoom;

public record EndStreamRoomCommand(Guid RoomId, Guid UserId) : IRequest<bool>;

public class EndStreamRoomCommandHandler : IRequestHandler<EndStreamRoomCommand, bool>
{
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly ILiveKitService _liveKitService;

    public EndStreamRoomCommandHandler(IStreamingServiceDbContext dbContext, ILiveKitService liveKitService)
    {
        _dbContext = dbContext;
        _liveKitService = liveKitService;
    }

    public async Task<bool> Handle(EndStreamRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Set<StreamRoom>()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

        if (room == null)
            throw new Exception("Stream room not found");

        // Optional: Perform authorization check if request.UserId is allowed to end the room (e.g. Host)

        if (room.Status == StreamRoomStatus.Ended)
            return true;

        var remoteEnded = await _liveKitService.EndRoomAsync(room.LiveKitRoomName, cancellationToken);
        if (!remoteEnded.IsSuccess)
        {
            throw new InvalidOperationException(remoteEnded.ErrorMessage ?? "Failed to close room on LiveKit.");
        }

        room.Status = StreamRoomStatus.Ended;
        room.EndedAt = DateTime.UtcNow;
        room.UpdatedAt = DateTime.UtcNow;
        _dbContext.Set<StreamRoom>().Update(room);

        if (room.IsRecordEnabled)
        {
            var hasPendingOrReadyRecording = await _dbContext.StreamRecordings
                .AnyAsync(r => r.StreamRoomId == room.Id &&
                    (r.Status == RecordingStatus.Processing || r.Status == RecordingStatus.Ready), cancellationToken);

            if (!hasPendingOrReadyRecording)
            {
                _dbContext.StreamRecordings.Add(new StreamRecording
                {
                    Id = Guid.NewGuid(),
                    StreamRoomId = room.Id,
                    FileName = $"{room.LiveKitRoomName}.mp4",
                    Status = RecordingStatus.Processing,
                    FileSizeBytes = 0,
                    Duration = TimeSpan.Zero,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
