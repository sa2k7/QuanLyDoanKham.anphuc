using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [AuthorizePermission("Kho.View")]
        public async Task<IActionResult> GetSupplies()
        {
            var supplies = await _context.SupplyItems
                .OrderBy(s => s.ItemName)
                .ToListAsync();
            return Ok(supplies);
        }

        // POST: api/Supplies
        [HttpPost]
        [AuthorizePermission("Kho.Edit")]
        public async Task<IActionResult> CreateSupply([FromBody] SupplyItem item)
        {
            _context.SupplyItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // POST: api/Supplies/import
        [HttpPost("import")]
        [AuthorizePermission("Kho.Edit")]
        public async Task<IActionResult> ImportStock([FromBody] StockMovementDto dto)
        {
            var item = await _context.SupplyItems.FindAsync(dto.SupplyId);
            if (item == null) return NotFound("Không tìm thấy vật tư");

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (int?)null;

            var movement = new StockMovement
            {
                SupplyId = dto.SupplyId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Note = dto.Note ?? "",
                MovementType = "IN",
                MovementDate = DateTime.Now,
                TotalValue = dto.Quantity * dto.UnitPrice,
                ItemName = item.ItemName,
                Unit = item.Unit,
                RecordedByUserId = userId
            };

            item.CurrentStock += dto.Quantity;
            item.UpdatedAt = DateTime.Now;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nhập kho thành công", currentStock = item.CurrentStock });
        }

        // POST: api/Supplies/export
        [HttpPost("export")]
        [AuthorizePermission("Kho.Edit")]
        public async Task<IActionResult> ExportStock([FromBody] StockMovementDto dto)
        {
            var item = await _context.SupplyItems.FindAsync(dto.SupplyId);
            if (item == null) return NotFound("Không tìm thấy vật tư");

            if (item.CurrentStock < dto.Quantity)
                return BadRequest("Số lượng tồn kho không đủ");

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (int?)null;

            var movement = new StockMovement
            {
                SupplyId = dto.SupplyId,
                MedicalGroupId = dto.MedicalGroupId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Note = dto.Note ?? "",
                MovementType = "OUT",
                MovementDate = DateTime.Now,
                TotalValue = dto.Quantity * dto.UnitPrice,
                ItemName = item.ItemName,
                Unit = item.Unit,
                RecordedByUserId = userId
            };

            item.CurrentStock -= dto.Quantity;
            item.UpdatedAt = DateTime.Now;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xuất kho thành công", currentStock = item.CurrentStock });
        }

        // GET: api/Supplies/movements
        [HttpGet("movements")]
        [AuthorizePermission("Kho.View")]
        public async Task<IActionResult> GetAllMovements()
        {
            var movements = await _context.StockMovements
                .OrderByDescending(m => m.MovementDate)
                .Include(m => m.RecordedByUser)
                .Include(m => m.MedicalGroup)
                .Take(500) // Limit to latest 500
                .ToListAsync();
            return Ok(movements);
        }

        // GET: api/Supplies/movements/{supplyId}
        [HttpGet("movements/{supplyId}")]
        [AuthorizePermission("Kho.View")]
        public async Task<IActionResult> GetMovements(int supplyId)
        {
            var movements = await _context.StockMovements
                .Where(m => m.SupplyId == supplyId)
                .OrderByDescending(m => m.MovementDate)
                .Include(m => m.RecordedByUser)
                .Include(m => m.MedicalGroup)
                .ToListAsync();
            return Ok(movements);
        }
    }
}
