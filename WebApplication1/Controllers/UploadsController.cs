using CinemaBooking.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Authorize(Roles = "Admin")]
    public class UploadsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Fail("Chọn một file ảnh."));

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiResponse<object>.Fail("Ảnh tối đa 5MB."));

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(ApiResponse<object>.Fail("Chỉ nhận JPG, PNG, WEBP hoặc GIF."));

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(folder);

            var name = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var path = Path.Combine(folder, name);
            await using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(ApiResponse<object>.Ok(new { url = $"/uploads/{name}" }));
        }
    }
}
