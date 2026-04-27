using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Hợp đồng với công ty khách hàng</summary>
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ContractCode { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = null!;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; }

        /// <summary>Draft | Pending | Approved | Cancelled</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        public int? ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public ICollection<ExaminationTeam> ExaminationTeams { get; set; } = new List<ExaminationTeam>();
    }
}
