using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IExcelGenerator
{
    byte[] GenerateAttendeeExcel(
        IReadOnlyList<AttendeeExportRow> rows,
        string eventTitle,
        DateTime exportedAt);

    byte[] GenerateOrderExcel(
        IReadOnlyList<OrderExportRow> orderRows,
        IReadOnlyList<TicketTypeSummaryExportRow> summaryRows,
        string eventTitle,
        DateTime exportedAt);

    byte[] GenerateAttendeeCsv(IReadOnlyList<AttendeeExportRow> rows);

    byte[] GenerateOrderCsv(IReadOnlyList<OrderExportRow> rows);
}