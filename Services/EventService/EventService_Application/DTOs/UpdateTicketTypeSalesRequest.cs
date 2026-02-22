using System;
using System.Collections.Generic;
using System.Text;

namespace EventService_Application.DTOs
{
    public class UpdateTicketTypeSalesRequest
    {
        public int QuantitySold { get; set; }
    }

    public class BulkTicketPurchaseRequest
    {
        public List<TicketPurchaseItem> Items { get; set; } = new();
    }

    public class TicketPurchaseItem
    {
        public Guid TicketTypeId { get; set; }
        public int Quantity { get; set; }
    }
}
