using BookingService_Domain.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class SessionDeletedEventConsumer(
    ISessionSnapshotRepository sessionSnapshotRepository,
    ILogger<SessionDeletedEventConsumer> logger
) : IConsumer<SessionDeletedEvent>
{
    public async Task Consume(ConsumeContext<SessionDeletedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received SessionDeletedEvent for session {SessionId} in event {EventId}",
            message.SessionId, message.EventId);

        var deleted = await sessionSnapshotRepository.DeleteAsync(message.SessionId);
        if (!deleted)
            logger.LogWarning("SessionSnapshot for session {SessionId} was not found — may have already been deleted",
                message.SessionId);
        else
            logger.LogInformation("SessionSnapshot deleted for session {SessionId}", message.SessionId);
    }
}
