using Common;
using IdentityService_Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace IdentityService_Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(Guid userId);
    Task<ApiResponse<UserProfileDto>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task<ApiResponse<UserProfileDto>> UpdateUserProfileWithAvatarAsync(Guid userId, UpdateUserProfileRequest request, IFormFile? avatarFile);
}
