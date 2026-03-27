namespace StreamingService_Application.Interfaces;

public record LiveKitOperationResult(bool IsSuccess, string? ErrorMessage = null);

public interface ILiveKitService
{
    Task<LiveKitOperationResult> CreateRoomAsync(string roomName, int maxParticipants, CancellationToken cancellationToken = default);
    Task<LiveKitOperationResult> EndRoomAsync(string roomName, CancellationToken cancellationToken = default);
}
