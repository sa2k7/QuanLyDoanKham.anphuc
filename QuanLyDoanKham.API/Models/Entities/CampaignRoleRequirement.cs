using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Yêu cầu số lượng nhân sự theo vai trò cho mỗi chiến dịch khám.
    /// Dùng để validate phân công (cảnh báo thiếu/thừa nhân sự theo vai trò).
    /// Unique constraint: (CampaignId, Role) — mỗi vai trò chỉ có 1 dòng/chiến dịch.
    /// </summary>
    public class CampaignRoleRequirement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK → MedicalGroups.GroupId</summary>
        public int CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public MedicalGroup? Campaign { get; set; }

        /// <summary>Vai trò nhân sự (giá trị từ StaffRole enum, lưu dạng string)</summary>
        [Required]
        [MaxLength(30)]
        public string Role { get; set; } = string.Empty;

        /// <summary>Số lượng nhân sự cần thiết cho vai trò này (≥ 1)</summary>
        [Range(1, int.MaxValue, ErrorMessage = "RequiredCount phải lớn hơn hoặc bằng 1")]
        public int RequiredCount { get; set; }
    }
}
