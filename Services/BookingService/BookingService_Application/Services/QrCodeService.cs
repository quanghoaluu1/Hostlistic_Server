using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;

namespace BookingService_Application.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly IPhotoService _photoService;

        public QrCodeService(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        public async Task<string> GenerateQrCodeAsync(string ticketCode)
        {
            try
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                // Ensure a proper quiet zone (margin) for scanning reliability
                using var qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, drawQuietZones: true);
                using var stream = new MemoryStream();
                qrCodeImage.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                // Convert to IFormFile for existing PhotoService
                var formFile = new FormFile(stream, 0, stream.Length, "qrcode", $"qr-{ticketCode}.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                var uploadResult = await _photoService.UploadPhotoAsync(formFile);
                return uploadResult.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Log exception here
                return string.Empty;
            }
        }
    }
}
