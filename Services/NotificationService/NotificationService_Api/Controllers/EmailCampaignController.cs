using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailCampaignController(IEmailCampaignService emailCampaignService, ICampaignSendService campaignSendService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await emailCampaignService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await emailCampaignService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailCampaignRequest request)
    {
        var result = await emailCampaignService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmailCampaignRequest request)
    {
        var result = await emailCampaignService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await emailCampaignService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
    /// <summary>
    /// Preview how many recipients will receive the campaign email.
    /// Call this before /send to show confirmation dialog in UI.
    /// </summary>
    [HttpGet("{campaignId:guid}/preview")]
    public async Task<IActionResult> Preview(Guid campaignId)
    {
        var result = await campaignSendService.PreviewAsync(campaignId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Trigger campaign send. Returns 202 Accepted immediately.
    /// Actual sending happens asynchronously via RabbitMQ consumer.
    /// Poll GET /status for progress.
    /// </summary>
    [HttpPost("{campaignId:guid}/send")]
    public async Task<IActionResult> Send(Guid campaignId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await campaignSendService.TriggerSendAsync(campaignId, userId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Poll campaign send status. Returns sent/failed/pending counts.
    /// Frontend can poll this every 2-3 seconds to show progress bar.
    /// </summary>
    [HttpGet("{campaignId:guid}/status")]
    public async Task<IActionResult> Status(Guid campaignId)
    {
        var result = await campaignSendService.GetStatusAsync(campaignId);
        return StatusCode(result.StatusCode, result);
    }
}
