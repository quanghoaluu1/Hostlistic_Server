namespace StreamingService_Application.Interfaces;

public interface IRecordingStorageService
{
    Task<StoredRecordingFile> SaveAsync(
        Stream input,
        string originalFileName,
        Guid eventId,
        Guid roomId,
        CancellationToken cancellationToken
    );
}

public sealed record StoredRecordingFile(
    string FileName,
    string RelativePath,
    string PlaybackUrl,
    long FileSizeBytes
);
