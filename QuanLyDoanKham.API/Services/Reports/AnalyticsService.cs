using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.Reports
{
    /// <summary>Service báo cáo và phân tích dữ liệu tổng hợp</summary>
    public interface IAnalyticsService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? from = null, DateTime? to = null);
        Task<ContractAnalytics> GetContractAnalyticsAsync(DateTime from, DateTime to);
        Task<MedicalGroupAnalytics> GetMedicalGroupAnalyticsAsync(DateTime from, DateTime to);
        Task<StaffAnalytics> GetStaffAnalyticsAsync(DateTime from, DateTime to);
        Task<FinancialAnalytics> GetFinancialAnalyticsAsync(DateTime from, DateTime to);
        Task<List<TopCompanyByRevenue>> GetTopCompaniesAsync(int top = 10, DateTime? from = null, DateTime? to = null);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Tổng quan dashboard</summary>
        public async Task<DashboardSummary> GetDashboardSummaryAsync(DateTime? from = null, DateTime? to = null)
        {
            var startDate = from?.Date ?? DateTime.Today.AddMonths(-1);
            var endDate = to?.Date ?? DateTime.Today;

            // Số liệu hợp đồng
            var totalContracts = await _context.Contracts.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active || c.Status == ContractStatus.Approved);
            var newContractsInPeriod = await _context.Contracts
                .CountAsync(c => c.CreatedAt.Date >= startDate &&
                                c.CreatedAt.Date <= endDate);

            // Số liệu đoàn khám
            var totalGroups = await _context.MedicalGroups.CountAsync();
            var groupsInPeriod = await _context.MedicalGroups
                .CountAsync(g => g.ExamDate.Date >= startDate && g.ExamDate.Date <= endDate);
            var completedGroups = await _context.MedicalGroups
                .CountAsync(g => g.Status == "Finished" || g.Status == "Locked");

            // Số liệu bệnh nhân
            var totalPatients = await _context.Patients.CountAsync();
            var patientsInPeriod = await _context.MedicalRecords
                .CountAsync(mr => mr.CreatedAt.Date >= startDate && mr.CreatedAt.Date <= endDate);

            // Số liệu nhân sự
            var totalStaff = await _context.Staffs.CountAsync(s => s.IsActive);
            var staffAssignedInPeriod = await _context.GroupStaffDetails
                .Select(gsd => gsd.StaffId)
                .Distinct()
                .CountAsync();

            // Doanh thu (từ CostSnapshots)
            var revenueData = await _context.Set<Models.CostSnapshot>()
                .Where(cs => cs.CreatedAt.Date >= startDate && cs.CreatedAt.Date <= endDate)
                .GroupBy(cs => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(cs => cs.Revenue),
                    TotalCost = g.Sum(cs => cs.TotalCost),
                    TotalProfit = g.Sum(cs => cs.GrossProfit)
                })
                .FirstOrDefaultAsync();

            return new DashboardSummary
            {
                Period = new PeriodInfo { From = startDate, To = endDate },
                Contracts = new ContractsInfo
                {
                    Total = totalContracts,
                    Active = activeContracts,
                    NewInPeriod = newContractsInPeriod
                },
                MedicalGroups = new MedicalGroupsInfo
                {
                    Total = totalGroups,
                    InPeriod = groupsInPeriod,
                    Completed = completedGroups
                },
                Patients = new PatientsInfo
                {
                    Total = totalPatients,
                    NewInPeriod = patientsInPeriod
                },
                Staff = new StaffInfo
                {
                    TotalActive = totalStaff,
                    AssignedInPeriod = staffAssignedInPeriod
                },
                Financial = new FinancialInfo
                {
                    Revenue = revenueData?.TotalRevenue ?? 0,
                    Cost = revenueData?.TotalCost ?? 0,
                    Profit = revenueData?.TotalProfit ?? 0,
                    ProfitMargin = revenueData?.TotalRevenue > 0
                        ? (revenueData.TotalProfit / revenueData.TotalRevenue) * 100
                        : 0
                }
            };
        }

        /// <summary>Phân tích hợp đồng</summary>
        public async Task<ContractAnalytics> GetContractAnalyticsAsync(DateTime from, DateTime to)
        {
            var contracts = await _context.Contracts
                .Include(c => c.Company)
                .Include(c => c.MedicalGroups)
                .Where(c => c.CreatedAt.Date >= from &&
                           c.CreatedAt.Date <= to)
                .ToListAsync();

            var byStatus = contracts.GroupBy(c => c.Status)
                .Select(g => new StatusStat { Status = g.Key.ToString(), Count = g.Count() })
                .ToList();

            var byMonth = contracts.GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new MonthStat { Month = g.Key, Count = g.Count(), Value = g.Sum(c => c.TotalAmount) })
                .OrderBy(x => x.Month)
                .ToList();

            return new ContractAnalytics
            {
                Period = new { From = from, To = to },
                TotalContracts = contracts.Count,
                TotalRevenue = contracts.Sum(c => c.TotalAmount),
                AverageContractValue = contracts.Any() ? contracts.Average(c => c.TotalAmount) : 0,
                ByStatus = byStatus,
                ByMonth = byMonth,
                TopCompanies = contracts.Where(c => c.Company != null)
                    .GroupBy(c => c.Company!.CompanyName)
                    .Select(g => new TopCompanyByRevenue
                    {
                        CompanyName = g.Key,
                        ContractCount = g.Count(),
                        TotalRevenue = g.Sum(c => c.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(5)
                    .ToList()
            };
        }

        /// <summary>Phân tích đoàn khám</summary>
        public async Task<MedicalGroupAnalytics> GetMedicalGroupAnalyticsAsync(DateTime from, DateTime to)
        {
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .Where(g => g.ExamDate.Date >= from && g.ExamDate.Date <= to)
                .ToListAsync();

            var groupIds = groups.Select(g => g.GroupId).ToList();

            // Tính số bệnh nhân cho mỗi đoàn
            var patientCounts = await _context.MedicalRecords
                .Where(mr => groupIds.Contains(mr.GroupId))
                .GroupBy(mr => mr.GroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToListAsync();

            var groupStats = groups.Select(g => new
            {
                g.GroupId,
                g.GroupName,
                g.ExamDate,
                g.Status,
                CompanyName = g.HealthContract?.Company?.CompanyName,
                PatientCount = patientCounts.FirstOrDefault(pc => pc.GroupId == g.GroupId)?.Count ?? 0
            }).ToList();

            var byStatus = groupStats.GroupBy(g => g.Status)
                .Select(g => new GroupStatusStat { Status = g.Key, Count = g.Count() })
                .ToList();

            var byMonth = groupStats.GroupBy(g => g.ExamDate.ToString("yyyy-MM"))
                .Select(g => new GroupMonthStat { Month = g.Key, Count = g.Count(), Patients = g.Sum(x => x.PatientCount) })
                .OrderBy(x => x.Month)
                .ToList();

            return new MedicalGroupAnalytics
            {
                Period = new { From = from, To = to },
                TotalGroups = groups.Count,
                TotalPatients = patientCounts.Sum(pc => pc.Count),
                AveragePatientsPerGroup = groups.Any() ? (double)patientCounts.Sum(pc => pc.Count) / groups.Count : 0,
                ByStatus = byStatus,
                ByMonth = byMonth,
                TopGroupsByPatients = groupStats
                    .OrderByDescending(g => g.PatientCount)
                    .Take(5)
                    .Select(g => new TopMedicalGroup
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        CompanyName = g.CompanyName,
                        PatientCount = g.PatientCount,
                        ExamDate = g.ExamDate
                    })
                    .ToList()
            };
        }

        /// <summary>Phân tích nhân sự</summary>
        public async Task<StaffAnalytics> GetStaffAnalyticsAsync(DateTime from, DateTime to)
        {
            var assignments = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.ExamDate.Date >= from && gsd.ExamDate.Date <= to)
                .ToListAsync();

            var byStaffType = assignments
                .GroupBy(gsd => gsd.Staff?.StaffType ?? "Unknown")
                .Select(g => new StaffTypeStat
                {
                    StaffType = g.Key,
                    Count = g.Count(),
                    WorkingDays = g.Sum(x => x.ShiftType),
                    TotalSalary = g.Sum(x => x.CalculatedSalary)
                })
                .ToList();

            var byPosition = assignments
                .GroupBy(gsd => gsd.WorkPosition ?? "Unknown")
                .Select(g => new PositionStat
                {
                    Position = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var topStaff = assignments
                .GroupBy(gsd => new { gsd.StaffId, Name = gsd.Staff?.FullName ?? "Unknown" })
                .Select(g => new TopStaff
                {
                    StaffId = g.Key.StaffId,
                    StaffName = g.Key.Name,
                    AssignmentCount = g.Count(),
                    WorkingDays = g.Sum(x => x.ShiftType),
                    TotalSalary = g.Sum(x => x.CalculatedSalary)
                })
                .OrderByDescending(x => x.AssignmentCount)
                .Take(10)
                .ToList();

            return new StaffAnalytics
            {
                Period = new { From = from, To = to },
                TotalAssignments = assignments.Count,
                TotalWorkingDays = assignments.Sum(a => a.ShiftType),
                TotalSalaryPaid = assignments.Sum(a => a.CalculatedSalary),
                ByStaffType = byStaffType,
                ByPosition = byPosition,
                TopStaff = topStaff
            };
        }

        /// <summary>Phân tích tài chính</summary>
        public async Task<FinancialAnalytics> GetFinancialAnalyticsAsync(DateTime from, DateTime to)
        {
            // Lấy dữ liệu từ CostSnapshots
            var snapshots = await _context.Set<Models.CostSnapshot>()
                .Where(cs => cs.CreatedAt.Date >= from && cs.CreatedAt.Date <= to)
                .ToListAsync();

            var byMonth = snapshots
                .GroupBy(cs => cs.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new MonthlyFinancial
                {
                    Month = g.Key,
                    Revenue = g.Sum(cs => cs.Revenue),
                    Cost = g.Sum(cs => cs.TotalCost),
                    Profit = g.Sum(cs => cs.GrossProfit),
                    ContractCount = g.Select(cs => cs.HealthContractId).Distinct().Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var costBreakdown = new CostBreakdown
            {
                StaffCosts = snapshots.Sum(cs => cs.LaborCost),
                SupplyCosts = snapshots.Sum(cs => cs.SupplyCost),
                FixedCosts = 0, // CostSnapshot không có FixedCost
                OverheadCosts = snapshots.Sum(cs => cs.OverheadCost)
            };

            return new FinancialAnalytics
            {
                Period = new { From = from, To = to },
                TotalRevenue = snapshots.Sum(cs => cs.Revenue),
                TotalCost = snapshots.Sum(cs => cs.TotalCost),
                GrossProfit = snapshots.Sum(cs => cs.GrossProfit),
                NetProfit = snapshots.Sum(cs => cs.GrossProfit), // TODO: Subtract taxes
                ProfitMargin = snapshots.Any() && snapshots.Sum(cs => cs.Revenue) > 0
                    ? (snapshots.Sum(cs => cs.GrossProfit) / snapshots.Sum(cs => cs.Revenue)) * 100
                    : 0,
                ByMonth = byMonth,
                CostBreakdown = costBreakdown
            };
        }

        /// <summary>Top công ty theo doanh thu</summary>
        public async Task<List<TopCompanyByRevenue>> GetTopCompaniesAsync(int top = 10, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Contracts
                .Include(c => c.Company)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(c => c.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(c => c.CreatedAt <= to.Value);

            var companies = await query
                .Where(c => c.Company != null)
                .GroupBy(c => new { c.Company!.CompanyId, c.Company.CompanyName })
                .Select(g => new TopCompanyByRevenue
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName,
                    ContractCount = g.Count(),
                    TotalRevenue = g.Sum(c => c.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToListAsync();

            return companies;
        }
    }

    // DTOs
    public class DashboardSummary
    {
        public PeriodInfo Period { get; set; } = null!;
        public ContractsInfo Contracts { get; set; } = null!;
        public MedicalGroupsInfo MedicalGroups { get; set; } = null!;
        public PatientsInfo Patients { get; set; } = null!;
        public StaffInfo Staff { get; set; } = null!;
        public FinancialInfo Financial { get; set; } = null!;
    }

    public class PeriodInfo { public DateTime From { get; set; } public DateTime To { get; set; } }
    public class ContractsInfo { public int Total { get; set; } public int Active { get; set; } public int NewInPeriod { get; set; } }
    public class MedicalGroupsInfo { public int Total { get; set; } public int InPeriod { get; set; } public int Completed { get; set; } }
    public class PatientsInfo { public int Total { get; set; } public int NewInPeriod { get; set; } }
    public class StaffInfo { public int TotalActive { get; set; } public int AssignedInPeriod { get; set; } }
    public class FinancialInfo { public decimal Revenue { get; set; } public decimal Cost { get; set; } public decimal Profit { get; set; } public decimal ProfitMargin { get; set; } }

    public class ContractAnalytics
    {
        public object Period { get; set; } = null!;
        public int TotalContracts { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageContractValue { get; set; }
        public List<StatusStat> ByStatus { get; set; } = new();
        public List<MonthStat> ByMonth { get; set; } = new();
        public List<TopCompanyByRevenue> TopCompanies { get; set; } = new();
    }

    public class StatusStat
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
    }

    public class MonthStat
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class MedicalGroupAnalytics
    {
        public object Period { get; set; } = null!;
        public int TotalGroups { get; set; }
        public int TotalPatients { get; set; }
        public double AveragePatientsPerGroup { get; set; }
        public List<GroupStatusStat> ByStatus { get; set; } = new();
        public List<GroupMonthStat> ByMonth { get; set; } = new();
        public List<TopMedicalGroup> TopGroupsByPatients { get; set; } = new();
    }

    public class GroupStatusStat
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
    }

    public class GroupMonthStat
    {
        public string Month { get; set; } = null!;
        public int Count { get; set; }
        public int Patients { get; set; }
    }

    public class StaffAnalytics
    {
        public object Period { get; set; } = null!;
        public int TotalAssignments { get; set; }
        public double TotalWorkingDays { get; set; }
        public decimal TotalSalaryPaid { get; set; }
        public List<StaffTypeStat> ByStaffType { get; set; } = new();
        public List<PositionStat> ByPosition { get; set; } = new();
        public List<TopStaff> TopStaff { get; set; } = new();
    }

    public class StaffTypeStat
    {
        public string StaffType { get; set; } = null!;
        public int Count { get; set; }
        public double WorkingDays { get; set; }
        public decimal TotalSalary { get; set; }
    }

    public class PositionStat
    {
        public string Position { get; set; } = null!;
        public int Count { get; set; }
    }

    public class FinancialAnalytics
    {
        public object Period { get; set; } = null!;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<MonthlyFinancial> ByMonth { get; set; } = new();
        public CostBreakdown CostBreakdown { get; set; } = null!;
    }

    public class TopCompanyByRevenue
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public int ContractCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopMedicalGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string? CompanyName { get; set; }
        public int PatientCount { get; set; }
        public DateTime ExamDate { get; set; }
    }

    public class TopStaff
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public int AssignmentCount { get; set; }
        public double WorkingDays { get; set; }
        public decimal TotalSalary { get; set; }
    }

    public class MonthlyFinancial
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public int ContractCount { get; set; }
    }

    public class CostBreakdown
    {
        public decimal StaffCosts { get; set; }
        public decimal SupplyCosts { get; set; }
        public decimal FixedCosts { get; set; }
        public decimal OverheadCosts { get; set; }
    }
}
