using Common;
using NotificationService_Application.Dtos;

namespace NotificationService_Application.Interfaces;

public interface IReminderService
{
    Task<ApiResponse<SetupRemindersResult>> SetupAutoRemindersAsync(
        Guid eventId,
        Guid organizerId,
        SetupAutoRemindersRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> CancelAutoRemindersAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}