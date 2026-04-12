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

        // Ca / Slot (sĂ¡ng/chiá»u/tá»‘i - cho phĂ©p nhiá»u Ä‘oĂ n/ngĂ y)
        [MaxLength(20)]
        public string Slot { get; set; } = "FullDay"; // Morning, Afternoon, Evening, FullDay

        [MaxLength(50)]
        public string? TeamCode { get; set; } // PhĂ¢n biá»‡t team A/B náº¿u nhiá»u Ä‘oĂ n/ngĂ y

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract? HealthContract { get; set; }

        // Tráº¡ng thĂ¡i: Draft, Open, InProgress, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

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
    }
}
