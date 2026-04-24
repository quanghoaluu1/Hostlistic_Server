using System.Net.Http.Json;
using System.Text.Json;
using AIService_Application.DTOs.External;
using AIService_Application.Interface;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIService_Infrastructure.ServiceClients;

/// <summary>
/// Typed HttpClient that calls BookingService on behalf of AIService
/// to gather data needed for the post-event AI summary feature.
/// </summary>
public class BookingServiceClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BookingServiceClient> logger) : IBookingServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc/>
    public async Task<ExternalAttendeeSummaryDto?> GetAttendeeSummaryAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetSingleAsync<ExternalAttendeeSummaryDto>(
            $"/api/events/{eventId}/attendees/summary",
            eventId,
            nameof(GetAttendeeSummaryAsync),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExternalOrderDto>> GetOrdersAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetListAsync<ExternalOrderDto>(
            $"/api/orders/event/{eventId}",
            eventId,
            nameof(GetOrdersAsync),
            ct);
    }

    /// <inheritdoc/>
    public async Task<ExternalCheckInStatsDto?> GetCheckInStatsAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetSingleAsync<ExternalCheckInStatsDto>(
            $"/api/checkin/event/{eventId}/stats",
            eventId,
            nameof(GetCheckInStatsAsync),
            ct);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Propagates the caller's Authorization header so BookingService can
    /// validate the organizer's JWT without requiring a separate S2S token.
    /// </summary>
    private void ForwardAuthorizationHeader()
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
    }

    private async Task<T?> GetSingleAsync<T>(
        string path,
        Guid eventId,
        string callerName,
        CancellationToken ct)
        where T : class
    {
        try
        {
            var response = await httpClient.GetAsync(path, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("[{Caller}] BookingService returned {Status} for event {EventId}",
                    callerName, response.StatusCode, eventId);
                return null;
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);

            if (envelope is null || !envelope.IsSuccess)
            {
                logger.LogWarning("[{Caller}] BookingService response unsuccessful for event {EventId}: {Msg}",
                    callerName, eventId, envelope?.Message);
                return null;
            }

            return envelope.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Caller}] Error fetching from BookingService for event {EventId}", callerName, eventId);
            return null;
        }
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(
        string path,
        Guid eventId,
        string callerName,
        CancellationToken ct)
    {
        try
        {
            var response = await httpClient.GetAsync(path, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("[{Caller}] BookingService returned {Status} for event {EventId}",
                    callerName, response.StatusCode, eventId);
                return [];
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<T>>>(JsonOptions, ct);

            return envelope?.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Caller}] Error fetching list from BookingService for event {EventId}", callerName, eventId);
            return [];
        }
    }
}
