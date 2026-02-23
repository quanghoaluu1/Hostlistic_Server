using System.ComponentModel.DataAnnotations;

namespace IdentityService_Application.DTOs;

public class GoogleLoginRequest
{
    [Required(ErrorMessage = "Google ID Token is required")]
    public required string IdToken { get; init; }
}