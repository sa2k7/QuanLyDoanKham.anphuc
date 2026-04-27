using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract? HealthContract { get; set; }

        // Link truc tiep den Doan kham (de import DSCN va loc theo doan)
        public int? MedicalGroupId { get; set; }
        [ForeignKey("MedicalGroupId")]
        public MedicalGroup? MedicalGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? IDCardNumber { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(50)]
        public string? Source { get; set; } = "Manual"; // Manual, ExcelImport

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
