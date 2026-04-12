using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Äá»‹nh nghÄ©a bÆ°á»›c phĂª duyá»‡t Ä‘a cáº¥p theo há»£p Ä‘á»“ng</summary>
    public class ApprovalStep
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        public int Order { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; } = null!;

        [MaxLength(100)]
        public string RequiredPermission { get; set; } = null!;
        public bool IsFinal { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
