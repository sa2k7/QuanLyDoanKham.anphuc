using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using System.Data;

namespace QuanLyDoanKham.API.Services.MedicalGroups
{
    public class MedicalGroupAutoAssignmentService : IMedicalGroupAutoAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MedicalGroupAutoAssignmentService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResult<AutoCreateGroupWithStaffResponseDto>> AutoCreateAndAssignAsync(AutoCreateGroupWithStaffRequestDto request, string userId)
        {
            // 1. Validate Idempotency (Simple check for now)
            var existingGroup = await _context.MedicalGroups
                .AnyAsync(g => g.GroupName == request.GroupName && g.ExamDate.Date == request.ExamDate.Date);
            if (existingGroup)
            {
                return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure("Đoàn khám với tên và ngày này đã tồn tại.");
            }

            // 2. Validate Contract
            var contract = await _context.Contracts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.HealthContractId == request.HealthContractId);

            if (contract == null) return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure("Không tìm thấy hợp đồng.");
            if (contract.Status != ContractStatus.Approved && contract.Status != ContractStatus.Active)
                return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure("Hợp đồng chưa được phê duyệt hoặc không còn hiệu lực.");

            // 3. Build Staffing Requirements
            var requirements = BuildStaffingRequirements(contract.ExpectedQuantity, request.TargetRatio, request.MinimumDoctors);
            int totalRequired = requirements.Values.Sum();

            // 4. Get Available Staff Pool
            var assignedStaffIdsOnThatDay = await _context.GroupStaffDetails
                .Include(d => d.MedicalGroup)
                .Where(d => d.MedicalGroup.ExamDate.Date == request.ExamDate.Date)
                .Select(d => d.StaffId)
                .Distinct()
                .ToListAsync();

            var staffPool = await _context.Staffs
                .Where(s => s.IsActive && !assignedStaffIdsOnThatDay.Contains(s.StaffId))
                .ToListAsync();

            // 5. Run Assignment Logic (Scoring & Selection)
            var assignedStaffList = new List<AssignedStaffDto>();
            var unassignedReasons = new List<UnassignedReasonDto>();
            var warnings = new List<string>();
            var selectedIds = new HashSet<int>();

            foreach (var req in requirements)
            {
                string position = req.Key;
                int needed = req.Value;
                int found = 0;

                // Score and sort candidates for this position
                var candidates = staffPool
                    .Where(s => !selectedIds.Contains(s.StaffId) && IsEligibleForPosition(s, position))
                    .Select(s => new { 
                        Staff = s, 
                        Score = CalculateScore(s, position) 
                    })
                    .OrderByDescending(c => c.Score)
                    .Take(needed)
                    .ToList();

                foreach (var c in candidates)
                {
                    assignedStaffList.Add(new AssignedStaffDto
                    {
                        StaffId = c.Staff.StaffId,
                        StaffName = c.Staff.FullName,
                        WorkPosition = position,
                        ShiftType = 1.0,
                        Reason = $"Match: {c.Score} pts"
                    });
                    selectedIds.Add(c.Staff.StaffId);
                    found++;
                }

                if (found < needed)
                {
                    unassignedReasons.Add(new UnassignedReasonDto { Position = position, Reason = $"Thiếu {needed - found} nhân sự" });
                    warnings.Add($"Không đủ nhân sự cho vị trí {position} (Cần {needed}, còn {found})");
                }
            }

            // 6. Check Strict Mode
            int doctorCount = assignedStaffList.Count(s => GetStaffTypeFromId(staffPool, s.StaffId) == "BacSi");
            if (request.AssignmentMode.ToLower() == "strict")
            {
                if (doctorCount < request.MinimumDoctors)
                    return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure($"Chế độ nghiêm ngặt: Không đủ số lượng Bác sĩ tối thiểu ({doctorCount}/{request.MinimumDoctors}).");
                
                if (warnings.Any())
                    return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure("Chế độ nghiêm ngặt: Không thể phân bổ đầy đủ nhân sự theo yêu cầu.");
            }

