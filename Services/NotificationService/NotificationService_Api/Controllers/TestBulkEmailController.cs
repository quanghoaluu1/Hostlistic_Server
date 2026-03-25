using Common.Messages;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Enums;
using NotificationService_Infrastructure.Data;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/test/bulk-email")]
[Authorize(Roles = "Admin")]
// ⚠️ Development only — remove or restrict before production
public class TestBulkEmailController(
    IPublishEndpoint publishEndpoint,
    NotificationServiceDbContext dbContext,
    ICampaignSendService campaignSendService,
    IEmailRateLimiter rateLimiter
) : ControllerBase
{
    // ──────────────────────────────────────────────────────────
    // STEP 1: Seed fake recipients via BookingConfirmedEvent
    // ──────────────────────────────────────────────────────────
    
    /// <summary>
    /// Publishes a fake BookingConfirmedEvent to RabbitMQ.
    /// The BookingConfirmedConsumer will pick it up and create EventRecipient records.
    ///
    /// Usage: POST /api/test/bulk-email/seed-recipient
    /// Body: { "eventId": "guid", "email": "test@example.com", "fullName": "Test User" }
    ///
    /// Test flow:
    /// 1. Call this endpoint 3-5 times with different emails
    /// 2. Check EventRecipient table → should have 3-5 rows
    /// 3. Check RabbitMQ management → messages consumed on booking-confirmed queue
    /// </summary>
    [HttpPost("seed-recipient")]
    public async Task<IActionResult> SeedRecipient([FromBody] SeedRecipientRequest request)
    {
        var ticketTypeId = request.TicketTypeId ?? Guid.NewGuid();
 
        await publishEndpoint.Publish(new BookingConfirmedEvent(
            EventId: request.EventId,
            UserId: request.UserId ?? Guid.NewGuid(),
            Email: request.Email,
            FullName: request.FullName,
            OrderId: Guid.NewGuid(),
            Tickets:
            [
                new BookingTicketInfo(
                    TicketTypeId: ticketTypeId,
                    TicketTypeName: request.TicketTypeName ?? "General Admission",
                    Quantity: 1
                )
            ],
            ConfirmedAt: DateTime.UtcNow
        ));
 
        return Ok(new
        {
            message = "BookingConfirmedEvent published to RabbitMQ",
            eventId = request.EventId,
            email = request.Email,
            hint = "Check EventRecipient table in ~1-2 seconds"
        });
    }
 
    /// <summary>
    /// Seed multiple recipients at once for bulk testing.
    ///
    /// Usage: POST /api/test/bulk-email/seed-batch?eventId={guid}&count=10
    /// Creates N fake recipients with emails test-1@hostlistic.tech, test-2@... etc.
    /// </summary>
    [HttpPost("seed-batch")]
    public async Task<IActionResult> SeedBatch(
        [FromQuery] Guid eventId,
        [FromQuery] int count = 5)
    {
        var tasks = new List<Task>();
        for (var i = 1; i <= count; i++)
        {
            var idx = i;  // capture for closure
            tasks.Add(publishEndpoint.Publish(new BookingConfirmedEvent(
                EventId: eventId,
                UserId: Guid.NewGuid(),
                Email: $"test-{idx}@hostlistic.tech",
                FullName: $"Test User {idx}",
                OrderId: Guid.NewGuid(),
                Tickets:
                [
                    new BookingTicketInfo(
                        TicketTypeId: Guid.NewGuid(),
                        TicketTypeName: idx % 2 == 0 ? "VIP" : "General",
                        Quantity: 1
                    )
                ],
                ConfirmedAt: DateTime.UtcNow
            )));
        }
 
        await Task.WhenAll(tasks);
 
        return Ok(new
        {
            message = $"Published {count} BookingConfirmedEvents",
            eventId,
            hint = "Check EventRecipient table in ~2-3 seconds"
        });
    }
 
    // ──────────────────────────────────────────────────────────
    // STEP 2: Verify ECST sync worked
    // ──────────────────────────────────────────────────────────
 
    /// <summary>
    /// Check EventRecipient table for a given event.
    /// Confirms the ECST consumer is working correctly.
    /// </summary>
    [HttpGet("recipients/{eventId:guid}")]
    public async Task<IActionResult> GetRecipients(Guid eventId)
    {
        var recipients = await dbContext.EventRecipients
            .Where(r => r.EventId == eventId)
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                r.UserId,
                r.Email,
                r.FullName,
                r.TicketTypeName,
                r.BookingConfirmedAt,
                r.IsCheckedIn,
                r.SyncedAt
            })
            .ToListAsync();
 
        return Ok(new
        {
            eventId,
            count = recipients.Count,
            recipients
        });
    }
 
    // ──────────────────────────────────────────────────────────
    // STEP 3: Quick-create campaign + trigger send (all-in-one)
    // ──────────────────────────────────────────────────────────
 
    /// <summary>
    /// Creates a Draft campaign and immediately triggers send.
    /// Combines CRUD + send into one call for fast testing.
    ///
    /// Usage: POST /api/test/bulk-email/quick-send
    /// Body: { "eventId": "guid", "campaignName": "Test Campaign", "htmlContent": "<p>Hello!</p>" }
    ///
    /// Test flow:
    /// 1. First seed recipients with /seed-batch
    /// 2. Call this endpoint
    /// 3. Poll GET /api/email-campaigns/{id}/status for progress
    /// 4. Check your email inbox (test-N@hostlistic.tech won't receive — use real emails in seed)
    /// </summary>
    [HttpPost("quick-send")]
    public async Task<IActionResult> QuickSend([FromBody] QuickSendRequest request)
    {
        // 1. Create campaign directly in DB
        var campaign = new NotificationService_Domain.Entities.EmailCampaign
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = request.CreatedBy ?? Guid.NewGuid(),
            Name = request.CampaignName ?? "[Test] Bulk Email Campaign",
            Content = request.HtmlContent ?? @"
                <div style='font-family: Arial; padding: 20px;'>
                    <h2>Test Bulk Email</h2>
                    <p>This is a test email from Hostlistic bulk email system.</p>
                    <p>If you received this, the BulkEmailConsumer is working correctly.</p>
                </div>",
            Status = EmailCampaignStatus.Draft,
            RecipientGroup = request.RecipientGroup ?? RecipientGroup.AllTicketHolders,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
 
        await dbContext.EmailCampaigns.AddAsync(campaign);
        await dbContext.SaveChangesAsync();
 
        // 2. Trigger send
        var result = await campaignSendService.TriggerSendAsync(
            campaign.Id,
            campaign.CreatedBy);
 
        return StatusCode(result.StatusCode, new
        {
            campaignId = campaign.Id,
            sendResult = result,
            pollUrl = $"/api/email-campaigns/{campaign.Id}/status",
            hint = "Poll the status URL every 2s to see progress"
        });
    }
 
    // ──────────────────────────────────────────────────────────
    // STEP 4: Check rate limiter state
    // ──────────────────────────────────────────────────────────
 
    /// <summary>
    /// Shows current Redis rate limiter state.
    /// Useful for debugging quota issues.
    /// </summary>
    [HttpGet("rate-limit")]
    public async Task<IActionResult> GetRateLimitStatus()
    {
        var remaining = await rateLimiter.GetRemainingQuotaAsync();
        var used = await rateLimiter.GetCurrentUsageAsync();
 
        return Ok(new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            emailsSentToday = used,
            remainingQuota = remaining,
            dailyLimit = 100
        });
    }
 
    // ──────────────────────────────────────────────────────────
    // STEP 5: Check campaign EmailLogs
    // ──────────────────────────────────────────────────────────
 
    /// <summary>
    /// Shows all EmailLogs for a campaign with their status.
    /// Use after /quick-send to verify per-recipient results.
    /// </summary>
    [HttpGet("campaign-logs/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignLogs(Guid campaignId)
    {
        var campaign = await dbContext.EmailCampaigns
            .Include(c => c.EmailLogs)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId);
 
        if (campaign is null)
            return NotFound("Campaign not found");
 
        return Ok(new
        {
            campaignId,
            campaignStatus = campaign.Status.ToString(),
            totalRecipients = campaign.TotalRecipients,
            sentCount = campaign.SentCount,
            failedCount = campaign.FailedCount,
            logs = campaign.EmailLogs.Select(l => new
            {
                l.Id,
                l.RecipientEmail,
                status = l.Status.ToString(),
                l.ErrorMessage,
                l.SentAt
            }).OrderBy(l => l.SentAt)
        });
    }
}
 
// ── Request DTOs for test endpoints ──
 
public class SeedRecipientRequest
{
    public Guid EventId { get; set; }
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? TicketTypeId { get; set; }
    public string? TicketTypeName { get; set; }
}
 
public class QuickSendRequest
{
    public Guid EventId { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? CampaignName { get; set; }
    public string? HtmlContent { get; set; }
    public RecipientGroup? RecipientGroup { get; set; }
}