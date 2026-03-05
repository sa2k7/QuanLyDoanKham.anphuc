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

        // GET: api/HealthContracts — Tất cả role quản lý & khách hàng
        [HttpGet]
        [Authorize(Roles = "Admin,ContractManager,MedicalGroupManager,Customer")]
        public async Task<ActionResult<IEnumerable<HealthContractDto>>> GetContracts()
        {
            // RBAC Logic: 
            // - If Role = Customer -> Only show their contracts
            // - If Role = Managers -> Show all
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            IQueryable<HealthContract> query = _context.Contracts.Include(c => c.Company);

            if (role == "Customer" && !string.IsNullOrEmpty(companyIdClaim))
            {
                int companyId = int.Parse(companyIdClaim);
                query = query.Where(c => c.CompanyId == companyId);
            }

            var list = await query.Select(c => new HealthContractDto
            {
                HealthContractId = c.HealthContractId,
                CompanyId = c.CompanyId,
                CompanyName = c.Company != null ? c.Company.CompanyName : "Không xác định",
                TotalAmount = c.TotalAmount,
                PatientCount = c.PatientCount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsFinished = c.IsFinished
            }).ToListAsync();

            return Ok(list);
        }

        // POST: api/HealthContracts — Admin và ContractManager
        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<ActionResult<HealthContract>> PostContract(HealthContractDto dto)
        {
            // RÀNG BUỘC: 1 Công ty chỉ được phép có 1 Hợp đồng duy nhất.
            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.CompanyId == dto.CompanyId);

            if (existingContract != null)
            {
                return BadRequest($"Công ty này đã có hợp đồng #{existingContract.HealthContractId}. Mỗi đối tác chỉ cho phép tồn tại một hợp đồng khám sức khỏe duy nhất.");
            }

            var contract = new HealthContract
            {
                CompanyId = dto.CompanyId,
                TotalAmount = dto.TotalAmount,
                PatientCount = dto.PatientCount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsFinished = false
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return Ok(contract);
        }

        // PUT: api/HealthContracts/{id} — Cập nhật hợp đồng
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> PutContract(int id, HealthContractDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // RÀNG BUỘC: Khi cập nhật công ty mới cho hợp đồng, phải kiểm tra xem công ty đó đã có hợp đồng khác chưa
            if (contract.CompanyId != dto.CompanyId)
            {
                var targetCompanyContract = await _context.Contracts
                    .AnyAsync(c => c.CompanyId == dto.CompanyId && c.HealthContractId != id);
                
                if (targetCompanyContract)
                {
                    return BadRequest("Công ty đối tác này đã có một hợp đồng khác trong hệ thống. Không thể chuyển giao.");
                }
            }

            contract.CompanyId = dto.CompanyId;
            contract.TotalAmount = dto.TotalAmount;
            contract.PatientCount = dto.PatientCount;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.IsFinished = dto.IsFinished;

            await _context.SaveChangesAsync();
            return Ok(contract);
        }

        // DELETE: api/HealthContracts/{id} — Xóa hợp đồng
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/HealthContracts/{id}/finish — Hoàn tất hợp đồng
        [HttpPut("{id}/finish")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> FinishContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            // RÀNG BUỘC: Kiểm tra xem tất cả các đoàn khám thuộc hợp đồng này đã hoàn thành chưa
            var unfinishedGroups = await _context.MedicalGroups
                .AnyAsync(g => g.HealthContractId == id && !g.IsFinished);

            if (unfinishedGroups)
            {
                return BadRequest(new { message = "Không thể kết thúc hợp đồng vì vẫn còn đoàn khám đang chạy. Hãy kết thúc tất cả đoàn khám trước!" });
            }

            contract.IsFinished = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hoàn thành hợp đồng thành công!" });
        }
    }
}
