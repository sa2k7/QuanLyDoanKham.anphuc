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
    [Authorize] // Tất cả phải đăng nhập
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Companies — Admin, ContractManager và MedicalStaff được xem
        [HttpGet]
        [Authorize(Roles = "Admin,ContractManager,MedicalStaff")] // Cho phép MedicalStaff xem để thống kê
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        // POST: api/Companies — Admin và ContractManager được tạo mới
        [HttpPost]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<ActionResult<Company>> PostCompany(CompanyDto dto)
        {
            // Kiểm tra trùng lặp
            if (await _context.Companies.AnyAsync(c => c.CompanyName == dto.CompanyName))
                return BadRequest("Tên công ty đã tồn tại trong hệ thống.");
            if (await _context.Companies.AnyAsync(c => c.TaxCode == dto.TaxCode))
                return BadRequest("Mã số thuế này đã được đăng ký.");
            if (await _context.Companies.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
                return BadRequest("Số điện thoại này đã được sử dụng bởi công ty khác.");
            if (await _context.Companies.AnyAsync(c => c.Address == dto.Address))
                return BadRequest("Địa chỉ này đã được đăng ký cho một công ty khác.");

            var company = new Company
            {
                ShortName = dto.ShortName,
                CompanyName = dto.CompanyName,
                Address = dto.Address,
                TaxCode = dto.TaxCode,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompanies", new { id = company.CompanyId }, company);
        }

        // PUT: api/Companies/{id} — Admin và ContractManager được sửa
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> UpdateCompany(int id, CompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            // Kiểm tra trùng lặp (loại trừ chính nó)
            if (await _context.Companies.AnyAsync(c => c.CompanyName == dto.CompanyName && c.CompanyId != id))
                return BadRequest("Tên công ty đã tồn tại.");
            if (await _context.Companies.AnyAsync(c => c.TaxCode == dto.TaxCode && c.CompanyId != id))
                return BadRequest("Mã số thuế đã tồn tại.");
            if (await _context.Companies.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber && c.CompanyId != id))
                return BadRequest("Số điện thoại đã tồn tại.");
            if (await _context.Companies.AnyAsync(c => c.Address == dto.Address && c.CompanyId != id))
                return BadRequest("Địa chỉ đã tồn tại.");

            company.ShortName = dto.ShortName;
            company.CompanyName = dto.CompanyName;
            company.TaxCode = dto.TaxCode;
            company.Address = dto.Address;
            company.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok(company);
        }

        // DELETE: api/Companies/{id} — Admin và ContractManager được xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ContractManager")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
