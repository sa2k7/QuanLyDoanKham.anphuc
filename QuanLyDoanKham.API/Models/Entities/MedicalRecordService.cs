using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class MedicalRecordService
    {
        [Key]
        public int Id { get; set; }

        public int RecordId { get; set; }
        [ForeignKey("RecordId")]
        public MedicalRecord MedicalRecord { get; set; }

        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public ExamService ExamService { get; set; }

        public int? ExecutingStaffId { get; set; }
        [ForeignKey("ExecutingStaffId")]
        public Staff ExecutingStaff { get; set; } // Người siêu âm / lấy máu

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0;

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "PaidByCompany"; // Unpaid, PaidByCompany, PaidByPatient

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed

        public string ResultData { get; set; } // JSON or text

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
    }
}
