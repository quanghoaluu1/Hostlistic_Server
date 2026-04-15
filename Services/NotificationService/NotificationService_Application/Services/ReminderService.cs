using Common;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos;
using NotificationService_Application.Dtos.ServiceClientDtos;
using NotificationService_Application.Interfaces;
using NotificationService_Application.Jobs;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class ReminderService(IEmailCampaignRepository campaignRepository,
    IEventServiceClient eventServiceClient,
    IBackgroundJobClient backgroundJobClient,
    ILogger<ReminderService> logger) : IReminderService
{
    private static readonly Dictionary<string, (int DaysOffset, int SendHour)> ReminderSchedule =
        new()
        {
            ["reminder_7day"]    = (7, 9),
            ["reminder_3day"]    = (3, 9),
            ["reminder_1day"]    = (1, 9),
            ["reminder_sameday"] = (0, 8),
        };

    public async Task<ApiResponse<SetupRemindersResult>> SetupAutoRemindersAsync(
        Guid eventId,
        Guid organizerId,
        SetupAutoRemindersRequest request,
        CancellationToken ct = default)
    {
        var eventDetail = await eventServiceClient.GetEventAsync(eventId, ct);
        if (eventDetail is null)
            return ApiResponse<SetupRemindersResult>.Fail(404, "Event not found.");
        
        if (eventDetail.StartDate is null)
            return ApiResponse<SetupRemindersResult>.Fail(400,
                "Event has no start date. Configure the event schedule before setting up reminders.");
        if (request.OverwriteExisting)
            await CancelPendingRemindersInternalAsync(eventId, ct);

        var created = new List<ReminderCampaignInfo>();
        var skippedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var reminderContent in request.Reminders)
        {
            if (!ReminderSchedule.TryGetValue(reminderContent.EmailType, out var schedule))
            {
                logger.LogWarning("Unknown EmailType {EmailType} — skipping", reminderContent.EmailType);
                skippedCount++;
                continue;
            }

            var scheduledUtc =
                ComputeScheduledUtc(eventDetail.StartDate.Value, eventDetail.TimeZoneId, schedule.DaysOffset, schedule.SendHour);
            // Skip — scheduled time already passed (allow 5 min buffer)
            if (scheduledUtc <= now.AddMinutes(5))
            {
                logger.LogInformation(
                    "Skipping {EmailType} for event {EventId}: scheduled {ScheduledUtc} is in the past",
                    reminderContent.EmailType, eventId, scheduledUtc);
                skippedCount++;
                continue;
            }
            
            var campaign = new EmailCampaign
            {
                EventId        = eventId,
                CreatedBy      = organizerId,
                Name           = reminderContent.Subject,
                Content        = reminderContent.HtmlBody,
                ScheduledDate  = scheduledUtc,
                Status         = EmailCampaignStatus.Draft,
                RecipientGroup = RecipientGroup.AllTicketHolders,
                IsAutoReminder = true
            };
            
            await campaignRepository.AddAsync(campaign);
            await campaignRepository.SaveChangesAsync();

            var jobId = backgroundJobClient.Schedule<SendReminderCampaignJob>(
                queue: "reminders",
                methodCall: job => job.ExecuteAsync(campaign.Id, CancellationToken.None),
                enqueueAt: scheduledUtc
            );
            campaign.HangfireJobId = jobId;
            await campaignRepository.SaveChangesAsync();
            
            created.Add(new ReminderCampaignInfo
            {
                CampaignId     = campaign.Id,
                EmailType      = reminderContent.EmailType,
                ScheduledAtUtc = scheduledUtc,
                HangfireJobId  = jobId
            });
            logger.LogInformation(
                "Scheduled {EmailType} for event {EventId} at {ScheduledUtc} (JobId={JobId})",
                reminderContent.EmailType, eventId, scheduledUtc, jobId);
        }
        var result = new SetupRemindersResult
        {
            CreatedReminders = created,
            SkippedCount     = skippedCount
        };
            
        return ApiResponse<SetupRemindersResult>.Success(
            201, $"Created {created.Count} reminder(s), skipped {skippedCount}.", result);
    }
    
    public async Task<ApiResponse<bool>> CancelAutoRemindersAsync(
        Guid eventId, CancellationToken cancellationToken = default)
    {
        var count = await CancelPendingRemindersInternalAsync(eventId, cancellationToken);

        return ApiResponse<bool>.Success(
            200, $"Cancelled {count} pending reminder(s).", true);
    }
    
    private async Task<int> CancelPendingRemindersInternalAsync(
        Guid eventId, CancellationToken ct)
    {
        var campaignQueryable = campaignRepository.GetQueryable();
        var pending = await campaignQueryable
            .Where(c => c.EventId == eventId
                        && c.IsAutoReminder
                        && c.Status == EmailCampaignStatus.Draft)
            .ToListAsync(ct);

        foreach (var campaign in pending)
        {
            if (!string.IsNullOrEmpty(campaign.HangfireJobId))
                backgroundJobClient.Delete(campaign.HangfireJobId);

            campaign.Status = EmailCampaignStatus.Cancelled;
        }

        if (pending.Count > 0)
            await campaignRepository.SaveChangesAsync();

        return pending.Count;
    }

    private static DateTime ComputeScheduledUtc(
        DateTime eventStartUtc, string? timeZoneId, int daysOffset, int sendHourLocal)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? "UTC");
        }
        catch
        {
            // Fallback to UTC nếu timezone ID không hợp lệ (Windows vs IANA mismatch)
            tz = TimeZoneInfo.Utc;
        }

        var localStart = TimeZoneInfo.ConvertTimeFromUtc(eventStartUtc, tz);

        // Ví dụ: event ngày 15, reminder_3day → gửi lúc 9:00 ngày 12
        var scheduledLocal = localStart.Date
            .AddDays(-daysOffset)
            .AddHours(sendHourLocal);

        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(scheduledLocal, DateTimeKind.Unspecified), tz);
    }
}