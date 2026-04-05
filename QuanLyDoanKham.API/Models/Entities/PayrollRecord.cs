using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class PayrollRecord
    {
        [Key]
        public int PayrollId { get; set; }

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public double TotalActualDays { get; set; }  // Ngày công thực tế (0.5/1.0)
        public int StandardWorkDays { get; set; } = 26;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailyRate { get; set; }

        /// <summary>Monthly | Daily</summary>
        [MaxLength(20)]
        public string SalaryType { get; set; } = "Monthly";

        /// <summary>Công thức: Monthly = (BaseSalary / StandardWorkDays) × TotalActualDays
        ///                   Daily   = DailyRate × TotalActualDays</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        // Backward compat alias
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSalary { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string GeneratedBy { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft | Confirmed | Paid
    }
}
