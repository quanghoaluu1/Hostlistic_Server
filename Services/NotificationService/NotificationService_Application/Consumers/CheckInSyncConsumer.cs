using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Consumers;

/// <summary>
/// Consumes <see cref="CheckInCompletedEvent"/> published by BookingService when an attendee
/// successfully scans their QR code at the event entrance (EventLevel check-in only).
///
/// Responsibility: flip <c>EventRecipient.IsCheckedIn = true</c> so that
/// <see cref="EventCompletedConsumer"/> can later target only checked-in attendees for the
/// automated "Thank You" email campaign.
///
/// Idempotency: <c>MarkCheckedInAsync</c> issues a bulk UPDATE — re-delivery is safe because
/// setting an already-true flag to true is a no-op.
///
/// Note: session-level check-ins are deliberately ignored here; they do not affect the
/// "has attended" flag used for post-event emails.
/// </summary>
public class CheckInSyncConsumer(
    IEventRecipientRepository recipientRepository,
    ILogger<CheckInSyncConsumer> logger) : IConsumer<CheckInCompletedEvent>
{
    private const int EventLevel = 0; // CheckInType.EventLevel == 0

    public async Task Consume(ConsumeContext<CheckInCompletedEvent> context)
    {
        var msg = context.Message;

        // ── Entry-point diagnostic log (visible at Information level) ─────────
        logger.LogInformation(
            "CheckInSyncConsumer: received CheckInCompletedEvent — " +
            "CheckIn {CheckInId}, Ticket {TicketId}, Event {EventId}, " +
            "User {UserId}, CheckInType {CheckInType}.",
            msg.CheckInId, msg.TicketId, msg.EventId, msg.UserId, msg.CheckInType);

        // Only event-level check-ins mark an attendee as "attended"
        if (msg.CheckInType != EventLevel)
        {
            logger.LogDebug(
                "CheckInSyncConsumer: skipping session-level check-in {CheckInId} for ticket {TicketId}.",
                msg.CheckInId, msg.TicketId);
            return;
        }

        logger.LogInformation(
            "CheckInSyncConsumer: marking UserId {UserId} as checked-in for Event {EventId}.",
            msg.UserId, msg.EventId);

        try
        {
            await recipientRepository.MarkCheckedInAsync(msg.EventId, msg.UserId);

            logger.LogInformation(
                "CheckInSyncConsumer: IsCheckedIn successfully updated for UserId {UserId}, Event {EventId}.",
                msg.UserId, msg.EventId);
        }
        catch (Exception ex)
        {
            // Re-throw so MassTransit's retry policy retries the message.
            // Without this, a DB failure would silently drop the update — the
            // attendee's IsCheckedIn flag would stay false and they would NOT
            // receive the Thank-You email after event completion.
            logger.LogError(ex,
                "CheckInSyncConsumer: FAILED to mark UserId {UserId} as checked-in for Event {EventId}. " +
                "MassTransit will retry. CheckInId: {CheckInId}.",
                msg.UserId, msg.EventId, msg.CheckInId);
            throw;
        }
    }
}
