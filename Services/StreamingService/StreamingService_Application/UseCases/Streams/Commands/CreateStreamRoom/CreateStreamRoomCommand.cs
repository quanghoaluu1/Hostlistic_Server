using MediatR;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;
// Assuming there is a DbContext interface or we inject StreamingDbContext directly. Using DbContext for now.
using Microsoft.EntityFrameworkCore;
// Note: You should replace `DbContext` with your actual DbContext application interface like `IApplicationDbContext`

namespace StreamingService_Application.UseCases.Streams.Commands.CreateStreamRoom;

public record CreateStreamRoomCommand : IRequest<Guid>
{
    public Guid EventId { get; init; }
    public Guid? TrackId { get; init; }
    public Guid? SessionId { get; init; }
    public string Title { get; init; } = null!;
    public int MaxParticipants { get; init; } = 100;
    public Guid CreatedBy { get; init; }
    public DateTime? ScheduledStartAt { get; init; }
    public bool IsRecordEnabled { get; init; } = true;
}

public class CreateStreamRoomCommandHandler : IRequestHandler<CreateStreamRoomCommand, Guid>
{
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly ILiveKitService _liveKitService;
    private readonly IEventServiceClient _eventServiceClient;

    public CreateStreamRoomCommandHandler(IStreamingServiceDbContext dbContext, ILiveKitService liveKitService, IEventServiceClient eventServiceClient)
    {
        _dbContext = dbContext;
        _liveKitService = liveKitService;
        _eventServiceClient = eventServiceClient;
    }

    public async Task<Guid> Handle(CreateStreamRoomCommand request, CancellationToken cancellationToken)
    {
        // Ask EventService if user has right to create room
        var authResult = await _eventServiceClient.VerifyStreamAccessAsync(request.EventId, request.CreatedBy, cancellationToken);
        if (!authResult.IsAllowed)
        {
            throw new UnauthorizedAccessException(authResult.ErrorMessage ?? "Not allowed to open Livestream right now.");
        }
        
        if (authResult.Role != "Organizer" && authResult.Role != "CoOrganizer")
        {
            throw new UnauthorizedAccessException($"Mở phòng thất bại: Chỉ Organizer hoặc CoOrganizer được phép mở Stream (bạn đang có quyền {authResult.Role}).");
        }

        if (request.TrackId is null || request.TrackId == Guid.Empty)
        {
            throw new InvalidOperationException("A track must be selected before starting the livestream.");
        }

        var existingRoom = await _dbContext.Set<StreamRoom>()
            .Where(r =>
                r.EventId == request.EventId &&
                r.TrackId == request.TrackId &&
                r.Status == StreamRoomStatus.Live)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRoom != null)
        {
            return existingRoom.Id;
        }

        var roomName = $"room-{Guid.NewGuid():N}";
        
        // 1. Ask LiveKit server to create the room
        var roomCreated = await _liveKitService.CreateRoomAsync(roomName, request.MaxParticipants, cancellationToken);
        if (!roomCreated.IsSuccess)
        {
            throw new InvalidOperationException(roomCreated.ErrorMessage ?? "Failed to create room on LiveKit server.");
        }

        // 2. Create the room in our local database
        var streamRoom = new StreamRoom
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            TrackId = request.TrackId,
            SessionId = request.SessionId,
            LiveKitRoomName = roomName,
            LiveKitRoomSid = string.Empty, // Will be populated by webhook when room starts
            Status = StreamRoomStatus.Scheduled,
            MaxParticipants = request.MaxParticipants,
            IsRecordEnabled = request.IsRecordEnabled,
            CreatedBy = request.CreatedBy,
            ScheduledStartAt = request.ScheduledStartAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            _dbContext.Set<StreamRoom>().Add(streamRoom);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _liveKitService.EndRoomAsync(roomName, cancellationToken);
            throw;
        }

        return streamRoom.Id;
    }
}
