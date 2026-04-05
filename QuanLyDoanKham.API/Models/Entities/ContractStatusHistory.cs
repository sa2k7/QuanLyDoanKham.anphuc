using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lịch sử thay đổi trạng thái hợp đồng</summary>
    public class ContractStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [MaxLength(50)]
        public string OldStatus { get; set; }

        [MaxLength(50)]
        public string NewStatus { get; set; }

        public string Note { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string ChangedBy { get; set; }
    }
}
