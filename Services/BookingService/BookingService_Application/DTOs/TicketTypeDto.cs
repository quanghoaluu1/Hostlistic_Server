using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.DTOs
{
    public class TicketTypeDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid? SessionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string? Description { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantitySold { get; set; }
        public DateTime SaleStartDate { get; set; }
        public DateTime SaleEndTime { get; set; }
        public int MinPerOrder { get; set; }
        public int MaxPerOrder { get; set; }
        public bool IsRequireHolderInfo { get; set; }
    }
}
