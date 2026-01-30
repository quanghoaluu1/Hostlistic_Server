using Common.DTOs;

namespace IdentityService_Application.DTOs;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public UserDto User { get; set; } = null!;
}