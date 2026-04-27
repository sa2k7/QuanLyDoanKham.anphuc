using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.DTOs.Reports;

namespace QuanLyDoanKham.API.Services
{
    public interface IGroupAnalyticsService
    {
        Task<List<GroupSummaryReportDto>> GetAllGroupSummariesAsync(DateTime? from, DateTime? to);
        Task<GroupAnalyticsDto?> GetGroupAnalyticsAsync(int groupId);
        Task<GroupPnlDto?> GetGroupPnlAsync(int groupId);
        Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to);
    }

    public class GroupAnalyticsService : IGroupAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public GroupAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GroupSummaryReportDto>> GetAllGroupSummariesAsync(DateTime? from, DateTime? to)
        {
            var query = _context.MedicalGroups
                .Include(g => g.HealthContract).ThenInclude(hc => hc!.Company)
                .Include(g => g.MedicalRecords)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(g => g.ExamDate >= from.Value);
            if (to.HasValue)
                query = query.Where(g => g.ExamDate <= to.Value);

            var groups = await query.OrderByDescending(g => g.ExamDate).ToListAsync();

            return groups.Select(g =>
            {
                var total      = g.MedicalRecords?.Count ?? 0;
                var completed  = g.MedicalRecords?.Count(r => r.Status == RecordStatus.Completed) ?? 0;
                return new GroupSummaryReportDto
                {
                    GroupId        = g.GroupId,
                    GroupName      = g.GroupName,
                    CompanyName    = g.HealthContract?.Company?.CompanyName ?? "—",
                    ExamDate       = g.ExamDate.ToString("dd/MM/yyyy"),
                    TotalPatients  = total,
                    Completed      = completed,
                    CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
                    Status         = g.Status ?? "DRAFT"
                };
            }).ToList();
        }

        public async Task<GroupAnalyticsDto?> GetGroupAnalyticsAsync(int groupId)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract).ThenInclude(hc => hc!.Company)
                .Include(g => g.MedicalRecords)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return null;

            var records = group.MedicalRecords?.ToList() ?? new List<MedicalRecord>();
            var total   = records.Count;

            var completed  = records.Count(r => r.Status == RecordStatus.Completed);
            var qcPending  = records.Count(r => r.Status == RecordStatus.QcPending);
            var inProgress = records.Count(r => r.Status == RecordStatus.InProgress);
            var noShow     = records.Count(r => r.Status == RecordStatus.NoShow);
            var cancelled  = records.Count(r => r.Status == RecordStatus.Cancelled);
            var checkedIn  = records.Count(r => r.Status != RecordStatus.Created && r.Status != RecordStatus.Ready);

            // Avg completion time
            double? avgMinutes = null;
            var completedWithTimes = records
                .Where(r => r.Status == RecordStatus.Completed && r.CheckInAt.HasValue && r.UpdatedAt.HasValue)
                .ToList();
            if (completedWithTimes.Any())
            {
                avgMinutes = completedWithTimes
                    .Average(r => (r.UpdatedAt!.Value - r.CheckInAt!.Value).TotalMinutes);
                avgMinutes = Math.Round(avgMinutes.Value, 1);
            }

            // Top diagnoses — fetch from ExamResults
            var examResults = await _context.ExamResults
                .Where(e => e.GroupId == groupId && !string.IsNullOrEmpty(e.Diagnosis))
                .Select(e => e.Diagnosis)
                .ToListAsync();

            var totalDiags = examResults.Count;
            var topDiagnoses = examResults
                .GroupBy(d => d!.Trim().ToLowerInvariant())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g =>
                {
                    var rawName = examResults.First(d => d!.Trim().ToLowerInvariant() == g.Key)!;
                    return new DiagnosisFrequencyDto
                    {
                        Diagnosis  = rawName,
                        Count      = g.Count(),
                        Percentage = totalDiags > 0 ? Math.Round((double)g.Count() / totalDiags * 100, 1) : 0
                    };
                })
                .ToList();

            return new GroupAnalyticsDto
            {
                GroupId               = group.GroupId,
                GroupName             = group.GroupName,
                CompanyName           = group.HealthContract?.Company?.CompanyName ?? "—",
                ExamDate              = group.ExamDate.ToString("dd/MM/yyyy"),
                TotalPatients         = total,
                CheckedIn             = checkedIn,
                Completed             = completed,
                QcPending             = qcPending,
                InProgress            = inProgress,
                NoShow                = noShow,
                Cancelled             = cancelled,
                CompletionRate        = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
                NoShowRate            = total > 0 ? Math.Round((double)noShow / total * 100, 1) : 0,
                QcPassRate            = (completed + qcPending) > 0
                                            ? Math.Round((double)completed / (completed + qcPending) * 100, 1) : 0,
                StationStats          = new List<StationThroughputDto>(), // Removed station-level analytics
                TopDiagnoses          = topDiagnoses,
                AvgCompletionMinutes  = avgMinutes
            };
        }

        public async Task<GroupPnlDto?> GetGroupPnlAsync(int groupId)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract).ThenInclude(hc => hc!.Company)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return null;

            var laborCost = await _context.GroupStaffDetails
                .Where(d => d.GroupId == groupId && d.WorkStatus == "Joined")
                .SumAsync(d => (decimal?)d.CalculatedSalary) ?? 0;

            var materialCost = await _context.StockMovements
                .Where(m => m.MedicalGroupId == groupId && m.MovementType == "OUT")
                .SumAsync(m => (decimal?)m.TotalValue) ?? 0;

            var groupCost = await _context.GroupCosts
                .FirstOrDefaultAsync(c => c.GroupId == groupId);

            var otherCost = groupCost?.OtherCost ?? 0;
            var totalCost = laborCost + materialCost + otherCost;

            var contractValue = group.HealthContract?.TotalAmount ?? 0;
            var profit = contractValue - totalCost;
            var profitMargin = contractValue > 0 ? Math.Round((double)profit / (double)contractValue * 100, 1) : 0;

            // Chi tiết nhân sự
            var staffDetails = await _context.GroupStaffDetails
                .Include(d => d.Staff)
                .Where(d => d.GroupId == groupId && d.WorkStatus == "Joined")
                .Select(d => new PnlStaffLineDto
                {
                    StaffName  = d.Staff != null ? d.Staff.FullName : "—",
                    ShiftType  = d.ShiftType,
                    DailyRate  = d.Staff != null ? d.Staff.DailyRate : 0,
                    Salary     = d.CalculatedSalary
                })
                .ToListAsync();

            // Chi tiết vật tư
            var materialDetails = await _context.StockMovements
                .Where(m => m.MedicalGroupId == groupId && m.MovementType == "OUT")
                .Select(m => new PnlMaterialLineDto
                {
                    ItemName   = m.ItemName,
                    Unit       = m.Unit,
                    Quantity   = m.Quantity,
                    UnitPrice  = m.UnitPrice,
                    TotalValue = m.TotalValue
                })
                .ToListAsync();

            var patientCount = await _context.MedicalRecords
                .CountAsync(mr => mr.GroupId == groupId);

            return new GroupPnlDto
            {
                GroupId        = group.GroupId,
                GroupName      = group.GroupName,
                CompanyName    = group.HealthContract?.Company?.CompanyName ?? "—",
                ExamDate       = group.ExamDate.ToString("dd/MM/yyyy"),
                ContractValue  = contractValue,
                LaborCost      = laborCost,
                MaterialCost   = materialCost,
                OtherCost      = otherCost,
                TotalCost      = totalCost,
                Profit         = profit,
                ProfitMargin   = profitMargin,
                PatientCount   = patientCount,
                StaffLines     = staffDetails,
                MaterialLines  = materialDetails
            };
        }

        public async Task<PeriodSummaryDto> GetPeriodSummaryAsync(DateTime from, DateTime to)
        {
            var groupsInPeriod = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .Where(g => g.ExamDate >= from && g.ExamDate <= to)
                .ToListAsync();

            var groupIds = groupsInPeriod.Select(g => g.GroupId).ToList();

            // Đếm bệnh nhân từ MedicalRecords
            var patientsByGroup = await _context.MedicalRecords
                .Where(mr => groupIds.Contains(mr.GroupId))
                .GroupBy(mr => mr.GroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToListAsync();
            var totalPatients = patientsByGroup.Sum(x => x.Count);

            var totalLaborCost = await _context.GroupStaffDetails
                .Where(d => groupIds.Contains(d.GroupId) && d.WorkStatus == "Joined")
                .SumAsync(d => (decimal?)d.CalculatedSalary) ?? 0;

            var totalMaterialCost = await _context.StockMovements
                .Where(m => m.MedicalGroupId != null && groupIds.Contains(m.MedicalGroupId!.Value) && m.MovementType == "OUT")
                .SumAsync(m => (decimal?)m.TotalValue) ?? 0;

            var otherCosts = await _context.GroupCosts
                .Where(c => groupIds.Contains(c.GroupId))
                .SumAsync(c => (decimal?)c.OtherCost) ?? 0;

            var totalRevenue = groupsInPeriod
                .Sum(g => g.HealthContract?.TotalAmount ?? 0);

            var totalCost = totalLaborCost + totalMaterialCost + otherCosts;
            var profit = totalRevenue - totalCost;

            return new PeriodSummaryDto
            {
                From           = from.ToString("dd/MM/yyyy"),
                To             = to.ToString("dd/MM/yyyy"),
                TotalGroups    = groupsInPeriod.Count,
                TotalPatients  = totalPatients,
                TotalRevenue   = totalRevenue,
                TotalLaborCost = totalLaborCost,
                TotalMaterialCost = totalMaterialCost,
                TotalOtherCost = otherCosts,
                TotalCost      = totalCost,
                Profit         = profit,
                ProfitMargin   = totalRevenue > 0 ? Math.Round((double)profit / (double)totalRevenue * 100, 1) : 0
            };
        }
    }
}
