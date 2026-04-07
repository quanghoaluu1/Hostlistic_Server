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
    public List<EventTrendDto> EventTrend { get; set; } = new();
}
public class UserTrendDto
{
    public DateTime Week { get; set; }
    public int Users { get; set; }
}

public class EventTrendDto
{
    public int Month { get; set; }
    public int Events { get; set; }
}

public class EventDashboardDto
{
    public int Total { get; set; }
    public List<EventByStatusDto> ByStatus { get; set; } = new();
    public List<EventByDateDto> ByDate { get; set; } = new();
}

public class EventByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EventByDateDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}