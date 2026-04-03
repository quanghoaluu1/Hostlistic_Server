using BookingService_Application.DTOs;
using Common;
using System;
using System.Collections.Generic;
using System.Text;
using BookingService_Application.DTOs.PayOs;

namespace BookingService_Application.Interfaces
{
    public interface ITicketPurchaseService
    {
        Task<ApiResponse<PurchaseTicketResponse>> PurchaseTicketsAsync(PurchaseTicketRequest request);
        Task<ApiResponse<InventoryCheckResponse>> CheckTicketAvailabilityAsync(InventoryCheckRequest request);
        Task<ApiResponse<PayOsCheckoutResponse>> InitiatePayOsPurchaseAsync(PurchaseTicketRequest request);

        Task<List<TicketDto>> GenerateTicketsWithQrCodesAsync(
            Guid orderId,
            List<TicketItemRequest> ticketItems,
            Guid eventId,
            string eventName = "",
            string? buyerName = null,
            string? buyerEmail = null);

        Task<ApiResponse<FreeTicketPurchaseResponse>> PurchaseFreeTicketsAsync(FreeTicketPurchaseRequest request);
    }
}
