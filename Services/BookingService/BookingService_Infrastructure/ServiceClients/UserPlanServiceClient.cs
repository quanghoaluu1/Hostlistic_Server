using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class UserPlanServiceClient : IUserPlanServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserPlanServiceClient> _logger;

    public UserPlanServiceClient(IHttpClientFactory httpClientFactory, ILogger<UserPlanServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<UserPlanDto?> GetByIdAsync(Guid userPlanId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            var response = await client.GetAsync($"/api/UserPlans/{userPlanId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUserPlanById failed: {Status}", response.StatusCode);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserPlanDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IdentityService for UserPlan {UserPlanId}", userPlanId);
            return null;
        }
    }

    public async Task<IEnumerable<UserPlanDto>> GetByUserIdAsync(Guid userId, bool onlyActive = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            var response = await client.GetAsync($"/api/UserPlans/by-user/{userId}?onlyActive={onlyActive}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUserPlansByUserId failed: {Status}", response.StatusCode);
                return [];
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserPlanDto>>>();
            return apiResponse?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IdentityService for UserPlans of user {UserId}", userId);
            return [];
        }
    }
}
