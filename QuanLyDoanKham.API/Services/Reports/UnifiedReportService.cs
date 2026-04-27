using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.Reports
{
    /// <summary>Service thống kê tổng hợp - Kết hợp Analytics và Reporting</summary>
    public interface IUnifiedReportService
    {
        Task<UnifiedDashboardDto> GetUnifiedDashboardAsync(DateTime? from, DateTime? to);
        Task<RevenueChartDto> GetRevenueChartAsync(DateTime from, DateTime to);
        Task<ContractChartDto> GetContractChartAsync(DateTime from, DateTime to);
        Task<MedicalGroupChartDto> GetMedicalGroupChartAsync(DateTime from, DateTime to);
        Task<TopStatsDto> GetTopStatsAsync(DateTime? from, DateTime? to);
        Task<List<LowStockItemDto>> GetLowStockItemsAsync();
    }

    public class UnifiedReportService : IUnifiedReportService
    {
        private readonly ApplicationDbContext _context;

        public UnifiedReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UnifiedDashboardDto> GetUnifiedDashboardAsync(DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.Now.AddMonths(-1);
            var endDate = to ?? DateTime.Now;

            // Tổng quan
            var totalContracts = await _context.Contracts.CountAsync();
            var totalGroups = await _context.MedicalGroups.CountAsync();
            var totalStaff = await _context.Staffs.CountAsync(s => s.IsActive);
            var totalPatients = await _context.Patients.CountAsync();
            var totalSupplies = await _context.SupplyItems.CountAsync();

            // Doanh thu từ hợp đồng
            var contracts = await _context.Contracts
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToListAsync();
            
            var totalRevenue = contracts.Sum(c => c.TotalAmount);
            var activeContracts = contracts.Count(c => c.Status == ContractStatus.Active);
            var finishedContracts = contracts.Count(c => c.Status == ContractStatus.Finished);
            var pendingContracts = contracts.Count(c => c.Status == ContractStatus.Draft || c.Status == ContractStatus.PendingApproval);

            // Doanh thu thực tế từ MedicalRecords đã hoàn thành (qua MedicalGroup.HealthContract.UnitPrice)
            var actualRevenue = await _context.MedicalRecords
                .Include(mr => mr.MedicalGroup).ThenInclude(mg => mg!.HealthContract)
                .Where(mr => mr.MedicalGroup != null && mr.MedicalGroup.ExamDate >= startDate && mr.MedicalGroup.ExamDate <= endDate)
                .Where(mr => mr.Status == "Completed" || mr.Status == "Finished")
                .SumAsync(mr => (decimal?)(mr.MedicalGroup!.HealthContract != null ? mr.MedicalGroup.HealthContract.UnitPrice : 0)) ?? 0;

            // Lấy cost từ CostSnapshot nếu có, nếu không thì estimate
            var costSnapshots = await _context.Set<Models.CostSnapshot>()
                .Where(cs => cs.CreatedAt >= startDate && cs.CreatedAt <= endDate)
                .ToListAsync();

            var totalCost = costSnapshots.Any() 
                ? costSnapshots.Sum(cs => cs.TotalCost) 
                : actualRevenue * 0.6m; // Estimate 60% cost nếu chưa có snapshot
            
            var grossProfit = actualRevenue - totalCost;

            // Đoàn khám trong kỳ
            var groupsInPeriod = await _context.MedicalGroups
                .CountAsync(g => g.ExamDate >= startDate && g.ExamDate <= endDate);

            var completedGroups = await _context.MedicalGroups
                .CountAsync(g => (g.Status == "Completed" || g.Status == "Finished") && g.ExamDate >= startDate && g.ExamDate <= endDate);

            // Nhân sự đã phân công trong kỳ
            var staffAssigned = await _context.GroupStaffDetails
                .Where(gsd => gsd.AssignedAt >= startDate && gsd.AssignedAt <= endDate)
                .Select(gsd => gsd.StaffId)
                .Distinct()
                .CountAsync();

            return new UnifiedDashboardDto
            {
                Period = new PeriodDto { From = startDate, To = endDate },
                Overview = new OverviewDto
                {
                    TotalContracts = totalContracts,
                    TotalMedicalGroups = totalGroups,
                    TotalStaff = totalStaff,
                    TotalPatients = totalPatients,
                    TotalSupplies = totalSupplies
                },
                Financial = new FinancialDto
                {
                    TotalRevenue = totalRevenue,
                    ActualRevenue = actualRevenue,
                    TotalCost = totalCost,
                    GrossProfit = grossProfit,
                    ProfitMargin = actualRevenue > 0 ? (grossProfit / actualRevenue) * 100 : 0
                },
                Contracts = new ContractStatsDto
                {
                    Total = totalContracts,
                    Active = activeContracts,
                    Finished = finishedContracts,
                    Pending = pendingContracts
                },
                MedicalGroups = new MedicalGroupStatsDto
                {
                    Total = totalGroups,
                    InPeriod = groupsInPeriod,
                    Completed = completedGroups
                },
                Staff = new StaffStatsDto
                {
                    TotalActive = totalStaff,
                    AssignedInPeriod = staffAssigned
                }
            };
        }

        public async Task<RevenueChartDto> GetRevenueChartAsync(DateTime from, DateTime to)
        {
            var months = new List<MonthlyRevenueDto>();
            var current = new DateTime(from.Year, from.Month, 1);
            var end = new DateTime(to.Year, to.Month, 1);

            while (current <= end)
            {
                var monthStart = current;
                var monthEnd = current.AddMonths(1).AddDays(-1);

                // Revenue từ MedicalRecords đã hoàn thành (qua MedicalGroup.HealthContract.UnitPrice)
                var revenue = await _context.MedicalRecords
                    .Include(mr => mr.MedicalGroup).ThenInclude(mg => mg!.HealthContract)
                    .Where(mr => mr.MedicalGroup != null && mr.MedicalGroup.ExamDate >= monthStart && mr.MedicalGroup.ExamDate <= monthEnd)
                    .Where(mr => mr.Status == "Completed" || mr.Status == "Finished")
                    .SumAsync(mr => (decimal?)(mr.MedicalGroup!.HealthContract != null ? mr.MedicalGroup.HealthContract.UnitPrice : 0)) ?? 0;

                // Cost từ CostSnapshot hoặc estimate
                var costSnapshots = await _context.Set<Models.CostSnapshot>()
                    .Where(cs => cs.CreatedAt >= monthStart && cs.CreatedAt <= monthEnd)
                    .ToListAsync();
                
                var cost = costSnapshots.Any() 
                    ? costSnapshots.Sum(cs => cs.TotalCost) 
                    : revenue * 0.6m;

                months.Add(new MonthlyRevenueDto
                {
                    Month = current.ToString("MM/yyyy"),
                    Revenue = revenue,
                    Cost = cost,
                    Profit = revenue - cost
                });

                current = current.AddMonths(1);
            }

            return new RevenueChartDto { Data = months };
        }

        public async Task<ContractChartDto> GetContractChartAsync(DateTime from, DateTime to)
        {
            var contracts = await _context.Contracts
                .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                .ToListAsync();
            
            var byStatus = contracts
                .GroupBy(c => c.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Value = g.Sum(c => c.TotalAmount)
                })
                .ToList();

            var byMonth = await _context.Contracts
                .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new MonthCountDto
                {
                    Month = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Count = g.Count(),
                    Value = g.Sum(c => c.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return new ContractChartDto
            {
                ByStatus = byStatus,
                ByMonth = byMonth
            };
        }

        public async Task<MedicalGroupChartDto> GetMedicalGroupChartAsync(DateTime from, DateTime to)
        {
            var byStatus = await _context.MedicalGroups
                .Where(g => g.ExamDate >= from && g.ExamDate <= to)
                .GroupBy(g => g.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            var byMonth = await _context.MedicalGroups
                .Where(g => g.ExamDate >= from && g.ExamDate <= to)
                .GroupBy(g => new { g.ExamDate.Year, g.ExamDate.Month })
                .Select(g => new MonthCountDto
                {
                    Month = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return new MedicalGroupChartDto
            {
                ByStatus = byStatus,
                ByMonth = byMonth
            };
        }

        public async Task<TopStatsDto> GetTopStatsAsync(DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.Now.AddMonths(-1);
            var endDate = to ?? DateTime.Now;

            // Top công ty theo doanh thu
            var topCompanies = await _context.Contracts
                .Include(c => c.Company)
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToListAsync();

            var topCompaniesGrouped = topCompanies
                .GroupBy(c => new { c.CompanyId, CompanyName = c.Company?.CompanyName ?? "Không xác định" })
                .Select(g => new TopCompanyDto
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    ContractCount = g.Count(),
                    TotalRevenue = g.Sum(c => c.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            // Top nhân sự theo số ca khám
            var staffAssignments = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.AssignedAt >= startDate && gsd.AssignedAt <= endDate)
                .ToListAsync();

            var topStaff = staffAssignments
                .GroupBy(gsd => new { gsd.StaffId, FullName = gsd.Staff?.FullName ?? "Không xác định" })
                .Select(g => new TopStaffDto
                {
                    StaffId = g.Key.StaffId,
                    StaffName = g.Key.FullName,
                    AssignmentCount = g.Count()
                })
                .OrderByDescending(x => x.AssignmentCount)
                .Take(5)
                .ToList();

            // Top đoàn khám theo số bệnh nhân
            var groupsWithRecords = await _context.MedicalGroups
                .Include(g => g.MedicalRecords)
                .Where(g => g.ExamDate >= startDate && g.ExamDate <= endDate)
                .ToListAsync();

            var topGroups = groupsWithRecords
                .Select(g => new TopMedicalGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    PatientCount = g.MedicalRecords?.Count ?? 0
                })
                .OrderByDescending(x => x.PatientCount)
                .Take(5)
                .ToList();

            return new TopStatsDto
            {
                TopCompanies = topCompaniesGrouped,
                TopStaff = topStaff,
                TopMedicalGroups = topGroups
            };
        }

        public async Task<List<LowStockItemDto>> GetLowStockItemsAsync()
        {
            var items = await _context.SupplyItems
                .Where(s => s.CurrentStock <= s.MinStockLevel)
                .Select(s => new LowStockItemDto
                {
                    SupplyId = s.SupplyId,
                    SupplyName = s.ItemName,
                    CurrentStock = s.CurrentStock,
                    MinStockLevel = s.MinStockLevel,
                    Unit = s.Unit,
                    Category = s.Category
                })
                .OrderBy(s => s.CurrentStock)
                .ToListAsync();

            return items;
        }
    }

    // DTOs
    public class UnifiedDashboardDto
    {
        public PeriodDto Period { get; set; } = null!;
        public OverviewDto Overview { get; set; } = null!;
        public FinancialDto Financial { get; set; } = null!;
        public ContractStatsDto Contracts { get; set; } = null!;
        public MedicalGroupStatsDto MedicalGroups { get; set; } = null!;
        public StaffStatsDto Staff { get; set; } = null!;
    }

    public class PeriodDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class OverviewDto
    {
        public int TotalContracts { get; set; }
        public int TotalMedicalGroups { get; set; }
        public int TotalStaff { get; set; }
        public int TotalPatients { get; set; }
        public int TotalSupplies { get; set; }
    }

    public class FinancialDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal ActualRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class ContractStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Finished { get; set; }
        public int Pending { get; set; }
    }

    public class MedicalGroupStatsDto
    {
        public int Total { get; set; }
        public int InPeriod { get; set; }
        public int Completed { get; set; }
    }

    public class StaffStatsDto
    {
        public int TotalActive { get; set; }
        public int AssignedInPeriod { get; set; }
    }

    public class RevenueChartDto
    {
        public List<MonthlyRevenueDto> Data { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    public class ContractChartDto
    {
        public List<StatusCountDto> ByStatus { get; set; } = new();
        public List<MonthCountDto> ByMonth { get; set; } = new();
    }

    public class MedicalGroupChartDto
    {
        public List<StatusCountDto> ByStatus { get; set; } = new();
        public List<MonthCountDto> ByMonth { get; set; } = new();
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class MonthCountDto
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class TopStatsDto
    {
        public List<TopCompanyDto> TopCompanies { get; set; } = new();
        public List<TopStaffDto> TopStaff { get; set; } = new();
        public List<TopMedicalGroupDto> TopMedicalGroups { get; set; } = new();
    }

    public class TopCompanyDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public int ContractCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopStaffDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public int AssignmentCount { get; set; }
    }

    public class TopMedicalGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public DateTime ExamDate { get; set; }
        public int PatientCount { get; set; }
    }

    public class LowStockItemDto
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public string Unit { get; set; } = null!;
        public string Category { get; set; } = null!;
    }
}
