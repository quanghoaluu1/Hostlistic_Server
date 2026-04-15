using System.Net.Http.Json;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos.ServiceClientDtos;
using NotificationService_Application.Interfaces;

namespace NotificationService_Infrastructure.ServiceClients;

public class BookingServiceClient(IHttpClientFactory httpClientFactory, ILogger<EventServiceClient> logger) : IBookingServiceClient
{
    public async Task<List<EmailRecipientDto>> GetEmailRecipientsAsync(
        Guid eventId,
        int recipientGroup,
        List<Guid>? ticketTypeIds = null,
        List<Guid>? specificUserIds = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<KeyValuePair<string, string?>>
            {
                new("recipientGroup", recipientGroup.ToString())
            };

            if (ticketTypeIds?.Count > 0)
                queryParams.AddRange(ticketTypeIds.Select(id =>
                    new KeyValuePair<string, string?>("ticketTypeIds", id.ToString())));

            if (specificUserIds?.Count > 0)
                queryParams.AddRange(specificUserIds.Select(id =>
                    new KeyValuePair<string, string?>("specificUserIds", id.ToString())));

            var qs = QueryString.Create(queryParams);
            var url = $"api/attendees/events/{eventId}/email-recipients{qs}";

            var response = await httpClientFactory.CreateClient("BookingService")
                .GetFromJsonAsync<ApiResponse<List<EmailRecipientDto>>>(url, ct);

            return response?.IsSuccess == true ? response.Data ?? [] : [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to get email recipients for event {EventId}", eventId);
            return [];
        }
    }
}