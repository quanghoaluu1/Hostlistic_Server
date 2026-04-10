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
public class UserDashboardDto
{
    public int TotalUsers { get; set; }
    public List<UserTrendDto> UserTrend { get; set; } = new();
}
public class UserTrendDto
{
    public DateTime Week { get; set; }
    public int Users { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
}
