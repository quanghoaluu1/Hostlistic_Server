using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class EventCompletedConsumer(ILogger<EventCompletedConsumer> logger) : IConsumer<EventCompletedMessage>
{
    public Task Consume(ConsumeContext<EventCompletedMessage> context)
    {
        // Settlement logic is deferred to a future implementation phase.
        // Completing without error prevents MassTransit from retrying and
        // moving the message to the Dead Letter Queue.
        logger.LogWarning(
            "EventCompletedConsumer received message for Event {EventId} but settlement is not yet implemented. Deferring.",
            context.Message.EventId);

        return Task.CompletedTask;
    }
}