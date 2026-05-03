using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lịch khám - sinh tự đá»™ng khi tạo đoĂ n</summary>
    public class ScheduleCalendar
    {
        [Key]
        public int CalendarId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public DateTime ExamDate { get; set; }

        // NULL = toĂ n đoĂ n, có StaffId = lịch riêng nhân viên
        public int? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; } = null!;

        // ---- Chấm công QR ----
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [MaxLength(200)]
        public string Note { get; set; } = "";

        public bool IsConfirmed { get; set; } = false;
    }
}
