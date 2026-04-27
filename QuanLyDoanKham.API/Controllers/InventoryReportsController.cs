using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>Báo cáo và thống kê tồn kho</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Báo cáo tồn kho theo kỳ (Đầu kỳ, Nhập, Xuất, Cuối kỳ)</summary>
        [HttpGet("periodic-stock-report")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetPeriodicStockReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var from = fromDate.Date;
            var to = toDate.Date.AddDays(1).AddTicks(-1);

            var items = await _context.SupplyItems.ToListAsync();
            var report = new List<object>();

            foreach (var item in items)
            {
                // Tồn đầu kỳ (trước fromDate)
                var inboundBefore = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "IN" && m.MovementDate < from)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;
                var outboundBefore = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "OUT" && m.MovementDate < from)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;
                var tonDauKy = inboundBefore - outboundBefore;

                // Phát sinh trong kỳ (từ fromDate đến toDate)
                var nhapTrongKy = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "IN" && m.MovementDate >= from && m.MovementDate <= to)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;
                var xuatTrongKy = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "OUT" && m.MovementDate >= from && m.MovementDate <= to)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                // Tồn cuối kỳ
                var tonCuoiKy = tonDauKy + nhapTrongKy - xuatTrongKy;

                report.Add(new
                {
                    item.SupplyId,
                    item.ItemName,
                    item.Category,
                    item.Unit,
                    TonDauKy = tonDauKy,
                    NhapTrongKy = nhapTrongKy,
                    XuatTrongKy = xuatTrongKy,
                    TonCuoiKy = tonCuoiKy,
                    UnitPrice = item.TypicalUnitPrice,
                    TotalValue = tonCuoiKy * item.TypicalUnitPrice
                });
            }

            return Ok(new {
                Summary = new { FromDate = from, ToDate = to, TotalItems = items.Count },
                Items = report
            });
        }


        /// <summary>Báo cáo tồn kho hiện tại</summary>
        [HttpGet("stock-report")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetStockReport([FromQuery] DateTime? asOfDate)
        {
            var date = asOfDate?.Date ?? DateTime.Today;

            // Lấy tất cả vật tư với tính toán tồn kho
            var items = await _context.SupplyItems.ToListAsync();

            var report = new List<object>();

            foreach (var item in items)
            {
                // Tính tồn đầu kỳ (tất cả nhập trước ngày report)
                var inboundBefore = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId &&
                                m.MovementType == "IN" &&
                                m.MovementDate.Date <= date)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                // Tính xuất trong kỳ (tất cả xuất trước ngày report)
                var outboundBefore = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId &&
                                m.MovementType == "OUT" &&
                                m.MovementDate.Date <= date)
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                var currentStock = inboundBefore - outboundBefore;

                // Tính giá trị tồn kho
                var avgPrice = item.TypicalUnitPrice;

                report.Add(new
                {
                    item.SupplyId,
                    item.ItemName,
                    item.Unit,
                    item.Category,
                    CurrentStock = currentStock,
                    MinStock = item.MinStockLevel,
                    ReorderPoint = item.MinStockLevel,
                    UnitPrice = avgPrice,
                    StockValue = currentStock * avgPrice,
                    Status = GetStockStatus(currentStock, item.MinStockLevel, item.MinStockLevel),
                });
            }

            var summary = new
            {
                TotalItems = items.Count,
                TotalStockValue = report.Sum(r => (decimal)((dynamic)r).StockValue),
                LowStockItems = report.Count(r => ((dynamic)r).Status == "LOW"),
                OutOfStockItems = report.Count(r => ((dynamic)r).Status == "OUT"),
                ReportDate = date
            };

            return Ok(new { summary, items = report });
        }

        private string GetStockStatus(int current, int min, int reorderPoint)
        {
            if (current <= 0) return "OUT";
            if (current < min) return "LOW";
            if (current < reorderPoint) return "REORDER";
            return "OK";
        }

        /// <summary>Báo cáo nhập/xuất kho trong kỳ</summary>
        [HttpGet("movement-report")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetMovementReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? movementType = null)
        {
            var query = _context.StockMovements
                .Where(m => m.MovementDate.Date >= from.Date &&
                           m.MovementDate.Date <= to.Date)
                .Include(m => m.SupplyItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(movementType))
                query = query.Where(m => m.MovementType == movementType);

            var movements = await query
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            var report = movements.Select(m => new
            {
                m.MovementId,
                m.MovementType,
                m.MovementDate,
                m.SupplyId,
                ItemName = m.SupplyItem?.ItemName ?? m.ItemName,
                m.Quantity,
                m.Unit,
                m.UnitPrice,
                TotalValue = m.TotalValue,
                m.Note,
                m.MedicalGroupId
            });

            var summary = new
            {
                FromDate = from,
                ToDate = to,
                TotalMovements = movements.Count,
                TotalInbound = movements.Where(m => m.MovementType == "IN").Sum(m => m.Quantity),
                TotalOutbound = movements.Where(m => m.MovementType == "OUT").Sum(m => m.Quantity),
                TotalInboundValue = movements.Where(m => m.MovementType == "IN")
                    .Sum(m => m.TotalValue),
                TotalOutboundValue = movements.Where(m => m.MovementType == "OUT")
                    .Sum(m => m.TotalValue)
            };

            return Ok(new { summary, movements = report });
        }

        /// <summary>Báo cáo tồn kho theo nhóm</summary>
        [HttpGet("by-category")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetReportByCategory()
        {
            // Group by Category field in SupplyItem
            var items = await _context.SupplyItems.ToListAsync();
            var categories = items.GroupBy(i => i.Category).ToList();

            var report = new List<object>();

            foreach (var category in categories)
            {
                var supplyIds = category.Select(i => i.SupplyId).ToList();

                var totalInbound = await _context.StockMovements
                    .Where(m => supplyIds.Contains(m.SupplyId ?? 0) && m.MovementType == "IN")
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                var totalOutbound = await _context.StockMovements
                    .Where(m => supplyIds.Contains(m.SupplyId ?? 0) && m.MovementType == "OUT")
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                var currentStock = totalInbound - totalOutbound;

                report.Add(new
                {
                    CategoryName = category.Key,
                    ItemCount = category.Count(),
                    TongNhap = totalInbound,
                    TongXuat = totalOutbound,
                    TonHienTai = currentStock
                });
            }

            return Ok(report);
        }

        /// <summary>Báo cáo vật tư sắp hết (cần đặt hàng)</summary>
        [HttpGet("reorder-alert")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetReorderAlert()
        {
            var items = await _context.SupplyItems
                .Where(i => i.MinStockLevel > 0)
                .ToListAsync();

            var alertItems = new List<object>();

            foreach (var item in items)
            {
                var inbound = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "IN")
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                var outbound = await _context.StockMovements
                    .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "OUT")
                    .SumAsync(m => (int?)m.Quantity) ?? 0;

                var currentStock = inbound - outbound;

                if (currentStock <= item.MinStockLevel)
                {
                    alertItems.Add(new
                    {
                        item.SupplyId,
                        item.ItemName,
                        item.Unit,
                        CurrentStock = currentStock,
                        MinStockLevel = item.MinStockLevel,
                        SuggestedOrderQty = item.MinStockLevel - currentStock,
                        item.Category,
                        Urgency = currentStock == 0 ? "CRITICAL" : currentStock <= item.MinStockLevel / 2 ? "HIGH" : "MEDIUM"
                    });
                }
            }

            return Ok(new
            {
                alertCount = alertItems.Count,
                items = alertItems.OrderBy(i => ((dynamic)i).Urgency == "CRITICAL" ? 0 :
                                                ((dynamic)i).Urgency == "HIGH" ? 1 : 2)
            });
        }

        /// <summary>Báo cáo sử dụng vật tư theo đoàn khám</summary>
        [HttpGet("by-medical-group/{groupId}")]
        [AuthorizePermission("Kho.Reports")]
        public async Task<IActionResult> GetReportByMedicalGroup(int groupId)
        {
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null) return NotFound("Không tìm thấy đoàn khám");

            var movements = await _context.StockMovements
                .Where(m => m.MedicalGroupId == groupId && m.MovementType == "OUT")
                .Include(m => m.SupplyItem)
                .ToListAsync();

            var report = movements.Select(m => new
            {
                m.SupplyId,
                ItemName = m.SupplyItem?.ItemName ?? m.ItemName,
                m.Quantity,
                m.Unit,
                m.UnitPrice,
                TotalValue = m.TotalValue,
                m.Note,
                m.MovementDate
            });

            var summary = new
            {
                groupId,
                group.GroupName,
                group.ExamDate,
                TotalItemsUsed = movements.Count,
                TotalQuantity = movements.Sum(m => m.Quantity),
                TotalValue = movements.Sum(m => m.TotalValue)
            };

            return Ok(new { summary, items = report });
        }

        /// <summary>Export báo cáo nhập/xuất kho (dữ liệu raw để frontend tạo Excel/PDF)</summary>
        [HttpGet("export")]
        [AuthorizePermission("Kho.Export")]
        public async Task<IActionResult> ExportReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string type = "movements")
        {
            object data;
            string filename;

            if (type == "movements")
            {
                var movements = await _context.StockMovements
                    .Where(m => m.MovementDate.Date >= from.Date &&
                               m.MovementDate.Date <= to.Date)
                    .Include(m => m.SupplyItem)
                    .OrderByDescending(m => m.MovementDate)
                    .Select(m => new
                    {
                        Ngay = m.MovementDate.ToString("dd/MM/yyyy"),
                        Loai = m.MovementType == "IN" ? "Nhập" : "Xuất",
                        TenHang = m.ItemName,
                        DonVi = m.Unit,
                        SoLuong = m.Quantity,
                        DonGia = m.UnitPrice,
                        ThanhTien = m.TotalValue,
                        LyDo = m.Note,
                        MaDoan = m.MedicalGroupId
                    })
                    .ToListAsync();

                data = movements;
                filename = $"BaoCao_NhapXuat_{from:yyyyMMdd}_{to:yyyyMMdd}.json";
            }
            else
            {
                // Stock report
                var items = await _context.SupplyItems.ToListAsync();

                var stockData = new List<object>();
                foreach (var item in items)
                {
                    var inbound = await _context.StockMovements
                        .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "IN")
                        .SumAsync(m => (int?)m.Quantity) ?? 0;

                    var outbound = await _context.StockMovements
                        .Where(m => m.SupplyId == item.SupplyId && m.MovementType == "OUT")
                        .SumAsync(m => (int?)m.Quantity) ?? 0;

                    stockData.Add(new
                    {
                        TenHang = item.ItemName,
                        NhomHang = item.Category,
                        DonVi = item.Unit,
                        TonDau = inbound,
                        TongXuat = outbound,
                        TonHienTai = inbound - outbound,
                        TonToiThieu = item.MinStockLevel,
                        TrangThai = GetStockStatus(inbound - outbound, item.MinStockLevel, item.MinStockLevel)
                    });
                }

                data = stockData;
                filename = $"BaoCao_TonKho_{DateTime.Today:yyyyMMdd}.json";
            }

            return Ok(new { filename, type, from, to, data });
        }
    }
}
