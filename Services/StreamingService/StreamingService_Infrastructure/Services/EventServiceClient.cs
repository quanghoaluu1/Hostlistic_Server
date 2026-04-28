using System.Net.Http.Json;
using StreamingService_Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace StreamingService_Infrastructure.Services;

public class EventServiceClient : IEventServiceClient
{
    private readonly HttpClient _httpClient;

    public EventServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StreamAuthResponseDto> VerifyStreamAccessAsync(Guid eventId, Guid userId, Guid? trackId = null, CancellationToken cancellationToken = default)
    {
        var trackQuery = trackId.HasValue ? $"&trackId={trackId.Value}" : string.Empty;
        var response = await _httpClient.GetAsync($"/api/Event/{eventId}/stream-auth?userId={userId}{trackQuery}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
            throw new Exception($"EventService returned {(int)response.StatusCode}.");

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StreamAuthResponseDto>>(cancellationToken: cancellationToken);
        
        if (result == null || !result.IsSuccess || result.Data == null)
        {
            return new StreamAuthResponseDto 
            { 
                IsAllowed = false, 
                ErrorMessage = result?.Message ?? "Failed to authenticate with Event Service." 
            };
        }

        return result.Data;
    }

    public async Task<TicketTypeStreamingAccessDto?> GetTicketTypeStreamingAccessAsync(Guid ticketTypeId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/TicketTypes/{ticketTypeId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TicketTypeStreamingAccessDto>>(cancellationToken: cancellationToken);
        if (result == null || !result.IsSuccess || result.Data == null)
            return null;

        return result.Data;
    }

    public async Task<EventChatAccessResponseDto> GetEventChatAccessAsync(Guid eventId, Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/events/{eventId}/engagement/chat-access?sessionId={sessionId}&userId={userId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"EventService returned {(int)response.StatusCode}.");

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<EventChatAccessResponseDto>>(cancellationToken: cancellationToken);
        if (result == null || !result.IsSuccess || result.Data == null)
        {
            throw new Exception(result?.Message ?? "Failed to retrieve chat access from Event Service.");
        }

        return result.Data;
    }
}
