using BookingService_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Domain.Interfaces
{
    public interface IInventoryReservationRepository
    {
        Task<InventoryReservation> CreateReservationAsync(InventoryReservation reservation);
        Task<List<InventoryReservation>> GetReservationsByIdAsync(Guid reservationId);
        Task DeleteReservationsAsync(Guid reservationId);
        Task<bool> SaveChangesAsync();
    }
}
