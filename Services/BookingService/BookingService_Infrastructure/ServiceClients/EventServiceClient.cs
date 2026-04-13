using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Application.Services;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class EventServiceClient : IEventServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EventServiceClient> _logger;

    public EventServiceClient(IHttpClientFactory httpClientFactory, ILogger<EventServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<EventInfoDto?> GetEventInfoAsync(Guid eventId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("EventService");
            // EventService controller is `EventController` (singular)
            var url = $"/api/Event/{eventId}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("EventService GetEventInfo failed: {Status} - {Error}", response.StatusCode, error);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventInfoDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling EventService for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<TicketTypeInfoDto?> GetTicketTypeInfoAsync(Guid ticketTypeId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("EventService");
            var url = $"/api/TicketTypes/{ticketTypeId}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("EventService GetTicketTypeInfo failed: {Status} - {Error}", response.StatusCode, error);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TicketTypeInfoDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling EventService for ticket type {TicketTypeId}", ticketTypeId);
            return null;
        }
    }
    
    public async Task<List<TicketTypeDto>?> GetTicketTypesByEventIdAsync(Guid eventId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("EventService");
            var response = await httpClient.GetAsync($"/api/TicketTypes/event/{eventId}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("EventService GetTicketTypesByEventId failed: {Status} - {Error}", response.StatusCode, error);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketTypeDto>>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling EventService for ticket types of event {EventId}", eventId);
            return null;
        }
    }

    public async Task<EventSettlementInfoDto?> GetEventSettlementInfoAsync(Guid eventId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("EventService");
            var response = await httpClient.GetAsync($"/api/Event/{eventId}");

            if (!response.IsSuccessStatusCode) return null;

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<EventSettlementInfoDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settlement info for event {EventId}", eventId);
            return null;
        }
    }
    
    
}

