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
                .Select(c => new HealthContractDto
                {
                    HealthContractId = c.HealthContractId,
                    CompanyId = c.CompanyId,
                    CompanyName = c.Company.CompanyName,
                    SigningDate = c.SigningDate,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    UnitPrice = c.UnitPrice,
                    ExpectedQuantity = c.ExpectedQuantity,
                    UnitName = c.UnitName,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    FilePath = c.FilePath
                })
                .ToListAsync();
        }

        // POST: api/HealthContracts
        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<ActionResult<HealthContract>> PostContract(HealthContractDto dto)
        {
            var contract = new HealthContract
            {
                CompanyId = dto.CompanyId,
                SigningDate = dto.SigningDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UnitPrice = dto.UnitPrice,
                ExpectedQuantity = dto.ExpectedQuantity,
                UnitName = dto.UnitName,
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

            contract.CompanyId = dto.CompanyId;
            contract.SigningDate = dto.SigningDate;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.UnitPrice = dto.UnitPrice;
            contract.ExpectedQuantity = dto.ExpectedQuantity;
            contract.UnitName = dto.UnitName;
            contract.TotalAmount = dto.UnitPrice * dto.ExpectedQuantity;
            contract.Status = dto.Status;
            contract.FilePath = dto.FilePath;

            await _context.SaveChangesAsync();
            return Ok(contract);
        }

        // POST: api/HealthContracts/upload-contract
        [HttpPost("upload-contract")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> UploadContract([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { path = $"uploads/contracts/{fileName}" });
        }
    }
}
