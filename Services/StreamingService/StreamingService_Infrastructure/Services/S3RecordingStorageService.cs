using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure.Services;

public sealed class S3RecordingStorageService : IRecordingStorageService
{
    private readonly RecordingS3Settings _settings;
    private readonly IAmazonS3 _s3;

    public S3RecordingStorageService(IOptions<RecordingS3Settings> settings)
    {
        _settings = settings.Value;

        var endpoint = (_settings.Endpoint ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("RecordingS3:Endpoint is required when RecordingS3 is enabled.");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint.TrimEnd('/'),
            ForcePathStyle = _settings.ForcePathStyle
        };

        // For MinIO/dev you might use HTTP.
        config.UseHttp = !_settings.UseSsl;

        _s3 = new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, config);
    }

    public async Task<StoredRecordingFile> SaveAsync(
        Stream input,
        string originalFileName,
        Guid eventId,
        Guid roomId,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_settings.Bucket))
        {
            throw new InvalidOperationException("RecordingS3:Bucket is required when RecordingS3 is enabled.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mp4";
        }

        var safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var key = $"{eventId:N}/{roomId:N}/{safeFileName}";

        await EnsureBucketExistsAsync(_settings.Bucket, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = _settings.Bucket,
            Key = key,
            InputStream = input,
            AutoCloseStream = false,
            ContentType = GuessContentType(extension)
        };

        await _s3.PutObjectAsync(request, cancellationToken);

        // For S3, the "relative path" can just be the object key.
        var playbackUrl = BuildPlaybackUrl(key);

        return new StoredRecordingFile(
            safeFileName,
            key,
            playbackUrl,
            FileSizeBytes: -1
        );
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3, bucketName))
        {
            return;
        }

        await _s3.PutBucketAsync(new PutBucketRequest
        {
            BucketName = bucketName
        }, cancellationToken);
    }

    private string BuildPlaybackUrl(string key)
    {
        if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
        {
            return $"{_settings.PublicBaseUrl!.TrimEnd('/')}/{key}";
        }

        var baseUrl = _settings.Endpoint.TrimEnd('/');
        if (_settings.ForcePathStyle)
        {
            return $"{baseUrl}/{_settings.Bucket}/{key}";
        }

        // virtual-host style: https://bucket.endpoint/key
        // (may not work with all MinIO setups, so path-style is preferred)
        var uri = new Uri(baseUrl);
        return $"{uri.Scheme}://{_settings.Bucket}.{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}/{key}";
    }

    private static string GuessContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".m4v" => "video/x-m4v",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}

