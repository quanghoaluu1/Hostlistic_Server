using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserNotificationController(IUserNotificationService userNotificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await userNotificationService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await userNotificationService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        var result = await userNotificationService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var userId = GetCurrentUserId();
        var result = await userNotificationService.GetUnreadByUserIdAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await userNotificationService.MarkAsReadAsync(id, userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("notification/{notificationId:guid}")]
    public async Task<IActionResult> GetByNotificationId(Guid notificationId)
    {
        var result = await userNotificationService.GetByNotificationIdAsync(notificationId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserNotificationRequest request)
    {
        var result = await userNotificationService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserNotificationRequest request)
    {
        var result = await userNotificationService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await userNotificationService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
