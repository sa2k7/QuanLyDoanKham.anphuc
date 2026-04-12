using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyDoanKham.API.Models
{
    /// <summary>Vá»‹ trĂ­ trong Ä‘oĂ n khĂ¡m: Tiáº¿p nháº­n, KhĂ¡m ná»™i, Láº¥y mĂ¡u...</summary>
    public class MedicalGroupPosition
    {
        [Key]
        public int Id { get; set; }

        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public virtual Position? Position { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual MedicalGroup MedicalGroup { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } = null!; // "Tiáº¿p nháº­n", "KhĂ¡m ná»™i", "SiĂªu Ă¢m"...

        public int RequiredCount { get; set; } = 1; // Sá»‘ ngÆ°á» i cáº§n
        public int AssignedCount { get; set; } = 0; // Sá»‘ ngÆ°á» i Ä‘Ă£ gĂ¡n (tá»± tá»•ng há»£p)

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        public int SortOrder { get; set; } = 0;
    }
}
