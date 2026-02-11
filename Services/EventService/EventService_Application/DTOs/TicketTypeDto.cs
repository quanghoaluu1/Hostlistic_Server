using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

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
    public SaleEndUnit SaleEndUnit { get; set; }
    public SaleEndWhen SaleEndWhen { get; set; }
    public int MinPerOrder { get; set; }
    public int MaxPerOrder { get; set; }
    public bool IsRequireHolderInfo { get; set; }
    public TicketTypeStatus Status { get; set; }
    public SaleChannel SaleChannel { get; set; }
}

public class CreateTicketTypeRequest
{
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string? Description { get; set; }
    public int QuantityAvailable { get; set; }
    public DateTime SaleStartDate { get; set; }
    public DateTime SaleEndTime { get; set; }
    public int MinPerOrder { get; set; }
    public int MaxPerOrder { get; set; }
    public bool IsRequireHolderInfo { get; set; }
    public SaleChannel SaleChannel { get; set; }
}

public class UpdateTicketTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string? Description { get; set; }
    public int QuantityAvailable { get; set; }
    public DateTime SaleStartDate { get; set; }
    public DateTime SaleEndTime { get; set; }
    public int MinPerOrder { get; set; }
    public int MaxPerOrder { get; set; }
    public bool IsRequireHolderInfo { get; set; }
    public TicketTypeStatus Status { get; set; }
    public SaleChannel SaleChannel { get; set; }
}
