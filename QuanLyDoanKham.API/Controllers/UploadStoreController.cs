using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadStoreController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadStoreController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("clinical")]
        public async Task<IActionResult> UploadClinicalImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file nào được nạp." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận ảnh (jpg, png, webp)." });

            if (file.Length > 10 * 1024 * 1024) // 10MB
                return BadRequest(new { message = "Dung lượng file vượt quá 10MB." });

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "clinical");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/clinical/{uniqueFileName}";
            return Ok(new { url, message = "Upload thành công." });
        }
    }
}
