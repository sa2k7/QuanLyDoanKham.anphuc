using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [MaxLength(20)]
        public string? EmployeeCode { get; set; }

        [Required(ErrorMessage = "Há» vĂ  tĂªn nhĂ¢n sá»± khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [MaxLength(100)]
        public string? FullNameUnsigned { get; set; }

        public int? BirthYear { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? IDCardNumber { get; set; }

        [MaxLength(20)]
        public string? TaxCode { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string? BankAccountName { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        // BacSi, DieuDuong, KyThuatVien, NhanVienHoTro
        [MaxLength(50)]
        public string? StaffType { get; set; }

        [MaxLength(100)]
        public string? Specialty { get; set; } // ChuyĂªn khoa

        [MaxLength(100)]
        public string? DepartmentName { get; set; } // TĂªn phĂ²ng ban/bá»™ pháº­n (LÆ°u dáº¡ng chuá»—i)

        [MaxLength(50)]
        public string? CertificateNumber { get; set; } // Sá»‘ CCHN

        [MaxLength(20)]
        public string? EmployeeType { get; set; } // NoiBo, ThueNgoai

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailyRate { get; set; } // ÄÆ¡n giĂ¡ ngĂ y

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; } // LÆ°Æ¡ng cÆ¡ báº£n thĂ¡ng

        public int StandardWorkDays { get; set; } = 26; // CĂ´ng chuáº©n máº·c Ä‘á»‹nh

        // Loáº¡i lÆ°Æ¡ng: ByDay (tĂ­nh theo DailyRate), ByMonth (BaseSalary Ă· StdDays Ă— actual)
        [MaxLength(20)]
        public string SalaryType { get; set; } = "ByDay";

        public string? IDCardFrontPath { get; set; }
        public string? IDCardBackPath { get; set; }
        public string? PracticeCertificatePath { get; set; }
        public string? AvatarPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        public ICollection<GroupStaffDetail> GroupStaffDetails { get; set; } = new List<GroupStaffDetail>();
    }
}
