using System.Security.Claims;
using Common;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailCampaignController(
    IEmailCampaignService emailCampaignService,
    ICampaignSendService campaignSendService,
    IExcelInviteParser excelInviteParser,
    IEmailCampaignRepository emailCampaignRepository,
    IEmailLogRepository emailLogRepository) : ControllerBase
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

    /// <summary>
    /// Import recipients from an Excel (.xlsx) file.
    /// Parses, validates, and bulk-inserts EmailLog rows with Pending status.
    /// Updates the campaign's RecipientGroup to ManualList and TotalRecipients.
    /// </summary>
    [HttpPost("{campaignId:guid}/import-recipients")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportRecipients(Guid campaignId, IFormFile? file)
    {
        // 1. Validate file presence and extension
        if (file is null || file.Length == 0)
        {
            var err = ApiResponse<ImportInviteResult>.Fail(400, "No file uploaded.");
            return BadRequest(err);
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var err = ApiResponse<ImportInviteResult>.Fail(400, "Only .xlsx files are supported.");
            return BadRequest(err);
        }

        // 2. Parse the workbook
        await using var stream = file.OpenReadStream();
        var parseResult = excelInviteParser.Parse(stream);

        // 3. Reject when every row was invalid
        if (parseResult.ValidRows == 0)
        {
            var err = ApiResponse<ImportInviteResult>.Fail(
                422,
                "No valid recipients found in the uploaded file. " +
                "Ensure Column A = Name and Column B = a valid e-mail address.");
            return UnprocessableEntity(err);
        }

        // 4. Fetch the campaign
        var campaign = await emailCampaignRepository.GetByIdAsync(campaignId);
        if (campaign is null)
        {
            var err = ApiResponse<ImportInviteResult>.Fail(404, "Campaign not found.");
            return NotFound(err);
        }

        // 5. Update campaign metadata
        campaign.RecipientGroup  = RecipientGroup.ManualList;
        campaign.TotalRecipients = parseResult.ValidRows;
        await emailCampaignRepository.UpdateAsync(campaign);

        // 6. Map valid recipients to EmailLog entities
        var now  = DateTime.UtcNow;
        var logs = parseResult.Recipients.Select(r => new EmailLog
        {
            Id             = Guid.NewGuid(),
            CampaignId     = campaignId,
            RecipientEmail = r.Email,
            SentTo         = Guid.Empty,   // external user — no internal UserId
            SentAt         = now,
            Status         = DeliveryStatus.Pending,
        }).ToList();

        // 7. Bulk-insert and persist
        await emailLogRepository.AddRangeAsync(logs);
        await emailLogRepository.SaveChangesAsync();
        await emailCampaignRepository.SaveChangesAsync();

        // 8. Return parse summary
        var ok = ApiResponse<ImportInviteResult>.Success(
            200,
            $"Successfully imported {parseResult.ValidRows} recipients " +
            $"({parseResult.SkippedRows} skipped).",
            parseResult);

        return Ok(ok);
    }
}
