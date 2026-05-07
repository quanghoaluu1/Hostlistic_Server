using Common;

namespace NotificationService_Application.Interfaces;

public interface IThankYouEmailService
{
    /// <summary>
    /// Creates a Thank-You email campaign targeting all checked-in attendees for the given event,
    /// then sends the emails synchronously (directly via Resend, no RabbitMQ queue).
    /// Safe to call multiple times — idempotent via IsAutoReminder guard.
    /// </summary>
    Task<ApiResponse<ThankYouEmailResult>> SendThankYouEmailsAsync(
        Guid eventId,
        string eventTitle,
        Guid organizerId,
        DateTime completedAt,
        CancellationToken ct = default);
}

public sealed record ThankYouEmailResult(
    Guid CampaignId,
    int TotalRecipients,
    int Sent,
    int Failed,
    string Message);
