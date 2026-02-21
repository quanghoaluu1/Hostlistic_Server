using System.ComponentModel.DataAnnotations;

namespace IdentityService_Application.DTOs;

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Otp { get; set; }

    [Required]
    [MinLength(6)]
    public required string NewPassword { get; set; }
}
