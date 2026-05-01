using System.Text.RegularExpressions;
using ClosedXML.Excel;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;

namespace NotificationService_Infrastructure.Services;

/// <summary>
/// Reads an .xlsx workbook and produces a validated, deduplicated recipient list.
/// Row 1 is treated as a header and skipped.
/// Column 1 = Name, Column 2 = Email.
/// </summary>
public sealed partial class ExcelInviteParser : IExcelInviteParser
{
    // Pre-compiled regex — RFC 5322 "good-enough" pattern used across the codebase.
    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    public ImportInviteResult Parse(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var sheet = workbook.Worksheets.First();

        var rawRows = sheet.RowsUsed()
                          .Skip(1) // skip header row
                          .ToList();

        int totalRows = rawRows.Count;
        var invalidEmails = new List<string>();
        var validRows     = new List<ExcelInviteRow>();

        foreach (var row in rawRows)
        {
            var name  = row.Cell(1).GetString().Trim();
            var email = row.Cell(2).GetString().Trim();

            // Skip entirely blank rows
            if (string.IsNullOrWhiteSpace(email))
            {
                invalidEmails.Add($"(row {row.RowNumber()}) — empty email");
                continue;
            }

            // Validate e-mail format
            if (!EmailRegex().IsMatch(email))
            {
                invalidEmails.Add(email);
                continue;
            }

            // Fall back to email when Name column is blank
            if (string.IsNullOrWhiteSpace(name))
                name = email;

            validRows.Add(new ExcelInviteRow(name, email));
        }

        // Deduplicate by e-mail (case-insensitive), keep first occurrence
        var deduplicated = validRows
            .GroupBy(r => r.Email, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        int skippedRows = totalRows - deduplicated.Count - invalidEmails.Count
                          + (validRows.Count - deduplicated.Count); // dupes

        // Simpler & accurate: skipped = total - dedup valid
        skippedRows = totalRows - deduplicated.Count;

        return new ImportInviteResult(
            TotalRows:     totalRows,
            ValidRows:     deduplicated.Count,
            SkippedRows:   skippedRows,
            InvalidEmails: invalidEmails,
            Recipients:    deduplicated);
    }
}
