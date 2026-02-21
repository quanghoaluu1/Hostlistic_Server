using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailLogController(IEmailLogService emailLogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await emailLogService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await emailLogService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("campaign/{campaignId:guid}")]
    public async Task<IActionResult> GetByCampaignId(Guid campaignId)
    {
        var result = await emailLogService.GetByCampaignIdAsync(campaignId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailLogRequest request)
    {
        var result = await emailLogService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmailLogRequest request)
    {
        var result = await emailLogService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await emailLogService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
