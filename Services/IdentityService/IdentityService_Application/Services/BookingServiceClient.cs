using System.Text;
using System.Text.Json;
using Common;
using IdentityService_Application.Interfaces;

namespace IdentityService_Application.Services;

public class BookingServiceClient(IHttpClientFactory httpClientFactory) : IBookingServiceClient
{
    public async Task<ApiResponse<object>> GetUserOrdersAsync(Guid userId, string? authHeader)
    {
        var client = httpClientFactory.CreateClient("BookingService");
        if (!string.IsNullOrEmpty(authHeader))
            client.DefaultRequestHeaders.Add("Authorization", authHeader);

        var response = await client.GetAsync($"api/orders/user/{userId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content);
            return ApiResponse<object>.Success(200, "User orders retrieved successfully", data);
        }

        return ApiResponse<object>.Fail(400, "Failed to retrieve user orders");
    }

    public async Task<ApiResponse<object>> CreateWalletAsync(Guid userId)
    {
        var client = httpClientFactory.CreateClient("BookingService");
        var body = JsonSerializer.Serialize(new { userId, currency = "VND" });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/wallets", content);
        if (response.IsSuccessStatusCode)
            return ApiResponse<object>.Success(201, "Wallet created successfully", null);

        return ApiResponse<object>.Fail(400, "Failed to create wallet for user");
    }
}
