using System.ComponentModel.DataAnnotations;

namespace IdentityService_Application.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
