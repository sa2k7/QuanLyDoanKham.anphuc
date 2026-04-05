using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Định biên vị trí cho từng đoàn</summary>
    public class GroupPositionQuota
    {
        [Key]
        public int Id { get; set; }

        public int MedicalGroupId { get; set; }
        [ForeignKey("MedicalGroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public int PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position Position { get; set; }

        public int Required { get; set; } = 1;
        public int Assigned { get; set; } = 0;
    }
}
