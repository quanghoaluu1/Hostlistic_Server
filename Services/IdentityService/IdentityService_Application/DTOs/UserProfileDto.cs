using Common.DTOs;

namespace IdentityService_Application.DTOs;

public class UpdateUserProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UserProfileDto : UserDto
{
    public string? PhoneNumber { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}