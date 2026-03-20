using BookingService_Application.DTOs;
using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.Interfaces
{
    public interface ITicketPurchaseService
    {
        Task<ApiResponse<PurchaseTicketResponse>> PurchaseTicketsAsync(PurchaseTicketRequest request);
        Task<ApiResponse<InventoryCheckResponse>> CheckTicketAvailabilityAsync(InventoryCheckRequest request);

        Task<List<TicketDto>> GenerateTicketsWithQrCodesAsync(
            Guid orderId,
            List<TicketItemRequest> ticketItems);
    }
}
