using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class ExamService
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ServiceName { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } // Khám lâm sàng, Xét nghiệm máu, Siêu âm, X-Quang

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
