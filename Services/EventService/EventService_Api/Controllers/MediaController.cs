using Common;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(IPhotoService photoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile photo, string folderName)
    {
       var result = await photoService.UploadPhotoAsync(photo, folderName);
       var imageUrl = result.SecureUrl.AbsoluteUri;
       if (result.Error != null) return BadRequest(ApiResponse<string>.Fail(400, result.Error.Message));
       var publicId = result.PublicId;
       var uploadedImage = new
       {
           ImageUrl = imageUrl,
           PublicId = publicId
       };
       return Ok(ApiResponse<object>.Success(200, result.Status, uploadedImage));
    }
}