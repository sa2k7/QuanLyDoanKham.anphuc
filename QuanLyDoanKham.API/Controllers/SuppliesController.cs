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
    public class SuppliesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuppliesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Supplies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplyDto>>> GetSupplies()
        {
            var today = DateTime.Today;
            return await _context.Supplies
                .Select(s => new SupplyDto
                {
                    SupplyId = s.SupplyId,
                    SupplyName = s.SupplyName,
                    Unit = s.Unit,
                    IsFixedAsset = s.IsFixedAsset,
                    Category = s.Category,
                    LotNumber = s.LotNumber,
                    ExpirationDate = s.ExpirationDate,
                    ManufactureDate = s.ManufactureDate,
                    MinStockLevel = s.MinStockLevel,
                    UnitPrice = s.UnitPrice,
                    TotalStock = s.TotalStock,
                    // Canh bao HSD con < 30 ngay
                    IsExpiringSoon = s.ExpirationDate.HasValue && s.ExpirationDate.Value <= today.AddDays(30),
                    // Canh bao ton kho duoi nguong toi thieu
                    IsLowStock = s.TotalStock <= s.MinStockLevel
                })
                .ToListAsync();
        }


        // DELETE: api/Supplies/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<IActionResult> DeleteSupply(int id)
        {
            var supply = await _context.Supplies.FindAsync(id);
            if (supply == null) return NotFound("Không tìm thấy vật tư.");

            var isUsed = await _context.SupplyInventoryDetails.AnyAsync(d => d.SupplyId == id);
            if (isUsed) return BadRequest("Vật tư này đã phát sinh giao dịch phiếu kho, không thể xóa.");

            _context.Supplies.Remove(supply);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa vật tư thành công.");
        }

        // POST: api/Supplies (Tạo mới loại vật tư)
        [HttpPost]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<ActionResult<Supply>> PostSupply(SupplyDto dto)
        {
            if (await _context.Supplies.AnyAsync(s => s.SupplyName == dto.SupplyName))
                return BadRequest($"Vật tư '{dto.SupplyName}' đã tồn tại trong danh mục.");

            var supply = new Supply
            {
                SupplyName = dto.SupplyName,
                Unit = dto.Unit,
                IsFixedAsset = dto.IsFixedAsset,
                UnitPrice = dto.UnitPrice,
                TotalStock = 0
            };

            _context.Supplies.Add(supply);
            await _context.SaveChangesAsync();
            return Ok(supply);
        }

        // GET: api/Supplies/vouchers
        [HttpGet("vouchers")]
        public async Task<ActionResult<IEnumerable<SupplyInventoryVoucherDto>>> GetVouchers()
        {
            return await _context.SupplyInventoryVouchers
                .Include(v => v.MedicalGroup)
                .Include(v => v.CreatedByUser)
                .OrderByDescending(v => v.CreateDate)
                .Select(v => new SupplyInventoryVoucherDto
                {
                    VoucherId = v.VoucherId,
                    VoucherCode = v.VoucherCode,
                    CreateDate = v.CreateDate,
                    Type = v.Type,
                    GroupId = v.GroupId,
                    GroupName = v.MedicalGroup != null ? v.MedicalGroup.GroupName : null,
                    CreatedBy = v.CreatedByUser.FullName,
                    Note = v.Note
                })
                .ToListAsync();
        }

        // POST: api/Supplies/vouchers (Tạo phiếu nhập/xuất)
        [HttpPost("vouchers")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<IActionResult> CreateVoucher(CreateVoucherDto dto)
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            var voucher = new SupplyInventoryVoucher
            {
                VoucherCode = (dto.Type == "IMPORT" ? "NK" : "XK") + DateTime.Now.ToString("yyyyMMddHHmmss"),
                CreateDate = DateTime.Now,
                Type = dto.Type,
                GroupId = dto.GroupId,
                CreatedByUserId = user.UserId,
                Note = dto.Note
            };

            foreach (var item in dto.Details)
            {
                var supply = await _context.Supplies.FindAsync(item.SupplyId);
                if (supply == null) continue;

                if (dto.Type == "EXPORT" && supply.TotalStock < item.Quantity)
                    return BadRequest($"Vật tư {supply.SupplyName} không đủ tồn kho.");

                voucher.Details.Add(new SupplyInventoryDetail
                {
                    SupplyId = item.SupplyId,
                    Quantity = item.Quantity,
                    Price = supply.UnitPrice
                });

                // Cập nhật tồn kho thực tế
                if (dto.Type.ToUpper() == "IMPORT") 
                {
                    supply.TotalStock += item.Quantity;
                }
                else if (dto.Type.ToUpper() == "EXPORT")
                {
                    supply.TotalStock -= item.Quantity;
                }
            }

            _context.SupplyInventoryVouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo phiếu thành công", voucherCode = voucher.VoucherCode });
        }
        // DELETE: api/Supplies/vouchers/{id}
        [HttpDelete("vouchers/{id}")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.SupplyInventoryVouchers
                .Include(v => v.Details)
                .FirstOrDefaultAsync(v => v.VoucherId == id);

            if (voucher == null) return NotFound("Không tìm thấy phiếu kho.");

            // Hoàn tác tồn kho
            foreach (var detail in voucher.Details)
            {
                var supply = await _context.Supplies.FindAsync(detail.SupplyId);
                if (supply != null)
                {
                    if (voucher.Type.ToUpper() == "IMPORT")
                    {
                        supply.TotalStock -= detail.Quantity;
                    }
                    else if (voucher.Type.ToUpper() == "EXPORT")
                    {
                        supply.TotalStock += detail.Quantity;
                    }
                }
            }

            _context.SupplyInventoryVouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa phiếu kho và hoàn tác tồn kho thành công.");
        }
    }
}
