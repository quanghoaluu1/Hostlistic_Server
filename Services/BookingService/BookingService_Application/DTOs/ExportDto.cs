namespace BookingService_Application.DTOs;

public sealed record AttendeeExportRow(
    string OrderId,
    string BuyerName,
    string BuyerEmail,
    DateTime OrderDate,
    decimal TotalAmount,
    string TicketCode,
    string TicketTypeName,
    decimal TicketPrice,
    string? HolderName,
    string? HolderEmail,
    string? HolderPhone,
    bool IsCheckedIn,
    DateTime? CheckInTime);

public sealed record OrderExportRow(
    string OrderId,
    string BuyerName,
    string BuyerEmail,
    DateTime OrderDate,
    string Status,
    string TicketTypeName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string PaymentMethod,
    string PaymentStatus,
    string? TransactionId);

public sealed record TicketTypeSummaryExportRow(
    string TicketTypeName,
    double Price,
    int QuantityAvailable,
    int QuantitySold,
    int CheckedInCount,
    double Revenue);

public sealed record ExportFileResult(
    byte[] FileContent,
    string ContentType,
    string FileName);

public enum ExportFormat
{
    Xlsx,
    Csv
}