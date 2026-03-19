using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HealthContracts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthContractDto>>> GetContracts()
        {
            return await _context.Contracts
                .Include(c => c.Company)
                .Include(c => c.StatusHistories)
                .Select(c => new HealthContractDto
                {
                    HealthContractId = c.HealthContractId,
                    CompanyId = c.CompanyId,
                    ShortName = c.Company.ShortName,
                    CompanyName = c.Company.CompanyName,
                    SigningDate = c.SigningDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    UnitPrice = c.UnitPrice,
                    ExpectedQuantity = c.ExpectedQuantity,
                    UnitName = c.UnitName,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    FilePath = c.FilePath,
                    StatusHistories = c.StatusHistories.Select(h => new ContractStatusHistoryDto
                    {
                        Id = h.Id,
                        OldStatus = h.OldStatus,
                        NewStatus = h.NewStatus,
                        Note = h.Note,
                        ChangedAt = h.ChangedAt,
                        ChangedBy = h.ChangedBy
                    }).OrderByDescending(h => h.ChangedAt).ToList()
                })
                .ToListAsync();
        }

        // POST: api/HealthContracts
        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<ActionResult<HealthContract>> PostContract(HealthContractDto dto)
        {
            // Kiểm tra trùng lặp: Tuyệt đối không cho phép 1 công ty có 2 hợp đồng ký cùng 1 ngày
            if (await _context.Contracts.AnyAsync(c => c.CompanyId == dto.CompanyId && c.SigningDate.Date == dto.SigningDate.Date))
            {
                return BadRequest("Công ty này đã có một hợp đồng ký vào ngày này. Vui lòng kiểm tra lại.");
            }

            var contract = new HealthContract
            {
                CompanyId = dto.CompanyId,
                SigningDate = dto.SigningDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UnitPrice = dto.UnitPrice,
                ExpectedQuantity = dto.ExpectedQuantity,
                UnitName = string.IsNullOrEmpty(dto.UnitName) ? "Người" : dto.UnitName,
                TotalAmount = dto.UnitPrice * dto.ExpectedQuantity,
                Status = "Pending",
                FilePath = dto.FilePath
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return Ok(contract);
        }

        // PUT: api/HealthContracts/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> PutContract(int id, HealthContractDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (contract.Status == "Locked") return BadRequest("Hợp đồng đã khóa, không thể chỉnh sửa.");

            // Kiểm tra trùng lặp khi cập nhật
            if (await _context.Contracts.AnyAsync(c => c.CompanyId == dto.CompanyId && c.SigningDate.Date == dto.SigningDate.Date && c.HealthContractId != id))
            {
                return BadRequest("Thông tin trùng khớp với một hợp đồng khác (Cùng đối tác và ngày ký).");
            }

            contract.CompanyId = dto.CompanyId;
            contract.SigningDate = dto.SigningDate;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.UnitPrice = dto.UnitPrice;
            contract.ExpectedQuantity = dto.ExpectedQuantity;
            contract.UnitName = dto.UnitName;
            contract.TotalAmount = dto.UnitPrice * dto.ExpectedQuantity;
            
            if (contract.Status != dto.Status)
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = contract.Status,
                    NewStatus = dto.Status,
                    ChangedBy = User.Identity?.Name ?? "system",
                    ChangedAt = DateTime.Now,
                    Note = "Cập nhật từ form chỉnh sửa"
                });
                contract.Status = dto.Status;
            }
            
            contract.FilePath = dto.FilePath;

            await _context.SaveChangesAsync();
            return Ok(contract);
        }

        // PATCH: api/HealthContracts/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (contract.Status == dto.Status) return Ok(contract);

            // Ràng buộc: Chỉ kết thúc (Finished) khi tất cả các đoàn khám đã Finished hoặc Locked
            if (dto.Status == "Finished")
            {
                var unfinishedGroups = await _context.MedicalGroups
                    .AnyAsync(g => g.HealthContractId == id && g.Status != "Finished" && g.Status != "Locked");
                
                if (unfinishedGroups)
                {
                    return BadRequest("Không thể kết thúc hợp đồng vì vẫn còn đoàn khám đang triển khai (Chưa Finished/Locked).");
                }
            }

            _context.ContractStatusHistories.Add(new ContractStatusHistory
            {
                HealthContractId = id,
                OldStatus = contract.Status,
                NewStatus = dto.Status,
                ChangedBy = User.Identity?.Name ?? "system",
                ChangedAt = DateTime.Now,
                Note = dto.Note ?? "Cập nhật trạng thái trực tiếp"
            });

            contract.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật trạng thái hợp đồng.", status = contract.Status });
        }

        // PUT: api/HealthContracts/{id}/lock
        [HttpPut("{id}/lock")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> LockContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (contract.Status != "Locked")
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = contract.Status,
                    NewStatus = "Locked",
                    ChangedBy = User.Identity?.Name ?? "system",
                    ChangedAt = DateTime.Now,
                    Note = "Khóa hợp đồng"
                });
                contract.Status = "Locked";
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Hợp đồng đã được khóa an toàn." });
        }

        // PUT: api/HealthContracts/{id}/unlock
        [HttpPut("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (contract.Status != "Active")
            {
                _context.ContractStatusHistories.Add(new ContractStatusHistory
                {
                    HealthContractId = id,
                    OldStatus = contract.Status,
                    NewStatus = "Active",
                    ChangedBy = User.Identity?.Name ?? "system",
                    ChangedAt = DateTime.Now,
                    Note = "Mở khóa hợp đồng"
                });
                contract.Status = "Active";
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Đã mở khóa hợp đồng thành công." });
        }

        // DELETE: api/HealthContracts/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (contract.Status == "Locked") return BadRequest("Hợp đồng đang khóa, vui lòng mở khóa trước khi xóa.");

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa hợp đồng thành công." });
        }

        // POST: api/HealthContracts/{id}/upload
        [HttpPost("{id}/upload")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> UploadFile(int id, [FromForm] IFormFile file)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (file == null || file.Length == 0) return BadRequest("Tệp không hợp lệ.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            contract.FilePath = $"uploads/contracts/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { path = contract.FilePath });
        }
    }
}
