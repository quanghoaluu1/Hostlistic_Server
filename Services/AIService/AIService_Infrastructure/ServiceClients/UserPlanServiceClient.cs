using System.Net.Http.Json;
using AIService_Application.DTOs;
using AIService_Application.Interface;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIService_Infrastructure.ServiceClients;

public class UserPlanServiceClient(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserPlanServiceClient> logger) : IUserPlanServiceClient
{
    public async Task<IReadOnlyList<UserPlanDto>> GetByUserIdAsync(
        Guid userId,
        bool onlyActive = false,
        CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(client);

            var response = await client.GetAsync(
                $"/api/UserPlans/by-user/{userId}?onlyActive={onlyActive}",
                ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("GetUserPlans failed for user {UserId}: {Status}", userId, response.StatusCode);
                return [];
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<UserPlanDto>>>(ct);
            return (apiResponse?.Data ?? []).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling IdentityService for user plans {UserId}", userId);
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
