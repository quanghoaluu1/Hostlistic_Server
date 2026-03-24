using System.Net.Http.Json;
using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventService_Infrastructure.ServiceClients;

public class UserPlanServiceClient(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserPlanServiceClient> logger) : IUserPlanServiceClient
{
    public async Task<IEnumerable<UserPlanDto>> GetByUserIdAsync(Guid userId, bool onlyActive = false)
    {
        try
        {
            var client = httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);

            var response = await client.GetAsync($"/api/UserPlans/by-user/{userId}?onlyActive={onlyActive}");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("GetByUserIdAsync(UserPlans) failed: {Status}", response.StatusCode);
                return [];
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserPlanDto>>>();
            return apiResponse?.Data ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user plans for user {UserId}", userId);
            return [];
        }
    }

    private void ForwardAuthorizationHeader(HttpClient client)
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
    }
}
