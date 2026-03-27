using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace IdentityService_Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPhotoService _photoService;

    public UserService(IUserRepository userRepository, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _photoService = photoService;
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
}
