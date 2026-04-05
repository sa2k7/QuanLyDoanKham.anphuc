namespace QuanLyDoanKham.API.Authorization;

public static class PermissionConstants
{
    public const string PolicyPrefix = "PERM_";

    // Map trực tiếp với Permission.PermissionKey seed
    public static readonly string[] All =
    {
        "HopDong.View","HopDong.Create","HopDong.Edit","HopDong.Approve","HopDong.Reject","HopDong.Upload",
        "DoanKham.View","DoanKham.Create","DoanKham.Edit","DoanKham.SetPosition","DoanKham.AssignStaff","DoanKham.ManageOwn",
        "LichKham.ViewOwn","LichKham.ViewAll",
        "ChamCong.QR","ChamCong.CheckInOut","ChamCong.ViewAll",
        "Kho.View","Kho.Import","Kho.Export",
        "Luong.View","Luong.Manage",
        "NhanSu.View","NhanSu.Manage",
        "BaoCao.View","BaoCao.Export",
        "HeThong.UserManage","HeThong.RoleManage"
    };
}
