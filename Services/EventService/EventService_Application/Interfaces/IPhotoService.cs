using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace EventService_Application.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> UploadPhotoAsync(IFormFile photo, string folderName);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}