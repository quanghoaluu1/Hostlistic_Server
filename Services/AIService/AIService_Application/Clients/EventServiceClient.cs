using System.Net.Http.Json;
using System.Text.Json;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using Common;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public class EventServiceClient(HttpClient httpClient, ILogger<EventServiceClient> logger) : IEventServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default)
    {
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
}
