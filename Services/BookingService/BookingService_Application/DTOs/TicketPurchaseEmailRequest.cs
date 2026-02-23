using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.DTOs
{
    public class TicketPurchaseEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<TicketEmailInfo> Tickets { get; set; } = new();
    }

    public class TicketEmailInfo
    {
        public string TicketCode { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
