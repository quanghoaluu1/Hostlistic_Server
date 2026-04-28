using System.Net.Http.Json;
using Common;
using EventService_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventService_Infrastructure.ServiceClients;

public class BookingAccessClient(
    IHttpClientFactory httpClientFactory,
    ILogger<BookingAccessClient> logger) : IBookingAccessClient
{
    public async Task<BookingStreamAccessDto> GetStreamAccessAsync(Guid eventId, Guid userId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BookingService");
            var response = await client.GetAsync($"/api/Tickets/events/{eventId}/stream-access/{userId}/details");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "BookingService stream-access lookup failed for event {EventId} user {UserId}: {StatusCode}",
                    eventId,
                    userId,
                    response.StatusCode);
                return new BookingStreamAccessDto();
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BookingStreamAccessDto>>();
            return apiResponse?.IsSuccess == true && apiResponse.Data is not null
                ? apiResponse.Data
                : new BookingStreamAccessDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking BookingService stream access for event {EventId} user {UserId}", eventId, userId);
            return new BookingStreamAccessDto();
        }
    }
}
