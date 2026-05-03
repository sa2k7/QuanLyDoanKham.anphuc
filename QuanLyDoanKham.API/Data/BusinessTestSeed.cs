using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Data;

public static class BusinessTestSeed
{
    private const string TestPassword = "Test@123456";

    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        ["ContractCreator"] = ["HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Upload"],
        ["ContractApprover"] = ["HopDong.View", "HopDong.Approve", "HopDong.Reject"],
        ["ContractManager"] = ["HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Upload", "HopDong.Approve", "HopDong.Reject", "PhongBan.View", "WorkRule.View"],
        ["MedicalGroupManager"] = ["HopDong.View", "DoanKham.View", "DoanKham.Create", "DoanKham.Edit", "DoanKham.SetPosition", "DoanKham.AssignStaff", "DoanKham.ManageOwn", "DoanKham.Lock", "LichKham.ViewAll", "ChamCong.QR", "ChamCong.ViewAll", "AI.SuggestStaff", "BenhNhan.View", "NhanSu.View", "DieuPhoi.Edit"],
        ["GroupLeader"] = ["HopDong.View", "DoanKham.View", "DoanKham.ManageOwn", "LichKham.ViewOwn", "LichKham.ViewAll", "ChamCong.QR", "ChamCong.CheckInOut", "ChamCong.ViewAll", "BenhNhan.View"],
        ["WarehouseManager"] = ["Kho.View", "Kho.Edit", "Kho.Reports", "Kho.Import", "Kho.Export"],
        ["Accountant"] = ["HopDong.View", "BaoCao.View", "BaoCao.ViewFinance", "BaoCao.Export", "Luong.View", "Luong.Manage", "QuyetToan.Edit", "QuyetToan.Calculate", "QuyetToan.Finalize", "PhongBan.View"],
        ["PayrollManager"] = ["HopDong.View", "BaoCao.View", "BaoCao.ViewFinance", "BaoCao.Export", "Luong.View", "Luong.Manage", "QuyetToan.Edit", "QuyetToan.Calculate", "QuyetToan.Finalize", "PhongBan.View"],
        ["MedicalStaff"] = ["DoanKham.View", "LichKham.ViewOwn", "ChamCong.CheckInOut", "BenhNhan.View"],
        ["PersonnelManager"] = ["NhanSu.View", "NhanSu.Manage", "Luong.View", "ChamCong.ViewAll", "LichKham.ViewAll", "PhongBan.View", "PhongBan.Edit"],
        ["QA"] = ["DoanKham.View", "LichKham.ViewAll", "BaoCao.View", "BaoCao.QC", "KetQua.QCApprove", "BenhNhan.View"]
    };

    private static readonly TestAccount[] Accounts =
    [
        new("admin_master", "System Administrator", "Admin", "Ban giam doc", "Quan tri he thong", "Admin"),
        new("contract_creator", "Contract Creator", "ContractCreator", "Hanh chinh nhan su", "Tao hop dong", "NhanVienHoTro"),
        new("contract_approver", "Contract Approver", "ContractApprover", "Phong nhan su", "Duyet hop dong", "NhanVienHoTro"),
        new("medical_group_manager", "Medical Group Manager", "MedicalGroupManager", "Quan ly doan kham", "Quan ly doan", "NhanVienHoTro"),
        new("group_leader", "Group Leader", "GroupLeader", "Doan kham", "Truong doan", "NhanVienHoTro"),
        new("warehouse_staff", "Warehouse Staff", "WarehouseManager", "Kho", "Nhan vien kho", "NhanVienHoTro"),
        new("payroll_accountant", "Payroll Accountant", "Accountant", "Ke toan", "Ke toan luong/quyet toan", "NhanVienHoTro"),
        new("staff_user", "Staff User", "MedicalStaff", "Nhan vien doan kham", "Nhan vien thuong", "NhanVienHoTro")
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsurePermissionsAsync(db);
        await EnsureRolesAsync(db);
        await EnsureRolePermissionsAsync(db);
        await EnsureAccountsAsync(db);
    }

    private static async Task EnsurePermissionsAsync(ApplicationDbContext db)
    {
        var existing = await db.Permissions.ToDictionaryAsync(p => p.PermissionKey);

        foreach (var key in PermissionConstants.All)
        {
            if (existing.ContainsKey(key)) continue;

            var module = key.Contains('.') ? key.Split('.')[0] : "General";
            db.Permissions.Add(new Permission
            {
                PermissionKey = key,
                PermissionName = key,
                Module = module
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRolesAsync(ApplicationDbContext db)
    {
        var roleDescriptions = new Dictionary<string, string>
        {
            ["ContractCreator"] = "Tao va submit hop dong",
            ["ContractApprover"] = "Duyet hoac tu choi hop dong"
        };

        foreach (var roleName in RolePermissions.Keys.Concat(["Admin"]).Distinct())
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                db.Roles.Add(new AppRole
                {
                    RoleName = roleName,
                    Description = roleDescriptions.GetValueOrDefault(roleName, roleName)
                });
            }
            else if (string.IsNullOrWhiteSpace(role.Description))
            {
                role.Description = roleDescriptions.GetValueOrDefault(roleName, roleName);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRolePermissionsAsync(ApplicationDbContext db)
    {
        var permissions = await db.Permissions.ToDictionaryAsync(p => p.PermissionKey, p => p.PermissionId);

        foreach (var (roleName, permissionKeys) in RolePermissions)
        {
            var role = await db.Roles.FirstAsync(r => r.RoleName == roleName);
            await db.RolePermissions
                .Where(rp => rp.RoleId == role.RoleId)
                .ExecuteDeleteAsync();

            foreach (var key in permissionKeys.Distinct())
            {
                if (!permissions.TryGetValue(key, out var permissionId)) continue;
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.RoleId,
                    PermissionId = permissionId
                });
            }
        }

        var admin = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (admin != null)
        {
            var adminPermissionIds = await db.RolePermissions
                .Where(rp => rp.RoleId == admin.RoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            foreach (var permissionId in permissions.Values.Except(adminPermissionIds))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = admin.RoleId,
                    PermissionId = permissionId
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureAccountsAsync(ApplicationDbContext db)
    {
        var roles = await db.Roles.ToDictionaryAsync(r => r.RoleName, r => r.RoleId);

        foreach (var account in Accounts)
        {
            var staff = await EnsureStaffAsync(db, account);
            var roleId = roles[account.RoleName];
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == account.Username);

            if (user == null)
            {
                user = new AppUser
                {
                    Username = account.Username,
                    CreatedAt = DateTime.Now
                };
                db.Users.Add(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword);
            user.FullName = account.FullName;
            user.RoleId = roleId;
            user.StaffId = staff.StaffId;
            user.Email = $"{account.Username}@test.local";
            user.IsActive = true;
        }

        await db.SaveChangesAsync();
    }

    private static async Task<Staff> EnsureStaffAsync(ApplicationDbContext db, TestAccount account)
    {
        var staffCode = account.Username.Length <= 20 ? account.Username : account.Username[..20];
        var staff = await db.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode == staffCode);
        if (staff == null)
        {
            staff = new Staff
            {
                EmployeeCode = staffCode,
                CreatedDate = DateTime.Now
            };
            db.Staffs.Add(staff);
        }

        staff.FullName = account.FullName;
        staff.FullNameUnsigned = account.FullName;
        staff.DepartmentName = account.Department;
        staff.JobTitle = account.JobTitle;
        staff.StaffType = account.StaffType;
        staff.Email = $"{account.Username}@test.local";
        staff.PhoneNumber ??= "0900000000";
        staff.EmployeeType = "NoiBo";
        staff.SalaryType = "ByDay";
        staff.DailyRate = staff.DailyRate == 0 ? 500000 : staff.DailyRate;
        staff.BaseSalary = staff.BaseSalary == 0 ? 12000000 : staff.BaseSalary;
        staff.StandardWorkDays = staff.StandardWorkDays == 0 ? 26 : staff.StandardWorkDays;
        staff.IsActive = true;
        staff.ModifiedDate = DateTime.Now;

        await db.SaveChangesAsync();
        return staff;
    }

    private sealed record TestAccount(
        string Username,
        string FullName,
        string RoleName,
        string Department,
        string JobTitle,
        string StaffType);
}
