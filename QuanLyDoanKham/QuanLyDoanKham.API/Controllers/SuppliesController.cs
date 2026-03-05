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

        // GET: api/Supplies — Admin và WarehouseManager được xem
        [HttpGet]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<ActionResult<IEnumerable<SupplyDto>>> GetSupplies()
        {
            return await _context.Supplies
                .Select(s => new SupplyDto
                {
                    SupplyId = s.SupplyId,
                    SupplyName = s.SupplyName,
                    IsFixedAsset = s.IsFixedAsset,
                    UnitPrice = s.UnitPrice,
                    StockQuantity = s.StockQuantity
                })
                .ToListAsync();
        }

        // POST: api/Supplies (Nhập vật tư mới) — Admin và WarehouseManager được tạo
        [HttpPost]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<ActionResult<Supply>> PostSupply(SupplyDto dto)
        {
            var supply = new Supply
            {
                SupplyName = dto.SupplyName,
                IsFixedAsset = dto.IsFixedAsset,
                UnitPrice = dto.UnitPrice,
                StockQuantity = dto.StockQuantity
            };

            _context.Supplies.Add(supply);
            await _context.SaveChangesAsync();

            var supplyDto = new SupplyDto
            {
                SupplyId = supply.SupplyId,
                SupplyName = supply.SupplyName,
                IsFixedAsset = supply.IsFixedAsset,
                UnitPrice = supply.UnitPrice,
                StockQuantity = supply.StockQuantity
            };

            return CreatedAtAction(nameof(GetSupplies), new { id = supply.SupplyId }, supplyDto);
        }

        // PUT: api/Supplies/{id} — Admin và WarehouseManager được sửa
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<IActionResult> UpdateSupply(int id, SupplyDto dto)
        {
            var supply = await _context.Supplies.FindAsync(id);
            if (supply == null) return NotFound();

            supply.SupplyName = dto.SupplyName;
            supply.IsFixedAsset = dto.IsFixedAsset;
            supply.UnitPrice = dto.UnitPrice;
            supply.StockQuantity = dto.StockQuantity;

            await _context.SaveChangesAsync();
            return Ok(supply);
        }

        // DELETE: api/Supplies/{id} — Chỉ Admin được xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupply(int id)
        {
            var supply = await _context.Supplies.FindAsync(id);
            if (supply == null) return NotFound();

            _context.Supplies.Remove(supply);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Supplies/transactions — Admin và WarehouseManager được xem
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<ActionResult<IEnumerable<SupplyTransactionDto>>> GetAllTransactions()
        {
            return await _context.SupplyTransactions
                .Include(t => t.Supply)
                .Include(t => t.ProcessedUser)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new SupplyTransactionDto
                {
                    TransactionId = t.TransactionId,
                    SupplyId = t.SupplyId,
                    SupplyName = t.Supply.SupplyName,
                    Quantity = t.Quantity,
                    Type = t.Type,
                    Note = t.Note,
                    TransactionDate = t.TransactionDate,
                    ProcessedBy = t.ProcessedUser != null ? t.ProcessedUser.FullName : "System"
                })
                .ToListAsync();
        }

        // POST: api/Supplies/import/{id} — Admin và WarehouseManager được nhập kho
        [HttpPost("import/{id}")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        public async Task<IActionResult> ImportStock(int id, [FromBody] ImportStockDto dto)
        {
            var supply = await _context.Supplies.FindAsync(id);
            if (supply == null) return NotFound();

            supply.StockQuantity += dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Stock updated", NewQuantity = supply.StockQuantity });
        }
    }
}
