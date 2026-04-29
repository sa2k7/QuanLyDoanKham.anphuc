namespace QuanLyDoanKham.API.Authorization;

public static class PermissionConstants
{
    public const string PolicyPrefix = "PERM_";

    // Map trực tiếp với Permission.PermissionKey trong DB
    // Permissions hiện có trong DB (sau migration CompleteRBACPermissions)
    public static readonly string[] All =
    {
        // HopDong (1-6)
        "HopDong.View",
        "HopDong.Create",
        "HopDong.Edit",
        "HopDong.Approve",
        "HopDong.Reject",
        "HopDong.Upload",

        // DoanKham (10-16)
        "DoanKham.View",
        "DoanKham.Create",
        "DoanKham.Edit",
        "DoanKham.SetPosition",
        "DoanKham.AssignStaff",
        "DoanKham.ManageOwn",
        "DoanKham.Lock",

        // LichKham (20-21)
        "LichKham.ViewOwn",
        "LichKham.ViewAll",

        // ChamCong (30-32)
        "ChamCong.QR",
        "ChamCong.CheckInOut",
        "ChamCong.ViewAll",

        // BaoCao (40-41, 64-65)
        "BaoCao.View",
        "BaoCao.QC",
        "BaoCao.ViewFinance",
        "BaoCao.Export",

        // Kho (50-51, 140-142)
        "Kho.View",
        "Kho.Edit",
        "Kho.Reports",
        "Kho.Import",
        "Kho.Export",

        // QuyetToan/TaiChinh (60, 62-63)
        "QuyetToan.Edit",
        "QuyetToan.Calculate",
        "QuyetToan.Finalize",

        // HeThong (100-102)
        "HeThong.UserManage",
        "HeThong.RoleManage",
        "HeThong.AuditLog",

        // PhongBan (110-111)
        "PhongBan.View",
        "PhongBan.Edit",

        // WorkRule (120-121)
        "WorkRule.View",
        "WorkRule.Edit",

        // AI (130)
        "AI.SuggestStaff",

        // Luong (150-151) - Added in CompleteRBACPermissions migration
        "Luong.View",
        "Luong.Manage",

        // NhanSu (160-161) - Added in CompleteRBACPermissions migration
        "NhanSu.View",
        "NhanSu.Manage",

        // KetQua (170-171) - Added in CompleteRBACPermissions migration
        "KetQua.Write",
        "KetQua.QCApprove",

        // BenhNhan (180) - Added in CompleteRBACPermissions migration
        "BenhNhan.View",

        // DieuPhoi (190) - Added in CompleteRBACPermissions migration
        "DieuPhoi.Edit",
    };
}
