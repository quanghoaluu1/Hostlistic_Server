using System.Net;
using System.Net.Http.Json;
using StreamingService_Application.Interfaces;

namespace StreamingService_Infrastructure.Services;

public class BookingServiceClient : IBookingServiceClient
{
    private readonly HttpClient _httpClient;

    public BookingServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GuestLiveTicketValidationDto?> ValidateGuestLiveTicketAsync(Guid eventId, string ticketCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/Tickets/guest-live-access",
            new
            {
                EventId = eventId,
                TicketCode = ticketCode
            },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
            return null;

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GuestLiveTicketValidationDto>>(cancellationToken: cancellationToken);
        if (result == null || !result.IsSuccess || result.Data == null)
            return null;

        return result.Data;
    }
}
