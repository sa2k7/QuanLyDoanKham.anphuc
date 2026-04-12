using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lá»‹ch sá»­ thay Ä‘á»•i tráº¡ng thĂ¡i há»£p Ä‘á»“ng</summary>
    public class ContractStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        [MaxLength(50)]
        public string OldStatus { get; set; } = null!;

        [MaxLength(50)]
        public string NewStatus { get; set; } = null!;

        public string Note { get; set; } = null!;
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string ChangedBy { get; set; } = null!;
    }
}
