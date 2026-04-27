using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class MedicalGroup
    {
        [Key]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "TĂªn Ä‘oĂ n khĂ¡m khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        [MaxLength(200)]
        public string GroupName { get; set; } = null!;

        public DateTime ExamDate { get; set; }

        // Ca / Slot (sáng/chiều/tối - cho phép nhiều đoàn/ngày)
        [MaxLength(20)]
        public string Slot { get; set; } = "FullDay"; // Morning, Afternoon, Evening, FullDay

        [MaxLength(50)]
        public string? TeamCode { get; set; } // Phân biệt team A/B nếu nhiều đoàn/ngày

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract? HealthContract { get; set; }

        // Tráº¡ng thĂ¡i: Draft, Open, InProgress, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        [MaxLength(50)]
        public string? StartTime { get; set; } // Giá»  khĂ¡m

        [MaxLength(50)]
        public string? DepartureTime { get; set; } // Giá»  xuáº¥t phĂ¡t

        [MaxLength(500)]
        public string? ExamContent { get; set; } = "TỔNG QUÁT"; // Ná»™i dung khĂ¡m

        public string? ImportFilePath { get; set; }

        // NgÆ°á»i quáº£n lĂ½ Ä‘oĂ n khĂ¡m (AppUser)
        public int? ManagerUserId { get; set; }
        [ForeignKey("ManagerUserId")]
        public AppUser? ManagerUser { get; set; }

        // TrÆ°á»Ÿng Ä‘oĂ n (Staff - ngÆ°á»i cĂ³ thá»ƒ má»Ÿ QR)
        public int? GroupLeaderStaffId { get; set; }
        [ForeignKey("GroupLeaderStaffId")]
        public Staff? GroupLeaderStaff { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public ICollection<MedicalGroupPosition> Positions { get; set; } = new List<MedicalGroupPosition>();
        public ICollection<GroupStaffDetail> StaffDetails { get; set; } = new List<GroupStaffDetail>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}
