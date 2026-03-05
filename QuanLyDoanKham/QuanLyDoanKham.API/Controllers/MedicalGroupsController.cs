using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalGroups — Các vai trò vận hành và khách hàng
        [HttpGet]
        [Authorize(Roles = "Admin,MedicalGroupManager,MedicalStaff,Customer")]
        public async Task<ActionResult<IEnumerable<MedicalGroupDto>>> GetMedicalGroups()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            IQueryable<MedicalGroup> query = _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company);

            // RBAC: Customer only sees their company's groups
            if (role == "Customer" && !string.IsNullOrEmpty(companyIdClaim))
            {
                int companyId = int.Parse(companyIdClaim);
                query = query.Where(g => g.HealthContract.CompanyId == companyId);
            }

            var groups = await query
                .Select(g => new MedicalGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    HealthContractId = g.HealthContractId,
                    CompanyName = g.HealthContract != null && g.HealthContract.Company != null 
                                  ? g.HealthContract.Company.CompanyName 
                                  : "Không xác định",
                    IsFinished = g.IsFinished
                })
                .ToListAsync();

            return Ok(groups);
        }

        // POST: api/MedicalGroups — Admin và MedicalGroupManager
        [HttpPost]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<ActionResult<MedicalGroup>> PostMedicalGroup(MedicalGroupDto dto)
        {
            // RÀNG BUỘC: 1 hợp đồng chỉ được tạo 1 đoàn khám duy nhất.
            var existingGroup = await _context.MedicalGroups
                .FirstOrDefaultAsync(g => g.HealthContractId == dto.HealthContractId);

            if (existingGroup != null)
            {
                return BadRequest($"Hợp đồng này đã có đoàn khám '{existingGroup.GroupName}' được tạo trước đó. Mỗi hợp đồng chỉ được phép tạo duy nhất một đoàn khám.");
            }

            // Kiểm tra xem hợp đồng bản thân nó đã kết thúc chưa
            var contract = await _context.Contracts.FindAsync(dto.HealthContractId);
            if (contract != null && contract.IsFinished)
            {
                return BadRequest("Hợp đồng này đã được đánh dấu kết thúc hoàn toàn. Không thể tạo thêm đoàn khám mới.");
            }

            var entity = new MedicalGroup
            {
                GroupName = dto.GroupName,
                ExamDate = dto.ExamDate,
                HealthContractId = dto.HealthContractId,
                IsFinished = false
            };

            _context.MedicalGroups.Add(entity);
            await _context.SaveChangesAsync();

            dto.GroupId = entity.GroupId;
            dto.IsFinished = false;
            return Ok(dto);
        }

        // PUT: api/MedicalGroups/{id}/finish — Hoàn tất đoàn khám
        [HttpPut("{id}/finish")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> FinishGroup(int id)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            group.IsFinished = true;

            // ĐỒNG BỘ: Khi đoàn khám hoàn tất, hợp đồng đi kèm cũng phải kết thúc
            var contract = await _context.Contracts.FindAsync(group.HealthContractId);
            if (contract != null)
            {
                contract.IsFinished = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã kết thúc đoàn khám và tự động hoàn tất hợp đồng thành công!" });
        }
    }
}
