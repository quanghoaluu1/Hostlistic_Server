using System.Globalization;
using System.Text;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using ClosedXML.Excel;

namespace BookingService_Infrastructure.Services;

public class ExcelGenerator : IExcelGenerator
{
    public byte[] GenerateAttendeeExcel(
        IReadOnlyList<AttendeeExportRow> rows,
        string eventTitle,
        DateTime exportedAt)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Attendees");
        
        // --- Header info ---
        ws.Cell("A1").Value = eventTitle;
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:M1").Merge();

        ws.Cell("A2").Value = $"Exported: {exportedAt:yyyy-MM-dd HH:mm} UTC";
        ws.Cell("A2").Style.Font.Italic = true;
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;
        ws.Range("A2:M2").Merge();

        ws.Cell("A3").Value = $"Total Tickets: {rows.Count}";
        ws.Cell("B3").Value = $"Checked In: {rows.Count(r => r.IsCheckedIn)}";
        ws.Cell("C3").Value = $"No-show: {rows.Count(r => !r.IsCheckedIn)}";

        // --- Column headers (row 5) ---
        var headers = new[]
        {
            "Order ID", "Buyer Name", "Buyer Email", "Order Date",
            "Ticket Code", "Ticket Type", "Ticket Price",
            "Holder Name", "Holder Email", "Holder Phone",
            "Checked In", "Check-in Time", "Order Total"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(5, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // --- Data rows (starting row 6) ---
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var r = i + 6;

            ws.Cell(r, 1).Value = row.OrderId;
            ws.Cell(r, 2).Value = row.BuyerName;
            ws.Cell(r, 3).Value = row.BuyerEmail;
            ws.Cell(r, 4).Value = row.OrderDate;
            ws.Cell(r, 4).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
            ws.Cell(r, 5).Value = row.TicketCode;
            ws.Cell(r, 6).Value = row.TicketTypeName;
            ws.Cell(r, 7).Value = row.TicketPrice;
            ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 8).Value = row.HolderName ?? "";
            ws.Cell(r, 9).Value = row.HolderEmail ?? "";
            ws.Cell(r, 10).Value = row.HolderPhone ?? "";
            ws.Cell(r, 11).Value = row.IsCheckedIn ? "Yes" : "No";
            ws.Cell(r, 11).Style.Font.FontColor =
                row.IsCheckedIn ? XLColor.Green : XLColor.Red;
            if (row.CheckInTime.HasValue)
            {
                ws.Cell(r, 12).Value = row.CheckInTime.Value;
                ws.Cell(r, 12).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
            }
            ws.Cell(r, 13).Value = row.TotalAmount;
            ws.Cell(r, 13).Style.NumberFormat.Format = "#,##0";

            // Alternate row shading
            if (i % 2 == 1)
            {
                ws.Range(r, 1, r, 13).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#F2F7FB");
            }
        }

        // --- Auto-filter and column width ---
        ws.Range(5, 1, 5 + rows.Count, headers.Length).SetAutoFilter();
        ws.Columns().AdjustToContents(5, 5 + rows.Count);

        // Freeze header row
        ws.SheetView.FreezeRows(5);

