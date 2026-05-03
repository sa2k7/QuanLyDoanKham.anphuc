using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lịch sử thay đá»•i trạng thái hợp đá»“ng</summary>
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
