using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public DateTime ExamDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "CheckIn"; // CheckIn, InProgress, Completed

        public int? ConcludingDoctorId { get; set; }
        [ForeignKey("ConcludingDoctorId")]
        public Staff ConcludingDoctor { get; set; }

        public string Conclusion { get; set; }
        public string Notes { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MedicalRecordService> Services { get; set; } = new List<MedicalRecordService>();
    }
}
