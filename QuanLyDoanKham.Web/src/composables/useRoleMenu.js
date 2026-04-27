/**
 * useRoleMenu — Composable quản lý Menu và Home Dashboard theo vai trò (Role)
 *
 * Mỗi Role có:
 * - `menuItems`: Danh sách icon menu hiển thị trong Sidebar
 * - `defaultHomePanel`: Component panel hiển thị ở trang chủ
 * - `themeColor`: Màu accent chính của role đó
 * - `roleLabel`: Tên hiển thị
 *
 * Thứ tự ưu tiên xác định Role hiển thị:
 * Admin > MedicalGroupManager/QC > PersonnelManager/MedicalStaff > ContractManager > default
 */
import {
  LayoutDashboard, Building2, FileText, Stethoscope, UserRound,
  Package, Activity, Calculator, Users as UsersIcon, Wallet,
  BarChart3, User, ShieldCheck, ClipboardList, CalendarCheck,
  History, ClipboardCheck
} from 'lucide-vue-next'
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

// ── MENU DEFINITIONS PER ROLE ────────────────────────────────────────────────

const ADMIN_MENU = [
  { id: 'home',             name: 'Tổng quan',   icon: LayoutDashboard },
  { id: 'companies',        name: 'Công ty',      icon: Building2,    permission: 'HopDong.View' },
  { id: 'contracts',        name: 'Hợp đồng',    icon: FileText,     permission: 'HopDong.View' },
  { id: 'groups',           name: 'Đoàn khám',   icon: Stethoscope,  permission: 'DoanKham.View' },
  { id: 'patients',         name: 'Bệnh nhân',   icon: UserRound,    permission: 'DoanKham.View' },
  { id: 'settlement-report',name: 'Quyết toán',  icon: Calculator,   permission: 'BaoCao.View' },
  { id: 'staff',            name: 'Nhân sự',     icon: UsersIcon,    permission: 'NhanSu.View' },
  { id: 'payroll',          name: 'Tính lương',  icon: Wallet,       permission: 'Luong.View' },
  { id: 'attendance-summary', name: 'Chấm công', icon: ClipboardCheck, permission: 'ChamCong.ViewAll' },
  { id: 'supplies',         name: 'Vật tư',      icon: Package,      permission: 'Kho.View' },
  { id: 'analytics',        name: 'Thống kê',    icon: BarChart3,    permission: 'BaoCao.View' },
  { id: 'users',            name: 'Tài khoản',   icon: User,         permission: 'HeThong.UserManage' },
  { id: 'permissions',      name: 'Phân quyền',  icon: ShieldCheck,  permission: 'HeThong.RoleManage' },
]

// Bác sĩ / Kỹ Thuật Viên - tập trung vào phòng khám
const DOCTOR_MENU = [
  { id: 'home',          name: 'Hôm nay',       icon: CalendarCheck },
  { id: 'patients',      name: 'Bệnh nhân',     icon: UserRound,  permission: 'DoanKham.View' },
  { id: 'my-schedule',   name: 'Lịch của tôi',  icon: ClipboardList },
]

// Lễ tân / Điều phối - check-in và danh sách bệnh nhân
const RECEPTIONIST_MENU = [
  { id: 'home',          name: 'Tổng quan',     icon: LayoutDashboard },
  { id: 'patients',      name: 'Bệnh nhân',     icon: UserRound,   permission: 'DoanKham.View' },
  { id: 'groups',        name: 'Đoàn khám',     icon: Stethoscope, permission: 'DoanKham.View' },
]

// Kế toán / Tài chính
const FINANCE_MENU = [
  { id: 'home',             name: 'Tổng quan',   icon: LayoutDashboard },
  { id: 'settlement-report',name: 'Quyết toán',  icon: Calculator,  permission: 'BaoCao.View' },
  { id: 'analytics',        name: 'Thống kê',    icon: BarChart3,   permission: 'BaoCao.View' },
  { id: 'payroll',          name: 'Tính lương',  icon: Wallet,      permission: 'Luong.View' },
  { id: 'supplies',         name: 'Vật tư',      icon: Package,     permission: 'Kho.View' },
]

// QC / OMS Manager
const QC_MENU = [
  { id: 'home',          name: 'Giám sát',      icon: LayoutDashboard },
  { id: 'patients',      name: 'Bệnh nhân',     icon: UserRound,   permission: 'DoanKham.View' },
  { id: 'groups',        name: 'Đoàn khám',     icon: Stethoscope, permission: 'DoanKham.View' },
  { id: 'analytics',     name: 'Thống kê',      icon: BarChart3,   permission: 'BaoCao.View' },
]

// Contract Manager
const CONTRACT_MENU = [
  { id: 'home',      name: 'Tổng quan',    icon: LayoutDashboard },
  { id: 'companies', name: 'Công ty',      icon: Building2,   permission: 'HopDong.View' },
  { id: 'contracts', name: 'Hợp đồng',    icon: FileText,    permission: 'HopDong.View' },
  { id: 'groups',    name: 'Đoàn khám',   icon: Stethoscope, permission: 'DoanKham.View' },
]

// ── ROLE PROFILES ─────────────────────────────────────────────────────────────

