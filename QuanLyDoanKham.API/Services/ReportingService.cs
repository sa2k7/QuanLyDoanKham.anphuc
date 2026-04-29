using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services
{
    public interface IReportingService
    {
        Task<DashboardKpiDto> GetDashboardKpisAsync(DateTime? startDate, DateTime? endDate);
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime? startDate, DateTime? endDate);
        Task<List<StaffEfficiencyDto>> GetStaffEfficiencyAsync(DateTime? startDate, DateTime? endDate);
        Task<OperationalSummaryDto> GetOperationalSummaryAsync(int? year, int? month);
        Task<List<StaffPayrollSummaryDto>> GetPayrollSummaryAsync(int? year, int? month);
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

            // 1. Doanh thu từ hợp đồng trong kỳ (tránh navigation property phức tạp)
            var contractRevenue = await _context.Contracts
                .Where(c => c.StartDate <= end && c.EndDate >= start && c.Status != ContractStatus.Rejected)
                .SumAsync(c => (decimal?)c.TotalAmount) ?? 0;

            var extraRevenue = await _context.Contracts
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != ContractStatus.Rejected)
                .SumAsync(c => (decimal?)c.ExtraServiceRevenue) ?? 0;

            var totalRevenue = contractRevenue + extraRevenue;

            // 2. Chi phí nhân sự
            var staffCost = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .SumAsync(g => (decimal?)g.CalculatedSalary) ?? 0;

            // Chi phí vật tư
            decimal supplyCostOut = await _context.Set<StockMovement>()
                .Where(sm => sm.MovementDate >= start && sm.MovementDate <= end && sm.MovementType == "OUT")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            decimal supplyCostIn = await _context.Set<StockMovement>()
                .Where(sm => sm.MovementDate >= start && sm.MovementDate <= end && sm.MovementType == "IN")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            decimal materialCost = Math.Max(0, supplyCostOut - supplyCostIn);

            // Chi phí chung
            decimal overheadCost = 0;
            try
            {
                overheadCost = await _context.Set<Overhead>()
                    .Where(o => o.IncurredAt >= start && o.IncurredAt <= end)
                    .SumAsync(o => (decimal?)o.Amount) ?? 0;
            }
            catch { /* Bảng Overhead có thể chưa có dữ liệu */ }

            var totalCost = staffCost + materialCost + overheadCost;
            var netProfit = totalRevenue - totalCost;

            // 3. HR Performance
            var totalStaffAssignments = await _context.GroupStaffDetails.CountAsync(g => g.ExamDate >= start && g.ExamDate <= end);
            var actualJoinedStaff = await _context.GroupStaffDetails.CountAsync(g => g.ExamDate >= start && g.ExamDate <= end && g.WorkStatus == "Joined");
            var hrPerformance = totalStaffAssignments > 0 ? (double)actualJoinedStaff / totalStaffAssignments * 100 : 0;

            // 4. Material Deviation
            var expectedMaterialCost = totalRevenue * 0.10m;
            var materialDeviation = materialCost - expectedMaterialCost;

            // 5. Completion Rate
            var totalGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .CountAsync();
            var finishedGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.Status == "Finished")
                .CountAsync();
            var completionRate = totalGroups > 0 ? (double)finishedGroups / totalGroups * 100 : 0;

            // 6. Xu hướng doanh thu (6 tháng gần nhất) - dùng hợp đồng thay vì MedicalRecords
            var trendStart = start.AddMonths(-5);
            var contractTrend = await _context.Contracts
                .Where(c => c.StartDate >= trendStart && c.StartDate <= end && c.Status != ContractStatus.Rejected)
                .GroupBy(c => new { c.StartDate.Year, c.StartDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(c => c.TotalAmount) })
                .ToListAsync();

            var revenueTrend = contractTrend
                .Select(x => new ChartPointDto
                {
                    Label = $"{x.Month}/{x.Year}",
                    Value = x.Total
                })
                .OrderBy(x => {
                    var parts = x.Label.Split('/');
                    return int.Parse(parts[1]) * 100 + int.Parse(parts[0]);
                })
                .ToList();

            if (!revenueTrend.Any())
                revenueTrend.Add(new ChartPointDto { Label = $"{DateTime.Now.Month}/{DateTime.Now.Year}", Value = 0 });

            return new DashboardKpiDto
            {
                TotalRevenue = totalRevenue,
                NetProfit = netProfit,
                CompletionRate = Math.Round(completionRate, 1),
                ActiveGroupsCount = await _context.MedicalGroups.CountAsync(g => g.Status == "Open"),
                HrPerformance = Math.Round(hrPerformance, 1),
                MaterialDeviation = materialDeviation,
                RevenueTrend = revenueTrend
            };
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            // Doanh thu thực tế: số ca COMPLETED × đơn giá + Extra Services
            var completedRevenueItems = await _context.MedicalRecords
                .Where(r => r.Status == "COMPLETED"
                         && r.CheckInAt >= start && r.CheckInAt <= end
                         && r.MedicalGroup != null
                         && r.MedicalGroup.HealthContract != null)
                .Select(r => r.MedicalGroup!.HealthContract!.UnitPrice)
                .ToListAsync();
            
            var packageRevenue = completedRevenueItems.Sum();
            
            var extraRevenue = await _context.Contracts
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != ContractStatus.Rejected)
                .SumAsync(c => (decimal?)c.ExtraServiceRevenue) ?? 0;

            var revenue = packageRevenue + extraRevenue;

            // Doanh thu kế hoạch (để tính variance)
            var plannedRevenue = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != ContractStatus.Rejected)
                .SumAsync(c => (decimal?)c.TotalAmount) ?? 0;

            int actualQty = completedRevenueItems.Count;
            int plannedQty = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != ContractStatus.Rejected)
                .SumAsync(c => (int?)c.ExpectedQuantity) ?? 0;

            var staffCost = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .SumAsync(g => (decimal?)g.CalculatedSalary) ?? 0;

            // Chi phí vật tư từ StockMovement (OUT - IN)
            decimal supplyCostOut = await _context.Set<StockMovement>()
                .Where(sm => sm.MovementDate >= start && sm.MovementDate <= end && sm.MovementType == "OUT")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            decimal supplyCostIn = await _context.Set<StockMovement>()
                .Where(sm => sm.MovementDate >= start && sm.MovementDate <= end && sm.MovementType == "IN")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            decimal materialCost = supplyCostOut - supplyCostIn;

            // Chi phí chung khác
            decimal overheadCost = await _context.Set<Overhead>()
                .Where(o => o.IncurredAt >= start && o.IncurredAt <= end)
                .SumAsync(o => (decimal?)o.Amount) ?? 0;



            var topContracts = await _context.Contracts
                .Include(c => c.Company)
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != ContractStatus.Rejected)
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
                PlannedRevenue = plannedRevenue,
                ActualQuantity = actualQty,
                PlannedQuantity = plannedQty,
                VarianceQuantity = actualQty - plannedQty,
                VarianceRevenue = revenue - plannedRevenue,
                StaffCost = staffCost,
                OtherCost = overheadCost,
                Margin = revenue > 0 ? (revenue - (staffCost + materialCost + overheadCost)) / revenue * 100 : 0,
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
                    TotalSalary = g.Sum(d => d.CalculatedSalary)
                })
                .OrderByDescending(s => s.TotalGroups)
                .ToList();

            return staffPerformance;
        }

        public async Task<OperationalSummaryDto> GetOperationalSummaryAsync(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;

            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var totalGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= startDate && g.ExamDate <= endDate)
                .CountAsync();

            var totalPatients = await _context.MedicalRecords
                .Where(r => r.CheckInAt >= startDate && r.CheckInAt <= endDate)
                .CountAsync();

            var totalStaffDeployed = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= startDate && g.ExamDate <= endDate && g.WorkStatus == "Joined")
                .CountAsync();

            var pendingContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.PendingApproval || c.Status == ContractStatus.Draft)
                .CountAsync();

            return new OperationalSummaryDto
            {
                TotalMedicalGroupsThisMonth = totalGroups,
                TotalPatientsThisMonth = totalPatients,
                TotalStaffDeployedThisMonth = totalStaffDeployed,
                PendingContractsCount = pendingContracts
            };
        }

        public async Task<List<StaffPayrollSummaryDto>> GetPayrollSummaryAsync(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;

            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var staffData = await _context.Staffs
                .Include(s => s.GroupStaffDetails)
                .Select(s => new StaffPayrollSummaryDto
                {
                    StaffId = s.StaffId,
                    StaffName = s.FullName,
                    JobTitle = s.JobTitle ?? s.StaffType,
                    BaseSalary = s.BaseSalary,
                    TotalShifts = s.GroupStaffDetails.Count(sh => sh.ExamDate >= startDate && sh.ExamDate <= endDate && sh.WorkStatus == "Joined" && sh.MedicalGroup.Status == "Locked"),
                    TotalDays = s.GroupStaffDetails.Where(sh => sh.ExamDate >= startDate && sh.ExamDate <= endDate && sh.WorkStatus == "Joined" && sh.MedicalGroup.Status == "Locked").Select(sh => sh.ExamDate.Date).Distinct().Count(),
                    TotalSalary = s.GroupStaffDetails.Where(sh => sh.ExamDate >= startDate && sh.ExamDate <= endDate && sh.WorkStatus == "Joined" && sh.MedicalGroup.Status == "Locked").Sum(sh => sh.CalculatedSalary)
                })
                .ToListAsync();

            return staffData.OrderByDescending(x => x.TotalSalary).ToList();
        }

    }
}
