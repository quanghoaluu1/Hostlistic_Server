namespace NotificationService_Application.DTOs;

/// <summary>
/// Represents one parsed, valid row from the uploaded Excel file.
/// </summary>
public record ExcelInviteRow(string Name, string Email);

/// <summary>
/// Summary returned to the caller after parsing the Excel workbook.
/// </summary>
public record ImportInviteResult(
    int TotalRows,
    int ValidRows,
    int SkippedRows,
    List<string> InvalidEmails,
    List<ExcelInviteRow> Recipients);
