using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [MaxLength(100)]
        public string ShortName { get; set; }

        [Required(ErrorMessage = "TĂªn cĂ´ng ty khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(50)]
        public string TaxCode { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        [MaxLength(200)]
        public string ContactPerson { get; set; }

        [MaxLength(15)]
        public string ContactPhone { get; set; }

        public int? CompanySize { get; set; }

        [MaxLength(100)]
        public string Industry { get; set; }

        public string Notes { get; set; }
    }
}
