using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class UserPlanServiceClient : IUserPlanServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserPlanServiceClient> _logger;

    public UserPlanServiceClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserPlanServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<UserPlanDto?> GetByIdAsync(Guid userPlanId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);
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
            ForwardAuthorizationHeader(client);
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

    public async Task<SubscriptionPlanDto?> GetSubscriptionPlanByIdAsync(Guid subscriptionPlanId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);
            var response = await client.GetAsync($"/api/SubscriptionPlans/{subscriptionPlanId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetSubscriptionPlanById failed: {Status}", response.StatusCode);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SubscriptionPlanDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IdentityService for SubscriptionPlan {SubscriptionPlanId}", subscriptionPlanId);
            return null;
        }
    }

    public async Task<UserPlanDto?> CreateUserPlanAsync(CreateUserPlanRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);
            var response = await client.PostAsJsonAsync("/api/UserPlans", request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateUserPlan failed: {Status}", response.StatusCode);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserPlanDto>>();
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating UserPlan for user {UserId}", request.UserId);
            return null;
        }
    }

    public async Task<bool> CancelUserPlanAsync(Guid userPlanId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);
            var response = await client.PostAsync($"/api/UserPlans/{userPlanId}/cancel", null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CancelUserPlan failed for {UserPlanId}: {Status}", userPlanId, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling UserPlan {UserPlanId}", userPlanId);
            return false;
        }
    }

    private void ForwardAuthorizationHeader(HttpClient client)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
    }
}
