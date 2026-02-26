using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPlansController(IUserPlanService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserPlanDto dto)
    {
        var result = await service.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] bool onlyActive = false)
    {
        var result = await service.GetByUserIdAsync(userId, onlyActive);
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserPlanDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await service.CancelAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