const ROLE_PROFILES = {
  Admin: {
    menuItems: ADMIN_MENU,
    homePanel: 'AdminHomePanel',
    themeColor: 'text-slate-900',
    themeBg: 'from-slate-800 to-slate-700',
    accentClass: 'bg-slate-900 text-white',
    roleLabel: 'Quản trị hệ thống',
    defaultMenu: 'home',
    greeting: 'Bảng điều khiển toàn hệ thống'
  },
  MedicalGroupManager: {
    menuItems: QC_MENU,
    homePanel: 'QcHomePanel',
    themeColor: 'text-emerald-700',
    themeBg: 'from-emerald-600 to-teal-500',
    accentClass: 'bg-emerald-600 text-white',
    roleLabel: 'Quản lý đoàn khám',
    defaultMenu: 'home',
    greeting: 'Trung tâm giám sát'
  },
  MedicalStaff: {
    menuItems: DOCTOR_MENU,
    homePanel: 'DoctorHomePanel',
    themeColor: 'text-sky-700',
    themeBg: 'from-sky-600 to-indigo-500',
    accentClass: 'bg-sky-600 text-white',
    roleLabel: 'Nhân viên y tế',
    defaultMenu: 'home',
    greeting: 'Phiên làm việc của bạn'
  },
  ContractManager: {
    menuItems: CONTRACT_MENU,
    homePanel: 'ContractHomePanel',
    themeColor: 'text-teal-700',
    themeBg: 'from-teal-600 to-emerald-500',
    accentClass: 'bg-teal-600 text-white',
    roleLabel: 'Quản lý hợp đồng',
    defaultMenu: 'home',
    greeting: 'Trung tâm hợp đồng'
  },
  PersonnelManager: {
    menuItems: [
      { id: 'home',    name: 'Tổng quan',  icon: LayoutDashboard },
      { id: 'staff',   name: 'Nhân sự',    icon: UsersIcon, permission: 'NhanSu.View' },
      { id: 'payroll', name: 'Tính lương', icon: Wallet,    permission: 'Luong.View' },
      { id: 'attendance-summary', name: 'Chấm công', icon: ClipboardCheck, permission: 'ChamCong.ViewAll' },
    ],
    homePanel: 'AdminHomePanel',
    themeColor: 'text-rose-700',
    themeBg: 'from-rose-600 to-pink-500',
    accentClass: 'bg-rose-600 text-white',
    roleLabel: 'Quản lý nhân sự',
    defaultMenu: 'home',
    greeting: 'Quản lý đội ngũ y tế'
  },
  PayrollManager: {
    menuItems: FINANCE_MENU,
    homePanel: 'FinanceHomePanel',
    themeColor: 'text-emerald-700',
    themeBg: 'from-emerald-500 to-teal-400',
    accentClass: 'bg-emerald-500 text-white',
    roleLabel: 'Kế toán / Tài chính',
    defaultMenu: 'home',
    greeting: 'Bảng tài chính'
  },
  WarehouseManager: {
    menuItems: [
      { id: 'home',     name: 'Tổng quan', icon: LayoutDashboard },
      { id: 'supplies', name: 'Vật tư',    icon: Package, permission: 'Kho.View' },
    ],
    homePanel: 'AdminHomePanel',
    themeColor: 'text-violet-700',
    themeBg: 'from-violet-600 to-purple-500',
    accentClass: 'bg-violet-600 text-white',
    roleLabel: 'Quản lý kho',
    defaultMenu: 'home',
    greeting: 'Cổng quản lý kho vật tư'
  }
}

const DEFAULT_PROFILE = {
  menuItems: [
    { id: 'home',          name: 'Tổng quan',   icon: LayoutDashboard },
    { id: 'patients',      name: 'Bệnh nhân',   icon: UserRound },
  ],
  homePanel: 'DoctorHomePanel',
  themeColor: 'text-primary',
  themeBg: 'from-teal-500 to-primary',
  accentClass: 'bg-primary text-white',
  roleLabel: 'Nhân viên',
  defaultMenu: 'home',
  greeting: 'Hệ thống quản lý'
}

// ── COMPOSABLE ────────────────────────────────────────────────────────────────

export function useRoleMenu() {
  const authStore = useAuthStore()

  /**
   * Xác định role ưu tiên cao nhất của user để chọn profile phù hợp.
   * Thứ tự ưu tiên: Admin > MedicalGroupManager > MedicalStaff > ContractManager
   * > PersonnelManager > PayrollManager > WarehouseManager
   */
  const resolveRoleProfile = computed(() => {
    const roles = authStore.userRoles || []
    const primaryRole = authStore.userRole || ''

    const priorityOrder = [
      'Admin',
      'MedicalGroupManager',
      'MedicalStaff',
      'ContractManager',
      'PersonnelManager',
      'PayrollManager',
      'WarehouseManager'
    ]

    for (const role of priorityOrder) {
      if (roles.includes(role) || primaryRole === role) {
        return ROLE_PROFILES[role] || DEFAULT_PROFILE
      }
    }
    return DEFAULT_PROFILE
  })

  const roleProfile = computed(() => resolveRoleProfile.value)
  const menuItems = computed(() => roleProfile.value.menuItems)
  const homePanel = computed(() => roleProfile.value.homePanel)
  const themeColor = computed(() => roleProfile.value.themeColor)
  const themeBg = computed(() => roleProfile.value.themeBg)
  const accentClass = computed(() => roleProfile.value.accentClass)
  const roleLabel = computed(() => roleProfile.value.roleLabel)
  const defaultMenu = computed(() => roleProfile.value.defaultMenu)
  const greeting = computed(() => roleProfile.value.greeting)

  /**
   * Lọc menuItems dựa trên permissions của user.
   * Nếu item không có `permission` field → luôn hiển thị.
   */
  const filteredMenuItems = computed(() =>
    menuItems.value.filter(item => {
      if (!item.permission) return true
      return authStore.hasPermission(item.permission)
    })
  )

  return {
    roleProfile,
    menuItems,
    filteredMenuItems,
    homePanel,
    themeColor,
    themeBg,
    accentClass,
    roleLabel,
    defaultMenu,
    greeting
  }
}
