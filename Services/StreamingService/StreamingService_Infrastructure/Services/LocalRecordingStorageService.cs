using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure.Services;

public sealed class LocalRecordingStorageService : IRecordingStorageService
{
    private readonly RecordingStorageSettings _settings;

    public LocalRecordingStorageService(IOptions<RecordingStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<StoredRecordingFile> SaveAsync(
        Stream input,
        string originalFileName,
        Guid eventId,
        Guid roomId,
        CancellationToken cancellationToken
    )
    {
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mp4";
        }

        var safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var relativeDirectory = Path.Combine(eventId.ToString("N"), roomId.ToString("N"));
        var rootPath = ResolveRootPath();
        var absoluteDirectory = Path.Combine(rootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var absoluteFilePath = Path.Combine(absoluteDirectory, safeFileName);

        await using (var output = File.Create(absoluteFilePath))
        {
            await input.CopyToAsync(output, cancellationToken);
        }

        var fileInfo = new FileInfo(absoluteFilePath);
        var relativePath = Path.Combine(relativeDirectory, safeFileName).Replace("\\", "/");
        var requestPath = NormalizeRequestPath(_settings.RequestPath);
        var playbackUrl = !string.IsNullOrWhiteSpace(_settings.PublicBaseUrl)
            ? $"{_settings.PublicBaseUrl!.TrimEnd('/')}{requestPath}/{relativePath}"
            : $"{requestPath}/{relativePath}";

        return new StoredRecordingFile(
            safeFileName,
            relativePath,
            playbackUrl,
            fileInfo.Length
        );
    }

    private string ResolveRootPath()
    {
        var configured = _settings.RootPath;
        if (Path.IsPathRooted(configured))
        {
            return configured;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
    }

    private static string NormalizeRequestPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/recordings";
        }

        return value.StartsWith('/') ? value.TrimEnd('/') : $"/{value.TrimEnd('/')}";
    }
}
