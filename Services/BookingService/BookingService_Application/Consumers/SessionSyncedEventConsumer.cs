using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class SessionSyncedEventConsumer(
    ISessionSnapshotRepository sessionSnapshotRepository,
    ILogger<SessionSyncedEventConsumer> logger
) : IConsumer<SessionSyncedEvent>
{
    public async Task Consume(ConsumeContext<SessionSyncedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received SessionSyncedEvent for session {SessionId} in event {EventId}",
            message.SessionId, message.EventId);

        var snapshot = new SessionSnapshot
        {
            Id = message.SessionId,
            EventId = message.EventId,
            TrackId = message.TrackId,
            Title = message.Title,
            StartTime = message.StartTime,
            EndTime = message.EndTime,
            Location = message.Location,
            SessionOrder = message.SessionOrder,
            LastSyncedAt = DateTime.UtcNow
        };

        await sessionSnapshotRepository.UpsertAsync(snapshot);
        logger.LogInformation("SessionSnapshot upserted for session {SessionId}", message.SessionId);
    }
}
