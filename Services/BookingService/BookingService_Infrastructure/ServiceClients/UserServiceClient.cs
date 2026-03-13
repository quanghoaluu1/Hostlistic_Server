using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Application.Services;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class UserServiceClient : IUserServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(IHttpClientFactory httpClientFactory, ILogger<UserServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("UserService");
            var url = $"/api/Users/{userId}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UserService GetUserInfo failed: {Status} - {Error}", response.StatusCode, error);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserInfoDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService for user {UserId}", userId);
            return null;
        }
    }
}

