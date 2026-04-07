using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using BookingService_Infrastructure.Data;
using Common;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Services;

public class AttendeeService(BookingServiceDbContext dbContext) : IAttendeeService
{
    public async Task<ApiResponse<AttendeeListResponse>> GetAttendeesAsync(
        Guid eventId,
        AttendeeListRequest request,
        CancellationToken ct = default)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);

        var query = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.EventId == eventId && o.Status == OrderStatus.Confirmed);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(o =>
                (o.BuyerName != null && o.BuyerName.ToLower().Contains(search)) ||
                (o.BuyerEmail != null && o.BuyerEmail.ToLower().Contains(search)));
        }

        if (request.TicketTypeId.HasValue)
        {
            query = query.Where(o => o.OrderDetails.Any(od => od.TicketTypeId == request.TicketTypeId.Value));
        }

        if (request.IsCheckedIn.HasValue)
        {
            query = request.IsCheckedIn.Value
                ? query.Where(o => o.Tickets.Any(t => t.IsUsed))
                : query.Where(o => o.Tickets.All(t => !t.IsUsed));
        }

        query = (request.SortBy.ToLower(), request.SortOrder.ToLower()) switch
        {
            ("buyername", "asc") => query.OrderBy(o => o.BuyerName),
            ("buyername", _) => query.OrderByDescending(o => o.BuyerName),
            ("orderdate", "asc") => query.OrderBy(o => o.OrderDate),
            _ => query.OrderByDescending(o => o.OrderDate)
        };

        var totalCount = await query.CountAsync(ct);

        var attendees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AttendeeDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                BuyerName = o.BuyerName ?? string.Empty,
                BuyerEmail = o.BuyerEmail ?? string.Empty,
                BuyerAvatarUrl = o.BuyerAvatarUrl,
                OrderDate = o.OrderDate,
                TotalAmount = o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
                TotalTickets = o.Tickets.Count,
                CheckedInTickets = o.Tickets.Count(t => t.IsUsed),
                Tickets = o.Tickets.Select(t => new AttendeeTicketDto
                {
                    TicketId = t.Id,
                    TicketCode = t.TicketCode,
                    TicketTypeName = t.TicketTypeName,
                    IsUsed = t.IsUsed,
                    HolderName = t.HolderName,
                    HolderEmail = t.HolderEmail,
                    HolderPhone = t.HolderPhone
                }).ToList()
            })
            .ToListAsync(ct);

        var response = new AttendeeListResponse
        {
            Attendees = attendees,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return ApiResponse<AttendeeListResponse>.Success(200, "Attendees retrieved", response);
    }

    public async Task<ApiResponse<AttendeeSummaryDto>> GetAttendeeSummaryAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        var confirmedOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.EventId == eventId && o.Status == OrderStatus.Confirmed)
            .Select(o => new
            {
                OrderDetails = o.OrderDetails.Select(od => new
                {
                    od.TicketTypeName,
                    od.TicketTypeId,
                    od.Quantity,
                    od.UnitPrice
                }).ToList(),
                Tickets = o.Tickets.Select(t => new
                {
                    t.TicketTypeName,
                    t.IsUsed
                }).ToList()
            })
            .ToListAsync(ct);

        var totalOrders = confirmedOrders.Count;
        var totalTicketsSold = confirmedOrders.Sum(o => o.Tickets.Count);
        var totalCheckedIn = confirmedOrders.Sum(o => o.Tickets.Count(t => t.IsUsed));
        var totalRevenue = confirmedOrders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice));

        // Group by TicketTypeName using tickets for counts, orderDetails for revenue
        var ticketsByType = confirmedOrders
            .SelectMany(o => o.Tickets)
            .GroupBy(t => t.TicketTypeName)
            .ToDictionary(
                g => g.Key,
                g => new { TicketCount = g.Count(), CheckedInCount = g.Count(t => t.IsUsed) });

        var revenueByTypeName = confirmedOrders
            .SelectMany(o => o.OrderDetails)
            .GroupBy(od => od.TicketTypeName)
            .ToDictionary(g => g.Key, g => g.Sum(od => od.Quantity * od.UnitPrice));

        var allTypeNames = ticketsByType.Keys.Union(revenueByTypeName.Keys).Distinct();

        var byTicketType = allTypeNames.Select(name => new TicketTypeSummaryDto
        {
            TicketTypeName = name,
            TicketCount = ticketsByType.TryGetValue(name, out var t) ? t.TicketCount : 0,
            CheckedInCount = ticketsByType.TryGetValue(name, out var tc) ? tc.CheckedInCount : 0,
            Revenue = revenueByTypeName.TryGetValue(name, out var r) ? r : 0
        }).ToList();

        var summary = new AttendeeSummaryDto
        {
            TotalOrders = totalOrders,
            TotalTicketsSold = totalTicketsSold,
            TotalCheckedIn = totalCheckedIn,
            TotalRevenue = totalRevenue,
            ByTicketType = byTicketType
        };

        return ApiResponse<AttendeeSummaryDto>.Success(200, "Attendee summary retrieved", summary);
    }
}
