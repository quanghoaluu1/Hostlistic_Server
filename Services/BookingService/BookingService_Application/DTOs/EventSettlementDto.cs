using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public record EventSettlementDto(
    Guid Id,
    Guid EventId,
    Guid OrganizerId,
    decimal GrossVenue,
    decimal PlatformFeePercent,
    decimal PlatformFeeAmount,
    decimal NetRevenue,
    int TotalTicketsSold,
    int TotalOrders,
    SettlementStatus Status,
    DateTime? SettledAt
    );