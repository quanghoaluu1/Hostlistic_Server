using Common.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Enums;

namespace StreamingService_Application.Consumers;

public class EventCompletedConsumer : IConsumer<EventCompletedMessage>
{
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly ILiveKitService _liveKitService;
    private readonly ILogger<EventCompletedConsumer> _logger;

    public EventCompletedConsumer(
        IStreamingServiceDbContext dbContext,
        ILiveKitService liveKitService,
        ILogger<EventCompletedConsumer> logger)
    {
        _dbContext = dbContext;
        _liveKitService = liveKitService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventCompletedMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing EventCompletedMessage for Event {EventId}.", message.EventId);

        var activeRooms = await _dbContext.StreamRooms
            .Where(r => r.EventId == message.EventId && (r.Status == StreamRoomStatus.Live || r.Status == StreamRoomStatus.Scheduled))
            .ToListAsync();

        if (!activeRooms.Any())
        {
            _logger.LogInformation("No active rooms found for Event {EventId}.", message.EventId);
            return;
        }

        foreach (var room in activeRooms)
        {
            try
            {
                _logger.LogInformation("Ending room {RoomName} for Event {EventId}.", room.LiveKitRoomName, message.EventId);
                
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
                _logger.LogError(ex, "Error ending room {RoomName} for Event {EventId}.", room.LiveKitRoomName, message.EventId);
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully ended {Count} rooms for Event {EventId}.", activeRooms.Count, message.EventId);
    }
}
