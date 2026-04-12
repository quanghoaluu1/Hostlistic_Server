using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class ExportService(
    IOrderRepository orderRepository,
    ICheckinRepository checkinRepository,
    ITicketRepository ticketRepository,
    IExcelGenerator excelGenerator,
    IEventServiceClient eventServiceClient,
    ILogger<ExportService> logger
    ) : IExportService
{
    public async Task<ApiResponse<ExportFileResult>> ExportAttendeesAsync(
        Guid eventId, ExportFormat format, CancellationToken ct = default)
    {
        logger.LogInformation("Exporting attendees for event {EventId}, format {Format}",
            eventId, format);
        var orders = await orderRepository.GetConfirmedOrdersByEventIdAsync(eventId);
        if (!orders.Any())
        {
            return ApiResponse<ExportFileResult>.Fail(404,
                "No confirmed orders found for this event.");
        }
        var allTicketIds = orders.SelectMany(o => o.Tickets.Select(t => t.Id)).ToHashSet();
        var checkinQueryable = checkinRepository.GetCheckinQueryable();
        var checkinLookup = await checkinQueryable.AsNoTracking()
            .Where(c => allTicketIds.Contains(c.TicketId) && c.CheckInType == CheckInType.EventLevel)
            .Select(c => new { c.TicketId, c.CheckInTime })
            .ToDictionaryAsync(c => c.TicketId, c => c.CheckInTime, ct);
        var rows = orders
            .SelectMany(order => order.Tickets.Select(ticket =>
            {
                var isCheckedIn = checkinLookup.TryGetValue(ticket.Id, out var checkInTime);
                
                return new AttendeeExportRow(
                    OrderId: order.Id.ToString()[..8],
                    BuyerName: order.BuyerName ?? "N/A",
                    BuyerEmail: order.BuyerEmail ?? "N/A",
                    OrderDate: order.OrderDate,
                    TotalAmount: order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
                    TicketCode: ticket.TicketCode,
                    TicketTypeName: ticket.TicketTypeName ?? "N/A",
                    TicketPrice: order.OrderDetails
                        .FirstOrDefault(od => od.TicketTypeId == ticket.TicketTypeId)?
                        .UnitPrice ?? 0,
                    HolderName: ticket.HolderName,
                    HolderEmail: ticket.HolderEmail,
                    HolderPhone: ticket.HolderPhone,
                    IsCheckedIn: isCheckedIn,
                    CheckInTime: isCheckedIn ? checkInTime : null);
            })).ToList();
        
        var eventTitle =  eventServiceClient.GetEventInfoAsync(eventId).Result.Title;
        var sanitizedTitle = SanitizeFileName(eventTitle);
        var now = DateTime.UtcNow;
        
        return format switch
        {
            ExportFormat.Csv => ApiResponse<ExportFileResult>.Success(200, "Generate CSV Attendee success", new ExportFileResult(
                FileContent: excelGenerator.GenerateAttendeeCsv(rows),
                ContentType: "text/csv",
                FileName: $"{sanitizedTitle}_Attendees_{now:yyyyMMdd}.csv")),

            _ => ApiResponse<ExportFileResult>.Success(200, "Generate Excel Attendee success", new ExportFileResult(
                FileContent: excelGenerator.GenerateAttendeeExcel(rows, eventTitle, now),
                ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName: $"{sanitizedTitle}_Attendees_{now:yyyyMMdd}.xlsx"))
        };
    }

    public async Task<ApiResponse<ExportFileResult>> ExportOrdersAsync(
        Guid eventId, ExportFormat format, CancellationToken ct = default)
    {
        logger.LogInformation("Exporting orders for event {EventId}, format {Format}",
            eventId, format);

        var orders = await orderRepository.GetOrdersByEventIdAsync(eventId);
        if (!orders.Any())
        {
            return ApiResponse<ExportFileResult>.Fail(404,
                "No orders found for this event.");
        }
        
        var orderRows = orders
            .SelectMany(order => order.OrderDetails.Select(detail =>
            {
                var payment = order.Payments.FirstOrDefault();

                return new OrderExportRow(
                    OrderId: order.Id.ToString()[..8],
                    BuyerName: order.BuyerName ?? "N/A",
                    BuyerEmail: order.BuyerEmail ?? "N/A",
                    OrderDate: order.OrderDate,
                    Status: order.Status.ToString(),
                    TicketTypeName: detail.TicketTypeName ?? "N/A",
                    Quantity: detail.Quantity,
                    UnitPrice: detail.UnitPrice,
                    LineTotal: detail.Quantity * detail.UnitPrice,
                    PaymentMethod: payment?.PaymentMethod?.Name ?? "N/A",
                    PaymentStatus: payment?.Status.ToString() ?? "N/A",
                    TransactionId: payment?.TransactionId);
            }))
            .ToList();

        var ticketTypes = await eventServiceClient.GetTicketTypesByEventIdAsync(eventId);
        var confirmedOrderIds = orders.Where(o => o.Status == OrderStatus.Confirmed).Select(o => o.Id).ToHashSet();
        var eventTicketIds = await ticketRepository.GetTicketIdsByEventIdAsync(eventId);
        
        var checkinCounts = await checkinRepository.GetCheckinQueryable().AsNoTracking()
            .Where(c => eventTicketIds.Contains(c.TicketId)
            && c.CheckInType == CheckInType.EventLevel)
            .GroupBy(c => c.Ticket!.TicketTypeId)
            .Select(g => new { TicketTypeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TicketTypeId, x => x.Count, ct);

        var summaryRows = ticketTypes.Select(tt => new TicketTypeSummaryExportRow(
            TicketTypeName: tt.Name,
            Price: tt.Price,
            QuantityAvailable: tt.QuantityAvailable,
            QuantitySold: tt.QuantitySold,
            CheckedInCount: checkinCounts.GetValueOrDefault(tt.Id),
            Revenue: tt.Price * tt.QuantitySold)).ToList();
        var orderQueryable = orderRepository.GetOrderQueryable();
        var eventInfo = await eventServiceClient.GetEventInfoAsync(eventId);
        var eventTitle = eventInfo?.Title ?? "Event";

        var sanitizedTitle = SanitizeFileName(eventTitle);
        var now = DateTime.UtcNow;
        
        return format switch
        {
            ExportFormat.Csv => ApiResponse<ExportFileResult>.Success(400, "Generate CSV Order success", new ExportFileResult(
                FileContent: excelGenerator.GenerateOrderCsv(orderRows),
                ContentType: "text/csv",
                FileName: $"{sanitizedTitle}_Orders_{now:yyyyMMdd}.csv")),

            _ => ApiResponse<ExportFileResult>.Success(400, "Generate Excel Order success",new ExportFileResult(
                FileContent: excelGenerator.GenerateOrderExcel(
                    orderRows, summaryRows, eventTitle, now),
                ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName: $"{sanitizedTitle}_Orders_{now:yyyyMMdd}.xlsx"))
        };

    }



    private static string SanitizeFileName(string name) =>
        string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
            .Replace(' ', '_')
            .Truncate(50);
}

internal static class StringExtensions
{
    public static string Truncate(this string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}