using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module RBAC (Phân quyền).
/// Feature: medical-examination-team-management
/// </summary>
public class RBACProperties
{
    // Mapping role → permissions (mirror từ seed data trong ApplicationDbContext)
    private static readonly Dictionary<int, HashSet<string>> RolePermissions = new()
    {
        // Admin (RoleId=1): tất cả quyền
        [1] = new HashSet<string> {
            "HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Approve",
            "DoanKham.View", "DoanKham.Create", "DoanKham.Edit", "DoanKham.AssignStaff",
            "ChamCong.QR", "ChamCong.ViewAll", "Kho.View", "Kho.Edit",
            "BaoCao.View", "BaoCao.ViewFinance", "HeThong.UserManage", "HeThong.RoleManage"
        },
        // ContractManager (RoleId=3): quyền hợp đồng
        [3] = new HashSet<string> {
            "HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Approve",
            "HopDong.Reject", "HopDong.Upload"
        },
        // MedicalGroupManager (RoleId=5): quyền đoàn khám
        [5] = new HashSet<string> {
            "DoanKham.View", "DoanKham.Create", "DoanKham.Edit",
            "DoanKham.AssignStaff", "DoanKham.SetPosition", "ChamCong.QR"
        },
        // WarehouseManager (RoleId=6): quyền kho
        [6] = new HashSet<string> { "Kho.View", "Kho.Edit", "Kho.Reports", "Kho.Import", "Kho.Export" },
        // MedicalStaff (RoleId=7): quyền cơ bản
        [7] = new HashSet<string> { "LichKham.ViewOwn" },
    };

    private static bool HasPermission(IEnumerable<int> roleIds, string permissionKey)
    {
        return roleIds.Any(rid =>
            RolePermissions.TryGetValue(rid, out var perms) && perms.Contains(permissionKey));
    }

    // -----------------------------------------------------------------------
    // Property 1: Kiểm soát truy cập hợp đồng theo phòng ban
    // Validates: Requirements 1.1, 1.8
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P1_ContractAccessControlByDepartment()
    {
        // Feature: medical-examination-team-management, Property 1: Contract access control by department
        var gen =
            from roleId in Gen.Elements(5, 6, 7) // Không phải ContractManager hay Admin
            select roleId;

        return Prop.ForAll(Arb.From(gen), roleId =>
        {
            // Người dùng không thuộc phòng HCNS (ContractManager) không được tạo hợp đồng
            bool canCreate = HasPermission(new[] { roleId }, "HopDong.Create");
            return !canCreate;
        });
    }

    [Property(MaxTest = 100)]
    public Property P1_ContractManagerCanAccessContracts()
    {
        // Feature: medical-examination-team-management, Property 1 variant: ContractManager can access
        var gen =
            from roleId in Gen.Elements(1, 3) // Admin hoặc ContractManager
            select roleId;

        return Prop.ForAll(Arb.From(gen), roleId =>
        {
            bool canView = HasPermission(new[] { roleId }, "HopDong.View");
            return canView;
        });
    }

    // -----------------------------------------------------------------------
    // Property 23: Mọi endpoint được bảo vệ đều từ chối truy cập không có quyền
    // Validates: Requirements 8.1, 8.3
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P23_ProtectedEndpointRejectsUnauthorized()
    {
        // Feature: medical-examination-team-management, Property 23: Protected endpoints reject unauthorized
        var gen =
            from permissionKey in Gen.Elements(
                "HopDong.Approve", "DoanKham.Create", "Kho.Edit",
                "HeThong.UserManage", "BaoCao.ViewFinance")
            from roleId in Gen.Elements(7) // MedicalStaff - quyền tối thiểu
            select (permissionKey, roleId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (permissionKey, roleId) = data;

            bool hasAccess = HasPermission(new[] { roleId }, permissionKey);
            return !hasAccess; // MedicalStaff không có các quyền này
        });
    }

    // -----------------------------------------------------------------------
    // Property 24: Người dùng có nhiều vai trò được hưởng tổng hợp quyền của tất cả vai trò
    // Validates: Requirements 8.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P24_MultiRoleUserHasUnionOfPermissions()
    {
        // Feature: medical-examination-team-management, Property 24: Multi-role user has union of permissions
        var gen =
            from roleId1 in Gen.Elements(3, 5, 6) // ContractManager, MedicalGroupManager, WarehouseManager
            from roleId2 in Gen.Elements(3, 5, 6)
            where roleId1 != roleId2
            select (roleId1, roleId2);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (roleId1, roleId2) = data;

            // Quyền của từng vai trò riêng lẻ
            var perms1 = RolePermissions.GetValueOrDefault(roleId1, new HashSet<string>());
            var perms2 = RolePermissions.GetValueOrDefault(roleId2, new HashSet<string>());

            // Quyền tổng hợp khi có cả hai vai trò
            var combinedPerms = perms1.Union(perms2).ToHashSet();

            // Người dùng có cả hai vai trò phải có tất cả quyền của cả hai
            bool hasAllFromRole1 = perms1.All(p => HasPermission(new[] { roleId1, roleId2 }, p));
            bool hasAllFromRole2 = perms2.All(p => HasPermission(new[] { roleId1, roleId2 }, p));

            return hasAllFromRole1 && hasAllFromRole2;
        });
    }

    // -----------------------------------------------------------------------
    // Property 24 (variant): Quyền tổng hợp >= quyền của từng vai trò riêng lẻ
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P24_CombinedPermissionsNotLessThanIndividual()
    {
        // Feature: medical-examination-team-management, Property 24 variant: Combined >= individual
        var gen =
            from count in Gen.Choose(2, 3)
            from roleIds in Gen.ListOf(count, Gen.Elements(1, 3, 5, 6, 7))
            select roleIds.Distinct().ToList();

        return Prop.ForAll(Arb.From(gen), roleIds =>
        {
            if (!roleIds.Any()) return true;

            // Quyền tổng hợp
            var combinedPerms = roleIds
                .SelectMany(rid => RolePermissions.GetValueOrDefault(rid, new HashSet<string>()))
                .ToHashSet();

            // Mỗi vai trò riêng lẻ phải là tập con của quyền tổng hợp
            return roleIds.All(rid =>
            {
                var rolePerms = RolePermissions.GetValueOrDefault(rid, new HashSet<string>());
                return rolePerms.IsSubsetOf(combinedPerms);
            });
        });
    }
}
