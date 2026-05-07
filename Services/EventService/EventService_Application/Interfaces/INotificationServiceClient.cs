namespace EventService_Application.Interfaces;

public interface INotificationServiceClient
{
    /// <summary>
    /// Calls the NotificationService HTTP API to trigger Thank-You emails
    /// for all checked-in attendees of a completed event.
    /// Fire-and-forget safe — implementation must not throw.
    /// </summary>
    Task TriggerThankYouEmailAsync(
        Guid eventId,
        string eventTitle,
        Guid organizerId,
        DateTime completedAt,
        CancellationToken ct = default);
}
