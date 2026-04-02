using Common.Messages;
using EventService_Api.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace EventService_Api.Consumers;

public class CheckInCompletedEventConsumer(
    IHubContext<CheckInHub> hubContext,
    ILogger<CheckInCompletedEventConsumer> logger) : IConsumer<CheckInCompletedEvent>
{
    public async Task Consume(ConsumeContext<CheckInCompletedEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Received CheckInCompletedEvent — CheckIn: {CheckInId}, Event: {EventId}, Ticket: {TicketCode}",
            msg.CheckInId, msg.EventId, msg.TicketCode);

        try
        {
            await hubContext.Clients
                .Group($"event-{msg.EventId}")
                .SendAsync("CheckInUpdate", new
                {
                    msg.CheckInId,
                    msg.TicketId,
                    msg.AttendeeName,
                    msg.TicketCode,
                    msg.TicketTypeName,
                    msg.SessionId,
                    msg.SessionName,
                    msg.CheckInTime,
                    msg.CheckInType
                }, context.CancellationToken);

            logger.LogDebug(
                "Pushed CheckInUpdate to SignalR group event-{EventId} for check-in {CheckInId}",
                msg.EventId, msg.CheckInId);
        }
        catch (Exception ex)
        {
            // SignalR push is fire-and-forget — log but do not rethrow so MassTransit does not retry
            logger.LogWarning(ex,
                "Failed to push CheckInUpdate to SignalR group event-{EventId} for check-in {CheckInId}",
                msg.EventId, msg.CheckInId);
        }
    }
}
