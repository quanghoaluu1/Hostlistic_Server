using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPhotoService _photoService;

    public UserController(IUserService userService, IPhotoService photoService)
    {
        _userService = userService;
        _photoService = photoService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetUserProfileAsync(userId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileRequest request, IFormFile? avatarFile)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, request, avatarFile);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        var userId = GetCurrentUserId();
        var userProfile = await _userService.GetUserProfileAsync(userId);
        if (userProfile.Data == null) return NotFound(userProfile);
        
        var result = await _photoService.UploadPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error);
        
        var imageUrl = result.SecureUrl.AbsoluteUri;
        
        var updateRequest = new UpdateUserProfileRequest
        {
            FullName = userProfile.Data.FullName,
            PhoneNumber = userProfile.Data.PhoneNumber,
            AvatarUrl = imageUrl
        };
        
        var updateResult = await _userService.UpdateUserProfileWithAvatarAsync(userId, updateRequest, null);
        return Ok(updateResult);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
