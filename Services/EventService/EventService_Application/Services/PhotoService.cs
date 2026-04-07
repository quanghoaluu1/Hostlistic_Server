using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EventService_Application.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file, string folderName)
    {
        var uploadResult = new ImageUploadResult();
        if (file.Length <= 0) return uploadResult;
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Width(800).Height(500).Crop("fill").Gravity("face"),
            Folder = folderName,
        };
        uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}



public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
