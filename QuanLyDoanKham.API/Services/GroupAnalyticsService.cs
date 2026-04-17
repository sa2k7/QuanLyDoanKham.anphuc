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
    public interface IGroupAnalyticsService
    {
        Task<List<GroupSummaryReportDto>> GetAllGroupSummariesAsync(DateTime? from, DateTime? to);
        Task<GroupAnalyticsDto?> GetGroupAnalyticsAsync(int groupId);
    }

    public class GroupAnalyticsService : IGroupAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        private static readonly Dictionary<string, string> StationNames = new()
        {
            ["SINH_HIEU"]        = "Đo Sinh Hiệu",
            ["NOI_KHOA"]         = "Nội Khoa",
            ["MAT_TAI_MUI_HONG"] = "Mắt / TMH",
            ["XQUANG"]           = "X-Quang",
            ["SIEU_AM"]          = "Siêu Âm",
            ["TIM_MACH"]         = "Điện Tim",
            ["LAY_MAU"]          = "Lấy Mẫu",
            ["QC"]               = "Kiểm Soát QC",
        };

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
                .Include(g => g.MedicalRecords).ThenInclude(r => r.StationTasks)
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

            // Station throughput
            var allTasks = records.SelectMany(r => r.StationTasks ?? new List<RecordStationTask>()).ToList();
            var stationStats = allTasks
                .GroupBy(t => t.StationCode)
                .Select(g =>
                {
                    var done     = g.Count(t => t.Status == StationTaskStatus.StationDone);
                    var skipped  = g.Count(t => t.Status == StationTaskStatus.Skipped);
                    var withTime = g.Where(t => t.StartedAt.HasValue && t.CompletedAt.HasValue).ToList();
                    double? avgDur = withTime.Any()
                        ? Math.Round(withTime.Average(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes), 1)
                        : null;

                    return new StationThroughputDto
                    {
                        StationCode        = g.Key,
                        StationName        = StationNames.TryGetValue(g.Key, out var name) ? name : g.Key,
                        TotalAssigned      = g.Count(),
                        Completed          = done,
                        Skipped            = skipped,
                        AvgDurationMinutes = avgDur
                    };
                })
                .OrderByDescending(s => s.TotalAssigned)
                .ToList();

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
                StationStats          = stationStats,
                TopDiagnoses          = topDiagnoses,
                AvgCompletionMinutes  = avgMinutes
            };
        }
    }
}
