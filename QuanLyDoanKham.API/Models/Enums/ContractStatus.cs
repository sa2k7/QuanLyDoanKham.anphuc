namespace QuanLyDoanKham.API.Models.Enums
{
    /// <summary>Trạng thái hợp đồng khám sức khỏe doanh nghiệp</summary>
    public enum ContractStatus
    {
        /// <summary>Bản nháp - Đang soạn thảo</summary>
        Draft = 0,
        
        /// <summary>Chờ duyệt - Đã gửi đi phê duyệt</summary>
        PendingApproval = 1,
        
        /// <summary>Đã duyệt - Được phê duyệt hoàn toàn</summary>
        Approved = 2,
        
        /// <summary>Đang hoạt động - Đang thực hiện khám</summary>
        Active = 3,
        
        /// <summary>Đã hoàn thành - Kết thúc tất cả đợt khám</summary>
        Finished = 4,
        
        /// <summary>Đã từ chối - Không được phê duyệt</summary>
        Rejected = 5,
        
        /// <summary>Đã khóa - Không thể chỉnh sửa</summary>
        Locked = 6,
        
        /// <summary>Đã hủy - Bị hủy bỏ</summary>
        Cancelled = 7
    }
}
