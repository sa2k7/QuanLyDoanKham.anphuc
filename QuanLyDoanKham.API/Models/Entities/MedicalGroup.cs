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
        public string GroupName { get; set; }

        public DateTime ExamDate { get; set; }

        // Ca / Slot (sáng/chiều/tối - cho phép nhiều đoàn/ngày)
        [MaxLength(20)]
        public string Slot { get; set; } = "FullDay"; // Morning, Afternoon, Evening, FullDay

        [MaxLength(50)]
        public string TeamCode { get; set; } // Phân biệt team A/B nếu nhiều đoàn/ngày

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        // Trạng thái: Draft, Open, InProgress, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public string ImportFilePath { get; set; }

        // Người quản lý đoàn khám (AppUser)
        public int? ManagerUserId { get; set; }
        [ForeignKey("ManagerUserId")]
        public AppUser ManagerUser { get; set; }

        // Trưởng đoàn (Staff - người có thể mở QR)
        public int? GroupLeaderStaffId { get; set; }
        [ForeignKey("GroupLeaderStaffId")]
        public Staff GroupLeaderStaff { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        public ICollection<MedicalGroupPosition> Positions { get; set; } = new List<MedicalGroupPosition>();
        public ICollection<GroupStaffDetail> StaffDetails { get; set; } = new List<GroupStaffDetail>();
    }
}
