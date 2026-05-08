using BookingService_Domain.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class EventCompletedConsumer(
    ITicketRepository ticketRepository,
    ILogger<EventCompletedConsumer> logger) : IConsumer<EventCompletedMessage>
{
    public async Task Consume(ConsumeContext<EventCompletedMessage> context)
    {
        var eventId = context.Message.EventId;

        // 1. Mark tickets that are not checked in as IsExpired
        try
        {
            await ticketRepository.ExpireUnusedTicketsForEventAsync(eventId);
            logger.LogInformation("Successfully marked unused tickets as expired for Event {EventId}", eventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark unused tickets as expired for Event {EventId}", eventId);
            throw; // Re-throw to let MassTransit retry
        }

        // Settlement logic is deferred to a future implementation phase.
        // Completing without error prevents MassTransit from retrying and
        // moving the message to the Dead Letter Queue.
        logger.LogWarning(
            "EventCompletedConsumer received message for Event {EventId} but settlement is not yet implemented. Deferring.",
            eventId);
    }
}