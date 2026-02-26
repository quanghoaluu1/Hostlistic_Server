using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizerBankInfosController(IOrganizerBankInfoService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizerBankInfoDto dto)
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
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await service.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpGet("by-organization/{organizationId:guid}")]
    public async Task<IActionResult> GetByOrganization(Guid organizationId)
    {
        var result = await service.GetByOrganizationIdAsync(organizationId);
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizerBankInfoDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
