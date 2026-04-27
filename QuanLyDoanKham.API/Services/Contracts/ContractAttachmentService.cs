using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Contracts
{
    /// <summary>Service xử lý file đính kèm hợp đồng</summary>
    public interface IContractAttachmentService
    {
        Task<ContractAttachment> UploadAttachmentAsync(int contractId, IFormFile file, int uploadedByUserId);
        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<IEnumerable<ContractAttachment>> GetAttachmentsAsync(int contractId);
        Task<string?> GetAttachmentPathAsync(int attachmentId);
    }

    public class ContractAttachmentService : IContractAttachmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const string AttachmentsFolder = "attachments/contracts";

        public ContractAttachmentService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<ContractAttachment> UploadAttachmentAsync(int contractId, IFormFile file, int uploadedByUserId)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) throw new InvalidOperationException("Không tìm thấy hợp đồng");

            // Validate file
            var allowedTypes = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xls", ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedTypes.Contains(extension))
                throw new InvalidOperationException("Loại file không được hỗ trợ. Chỉ chấp nhận: PDF, DOC, DOCX, JPG, PNG, XLS, XLSX");

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
                throw new InvalidOperationException("File quá lớn. Tối đa 10MB");

            // Create folder
            var folderPath = Path.Combine(_environment.WebRootPath, AttachmentsFolder, contractId.ToString());
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);
            var relativePath = $"/{AttachmentsFolder}/{contractId}/{fileName}";

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save to database
            var attachment = new ContractAttachment
            {
                HealthContractId = contractId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileType = extension.TrimStart('.').ToUpper(),
                UploadedAt = DateTime.Now,
                UploadedBy = uploadedByUserId.ToString()
            };

            _context.ContractAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return attachment;
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.ContractAttachments.FindAsync(attachmentId);
            if (attachment == null) return false;

            // Delete physical file
            var fullPath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _context.ContractAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ContractAttachment>> GetAttachmentsAsync(int contractId)
        {
            return await _context.ContractAttachments
                .Where(a => a.HealthContractId == contractId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<string?> GetAttachmentPathAsync(int attachmentId)
        {
            var attachment = await _context.ContractAttachments.FindAsync(attachmentId);
            if (attachment == null) return null;

            var fullPath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/'));
            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}
