using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

public class BookingConfirmedConsumer(
    IEventRecipientRepository eventRecipientRepository,
    ILogger<BookingConfirmedConsumer> logger) : IConsumer<BookingConfirmedEvent>
{
    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Received BookingConfirmedEvent — Event: {EventId}, User: {UserId}, Tickets: {Count}",
            msg.EventId, msg.UserId, msg.Tickets.Count);

        // Create one EventRecipient per ticket type in the order
        // This enables SpecificTicketType targeting in campaigns
        foreach (var ticket in msg.Tickets)
        {
            var recipient = new EventRecipient
            {
                EventId = msg.EventId,
                UserId = msg.UserId,
                Email = msg.Email,
                FullName = msg.FullName,
                TicketTypeId = ticket.TicketTypeId,
                TicketTypeName = ticket.TicketTypeName,
                BookingConfirmedAt = msg.ConfirmedAt,
                SyncedAt = DateTime.UtcNow
            };

            await eventRecipientRepository.UpsertAsync(recipient);
        }

        await eventRecipientRepository.SaveChangesAsync();

        logger.LogInformation(
            "Synced {Count} EventRecipient records for Event {EventId}, User {UserId}",
            msg.Tickets.Count, msg.EventId, msg.UserId);
    }
}
