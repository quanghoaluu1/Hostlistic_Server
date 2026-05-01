using NotificationService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

/// <summary>
/// Parses an uploaded .xlsx stream and returns a validated recipient list.
/// Defined in Application so the controller can depend on the abstraction only.
/// </summary>
public interface IExcelInviteParser
{
    /// <summary>
    /// Reads <paramref name="fileStream"/>, validates rows, deduplicates by e-mail,
    /// and returns the full parse result including any invalid rows.
    /// </summary>
    ImportInviteResult Parse(Stream fileStream);
}
