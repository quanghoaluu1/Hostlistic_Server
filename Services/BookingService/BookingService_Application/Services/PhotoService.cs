using BookingService_Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if (file.Length <= 0) return uploadResult;
            await using var stream = file.OpenReadStream();

            var isQrCode = file.FileName.StartsWith("qr-", StringComparison.OrdinalIgnoreCase);

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                // For normal photos, we crop to a standard aspect ratio.
                // For QR codes, NEVER crop/resize here (cropping breaks scannability and can cut modules).
                Transformation = isQrCode
                    ? null
                    : new Transformation().Width(800).Height(500).Crop("fill").Gravity("face"),
                Folder = isQrCode ? "Booking_Service_QRCodes" : "Booking_Service_Photos",
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
}