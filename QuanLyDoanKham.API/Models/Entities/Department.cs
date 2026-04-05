using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "TĂªn tráº¡m khĂ¡m/phĂ²ng ban khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        [MaxLength(20)]
        public string DepartmentCode { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