        return WorkbookToBytes(workbook);
    }
    
    public byte[] GenerateOrderExcel(
        IReadOnlyList<OrderExportRow> orderRows,
        IReadOnlyList<TicketTypeSummaryExportRow> summaryRows,
        string eventTitle,
        DateTime exportedAt)
    {
        using var workbook = new XLWorkbook();

        // ── Sheet 1: Order Details ──
        var ws = workbook.Worksheets.Add("Order Details");

        ws.Cell("A1").Value = $"{eventTitle} — Orders Report";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Range("A1:L1").Merge();

        ws.Cell("A2").Value = $"Exported: {exportedAt:yyyy-MM-dd HH:mm} UTC";
        ws.Cell("A2").Style.Font.Italic = true;
        ws.Cell("A2").Style.Font.FontColor = XLColor.Gray;

        var orderHeaders = new[]
        {
            "Order ID", "Buyer Name", "Buyer Email", "Order Date",
            "Status", "Ticket Type", "Qty", "Unit Price",
            "Line Total", "Payment Method", "Payment Status", "Transaction ID"
        };

        for (var i = 0; i < orderHeaders.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = orderHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        for (var i = 0; i < orderRows.Count; i++)
        {
            var row = orderRows[i];
            var r = i + 5;

            ws.Cell(r, 1).Value = row.OrderId;
            ws.Cell(r, 2).Value = row.BuyerName;
            ws.Cell(r, 3).Value = row.BuyerEmail;
            ws.Cell(r, 4).Value = row.OrderDate;
            ws.Cell(r, 4).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
            ws.Cell(r, 5).Value = row.Status;
            ApplyStatusColor(ws.Cell(r, 5), row.Status);
            ws.Cell(r, 6).Value = row.TicketTypeName;
            ws.Cell(r, 7).Value = row.Quantity;
            ws.Cell(r, 8).Value = row.UnitPrice;
            ws.Cell(r, 8).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 9).Value = row.LineTotal;
            ws.Cell(r, 9).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 10).Value = row.PaymentMethod;
            ws.Cell(r, 11).Value = row.PaymentStatus;
            ws.Cell(r, 12).Value = row.TransactionId ?? "";

            if (i % 2 == 1)
                ws.Range(r, 1, r, 12).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#F2F7FB");
        }

        // Totals row
        var totalRow = 5 + orderRows.Count + 1;
        ws.Cell(totalRow, 8).Value = "Grand Total:";
        ws.Cell(totalRow, 8).Style.Font.Bold = true;
        ws.Cell(totalRow, 9).Value = orderRows.Sum(r => r.LineTotal);
        ws.Cell(totalRow, 9).Style.NumberFormat.Format = "#,##0";
        ws.Cell(totalRow, 9).Style.Font.Bold = true;

        ws.Range(4, 1, 4 + orderRows.Count, orderHeaders.Length).SetAutoFilter();
        ws.Columns().AdjustToContents(4, 4 + orderRows.Count);
        ws.SheetView.FreezeRows(4);

        // ── Sheet 2: Revenue Summary ──
        var ws2 = workbook.Worksheets.Add("Revenue Summary");

        ws2.Cell("A1").Value = $"{eventTitle} — Revenue Summary";
        ws2.Cell("A1").Style.Font.Bold = true;
        ws2.Cell("A1").Style.Font.FontSize = 14;
        ws2.Range("A1:F1").Merge();

        var summaryHeaders = new[]
        {
            "Ticket Type", "Price", "Available", "Sold", "Checked In", "Revenue"
        };

        for (var i = 0; i < summaryHeaders.Length; i++)
        {
            var cell = ws2.Cell(3, i + 1);
            cell.Value = summaryHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#27AE60");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (var i = 0; i < summaryRows.Count; i++)
        {
            var row = summaryRows[i];
            var r = i + 4;

            ws2.Cell(r, 1).Value = row.TicketTypeName;
            ws2.Cell(r, 2).Value = row.Price;
            ws2.Cell(r, 2).Style.NumberFormat.Format = "#,##0";
            ws2.Cell(r, 3).Value = row.QuantityAvailable;
            ws2.Cell(r, 4).Value = row.QuantitySold;
            ws2.Cell(r, 5).Value = row.CheckedInCount;
            ws2.Cell(r, 6).Value = row.Revenue;
            ws2.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        }

        // Summary totals
        var sTotalRow = 4 + summaryRows.Count + 1;
        ws2.Cell(sTotalRow, 1).Value = "TOTAL";
        ws2.Cell(sTotalRow, 1).Style.Font.Bold = true;
        ws2.Cell(sTotalRow, 3).Value = summaryRows.Sum(r => r.QuantityAvailable);
        ws2.Cell(sTotalRow, 3).Style.Font.Bold = true;
        ws2.Cell(sTotalRow, 4).Value = summaryRows.Sum(r => r.QuantitySold);
        ws2.Cell(sTotalRow, 4).Style.Font.Bold = true;
        ws2.Cell(sTotalRow, 5).Value = summaryRows.Sum(r => r.CheckedInCount);
        ws2.Cell(sTotalRow, 5).Style.Font.Bold = true;
        ws2.Cell(sTotalRow, 6).Value = summaryRows.Sum(r => r.Revenue);
        ws2.Cell(sTotalRow, 6).Style.NumberFormat.Format = "#,##0";
        ws2.Cell(sTotalRow, 6).Style.Font.Bold = true;

        ws2.Columns().AdjustToContents();

        return WorkbookToBytes(workbook);
    }
    
    public byte[] GenerateAttendeeCsv(IReadOnlyList<AttendeeExportRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Order ID,Buyer Name,Buyer Email,Order Date,Ticket Code," +
                       "Ticket Type,Ticket Price,Holder Name,Holder Email," +
                       "Holder Phone,Checked In,Check-in Time,Order Total");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                Escape(r.OrderId),
                Escape(r.BuyerName),
                Escape(r.BuyerEmail),
                r.OrderDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Escape(r.TicketCode),
                Escape(r.TicketTypeName),
                r.TicketPrice.ToString(CultureInfo.InvariantCulture),
                Escape(r.HolderName ?? ""),
                Escape(r.HolderEmail ?? ""),
                Escape(r.HolderPhone ?? ""),
                r.IsCheckedIn ? "Yes" : "No",
                r.CheckInTime?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "",
                r.TotalAmount.ToString(CultureInfo.InvariantCulture)));
        }

        // BOM for Excel to detect UTF-8 properly
        var bom = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(sb.ToString());
        return [.. bom, .. content];
    }

    public byte[] GenerateOrderCsv(IReadOnlyList<OrderExportRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Order ID,Buyer Name,Buyer Email,Order Date,Status," +
                       "Ticket Type,Qty,Unit Price,Line Total," +
                       "Payment Method,Payment Status,Transaction ID");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",",
                Escape(r.OrderId),
                Escape(r.BuyerName),
                Escape(r.BuyerEmail),
                r.OrderDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Escape(r.Status),
                Escape(r.TicketTypeName),
                r.Quantity,
                r.UnitPrice.ToString(CultureInfo.InvariantCulture),
                r.LineTotal.ToString(CultureInfo.InvariantCulture),
                Escape(r.PaymentMethod),
                Escape(r.PaymentStatus),
                Escape(r.TransactionId ?? "")));
        }

        var bom = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(sb.ToString());
        return [.. bom, .. content];
    }
    
    private static byte[] WorkbookToBytes(XLWorkbook workbook)
    {
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static void ApplyStatusColor(IXLCell cell, string status)
    {
        cell.Style.Font.FontColor = status switch
        {
            "Confirmed" => XLColor.FromHtml("#27AE60"),
            "Cancelled" => XLColor.FromHtml("#E74C3C"),
            "Refunded"  => XLColor.FromHtml("#F39C12"),
            "Pending"   => XLColor.FromHtml("#3498DB"),
            _           => XLColor.Black
        };
    }
}