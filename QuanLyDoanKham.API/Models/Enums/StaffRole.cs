namespace QuanLyDoanKham.API.Models.Enums
{
    /// <summary>
    /// Vai trò chuyên môn của nhân sự trong chiến dịch khám sức khỏe doanh nghiệp.
    /// Chỉ các vai trò này mới được phép phân công vào slot chiến dịch.
    /// </summary>
    public enum StaffRole
    {
        /// <summary>Tiếp nhận bệnh nhân</summary>
        TiepNhan,

        /// <summary>Khám nội khoa</summary>
        KhamNoi,

        /// <summary>Khám ngoại khoa</summary>
        KhamNgoai,

        /// <summary>Lấy mẫu xét nghiệm</summary>
        LayMau,

        /// <summary>Khám sản phụ khoa</summary>
        KhamSan,

        /// <summary>Siêu âm</summary>
        SieuAm,

        /// <summary>Trưởng đoàn khám</summary>
        TruongDoan
    }
}
