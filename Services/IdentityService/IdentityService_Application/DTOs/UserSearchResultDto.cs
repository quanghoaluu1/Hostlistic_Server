namespace IdentityService_Application.DTOs;

public record UserSearchResultDto(
    Guid Id,
    string FullName,
    string Email,
    string? AvatarUrl
);
