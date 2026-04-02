using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services
{
    public interface IReportingService
    {
        Task<DashboardKpiDto> GetDashboardKpisAsync(DateTime? startDate, DateTime? endDate);
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime? startDate, DateTime? endDate);
        Task<List<StaffEfficiencyDto>> GetStaffEfficiencyAsync(DateTime? startDate, DateTime? endDate);
        Task<List<InventoryAlertDto>> GetInventoryAlertsAsync();
    }

    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _context;

        public ReportingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardKpiDto> GetDashboardKpisAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            // 1. Tổng lượt khám (Expected từ Hợp đồng)
            var totalPatients = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
                .SumAsync(c => (int?)c.ExpectedQuantity) ?? 0;

            // 2. Doanh thu (Tổng giá trị Hợp đồng)
            var totalRevenue = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
                .SumAsync(c => (decimal?)c.TotalAmount) ?? 0;

            // 3. Chi phí nhân sự (Từ GroupStaffDetail)
            var staffCost = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .SumAsync(g => (decimal?)g.CalculatedSalary) ?? 0;
            
            // 4. Chi phí vật tư (Từ SupplyInventoryDetail - EXPORT)
            var supplyCost = await _context.SupplyInventoryDetails
                .Include(d => d.Voucher)
                .Where(d => d.Voucher.Type == "EXPORT" && d.Voucher.CreateDate >= start && d.Voucher.CreateDate <= end)
                .SumAsync(d => (decimal?)(d.Quantity * d.Price)) ?? 0;

            var netProfit = totalRevenue - staffCost - supplyCost;

            // 5. Tỷ lệ hoàn thành (Số đoàn đã Finish / Tổng số đoàn)
            var totalGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .CountAsync();
            var finishedGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.Status == "Finished")
                .CountAsync();
            
            var completionRate = totalGroups > 0 ? (double)finishedGroups / totalGroups * 100 : 0;

            // 6. Cảnh báo khẩn (Vật tư < MinStock + Hợp đồng Pending quá 7 ngày)
            var lowStockCount = await _context.Supplies.CountAsync(s => s.TotalStock < s.MinStockLevel);
            var overdueDate = DateTime.Now.AddDays(-7);
            var overdueContracts = await _context.Contracts.CountAsync(c => c.Status == "Pending" && c.SigningDate <= overdueDate);

            // 7. Biểu đồ doanh thu xu hướng (Theo tháng)
            var trendStart = start.AddMonths(-5);
            var revenueTrendRaw = await _context.Contracts
                .Where(c => c.SigningDate >= trendStart && c.Status != "Rejected")
                .GroupBy(c => new { c.SigningDate.Year, c.SigningDate.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Total = g.Sum(c => c.TotalAmount) 
                })
                .ToListAsync();

            var revenueTrend = revenueTrendRaw
                .Select(x => new ChartPointDto
                {
                    Label = $"{x.Month}/{x.Year}", // Định dạng chuỗi trong In-Memory sau khi ToListAsync
                    Value = x.Total
                })
                .OrderBy(x => x.Label)
                .ToList();

            return new DashboardKpiDto
            {
                TotalPatients = totalPatients,
                TotalRevenue = totalRevenue,
                NetProfit = netProfit,
                CompletionRate = Math.Round(completionRate, 1),
                ActiveGroupsCount = await _context.MedicalGroups.CountAsync(g => g.Status == "Open"),
                CriticalAlertsCount = lowStockCount + overdueContracts,
                RevenueTrend = revenueTrend
            };
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var revenue = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
                .SumAsync(c => (decimal?)c.TotalAmount) ?? 0;

            var staffCost = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .SumAsync(g => (decimal?)g.CalculatedSalary) ?? 0;

            var supplyCost = await _context.SupplyInventoryDetails
                .Include(d => d.Voucher)
                .Where(d => d.Voucher.Type == "EXPORT" && d.Voucher.CreateDate >= start && d.Voucher.CreateDate <= end)
                .SumAsync(d => (decimal?)(d.Quantity * d.Price)) ?? 0;

            var topContracts = await _context.Contracts
                .Include(c => c.Company)
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
                .OrderByDescending(c => c.TotalAmount)
                .Take(5)
                .Select(c => new ContractRevenueDto
                {
                    CompanyName = c.Company != null ? (c.Company.ShortName ?? c.Company.CompanyName) : "N/A",
                    Amount = c.TotalAmount,
                    Quantity = c.ExpectedQuantity
                })
                .ToListAsync();

            return new FinancialReportDto
            {
                Revenue = revenue,
                StaffCost = staffCost,
                SupplyCost = supplyCost,
                OtherCost = revenue * 0.05m, 
                Margin = revenue > 0 ? (revenue - staffCost - supplyCost) / revenue * 100 : 0,
                TopContracts = topContracts
            };
        }

        public async Task<List<StaffEfficiencyDto>> GetStaffEfficiencyAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var rawData = await _context.GroupStaffDetails
                .Include(d => d.Staff)
                .Where(d => d.ExamDate >= start && d.ExamDate <= end)
                .ToListAsync();

            var staffPerformance = rawData
                .GroupBy(d => new { d.StaffId, Name = d.Staff?.FullName ?? "Unknown", Type = d.Staff?.StaffType ?? "N/A" })
                .Select(g => new StaffEfficiencyDto
                {
                    StaffName = g.Key.Name,
                    Role = g.Key.Type,
                    TotalGroups = g.Count(),
                    DaysWorked = g.Select(d => d.ExamDate.Date).Distinct().Count(),
                    TotalSalary = g.Sum(d => d.CalculatedSalary),
                    EfficiencyScore = 0
                })
                .OrderByDescending(s => s.TotalGroups)
                .ToList();

            return staffPerformance;
        }

        public async Task<List<InventoryAlertDto>> GetInventoryAlertsAsync()
        {
            var supplies = await _context.Supplies
                .Where(s => s.TotalStock < s.MinStockLevel)
                .Select(s => new InventoryAlertDto
                {
                    SupplyId = s.SupplyId,
                    SupplyName = s.SupplyName,
                    CurrentStock = s.TotalStock,
                    MinStockLevel = s.MinStockLevel,
                    Status = s.TotalStock == 0 ? "OutOfStock" : (s.TotalStock < s.MinStockLevel / 2 ? "Critical" : "Low"),
                    DaysUntilExpired = s.ExpirationDate.HasValue ? (s.ExpirationDate.Value - DateTime.Now).Days : 999
                })
                .ToListAsync();

            return supplies;
        }
    }
}
