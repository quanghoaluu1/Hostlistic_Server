using System.Net.Http.Json;
using System.Text.Json;
using AIService_Application.DTOs.External;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public class EventServiceClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EventServiceClient> logger) : IEventServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // ── Existing Methods ──────────────────────────────────────────────────────

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        try
        {
            var response = await httpClient.GetAsync($"api/event/{eventId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("EventService returned {StatusCode} for event {EventId}", response.StatusCode, eventId);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventDetailDto>>(JsonOptions, ct);

            if (apiResponse is null || !apiResponse.IsSuccess)
            {
                logger.LogWarning("EventService returned unsuccessful response for event {EventId}: {Message}", eventId, apiResponse?.Message);
                return null;
            }

            return apiResponse.Data;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error calling EventService for event {EventId}", eventId);
            return null;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "EventService request timed out for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<LineupDetailDto?> GetEventLineupAsync(Guid eventId, CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        try
        {
            var response = await httpClient.GetAsync($"/api/events/{eventId}/lineup/", ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<LineupDetailDto>>(cancellationToken: ct);

            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch lineup for event {EventId}", eventId);
            return null;
        }
    }

    // ── Post-Event Summary Methods ────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ExternalEngagementAnalyticsDto?> GetEngagementAnalyticsAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetSingleAsync<ExternalEngagementAnalyticsDto>(
            $"/api/events/{eventId}/engagement/analytics",
            eventId,
            nameof(GetEngagementAnalyticsAsync),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExternalFeedbackDto>> GetFeedbackAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetListAsync<ExternalFeedbackDto>(
            $"/api/feedback/event/{eventId}",
            eventId,
            nameof(GetFeedbackAsync),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ExternalSurveyFormDto>> GetSurveysAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetListAsync<ExternalSurveyFormDto>(
            $"/api/events/{eventId}/surveys",
            eventId,
            nameof(GetSurveysAsync),
            ct);
    }

    /// <inheritdoc/>
    public async Task<ExternalSurveySummaryDto?> GetSurveySummaryAsync(
        Guid eventId,
        Guid surveyId,
        CancellationToken ct = default)
    {
        ForwardAuthorizationHeader();
        return await GetSingleAsync<ExternalSurveySummaryDto>(
            $"/api/events/{eventId}/surveys/{surveyId}/summary",
            eventId,
            nameof(GetSurveySummaryAsync),
            ct);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Propagates the caller's Authorization header so EventService can
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
                logger.LogWarning("[{Caller}] EventService returned {Status} for event {EventId}",
                    callerName, response.StatusCode, eventId);
                return null;
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);

            if (envelope is null || !envelope.IsSuccess)
            {
                logger.LogWarning("[{Caller}] EventService response unsuccessful for event {EventId}: {Msg}",
                    callerName, eventId, envelope?.Message);
                return null;
            }

            return envelope.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Caller}] Error fetching from EventService for event {EventId}", callerName, eventId);
            return null;
        }
    }

    /// <summary>
    /// Performs a GET that returns a list wrapped in ApiResponse&lt;List&lt;T&gt;&gt;.
    /// Returns an empty list on any failure without propagating exceptions.
    /// </summary>
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
                logger.LogWarning("[{Caller}] EventService returned {Status} for event {EventId}",
                    callerName, response.StatusCode, eventId);
                return [];
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<T>>>(JsonOptions, ct);

            return envelope?.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Caller}] Error fetching list from EventService for event {EventId}", callerName, eventId);
            return [];
        }
    }
}
