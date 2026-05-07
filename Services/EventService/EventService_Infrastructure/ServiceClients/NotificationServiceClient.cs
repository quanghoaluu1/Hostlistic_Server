using System.Net.Http.Json;
using EventService_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventService_Infrastructure.ServiceClients;

public class NotificationServiceClient(
    IHttpClientFactory httpClientFactory,
    ILogger<NotificationServiceClient> logger) : INotificationServiceClient
{
    public async Task TriggerThankYouEmailAsync(
        Guid eventId,
        string eventTitle,
        Guid organizerId,
        DateTime completedAt,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation(
                "NotificationServiceClient: calling POST /api/event-email/{EventId}/thank-you for Event '{Title}'.",
                eventId, eventTitle);

            var client = httpClientFactory.CreateClient("NotificationService");

            var payload = new
            {
                EventTitle = eventTitle,
                OrganizerId = organizerId,
                CompletedAt = completedAt
            };

            var response = await client.PostAsJsonAsync(
                $"/api/event-email/{eventId}/thank-you", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "NotificationServiceClient: Thank-You email trigger succeeded for Event {EventId}. " +
                    "HTTP {StatusCode}.",
                    eventId, (int)response.StatusCode);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "NotificationServiceClient: Thank-You email trigger FAILED for Event {EventId}. " +
                    "HTTP {StatusCode}: {Body}",
                    eventId, (int)response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            // Never throw — event completion must succeed even if notification fails.
            logger.LogError(ex,
                "NotificationServiceClient: Exception calling Thank-You email endpoint for Event {EventId}.",
                eventId);
        }
    }
}
