using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    public class StaffAssignmentValidationService : IStaffAssignmentValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffAssignmentValidationService> _logger;

        /// <summary>
        /// Allowed role values — must match StaffRole enum names exactly.
        /// Stored as a HashSet for O(1) lookup.
        /// </summary>
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(StaffRole.TiepNhan), "Tiếp nhận", "Tiep nhan",
            nameof(StaffRole.KhamNoi), "Khám nội", "Kham noi",
            nameof(StaffRole.KhamNgoai), "Khám ngoại", "Kham ngoai",
            nameof(StaffRole.LayMau), "Lấy máu", "Lay mau",
            nameof(StaffRole.KhamSan), "Khám sản", "Kham san",
            nameof(StaffRole.SieuAm), "Siêu âm", "Sieu am",
            nameof(StaffRole.TruongDoan), "Trưởng đoàn", "Truong doan"
        };

        public StaffAssignmentValidationService(
            ApplicationDbContext context,
            ILogger<StaffAssignmentValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Valid, string? ErrorMessage)> ValidateAssignmentAsync(
            int staffId,
            int campaignId,
            DateTime examDate,
            string staffRole)
        {
            if (string.IsNullOrWhiteSpace(staffRole))
                return (false, "Vui lòng nhập vai trò nhân sự");

            var normalizedRole = NormalizeRole(staffRole);

            // ── Rule 1: Role compatibility ────────────────────────────────────
            if (!AllowedRoles.Contains(normalizedRole))
            {
                _logger.LogWarning(
                    "Staff {StaffId} has incompatible role '{Role}' (Normalized: '{Normalized}') for campaign assignment",
                    staffId, staffRole, normalizedRole);
                return (false, $"Vai trò '{staffRole}' không hợp lệ hoặc không được phép phân công trực tiếp");
            }

            // ── Rule 2: 1 staff / 1 day / 1 campaign ─────────────────────────
            var examDateOnly = examDate.Date;

            var conflictingAssignment = await _context.GroupStaffDetails
                .AsNoTracking()
                .Include(g => g.MedicalGroup)
                .Include(g => g.Staff)
                .Where(g =>
                    g.StaffId == staffId &&
                    g.ExamDate.Date == examDateOnly &&
                    g.GroupId != campaignId)
                .Select(g => new
                {
                    g.GroupId,
                    CampaignName = g.MedicalGroup != null ? g.MedicalGroup.GroupName : "Unknown",
                    StaffName = g.Staff != null ? g.Staff.FullName : "Unknown"
                })
                .FirstOrDefaultAsync();

            if (conflictingAssignment != null)
            {
                _logger.LogWarning(
                    "Staff {StaffId} double-booking detected: already assigned to campaign {ConflictCampaignId} on {Date}",
                    staffId, conflictingAssignment.GroupId, examDateOnly);

                return (false,
                    $"Nhân sự {conflictingAssignment.StaffName} đã được phân công cho chiến dịch " +
                    $"{conflictingAssignment.CampaignName} vào ngày này");
            }

            // All rules passed
            return (true, null);
        }

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            
            role = role.Normalize(System.Text.NormalizationForm.FormC);
            
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Tiếp nhận", "TiepNhan" },
                { "Tiep nhan", "TiepNhan" },
                { "Khám nội", "KhamNoi" },
                { "Kham noi", "KhamNoi" },
                { "Khám ngoại", "KhamNgoai" },
                { "Kham ngoai", "KhamNgoai" },
                { "Lấy máu", "LayMau" },
                { "Lay mau", "LayMau" },
                { "Khám sản", "KhamSan" },
                { "Kham san", "KhamSan" },
                { "Siêu âm", "SieuAm" },
                { "Sieu am", "SieuAm" },
                { "Trưởng đoàn", "TruongDoan" },
                { "Truong doan", "TruongDoan" }
            };

            var trimmed = role.Trim();
            if (map.TryGetValue(trimmed, out var mapped)) return mapped;
            
            return trimmed;
        }
    }
}
