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

            var completedRecords = await _context.MedicalRecords
                .Where(r => r.Status == "COMPLETED"
                         && r.CheckInAt >= start && r.CheckInAt <= end
                         && r.MedicalGroup != null
                         && r.MedicalGroup.HealthContract != null)
                .Select(r => new { r.MedicalGroup!.HealthContractId, r.MedicalGroup!.HealthContract!.UnitPrice })
                .ToListAsync();
            
            var packageRevenue = completedRecords.Sum(r => r.UnitPrice);

            // 1.1 Doanh thu phát sinh từ hợp đồng trong kỳ
            var extraRevenue = await _context.Contracts
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != "Rejected")
                .SumAsync(c => (decimal?)c.ExtraServiceRevenue) ?? 0;

            var totalRevenue = packageRevenue + extraRevenue;

            // 2. Chi phí nhân sự (Từ GroupStaffDetail)
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

            var totalCost = staffCost + materialCost + overheadCost;
            var netProfit = totalRevenue - totalCost;
            
            // 4. HR Performance: Dựa trên tỷ lệ nhân viên đã tham gia trên tổng số lượt điều động
            var totalStaffAssignments = await _context.GroupStaffDetails.CountAsync(g => g.ExamDate >= start && g.ExamDate <= end);
            var actualJoinedStaff = await _context.GroupStaffDetails.CountAsync(g => g.ExamDate >= start && g.ExamDate <= end && g.WorkStatus == "Joined");
            var hrPerformance = totalStaffAssignments > 0 ? (double)actualJoinedStaff / totalStaffAssignments * 100 : 0;

            // 5. Material Deviation: Chênh lệch giữa chi phí thực tế và chi phí định mức (Giả định 10% doanh thu là định mức)
            var expectedMaterialCost = totalRevenue * 0.10m;
            var materialDeviation = materialCost - expectedMaterialCost;

            var totalGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end)
                .CountAsync();
            var finishedGroups = await _context.MedicalGroups
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.Status == "Finished")
                .CountAsync();
            var completionRate = totalGroups > 0 ? (double)finishedGroups / totalGroups * 100 : 0;

            // 3. Xu hướng doanh thu (6 tháng gần nhất) - Tính trên Doanh thu thực tế (Billed)
            var trendStart = start.AddMonths(-5);
            
            // Lấy doanh thu từ khám (Records)
            var recordRevenueRaw = await _context.MedicalRecords
                .Where(r => r.CheckInAt >= trendStart && r.CheckInAt <= end && r.Status == "COMPLETED")
                .GroupBy(r => new { r.CheckInAt!.Value.Year, r.CheckInAt!.Value.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Type = "Record", Total = g.Sum(r => r.MedicalGroup!.HealthContract!.UnitPrice) })
                .ToListAsync();

            // Lấy doanh thu phát sinh (Contract Extras)
            var contractExtraRaw = await _context.Contracts
                .Where(c => c.EndDate >= trendStart && c.EndDate <= end && c.Status != "Rejected")
                .GroupBy(c => new { c.EndDate.Year, c.EndDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Type = "Extra", Total = g.Sum(c => c.ExtraServiceRevenue) })
                .ToListAsync();

            // Gộp và nhóm lại theo tháng
            var revenueTrend = recordRevenueRaw.Concat(contractExtraRaw)
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new ChartPointDto 
                { 
                    Label = $"{g.Key.Month}/{g.Key.Year}", 
                    Value = g.Sum(x => x.Total) 
                })
                .OrderBy(x => {
                    var parts = x.Label.Split('/');
                    return int.Parse(parts[1]) * 100 + int.Parse(parts[0]);
                })
                .ToList();

            if (!revenueTrend.Any())
            {
                revenueTrend.Add(new ChartPointDto { Label = $"{DateTime.Now.Month}/{DateTime.Now.Year}", Value = 0 });
            }

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
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != "Rejected")
                .SumAsync(c => (decimal?)c.ExtraServiceRevenue) ?? 0;

            var revenue = packageRevenue + extraRevenue;

            // Doanh thu kế hoạch (để tính variance)
            var plannedRevenue = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
                .SumAsync(c => (decimal?)c.TotalAmount) ?? 0;

            int actualQty = completedRevenueItems.Count;
            int plannedQty = await _context.Contracts
                .Where(c => c.SigningDate >= start && c.SigningDate <= end && c.Status != "Rejected")
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
                .Where(c => c.Status == "Pending" || c.Status == "Draft")
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
