import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

/**
 * Composable cung cấp các helper kiểm tra phân quyền
 * trong Vue components và template.
 *
 * Cách dùng:
 * const { can, hasRole, isAdmin } = usePermission()
 * v-if="can('HopDong.Approve')"
 * v-if="hasRole('ContractManager')"
 */
export function usePermission() {
  const auth = useAuthStore()

  /** Kiểm tra permission key */
  const can = (permissionKey) => auth.hasPermission(permissionKey)

  /** Kiểm tra có ít nhất 1 trong danh sách permission */
  const canAny = (...keys) => auth.hasAnyPermission(...keys)

  /** Kiểm tra có TẤT CẢ permissions */
  const canAll = (...keys) => auth.hasAllPermissions(...keys)

  /** Kiểm tra role */
  const hasRole = (roleName) => auth.hasRole(roleName)

  /** Kiểm tra ít nhất 1 role */
  const hasAnyRole = (...roles) => auth.hasAnyRole(...roles)

  const isAdmin = computed(() => auth.isAdmin)

  // ──────────────────────────────────────────────────────────────
  // Shortcut permissions thường dùng
  // ──────────────────────────────────────────────────────────────

  // Hợp đồng
  const canViewContract   = computed(() => can('HopDong.View') || auth.isAdmin)
  const canCreateContract = computed(() => can('HopDong.Create') || auth.isAdmin)
  const canEditContract   = computed(() => can('HopDong.Edit') || auth.isAdmin)
  const canApproveContract= computed(() => can('HopDong.Approve') || auth.isAdmin)
  const canRejectContract = computed(() => can('HopDong.Reject') || auth.isAdmin)
  const canUploadContract = computed(() => can('HopDong.Upload') || auth.isAdmin)

  // Đoàn khám
  const canViewGroup      = computed(() => can('DoanKham.View') || auth.isAdmin)
  const canCreateGroup    = computed(() => can('DoanKham.Create') || auth.isAdmin)
  const canEditGroup      = computed(() => can('DoanKham.Edit') || auth.isAdmin)
  const canAssignStaff    = computed(() => can('DoanKham.AssignStaff') || auth.isAdmin)
  const canManageOwnGroup = computed(() => can('DoanKham.ManageOwn') || auth.isAdmin)

  // Chấm công
  const canOpenQR         = computed(() => can('ChamCong.QR') || auth.isAdmin)
  const canCheckInOut     = computed(() => can('ChamCong.CheckInOut') || auth.isAdmin)
  const canViewAttendance = computed(() => can('ChamCong.ViewAll') || auth.isAdmin)

  // Kho
  const canViewWarehouse  = computed(() => can('Kho.View') || auth.isAdmin)
  const canImportWarehouse= computed(() => can('Kho.Import') || auth.isAdmin)
  const canExportWarehouse= computed(() => can('Kho.Export') || auth.isAdmin)

  // Nhân sự
  const canViewStaff      = computed(() => can('NhanSu.View') || auth.isAdmin)
  const canManageStaff    = computed(() => can('NhanSu.Manage') || auth.isAdmin)

  // Lương
  const canViewPayroll    = computed(() => can('Luong.View') || auth.isAdmin)
  const canManagePayroll  = computed(() => can('Luong.Manage') || auth.isAdmin)

  // Báo cáo
  const canViewReport     = computed(() => can('BaoCao.View') || auth.isAdmin)
  const canExportReport   = computed(() => can('BaoCao.Export') || auth.isAdmin)

  // Hệ thống
  const canManageUsers    = computed(() => can('HeThong.UserManage') || auth.isAdmin)
  const canManageRoles    = computed(() => can('HeThong.RoleManage') || auth.isAdmin)

  return {
    // Core helpers
    can,
    canAny,
    canAll,
    hasRole,
    hasAnyRole,
    isAdmin,

    // Contract permissions
    canViewContract,
    canCreateContract,
    canEditContract,
    canApproveContract,
    canRejectContract,
    canUploadContract,

    // Group permissions
    canViewGroup,
    canCreateGroup,
    canEditGroup,
    canAssignStaff,
    canManageOwnGroup,

    // Attendance permissions
    canOpenQR,
    canCheckInOut,
    canViewAttendance,

    // Warehouse permissions
    canViewWarehouse,
    canImportWarehouse,
    canExportWarehouse,

    // Staff permissions
    canViewStaff,
    canManageStaff,

    // Payroll permissions
    canViewPayroll,
    canManagePayroll,

    // Report permissions
    canViewReport,
    canExportReport,

    // System permissions
    canManageUsers,
    canManageRoles
  }
}
