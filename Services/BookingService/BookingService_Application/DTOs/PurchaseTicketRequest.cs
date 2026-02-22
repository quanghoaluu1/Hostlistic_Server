using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.DTOs
{
    public class PurchaseTicketRequest
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<TicketItemRequest> TicketItems { get; set; } = new();
    }

    public class TicketItemRequest
    {
        public Guid TicketTypeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PurchaseTicketResponse
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public List<TicketDto> Tickets { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ProcessPaymentRequest
    {
        public Guid OrderId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string Gateway { get; set; } = string.Empty;
    }

    public class PurchaseConfirmationRequest
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public List<TicketDto> Tickets { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }

    public class InventoryCheckRequest
    {
        public List<TicketItemRequest> TicketItems { get; set; } = new();
    }

    public class InventoryCheckResponse
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TicketAvailabilityInfo> TicketAvailability { get; set; } = new();
    }

    public class TicketAvailabilityInfo
    {
        public Guid TicketTypeId { get; set; }
        public string TicketTypeName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int RequestedQuantity { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
