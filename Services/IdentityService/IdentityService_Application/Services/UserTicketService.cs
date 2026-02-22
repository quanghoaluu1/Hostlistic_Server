using Common;
using IdentityService_Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace IdentityService_Application.Services
{
    public class UserTicketService : IUserTicketService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserTicketService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<ApiResponse<object>> GetUserOrdersAsync(Guid userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var bookingServiceUrl = _configuration["Services:BookingService"] ?? "http://localhost:5077";

                var response = await client.GetAsync($"{bookingServiceUrl}/api/orders/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<object>(content);
                    return ApiResponse<object>.Success(200, "User orders retrieved successfully", data);
                }

                return ApiResponse<object>.Fail(400, "Failed to retrieve user orders");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail(500, $"Error retrieving user orders: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> GetUserTicketsAsync(Guid userId)
        {
            var ordersResult = await GetUserOrdersAsync(userId);
            if (!ordersResult.IsSuccess)
            {
                return ordersResult;
            }

            // Extract tickets from orders
            // You could process the orders data here to extract just the tickets
            return ApiResponse<object>.Success(200, "User tickets retrieved successfully", ordersResult.Data);
        }

        public async Task<ApiResponse<object>> GetUserTicketsWithEventDetailsAsync(Guid userId)
        {
            try
            {
                var ordersResult = await GetUserOrdersAsync(userId);
                if (!ordersResult.IsSuccess)
                {
                    return ordersResult;
                }

                // Here you could make additional calls to the Event Service to get event details
                var client = _httpClientFactory.CreateClient();
                var eventServiceUrl = _configuration["Services:EventService"] ?? "http://localhost:5139";

                // Process orders and enrich with event data
                // This is a simplified example - you'd need to parse the orders JSON and make calls for each event

                return ApiResponse<object>.Success(200, "User tickets with event details retrieved successfully", ordersResult.Data);
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail(500, $"Error retrieving tickets with event details: {ex.Message}");
            }
        }
    }
}
