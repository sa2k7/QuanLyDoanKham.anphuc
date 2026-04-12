using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lá»‹ch khĂ¡m - sinh tá»± Ä‘á»™ng khi táº¡o Ä‘oĂ n</summary>
    public class ScheduleCalendar
    {
        [Key]
        public int CalendarId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public DateTime ExamDate { get; set; }

        // NULL = toĂ n Ä‘oĂ n, cĂ³ StaffId = lá»‹ch riĂªng nhĂ¢n viĂªn
        public int? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; } = null!;

        // ---- Cháº¥m cĂ´ng QR ----
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [MaxLength(200)]
        public string Note { get; set; } = "";

        public bool IsConfirmed { get; set; } = false;
    }
}
