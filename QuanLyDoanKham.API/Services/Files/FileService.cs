using System.IO;

namespace QuanLyDoanKham.API.Services.Files
{
    public class FileService : IFileService
    {
        private readonly string _storageBasePath;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".png", ".xlsx", ".jpeg" };

        public FileService(IWebHostEnvironment env, IConfiguration config)
        {
            // [BUG-04 FIX] Lậy base path từ config nếu có (cho production persistent storage), 
            // nếu không thì mặc định vĂ o wwwroot/uploads
            _storageBasePath = config["Storage:BaseFolder"] ?? Path.Combine(env.WebRootPath, "uploads");
        }

        public async Task<(string? Path, string? Error)> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return (null, "Tệp không hợp lệ.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
                return (null, $"Ä ịnh dạng file không há»— trợ. Chỉ nhận: {string.Join(", ", _allowedExtensions)}");

            try
            {
                var targetFolder = Path.Combine(_storageBasePath, subFolder);
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(targetFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Nếu lưu trong wwwroot thì trả vá»  path tương đá»‘i để browser render được
                // Nếu lưu ngoĂ i wwwroot thì cần API path để surrogate (sẽ cần thêm Download controller)
                var relativePath = $"uploads/{subFolder}/{fileName}";
                return (relativePath, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lưu tệp: {ex.Message}");
            }
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // relativePath thưá» ng lĂ  "uploads/subFolder/file.ext"
            // Ta cần map phần sau "uploads/" vĂ o _storageBasePath
            var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (normalizedPath.StartsWith("uploads" + Path.DirectorySeparatorChar))
            {
                normalizedPath = normalizedPath.Substring(("uploads" + Path.DirectorySeparatorChar).Length);
            }

            var fullPath = Path.Combine(_storageBasePath, normalizedPath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public string[] GetAllowedExtensions() => _allowedExtensions;
    }
}
