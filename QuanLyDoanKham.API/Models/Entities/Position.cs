using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Danh mục vị trí chuẩn</summary>
    public class Position
    {
        [Key]
        public int PositionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // KHAM_NOI, LAY_MAU...

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string SpecialtyRequired { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }
    }
}
