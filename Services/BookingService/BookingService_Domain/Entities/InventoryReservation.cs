using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Domain.Entities
{
    public class InventoryReservation
    {
        public Guid Id { get; set; }
        public Guid ReservationId { get; set; }
        public Guid TicketTypeId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }
}
