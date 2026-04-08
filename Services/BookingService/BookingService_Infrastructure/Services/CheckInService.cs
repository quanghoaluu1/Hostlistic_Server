using System.Data;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Infrastructure.Data;
using Common;
using Common.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService_Infrastructure.Services;

public class CheckInService(
    BookingServiceDbContext dbContext,
    IQrCodeService qrCodeService,
    IEventServiceClient eventServiceClient,
    IUserServiceClient userServiceClient,
    IPublishEndpoint publishEndpoint,
    ILogger<CheckInService> logger
) : ICheckInService
{
    public async Task<ApiResponse<List<CheckInDto>>> GetEventCheckInsAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        var checkIns = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => new CheckInDto(
                c.Id,
                c.TicketId,
                c.SessionId,
                c.TicketCode,
                c.AttendeeName,
                c.AttendeeEmail,
                c.TicketTypeName,
                c.SessionName,
                c.EventTitle,
                c.CheckInTime,
                c.CheckInType,
                c.CheckedInByUserId))
            .ToListAsync(ct);

        return ApiResponse<List<CheckInDto>>.Success(200, "Check-ins retrieved", checkIns);
    }

    public async Task<ApiResponse<CheckInStatsResponse>> GetEventCheckInStatsAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        // TotalCheckedIn: distinct tickets with an event-level check-in
        var totalCheckedIn = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId && c.CheckInType == CheckInType.EventLevel)
            .Select(c => c.TicketId)
            .Distinct()
            .CountAsync(ct);

        // TotalTicketsSold: tickets on confirmed orders for this event
        var totalTicketsSold = await dbContext.Tickets
            .AsNoTracking()
            .Where(t => t.Order.EventId == eventId && t.Order.Status == OrderStatus.Confirmed)
            .CountAsync(ct);

        // TotalSessionCheckIns: count of session-level check-ins
        var totalSessionCheckIns = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId && c.CheckInType == CheckInType.SessionLevel)
            .CountAsync(ct);

        // ByTicketType: group event-level check-ins by TicketTypeName
        // Then look up total sold per TicketTypeId (from OrderDetails) and match via CheckIn → Ticket
        var checkedInByType = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId && c.CheckInType == CheckInType.EventLevel)
            .GroupBy(c => c.TicketTypeName)
            .Select(g => new { TicketTypeName = g.Key, CheckedIn = g.Count() })
            .ToListAsync(ct);

        // For TotalSold per type: sum OrderDetail.Quantity grouped by TicketTypeId on confirmed orders
        // Then match to TicketTypeName via CheckIn.TicketTypeName (using a join with Ticket.TicketTypeId)
        var soldByTypeId = await (
            from od in dbContext.OrderDetails.AsNoTracking()
            join o in dbContext.Orders.AsNoTracking() on od.OrderId equals o.Id
            where o.EventId == eventId && o.Status == OrderStatus.Confirmed
            group od by od.TicketTypeId into g
            select new { TicketTypeId = g.Key, TotalSold = g.Sum(x => x.Quantity) }
        ).ToDictionaryAsync(x => x.TicketTypeId, x => x.TotalSold, ct);

        // Map TicketTypeName → TicketTypeId using denormalized CheckIn data joined to Tickets
        var typeNameToId = (await dbContext.CheckIns
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .Join(dbContext.Tickets.AsNoTracking(),
                    c => c.TicketId,
                    t => t.Id,
                    (c, t) => new { c.TicketTypeName, t.TicketTypeId })
                .Distinct()
                .ToListAsync(ct))
            .GroupBy(x => x.TicketTypeName)
            .ToDictionary(g => g.Key, g => g.First().TicketTypeId);

        var byTicketType = checkedInByType.Select(x =>
        {
            var totalSold = typeNameToId.TryGetValue(x.TicketTypeName, out var typeId)
                            && soldByTypeId.TryGetValue(typeId, out var s) ? s : 0;
            return new TicketTypeBreakdown(x.TicketTypeName, x.CheckedIn, totalSold);
        }).ToList();

        // BySessions: group session-level check-ins by SessionId + SessionName
        var bySessions = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId && c.CheckInType == CheckInType.SessionLevel && c.SessionId.HasValue)
            .GroupBy(c => new { c.SessionId, c.SessionName })
            .Select(g => new SessionBreakdown(
                g.Key.SessionId!.Value,
                g.Key.SessionName ?? string.Empty,
                g.Count()))
            .ToListAsync(ct);

        var stats = new CheckInStatsResponse(
            TotalCheckedIn: totalCheckedIn,
            TotalTicketsSold: totalTicketsSold,
            TotalSessionCheckIns: totalSessionCheckIns,
            ByTicketType: byTicketType,
            BySessions: bySessions);

        return ApiResponse<CheckInStatsResponse>.Success(200, "Stats retrieved", stats);
    }

    public async Task<ApiResponse<TicketCheckInStatusResponse>> GetTicketCheckInStatusAsync(
        Guid eventId,
        Guid ticketId,
        CancellationToken ct = default)
    {
        var checkIns = await dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.EventId == eventId && c.TicketId == ticketId)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => new CheckInDto(
                c.Id,
                c.TicketId,
                c.SessionId,
                c.TicketCode,
                c.AttendeeName,
                c.AttendeeEmail,
                c.TicketTypeName,
                c.SessionName,
                c.EventTitle,
                c.CheckInTime,
                c.CheckInType,
                c.CheckedInByUserId))
            .ToListAsync(ct);

        var eventCheckIn = checkIns.FirstOrDefault(c => c.CheckInType == CheckInType.EventLevel);
        var sessionCheckIns = checkIns.Where(c => c.CheckInType == CheckInType.SessionLevel).ToList();

        var response = new TicketCheckInStatusResponse(
            IsCheckedIn: eventCheckIn is not null,
            EventCheckInTime: eventCheckIn?.CheckInTime,
            SessionCheckIns: sessionCheckIns);

        return ApiResponse<TicketCheckInStatusResponse>.Success(200, "Ticket check-in status retrieved", response);
    }

    public async Task<ApiResponse<CheckInScanResponse>> ScanAsync(
        CheckInScanRequest request,
        Guid staffUserId,
        CancellationToken ct = default)
    {
        // ── Step 1: Verify HMAC signature (local, O(1)) ──────────────────────
        if (!qrCodeService.VerifyQrPayload(request.QrPayload, out var ticketId, out var qrEventId))
            return ApiResponse<CheckInScanResponse>.Fail(400, "Invalid QR code");

        if (qrEventId != request.EventId)
            return ApiResponse<CheckInScanResponse>.Fail(400, "Ticket does not belong to this event");

        // ── Step 2: Validate session (local DB, no lock needed) ──────────────
        SessionSnapshot? sessionSnapshot = null;
        if (request.SessionId.HasValue)
        {
            sessionSnapshot = await dbContext.SessionSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && s.EventId == request.EventId, ct);

            if (sessionSnapshot is null)
                return ApiResponse<CheckInScanResponse>.Fail(404, "Session not found for this event");
        }

        // ── Step 3: Pre-fetch denormalized info outside the transaction ───────
        // Avoids holding the PG row lock while waiting for HTTP or slow queries.
        var ticketInfo = await dbContext.Tickets
            .AsNoTracking()
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

        if (ticketInfo is null)
            return ApiResponse<CheckInScanResponse>.Fail(404, "Ticket not found");

        // Fetch attendee info, ticket type name, and event title concurrently
        var userTask = userServiceClient.GetUserInfoAsync(ticketInfo.Order.UserId);
        var ticketTypeTask = eventServiceClient.GetTicketTypeInfoAsync(ticketInfo.TicketTypeId);
        var eventTask = eventServiceClient.GetEventInfoAsync(request.EventId);

        await Task.WhenAll(userTask, ticketTypeTask, eventTask);

        var attendeeName = (await userTask)?.FullName ?? string.Empty;
        var attendeeEmail = (await userTask)?.Email ?? string.Empty;
        var ticketTypeName = (await ticketTypeTask)?.Name ?? string.Empty;
        var eventTitle = (await eventTask)?.Title ?? string.Empty;

        // ── Step 4: Begin transaction + pessimistic lock ──────────────────────
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);

        try
        {
            // SELECT ... FOR UPDATE — acquires a row-level lock on the Ticket row.
            // Any concurrent scan for the same ticket will block here until this transaction commits.
            var ticket = await dbContext.Tickets
                .FromSqlRaw("""SELECT * FROM "Tickets" WHERE "Id" = {0} FOR UPDATE""", ticketId)
                .FirstOrDefaultAsync(ct);

            if (ticket is null)
            {
                await transaction.RollbackAsync(ct);
                return ApiResponse<CheckInScanResponse>.Fail(404, "Ticket not found");
            }

            var checkInType = request.SessionId.HasValue ? CheckInType.SessionLevel : CheckInType.EventLevel;

            // ── Step 5: Duplicate check-in guard ─────────────────────────────
            if (checkInType == CheckInType.EventLevel)
            {
                if (ticket.IsUsed)
                {
                    await transaction.RollbackAsync(ct);
                    return ApiResponse<CheckInScanResponse>.Fail(409, "Ticket has already been checked in");
                }

                var alreadyCheckedIn = await dbContext.CheckIns.AnyAsync(
                    c => c.TicketId == ticketId && c.CheckInType == CheckInType.EventLevel, ct);
                if (alreadyCheckedIn)
                {
                    await transaction.RollbackAsync(ct);
                    return ApiResponse<CheckInScanResponse>.Fail(409, "Ticket has already been checked in");
                }
            }
            else
            {
                var alreadyCheckedIn = await dbContext.CheckIns.AnyAsync(
                    c => c.TicketId == ticketId && c.SessionId == request.SessionId!.Value, ct);
                if (alreadyCheckedIn)
                {
                    await transaction.RollbackAsync(ct);
                    return ApiResponse<CheckInScanResponse>.Fail(409,
                        "Ticket has already been checked in to this session");
                }
            }

            // ── Step 6: Mark ticket + create CheckIn record (single commit) ──
            var checkInTime = DateTime.UtcNow;

            if (checkInType == CheckInType.EventLevel)
                ticket.IsUsed = true;  // EF change tracking handles the UPDATE — no dbContext.Update() call

            var checkIn = new CheckIn
            {
                // Id = Guid.CreateVersion7() is set by the entity's property initializer
                EventId = request.EventId,
                TicketId = ticketId,
                SessionId = request.SessionId,
                CheckedInByUserId = staffUserId,
                CheckInTime = checkInTime,
                CheckInType = checkInType,
                AttendeeName = attendeeName,
                AttendeeEmail = attendeeEmail,
                TicketCode = ticket.TicketCode,
                TicketTypeName = ticketTypeName,
                SessionName = sessionSnapshot?.Title,
                EventTitle = eventTitle
            };

            dbContext.CheckIns.Add(checkIn);
            await dbContext.SaveChangesAsync(ct);  // commits both IsUsed and CheckIn in one transaction
            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Check-in {CheckInId} recorded: ticket {TicketId}, event {EventId}, type {Type}",
                checkIn.Id, ticketId, request.EventId, checkInType);

            // ── Step 7: Publish event (fire-and-forget, after commit) ─────────
            try
            {
                await publishEndpoint.Publish(new CheckInCompletedEvent(
                    EventId: request.EventId,
                    CheckInId: checkIn.Id,
                    TicketId: ticketId,
                    SessionId: request.SessionId,
                    AttendeeName: attendeeName,
                    TicketCode: ticket.TicketCode,
                    TicketTypeName: ticketTypeName,
                    SessionName: sessionSnapshot?.Title,
                    CheckInTime: checkInTime,
                    CheckInType: (int)checkInType), ct);
            }
            catch (Exception ex)
            {
                // Publish failure must NOT affect the response — the DB commit already succeeded
                logger.LogWarning(ex,
                    "Failed to publish CheckInCompletedEvent for check-in {CheckInId}", checkIn.Id);
            }

            return ApiResponse<CheckInScanResponse>.Success(200, "Check-in successful", new CheckInScanResponse(
                CheckInId: checkIn.Id,
                TicketCode: ticket.TicketCode,
                AttendeeName: attendeeName,
                TicketTypeName: ticketTypeName,
                SessionName: sessionSnapshot?.Title,
                CheckInTime: checkInTime,
                IsSessionCheckIn: checkInType == CheckInType.SessionLevel));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            logger.LogError(ex, "Unexpected error during check-in scan for ticket {TicketId}", ticketId);
            return ApiResponse<CheckInScanResponse>.Fail(500, "An unexpected error occurred during check-in");
        }
    }
}
