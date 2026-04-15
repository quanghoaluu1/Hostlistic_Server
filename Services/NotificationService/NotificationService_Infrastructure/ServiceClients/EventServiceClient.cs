using System.Net.Http.Json;
using Common;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos.ServiceClientDtos;
using NotificationService_Application.Interfaces;

namespace NotificationService_Infrastructure.ServiceClients;

public class EventServiceClient(IHttpClientFactory httpClientFactory, ILogger<EventServiceClient> logger) : IEventServiceClient
{
    public async Task<EventDetailDto?> GetEventAsync(Guid eventId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("EventService")
                .GetFromJsonAsync<ApiResponse<EventDetailDto>>($"api/event/{eventId}", ct);
            if (!response.IsSuccess)
            {
                logger.LogWarning("EventService GetEventInfo failed: {Status} - {Error}", response.StatusCode, response.Message);
                return null;
            }
            return response?.IsSuccess == true ? response.Data : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get event {EventId} from EventService", eventId);
            return null;
        }
    }
}