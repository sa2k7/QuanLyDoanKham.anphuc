using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalGroups
{
    /// <summary>AI Service gợi ý phân công nhân sự vào đoàn khám</summary>
    public interface IStaffSuggestionService
    {
        Task<List<StaffSuggestionResult>> SuggestStaffForGroupAsync(int groupId);
        Task<List<StaffSuggestionResult>> SuggestStaffForPositionAsync(int groupId, string positionName);
    }

    public class StaffSuggestionService : IStaffSuggestionService
    {
        private readonly ApplicationDbContext _context;

        public StaffSuggestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Gợi ý phân công nhân sự cho toàn bộ đoàn khám</summary>
        public async Task<List<StaffSuggestionResult>> SuggestStaffForGroupAsync(int groupId)
        {
            var group = await _context.MedicalGroups
                .Include(mg => mg.Positions)
                .FirstOrDefaultAsync(mg => mg.GroupId == groupId);

            if (group == null)
                throw new InvalidOperationException("Không tìm thấy đoàn khám");

            var suggestions = new List<StaffSuggestionResult>();

            // Lấy các vị trí cần phân công từ GroupPositionQuota
            var quotas = await _context.GroupPositionQuotas
                .Where(q => q.MedicalGroupId == groupId)
                .Include(q => q.Position)
                .ToListAsync();

            foreach (var quota in quotas)
            {
                var positionSuggestions = await SuggestStaffForPositionInternalAsync(
                    groupId,
                    group.ExamDate,
                    quota.Position?.Name ?? "Nhân viên",
                    quota.Required);

                suggestions.AddRange(positionSuggestions);
            }

            // Nếu chưa có quota, gợi ý theo các vị trí mặc định
            if (!quotas.Any())
            {
                var defaultPositions = new[] { "Tiếp nhận", "Khám nội", "Khám ngoại", "Lấy máu", "Khám sản", "Siêu âm" };
                foreach (var position in defaultPositions)
                {
                    var positionSuggestions = await SuggestStaffForPositionInternalAsync(
                        groupId, group.ExamDate, position, 2);
                    suggestions.AddRange(positionSuggestions);
                }
            }

            return suggestions.OrderByDescending(s => s.Score).ToList();
        }

        /// <summary>Gợi ý nhân sự cho một vị trí cụ thể</summary>
        public async Task<List<StaffSuggestionResult>> SuggestStaffForPositionAsync(int groupId, string positionName)
        {
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null)
                throw new InvalidOperationException("Không tìm thấy đoàn khám");

            return await SuggestStaffForPositionInternalAsync(groupId, group.ExamDate, positionName, 5);
        }

        private async Task<List<StaffSuggestionResult>> SuggestStaffForPositionInternalAsync(
            int groupId, DateTime examDate, string positionName, int maxSuggestions)
        {
            var suggestions = new List<StaffSuggestionResult>();

            // 1. Lấy danh sách nhân sự đã được phân công vào ngày này (tránh trùng lặp)
            var assignedStaffIds = await _context.GroupStaffDetails
                .Where(gsd => gsd.ExamDate.Date == examDate.Date && gsd.GroupId != groupId)
                .Select(gsd => gsd.StaffId)
                .ToListAsync();

            // 2. Lấy tất cả nhân sự active
            var allStaff = await _context.Staffs
                .Where(s => s.IsActive)
                .Include(s => s.GroupStaffDetails)
                .ToListAsync();

            // 3. Lọc nhân sự chưa được phân công vào ngày này
            var availableStaff = allStaff
                .Where(s => !assignedStaffIds.Contains(s.StaffId))
                .ToList();

            // 4. Tính điểm và xếp hạng cho từng nhân sự
            foreach (var staff in availableStaff)
            {
                var score = CalculateStaffScore(staff, positionName, examDate);

                if (score > 0) // Chỉ lấy nhân sự có điểm > 0
                {
                    suggestions.Add(new StaffSuggestionResult
                    {
                        StaffId = staff.StaffId,
                        StaffName = staff.FullName,
                        StaffType = staff.StaffType,
                        JobTitle = staff.JobTitle,
                        PositionName = positionName,
                        Score = score,
                        Reason = GenerateReason(staff, positionName, score)
                    });
                }
            }

            // 5. Sắp xếp theo điểm và lấy top N
            return suggestions
                .OrderByDescending(s => s.Score)
                .Take(maxSuggestions)
                .ToList();
        }

        /// <summary>Tính điểm cho nhân sự dựa trên nhiều yếu tố</summary>
        private double CalculateStaffScore(Staff staff, string positionName, DateTime examDate)
        {
            double score = 0;

            // 1. Phù hợp với vị trí (0-30 điểm)
            score += CalculatePositionMatchScore(staff, positionName);

            // 2. Kinh nghiệm (0-25 điểm) - Số lần đã đi đoàn
            var pastAssignments = staff.GroupStaffDetails?.Count ?? 0;
            score += Math.Min(pastAssignments * 5, 25);

            // 3. Lịch sử làm vị trí này (0-20 điểm)
            var samePositionCount = staff.GroupStaffDetails?
                .Count(gsd => gsd.WorkPosition == positionName) ?? 0;
            score += Math.Min(samePositionCount * 10, 20);

            // 4. Tải công việc hiện tại (0-15 điểm) - Ưu tiên người ít việc
            var currentMonthAssignments = staff.GroupStaffDetails?
                .Count(gsd => gsd.ExamDate.Month == examDate.Month &&
                             gsd.ExamDate.Year == examDate.Year) ?? 0;
            score += Math.Max(15 - currentMonthAssignments * 3, 0);

            // 5. Loại nhân sự (0-10 điểm)
            if (staff.StaffType == "BacSi" && positionName.Contains("Khám"))
                score += 10;
            else if (staff.StaffType == "DieuDuong" && positionName.Contains("Tiếp nhận"))
                score += 8;
            else if (staff.StaffType == "KyThuatVien" && positionName.Contains("Siêu âm"))
                score += 10;

            return score;
        }

        private double CalculatePositionMatchScore(Staff staff, string positionName)
        {
            var positionKeywords = new Dictionary<string, string[]>
            {
                { "Tiếp nhận", new[] { "y tá", "điều dưỡng", "tiếp nhận" } },
                { "Khám nội", new[] { "bác sĩ", "nội khoa", "bs" } },
                { "Khám ngoại", new[] { "bác sĩ", "ngoại khoa", "bs" } },
                { "Lấy máu", new[] { "kỹ thuật", "xét nghiệm", "điều dưỡng" } },
                { "Khám sản", new[] { "bác sĩ", "sản khoa", "bs" } },
                { "Siêu âm", new[] { "kỹ thuật", "siêu âm", "radiology" } }
            };

            double score = 0;

            // Kiểm tra JobTitle phù hợp
            if (!string.IsNullOrEmpty(staff.JobTitle))
            {
                var jobTitle = staff.JobTitle.ToLower();
                if (positionKeywords.TryGetValue(positionName, out var keywords))
                {
                    foreach (var keyword in keywords)
                    {
                        if (jobTitle.Contains(keyword.ToLower()))
                        {
                            score = 30;
                            break;
                        }
                    }
                }
            }

            // Kiểm tra StaffType phù hợp
            if (!string.IsNullOrEmpty(staff.StaffType))
            {
                var staffType = staff.StaffType.ToLower();
                if (positionName.Contains("Khám") && staffType == "bacs")
                    score = Math.Max(score, 25);
                else if (positionName.Contains("Siêu âm") && staffType == "kythuatvien")
                    score = Math.Max(score, 25);
                else if (positionName.Contains("Lấy máu") && (staffType == "kythuatvien" || staffType == "dieu_duong"))
                    score = Math.Max(score, 20);
            }

            return score;
        }

        private string GenerateReason(Staff staff, string positionName, double score)
        {
            var reasons = new List<string>();

            if (score >= 70)
                reasons.Add("Rất phù hợp cho vị trí này");
            else if (score >= 50)
                reasons.Add("Phù hợp cho vị trí này");

            var pastAssignments = staff.GroupStaffDetails?.Count ?? 0;
            if (pastAssignments > 5)
                reasons.Add($"Có kinh nghiệm ({pastAssignments} lần đi đoàn)");

            var samePositionCount = staff.GroupStaffDetails?
                .Count(gsd => gsd.WorkPosition == positionName) ?? 0;
            if (samePositionCount > 0)
                reasons.Add($"Đã từng làm {positionName} {samePositionCount} lần");

            if (!string.IsNullOrEmpty(staff.Specialty))
                reasons.Add($"Chuyên môn: {staff.Specialty}");

            return string.Join("; ", reasons);
        }
    }

    public class StaffSuggestionResult
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public string? StaffType { get; set; }
        public string? JobTitle { get; set; }
        public string PositionName { get; set; } = null!;
        public double Score { get; set; }
        public string Reason { get; set; } = null!;
    }
}
