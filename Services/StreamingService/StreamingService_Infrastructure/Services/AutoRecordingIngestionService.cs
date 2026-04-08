using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure.Services;

public sealed class AutoRecordingIngestionService : BackgroundService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".m4v",
        ".mov",
        ".mkv",
        ".webm"
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecordingAutomationSettings _automationSettings;
    private readonly RecordingStorageSettings _storageSettings;
    private readonly ILogger<AutoRecordingIngestionService> _logger;

    public AutoRecordingIngestionService(
        IServiceScopeFactory scopeFactory,
        IOptions<RecordingAutomationSettings> automationOptions,
        IOptions<RecordingStorageSettings> storageOptions,
        ILogger<AutoRecordingIngestionService> logger)
    {
        _scopeFactory = scopeFactory;
        _automationSettings = automationOptions.Value;
        _storageSettings = storageOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_automationSettings.Enabled)
        {
            _logger.LogInformation("Auto recording ingestion is disabled.");
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Max(5, _automationSettings.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanInboxAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto recording ingestion scan failed.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ScanInboxAsync(CancellationToken cancellationToken)
    {
        var inboxRoot = ResolvePath(_automationSettings.InboxPath);
        Directory.CreateDirectory(inboxRoot);
        Directory.CreateDirectory(ResolvePath(_automationSettings.ProcessedPath));
        Directory.CreateDirectory(ResolvePath(_automationSettings.FailedPath));

        var files = Directory
            .EnumerateFiles(inboxRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => SupportedExtensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var filePath in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists || !IsStable(fileInfo))
                continue;

            if (!TryResolveRoomId(fileInfo.Directory, inboxRoot, out var roomId))
            {
                _logger.LogWarning("Skipping recording file without room folder: {FilePath}", fileInfo.FullName);
                continue;
            }

            var metadata = await TryReadMetadataAsync(fileInfo.FullName, cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IStreamingServiceDbContext>();
            var storageService = scope.ServiceProvider.GetRequiredService<IRecordingStorageService>();

            var room = await dbContext.StreamRooms
                .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);

            if (room == null)
            {
                _logger.LogWarning("Recording file {FilePath} references unknown room {RoomId}. Moving to failed.", fileInfo.FullName, roomId);
                MoveFile(fileInfo.FullName, ResolvePath(_automationSettings.FailedPath), inboxRoot);
                MoveMetadataFile(fileInfo.FullName, ResolvePath(_automationSettings.FailedPath), inboxRoot);
                continue;
            }

            if (!room.IsRecordEnabled)
            {
                _logger.LogInformation("Skipping recording import because room {RoomId} has recording disabled. Moving source to processed.", roomId);
                MoveFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);
                MoveMetadataFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);
                continue;
            }

            if (room.Status != StreamRoomStatus.Ended)
                continue;

            var duplicateExists = await dbContext.StreamRecordings
                .AsNoTracking()
                .AnyAsync(r => r.StreamRoomId == roomId
                    && r.Status == RecordingStatus.Ready
                    && r.FileSizeBytes == fileInfo.Length, cancellationToken);

            if (duplicateExists)
            {
                _logger.LogInformation("Recording already imported for room {RoomId} and file {FilePath}. Moving source to processed.", roomId, fileInfo.FullName);
                MoveFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);
                MoveMetadataFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);
                continue;
            }

            await using var input = fileInfo.OpenRead();
            var stored = await storageService.SaveAsync(input, fileInfo.Name, room.EventId, roomId, cancellationToken);

            var recording = await dbContext.StreamRecordings
                .Where(r => r.StreamRoomId == roomId && r.Status == RecordingStatus.Processing)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (recording == null)
            {
                // Reuse existing ready record for the same file instead of creating duplicates on retries.
                recording = await dbContext.StreamRecordings
                    .Where(r =>
                        r.StreamRoomId == roomId &&
                        r.Status == RecordingStatus.Ready &&
                        r.FileSizeBytes == stored.FileSizeBytes &&
                        r.FileName == stored.FileName)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (recording == null)
            {
                recording = new StreamRecording
                {
                    Id = Guid.NewGuid(),
                    StreamRoomId = roomId,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.StreamRecordings.Add(recording);
            }

            recording.FileName = stored.FileName;
            recording.StorageUrl = stored.PlaybackUrl;
            recording.FileSizeBytes = stored.FileSizeBytes;
            recording.Duration = TimeSpan.FromSeconds(Math.Max(0, metadata.DurationSeconds));
            recording.Status = RecordingStatus.Ready;
            recording.EgressId = string.IsNullOrWhiteSpace(metadata.EgressId) ? recording.EgressId : metadata.EgressId;
            recording.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            MoveFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);
            MoveMetadataFile(fileInfo.FullName, ResolvePath(_automationSettings.ProcessedPath), inboxRoot);

            _logger.LogInformation("Imported recording for room {RoomId} from {FilePath}.", roomId, fileInfo.FullName);
        }
    }

    private bool IsStable(FileInfo fileInfo)
    {
        var stableFor = TimeSpan.FromSeconds(Math.Max(5, _automationSettings.FileStableSeconds));
        return DateTime.UtcNow - fileInfo.LastWriteTimeUtc >= stableFor;
    }

    private string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        var rootPath = Path.IsPathRooted(_storageSettings.RootPath)
            ? _storageSettings.RootPath
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _storageSettings.RootPath));

        return Path.GetFullPath(Path.Combine(rootPath, configuredPath));
    }

    private static bool TryResolveRoomId(DirectoryInfo? directory, string inboxRoot, out Guid roomId)
    {
        roomId = Guid.Empty;
        var current = directory;
        var normalizedRoot = Path.GetFullPath(inboxRoot).TrimEnd(Path.DirectorySeparatorChar);

        while (current != null)
        {
            if (Guid.TryParse(current.Name, out roomId))
                return true;

            var currentPath = current.FullName.TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(currentPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
                break;

            current = current.Parent;
        }

        return false;
    }

    private async Task<RecordingMetadata> TryReadMetadataAsync(string filePath, CancellationToken cancellationToken)
    {
        var metadataPath = Path.ChangeExtension(filePath, ".json");
        if (!File.Exists(metadataPath))
            return new RecordingMetadata();

        try
        {
            await using var stream = File.OpenRead(metadataPath);
            var metadata = await JsonSerializer.DeserializeAsync<RecordingMetadata>(stream, cancellationToken: cancellationToken);
            return metadata ?? new RecordingMetadata();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read recording metadata sidecar for {FilePath}.", filePath);
            return new RecordingMetadata();
        }
    }

    private static void MoveFile(string sourceFilePath, string targetRoot, string inboxRoot)
    {
        if (!File.Exists(sourceFilePath))
            return;

        var relativePath = Path.GetRelativePath(inboxRoot, sourceFilePath);
        var destinationPath = Path.Combine(targetRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        if (File.Exists(destinationPath))
        {
            File.Delete(sourceFilePath);
            return;
        }

        File.Move(sourceFilePath, destinationPath);
    }

    private static void MoveMetadataFile(string sourceFilePath, string targetRoot, string inboxRoot)
    {
        var metadataPath = Path.ChangeExtension(sourceFilePath, ".json");
        if (!File.Exists(metadataPath))
            return;

        MoveFile(metadataPath, targetRoot, inboxRoot);
    }

    private sealed class RecordingMetadata
    {
        public double DurationSeconds { get; init; }
        public string? EgressId { get; init; }
    }
}
