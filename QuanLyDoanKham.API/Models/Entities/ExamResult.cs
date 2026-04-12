using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class ExamResult
    {
        [Key]
        public int ExamResultId { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        [MaxLength(100)]
        public string ExamType { get; set; } = null!; // VĂ­ dá»¥: KhĂ¡m ná»™i, XĂ©t nghiá»‡m mĂ¡u, X-Quang

        public string Result { get; set; } = null!;

        [MaxLength(500)]
        public string Diagnosis { get; set; } = null!;

        public int? DoctorStaffId { get; set; }
        [ForeignKey("DoctorStaffId")]
        public Staff Doctor { get; set; } = null!;

        public DateTime ExamDate { get; set; } = DateTime.Now;
    }
}
