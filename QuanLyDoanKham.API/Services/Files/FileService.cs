using System.IO;

namespace QuanLyDoanKham.API.Services.Files
{
    public class FileService : IFileService
    {
        private readonly string _storageBasePath;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".png", ".xlsx", ".jpeg" };

        public FileService(IWebHostEnvironment env, IConfiguration config)
        {
            // [BUG-04 FIX] Láº­y base path tá»« config náº¿u cĂ³ (cho production persistent storage), 
            // náº¿u khĂ´ng thĂ¬ máº·c Ä‘á»‹nh vĂ o wwwroot/uploads
            _storageBasePath = config["Storage:BaseFolder"] ?? Path.Combine(env.WebRootPath, "uploads");
        }

        public async Task<(string? Path, string? Error)> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return (null, "Tá»‡p khĂ´ng há»£p lá»‡.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
                return (null, $"Ä á»‹nh dáº¡ng file khĂ´ng há»— trá»£. Chá»‰ nháº­n: {string.Join(", ", _allowedExtensions)}");

            try
            {
                var targetFolder = Path.Combine(_storageBasePath, subFolder);
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(targetFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Náº¿u lÆ°u trong wwwroot thĂ¬ tráº£ vá»  path tÆ°Æ¡ng Ä‘á»‘i Ä‘á»ƒ browser render Ä‘Æ°á»£c
                // Náº¿u lÆ°u ngoĂ i wwwroot thĂ¬ cáº§n API path Ä‘á»ƒ surrogate (sáº½ cáº§n thĂªm Download controller)
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

            // relativePath thÆ°á» ng lĂ  "uploads/subFolder/file.ext"
            // Ta cáº§n map pháº§n sau "uploads/" vĂ o _storageBasePath
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
