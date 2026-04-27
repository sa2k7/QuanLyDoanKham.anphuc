using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Phòng ban nội bộ</summary>
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
