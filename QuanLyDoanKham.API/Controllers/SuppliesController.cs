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
            return await _context.Supplies
                .Select(s => new SupplyDto
                {
                    SupplyId = s.SupplyId,
                    SupplyName = s.SupplyName,
                    Unit = s.Unit,
                    IsFixedAsset = s.IsFixedAsset,
                    UnitPrice = s.UnitPrice,
                    TotalStock = s.TotalStock
                })
                .ToListAsync();
        }

        // POST: api/Supplies (Tạo mới loại vật tư)
        [HttpPost]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<ActionResult<Supply>> PostSupply(SupplyDto dto)
        {
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
                if (dto.Type == "IMPORT") supply.TotalStock += item.Quantity;
                else supply.TotalStock -= item.Quantity;
            }

            _context.SupplyInventoryVouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo phiếu thành công", voucherCode = voucher.VoucherCode });
        }
    }
}