            // 7. Persist in Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newGroup = new MedicalGroup
                {
                    GroupName = request.GroupName,
                    ExamDate = request.ExamDate,
                    HealthContractId = request.HealthContractId,
                    Status = "Open",
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                _context.MedicalGroups.Add(newGroup);
                await _context.SaveChangesAsync();

                foreach (var staff in assignedStaffList)
                {
                    var detail = new GroupStaffDetail
                    {
                        GroupId = newGroup.GroupId,
                        StaffId = staff.StaffId,
                        WorkPosition = staff.WorkPosition ?? "Unknown",
                        ShiftType = staff.ShiftType,
                        ExamDate = request.ExamDate, // Ensure ExamDate is set
                        WorkStatus = "Đang chờ",
                        CalculatedSalary = CalculateSalary(staffPool.First(s => s.StaffId == staff.StaffId), staff.ShiftType)
                    };
                    _context.GroupStaffDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResult<AutoCreateGroupWithStaffResponseDto>.Success(new AutoCreateGroupWithStaffResponseDto
                {
                    Group = new MedicalGroupDto { 
                        GroupId = newGroup.GroupId, 
                        GroupName = newGroup.GroupName, 
                        ExamDate = newGroup.ExamDate,
                        Status = newGroup.Status,
                        CompanyName = contract.Company?.CompanyName ?? "Unknown"
                    },
                    Summary = new AssignmentSummaryDto
                    {
                        RequiredHeadcount = totalRequired,
                        AssignedCount = assignedStaffList.Count,
                        MissingCount = totalRequired - assignedStaffList.Count,
                        Mode = request.AssignmentMode
                    },
                    AssignedStaff = assignedStaffList,
                    UnassignedReasons = unassignedReasons,
                    Warnings = warnings
                }, "Tạo đoàn và phân bổ nhân sự thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResult<AutoCreateGroupWithStaffResponseDto>.Failure($"Lỗi database: {ex.Message}");
            }
        }

        private Dictionary<string, int> BuildStaffingRequirements(int expectedQty, int targetRatio, int minDoctors)
        {
            int totalStaff = (int)Math.Ceiling((double)expectedQty / targetRatio);
            if (totalStaff < 5) totalStaff = 5; // Minimum team size

            var reqs = new Dictionary<string, int>();
            
            // Core positions
            reqs["Tiếp nhận"] = 1;
            reqs["Cân đo huyết áp"] = 1;
            reqs["Khám nội"] = Math.Max(1, minDoctors / 2);
            reqs["Khám ngoại"] = Math.Max(1, minDoctors - reqs["Khám nội"]);
            reqs["Lấy mẫu"] = Math.Max(1, totalStaff / 6);
            reqs["Siêu âm"] = 1;
            reqs["Hậu cần"] = 1;

            // Fill remaining as "Khác" or adjust
            int currentAllocated = reqs.Values.Sum();
            if (currentAllocated < totalStaff)
            {
                reqs["Khác"] = totalStaff - currentAllocated;
            }

            return reqs;
        }

        private bool IsEligibleForPosition(Staff s, string position)
        {
            if (s.StaffType == "BacSi")
            {
                // Bác sĩ chỉ được làm các vị trí Khám. Cấm làm các vị trí Hành chính / Lấy mẫu.
                var allowedForDrs = new[] { "Khám nội", "Khám ngoại", "Siêu âm", "Sản phụ khoa", "Khám sản phụ khoa", "Răng hàm mặt", "Tai mũi họng", "Mắt" };
                return allowedForDrs.Contains(position);
            }
            
            // Các vị trí còn lại
            return position switch
            {
                "Lấy mẫu" => s.StaffType == "DieuDuong" || s.StaffType == "KyThuatVien",
                "Tiếp nhận" or "Cân đo huyết áp" or "Hậu cần" => s.StaffType != "BacSi", 
                _ => true
            };
        }

        private int CalculateScore(Staff s, string position)
        {
            int score = 0;
            // 1. Specialty score (+40)
            if (!string.IsNullOrEmpty(s.Specialty) && position.Contains(s.Specialty)) score += 40;
            
            // 2. StaffType score (+30)
            if (position == "Khám nội" && s.StaffType == "BacSi") score += 30;
            if (position.Contains("Tiếp nhận") && s.StaffType == "NhanVienHoTro") score += 30;
            
            // 3. Workload balancing (+20 placeholder - for production would query assignment count)
            score += 20; 

            return score;
        }

        private decimal CalculateSalary(Staff s, double shiftType)
        {
            return s.DailyRate * (decimal)shiftType;
        }

        private string GetStaffTypeFromId(List<Staff> pool, int staffId)
        {
            return pool.FirstOrDefault(s => s.StaffId == staffId)?.StaffType ?? "Unknown";
        }
    }
}
