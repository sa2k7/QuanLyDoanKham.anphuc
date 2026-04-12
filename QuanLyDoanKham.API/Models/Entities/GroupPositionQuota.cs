using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Äá»‹nh biĂªn vá»‹ trĂ­ cho tá»«ng Ä‘oĂ n</summary>
    public class GroupPositionQuota
    {
        [Key]
        public int Id { get; set; }

        public int MedicalGroupId { get; set; }
        [ForeignKey("MedicalGroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public int PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position Position { get; set; } = null!;

        public int Required { get; set; } = 1;
        public int Assigned { get; set; } = 0;
    }
}
