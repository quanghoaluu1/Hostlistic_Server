using Common.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Enums;

namespace StreamingService_Application.Consumers;

public class SessionCompletedConsumer : IConsumer<SessionCompletedMessage>
{
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly ILiveKitService _liveKitService;
    private readonly ILogger<SessionCompletedConsumer> _logger;

    public SessionCompletedConsumer(
        IStreamingServiceDbContext dbContext,
        ILiveKitService liveKitService,
        ILogger<SessionCompletedConsumer> logger)
    {
        _dbContext = dbContext;
        _liveKitService = liveKitService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SessionCompletedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing SessionCompletedMessage for Session {SessionId}.", message.SessionId);

        var activeRooms = await _dbContext.StreamRooms
            .Where(r => r.SessionId == message.SessionId && (r.Status == StreamRoomStatus.Live || r.Status == StreamRoomStatus.Scheduled))
            .ToListAsync();

        if (!activeRooms.Any())
        {
            _logger.LogInformation("No active rooms found for Session {SessionId}.", message.SessionId);
            return;
        }

        foreach (var room in activeRooms)
        {
            try
            {
                _logger.LogInformation("Ending room {RoomName} for Session {SessionId}.", room.LiveKitRoomName, message.SessionId);
                
                // Terminate LiveKit room
                var result = await _liveKitService.EndRoomAsync(room.LiveKitRoomName);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to end LiveKit room {RoomName}: {Error}", room.LiveKitRoomName, result.ErrorMessage);
                }

                // Update room status
                room.Status = StreamRoomStatus.Ended;
                room.EndedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending room {RoomName} for Session {SessionId}.", room.LiveKitRoomName, message.SessionId);
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully ended {Count} rooms for Session {SessionId}.", activeRooms.Count, message.SessionId);
    }
}
