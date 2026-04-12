using System.Net.Http.Json;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class EventPermissionClient(IHttpClientFactory httpClientFactory, ILogger<EventPermissionClient> logger) : IEventPermissionClient
{
    private sealed record PermissionCheckResponse(bool Granted);

    public async Task<bool> HasPermissionAsync(
        Guid eventId, Guid userId, string permissionKey,
        CancellationToken ct = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("EventService");
            var url = $"/api/internal/auth/check?eventId={eventId}&userId={userId}&permission={permissionKey}";

            logger.LogDebug(
                "Checking permission '{Permission}' for user {UserId} on event {EventId}",
                permissionKey, userId, eventId);

            var res = await httpClient.GetAsync(url, ct);

            if (!res.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "EventService auth check returned {StatusCode} for event {EventId}, user {UserId}",
                    res.StatusCode, eventId, userId);
                return false;
            }

            var result = await res.Content.ReadFromJsonAsync<PermissionCheckResponse>(ct);
            return result?.Granted ?? false;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Failed to check permission with EventService for event {EventId}, user {UserId}. Denying access.",
                eventId, userId);
            return false;
        }
    }
}