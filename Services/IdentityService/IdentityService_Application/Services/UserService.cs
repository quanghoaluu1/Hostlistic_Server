using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IdentityService_Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository userRepository, IPhotoService photoService, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _photoService = photoService;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileDto>.Fail(404, "User not found");

        var userProfile = user.Adapt<UserProfileDto>();
        return ApiResponse<UserProfileDto>.Success(200, "User profile retrieved successfully", userProfile);
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileDto>.Fail(404, "User not found");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return ApiResponse<UserProfileDto>.Fail(400, "Full name is required");

        // Update user properties
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        if (!string.IsNullOrEmpty(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        var userProfile = user.Adapt<UserProfileDto>();
        return ApiResponse<UserProfileDto>.Success(200, "User profile updated successfully", userProfile);
    }

    public async Task<ApiResponse<UserProfileDto>> UpdateUserProfileWithAvatarAsync(Guid userId, UpdateUserProfileRequest request, IFormFile? avatarFile)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileDto>.Fail(404, "User not found");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return ApiResponse<UserProfileDto>.Fail(400, "Full name is required");

        // Upload new avatar if provided
        if (avatarFile is not null && avatarFile.Length > 0)
        {
            var uploadResult = await _photoService.UploadPhotoAsync(avatarFile);
            if (uploadResult.Error != null)
                return ApiResponse<UserProfileDto>.Fail(400, $"Avatar upload failed: {uploadResult.Error.Message}");

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var publicId = ExtractPublicIdFromUrl(user.AvatarUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }
            }

            user.AvatarUrl = uploadResult.SecureUrl.AbsoluteUri;
        }
        else if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            user.AvatarUrl = request.AvatarUrl;
        }

        // Update other properties
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        var userProfile = user.Adapt<UserProfileDto>();
        return ApiResponse<UserProfileDto>.Success(200, "User profile updated successfully", userProfile);
    }

    public async Task<ApiResponse<List<UserSearchResultDto>>> SearchByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length < 3)
            return ApiResponse<List<UserSearchResultDto>>.Fail(400, "Email query must be at least 3 characters.");

        var users = await _userRepository.SearchByEmailAsync(email.Trim().ToLower());

        var results = users.Select(u => new UserSearchResultDto(
            u.Id, u.FullName, u.Email, u.AvatarUrl
        )).ToList();

        return ApiResponse<List<UserSearchResultDto>>.Success(200, "Success", results);
    }

    private static string ExtractPublicIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');
            var fileName = pathSegments[^1]; // Get last segment
            return Path.GetFileNameWithoutExtension(fileName);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<ApiResponse<PagedResult<UserProfileDto>>> GetUserList(BaseQueryParams request)
    {
        var userList = await _userRepository.GetUsersAsync(request);
        var userProfiles = userList.Items.Select(u => u.Adapt<UserProfileDto>()).ToList();
        var result = new PagedResult<UserProfileDto>
        (
            userProfiles,
            userList.TotalItems,
            userList.CurrentPage,
            userList.PageSize
        );
        return ApiResponse<PagedResult<UserProfileDto>>.Success(200, "User list retrieved successfully", result);
    }

    public async Task<ApiResponse<UserDashboardDto>> GetUserDashboardAsync()
    {
        var today = DateTime.UtcNow;

        var startOfWeek = GetStartOfWeek(today);
        var startOf7WeeksAgo = startOfWeek.AddDays(-7 * 6);
        // ===== USER DATA =====
        var (totalUsers, userData) =
            await _userRepository.GetUserDashboardRawAsync(startOf7WeeksAgo);

        // convert week về DateTime
        var first = userData.FirstOrDefault();
        var type = first?.GetType();

        var yearProp = type?.GetProperty("Year");
        var weekProp = type?.GetProperty("Week");
        var countProp = type?.GetProperty("Count");

        var lookup = userData.ToDictionary(
            x => (
                Year: (int)yearProp!.GetValue(x)!,
                Week: (int)weekProp!.GetValue(x)!
            ),
            x => (int)countProp!.GetValue(x)!
        );

        // ===== BUILD USER TREND =====
        var userTrend = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var weekStart = startOf7WeeksAgo.AddDays(i * 7);

                lookup.TryGetValue(
                    (weekStart.Year, weekStart.DayOfYear / 7),
                    out var count
                );

                return new UserTrendDto
                {
                    Week = weekStart,
                    Users = count
                };
            })
            .ToList();

        // ===== EVENT DATA (CALL SERVICE) =====
        var eventDashboard = await GetEventTrendAsync();

        var eventTrend = eventDashboard.Data.ByDate
            .GroupBy(x => x.Date.Month)
            .Select(g => new EventTrendDto
            {
                Month = g.Key,
                Events = g.Sum(x => x.Count)
            })
            .OrderBy(x => x.Month)
            .ToList();
        var result = new UserDashboardDto
        {
            TotalUsers = totalUsers,
            UserTrend = userTrend,
            EventTrend = eventTrend
        };
        return ApiResponse<UserDashboardDto>.Success(200, "User dashboard data retrieved successfully", result);
    }

    private async Task<ApiResponse<EventDashboardDto>> GetEventTrendAsync()
    {
        var token = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"]
            .ToString();

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
        }

        var response = await _httpClient
            .GetFromJsonAsync<ApiResponse<EventDashboardDto>>("https://localhost:7075/api/Event/dashboard");
        //var raw = await _httpClient.GetStringAsync("https://localhost:7075/api/Event/dashboard");
        //Console.WriteLine(raw);

        return response!;
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
