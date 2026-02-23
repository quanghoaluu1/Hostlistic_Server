using BookingService_Application.DTOs;
using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.Interfaces
{
    public interface IInventoryService
    {
        Task<ApiResponse<InventoryCheckResponse>> CheckAvailabilityAsync(List<TicketItemRequest> items);
        Task<Guid> ReserveInventoryAsync(List<TicketItemRequest> items);
        Task ConfirmReservationAsync(Guid reservationId);
        Task ReleaseReservationAsync(Guid reservationId);
    }
}
