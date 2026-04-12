namespace QuanLyDoanKham.API.Authorization;

public static class PermissionConstants
{
    public const string PolicyPrefix = "PERM_";

    // Map trực tiếp với Permission.PermissionKey seed
    public static readonly string[] All =
    {
        "HopDong.View","HopDong.Create","HopDong.Edit","HopDong.Approve","HopDong.Reject","HopDong.Upload",
        "DoanKham.View","DoanKham.Create","DoanKham.Edit","DoanKham.SetPosition","DoanKham.AssignStaff","DoanKham.ManageOwn","DoanKham.Lock",
        "LichKham.ViewOwn","LichKham.ViewAll",
        "ChamCong.QR","ChamCong.CheckInOut","ChamCong.ViewAll",
        "Kho.View","Kho.Import","Kho.Export","Kho.Edit",
        "Luong.View","Luong.Manage",
        "NhanSu.View","NhanSu.Manage",
        "BaoCao.View","BaoCao.Export","BaoCao.QC",
        "KetQua.Write","KetQua.QCApprove",
        "BenhNhan.View",
        "DieuPhoi.Edit", "QuyetToan.Edit",
        "HeThong.UserManage","HeThong.RoleManage",
        // PHASE 1: Fine-grained financial & reporting permissions
        "QuyetToan.Calculate","QuyetToan.Finalize",
        "BaoCao.ViewFinance"
    };
}
