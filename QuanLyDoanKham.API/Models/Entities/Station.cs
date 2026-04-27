using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Danh má»¥c tráº¡m khĂ¡m (lookup table).
    /// VĂ­ dá»¥: CHECKIN, SINH_HIEU, LAY_MAU, XQUANG, SIEU_AM, ECG, NOI_KHOA, QC
    /// </summary>
    public class Station
    {
        [Key]
        [MaxLength(50)]
        public string StationCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string StationName { get; set; } = null!;

        /// <summary>ADMIN | CLINICAL | LAB | IMAGING</summary>
        [MaxLength(50)]
        public string ServiceType { get; set; } = "CLINICAL";

        /// <summary>Sá»‘ phĂºt trung bĂ¬nh phá»¥c vá»¥ 1 bá»‡nh nhĂ¢n táº¡i tráº¡m nĂ y</summary>
        public decimal DefaultMinutesPerPatient { get; set; } = 5;

        /// <summary>Work-in-progress limit: sá»‘ BN tá»‘i Ä‘a Ä‘ang thá»±c hiá»‡n Ä‘á»“ng thá»i táº¡i tráº¡m</summary>
        public int WipLimit { get; set; } = 1;

        /// <summary>Thá»© tá»± máº·c Ä‘á»‹nh trong luá»“ng khĂ¡m</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Lọc theo giới tính: null = tất cả, "Nam" = chỉ nam, "Nữ" = chỉ nữ.
        /// Dùng để tự động bỏ qua trạm PHỤ KHOA cho bệnh nhân nam khi check-in.
        /// </summary>
        [MaxLength(10)]
        public string? RequiredGender { get; set; } = null;

        public bool IsActive { get; set; } = true;

    }
}
