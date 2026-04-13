using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface IExportService
{
    Task<ApiResponse<ExportFileResult>> ExportAttendeesAsync(
        Guid eventId, ExportFormat format, CancellationToken ct = default);

    Task<ApiResponse<ExportFileResult>> ExportOrdersAsync(
        Guid eventId, ExportFormat format, CancellationToken ct = default);
}