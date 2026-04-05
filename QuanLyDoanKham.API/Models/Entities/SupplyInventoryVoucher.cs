using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class SupplyInventoryVoucher
    {
        [Key]
        public int VoucherId { get; set; }

        [MaxLength(20)]
        public string VoucherCode { get; set; }

        public DateTime CreateDate { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } // IMPORT, EXPORT

        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public int CreatedByUserId { get; set; }
        [ForeignKey("ApproverId")] // Logic từ Entities.cs cũ: CreatedByUserId
        public AppUser CreatedByUser { get; set; }

        public string Note { get; set; }

        public ICollection<SupplyInventoryDetail> Details { get; set; } = new List<SupplyInventoryDetail>();
    }
}
