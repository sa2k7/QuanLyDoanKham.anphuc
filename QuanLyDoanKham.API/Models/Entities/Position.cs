using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Danh má»¥c vá»‹ trĂ­ chuáº©n</summary>
    public class Position
    {
        [Key]
        public int PositionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = ""; // KHAM_NOI, LAY_MAU...

        [ForeignKey("Code")]
        public virtual Station? Station { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(100)]
        public string SpecialtyRequired { get; set; } = "";

        [MaxLength(200)]
        public string Description { get; set; } = "";
    }
}
