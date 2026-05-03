using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Danh mục trạm khám (lookup table).
    /// Ví dụ: CHECKIN, SINH_HIEU, LAY_MAU, XQUANG, SIEU_AM, ECG, NOI_KHOA, QC
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

        /// <summary>Sá»‘ phút trung bình phục vụ 1 bệnh nhân tại trạm nĂ y</summary>
        public decimal DefaultMinutesPerPatient { get; set; } = 5;

        /// <summary>Work-in-progress limit: sử‘ BN tá»‘i đa đang thực hiện đá»“ng thời tại trạm</summary>
        public int WipLimit { get; set; } = 1;

        /// <summary>Thứ tự mặc định trong luá»“ng khám</summary>
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
