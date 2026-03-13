using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class NotificationServiceClient : INotificationServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(IHttpClientFactory httpClientFactory, ILogger<NotificationServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendTicketPurchaseConfirmationAsync(PurchaseConfirmationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.CustomerEmail))
            {
                _logger.LogWarning("Skipping ticket confirmation email because customer email is empty");
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient("NotificationService");
            var url = "/api/Email/send-ticket-confirmation";

            var response = await httpClient.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("NotificationService send-ticket-confirmation failed: {Status} - {Error}",
                    response.StatusCode, error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling NotificationService for ticket confirmation email");
            return false;
        }
    }
}

