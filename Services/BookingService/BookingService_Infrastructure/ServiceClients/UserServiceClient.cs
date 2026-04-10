using System.Net.Http.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Application.Services;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.ServiceClients;

public class UserServiceClient : IUserServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserServiceClient(IHttpClientFactory httpClientFactory, ILogger<UserServiceClient> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("IdentityService");
            // IdentityService controller is `UserController` (singular)
            var url = $"/api/User/{userId}";

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

    public async Task<OrganizerBankInfoDto?> GetOrganizerBankInfoAsync(Guid userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("IdentityService");
            ForwardAuthorizationHeader(httpClient);
            var url = $"/api/organizerbankinfos/by-user/{userId}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UserService GetOrganizerBankInfo failed: {Status} - {Error}", response.StatusCode,
                    error);
                return null;
            }
            var body = await response.Content.ReadAsStringAsync();

// Log raw response để xem JSON thực tế trả về gì
            _logger.LogInformation("UserService BankInfo raw response: {Body}", body); 
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrganizerBankInfoDto>>>();
            return apiResponse?.Data.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService for user {UserId}", userId);
            return null;
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


