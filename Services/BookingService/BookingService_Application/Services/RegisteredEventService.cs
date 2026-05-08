using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Interfaces;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class RegisteredEventService(IOrderRepository orderRepository,
    IEventServiceClient eventServiceClient,
    ILogger<RegisteredEventService> logger) : IRegisteredEventService
{
    public async Task<ApiResponse<List<RegisteredEventDto>>> GetMyRegisteredEventsAsync(Guid userId)
    {
        // 1. Lấy tất cả confirmed orders của user — 1 round trip DB
        var orders = await orderRepository.GetConfirmedOrdersByUserIdAsync(userId);

        if (!orders.Any())
            return ApiResponse<List<RegisteredEventDto>>.Success(200, "No registered events", []);

        // 2. Lấy distinct EventIds để tránh gọi EventService trùng lặp
        //    Một user có thể có nhiều orders cho cùng 1 event (edge case)
        //    nhưng ta chỉ cần 1 record per event trên UI
        var distinctEventIds = orders
            .Select(o => o.EventId)
            .Distinct()
            .ToList();

        // 3. Gọi EventService song song cho tất cả events — Task.WhenAll
        var eventInfoTasks = distinctEventIds
            .Select(async eventId =>
            {
                var info = await eventServiceClient.GetEventInfoAsync(eventId);
                return (eventId, info);
            });

        var eventInfoResults = await Task.WhenAll(eventInfoTasks);

        // Build lookup: EventId → EventInfoDto (bỏ qua các event không tìm thấy)
        var eventInfoMap = eventInfoResults
            .Where(r => r.info is not null)
            .ToDictionary(r => r.eventId, r => r.info!);

        // 4. Group orders by EventId, lấy order mới nhất nếu có nhiều
        var registeredEvents = orders
            .GroupBy(o => o.EventId)
            .Select(group =>
            {
                var latestOrder = group.OrderByDescending(o => o.OrderDate).First();

                if (!eventInfoMap.TryGetValue(latestOrder.EventId, out var eventInfo))
                {
                    // EventService không trả data — log và bỏ qua
                    logger.LogWarning(
                        "EventService returned null for EventId {EventId}. Skipping.",
                        latestOrder.EventId);
                    return null;
                }

                return new RegisteredEventDto
                {
                    EventId = latestOrder.EventId,
                    Title = eventInfo.Title,
                    CoverImageUrl = eventInfo.CoverImageUrl,
                    StartDate = eventInfo.StartDate,
                    EndDate = eventInfo.EndDate,
                    Location = eventInfo.Location,
                    EventMode = eventInfo.EventMode,
                    EventStatus = eventInfo.EventStatus,
                    OrderId = latestOrder.Id,
                    OrderDate = latestOrder.OrderDate,
                    TotalTickets = latestOrder.Tickets.Count,
                    Tickets = latestOrder.Tickets.Select(t => new RegisteredEventTicketDto
                    {
                        TicketId = t.Id,
                        TicketCode = t.TicketCode,
                        TicketTypeName = t.TicketTypeName,
                        QrCodeUrl = t.QrCodeUrl,
                        IsUsed = t.IsUsed,
                        HolderName = t.HolderName
                    }).ToList()
                };
            })
            .Where(dto => dto is not null)
            .Cast<RegisteredEventDto>()
            // Sort: Ongoing → Upcoming → Completed
            .OrderBy(e => e.EventStatus switch
            {
                "OnGoing" => 0,
                "Published" => 1,
                _ => 2
            })
            .ThenBy(e => e.StartDate)
            .ToList();

        return ApiResponse<List<RegisteredEventDto>>.Success(
            200, "Registered events retrieved", registeredEvents);
    }
}