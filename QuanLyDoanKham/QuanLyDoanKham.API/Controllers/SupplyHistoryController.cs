using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplyHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplyHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SupplyHistory
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult> GetSupplyHistory()
        {
            var history = new List<object>();

            // Get export history from GroupSupplyDetails
            var exports = await _context.GroupSupplyDetails
                .Include(d => d.Supply)
                .Include(d => d.MedicalGroup)
                .Select(d => new
                {
                    Date = d.MedicalGroup.ExamDate,
                    Type = "Xuất kho",
                    SupplyName = d.Supply.SupplyName,
                    Quantity = d.QuantityUsed,
                    GroupName = d.MedicalGroup.GroupName,
                    Id = d.Id
                })
                .ToListAsync();

            history.AddRange(exports);

            // Get return history (where ReturnQuantity > 0)
            var returns = await _context.GroupSupplyDetails
                .Include(d => d.Supply)
                .Include(d => d.MedicalGroup)
                .Where(d => d.ReturnQuantity > 0)
                .Select(d => new
                {
                    Date = d.MedicalGroup.ExamDate,
                    Type = "Hoàn trả",
                    SupplyName = d.Supply.SupplyName,
                    Quantity = d.ReturnQuantity,
                    GroupName = d.MedicalGroup.GroupName,
                    Id = d.Id
                })
                .ToListAsync();

            history.AddRange(returns);

            // Sort by date descending (newest first)
            var sortedHistory = history.OrderByDescending(h => ((dynamic)h).Date).ToList();

            return Ok(sortedHistory);
        }
    }
}
