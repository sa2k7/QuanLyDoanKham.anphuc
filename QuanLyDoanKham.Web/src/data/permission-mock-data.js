/**
 * permission-mock-data.js
 * Dữ liệu giả lập cho module Phân Quyền Động (RBAC)
 * Map đúng với PermissionId đã seed trong ApplicationDbContext.cs
 */

// ── 8 Vai Trò (Roles) ─────────────────────────────────────────────────────────
export const MOCK_ROLES = [
  { roleId: 1, roleName: 'Admin',              description: 'Quản trị hệ thống' },
  { roleId: 2, roleName: 'PersonnelManager',    description: 'Quản lý Nhân sự' },
  { roleId: 3, roleName: 'ContractManager',      description: 'Quản lý Hợp đồng' },
  { roleId: 4, roleName: 'PayrollManager',       description: 'Quản lý tính lương' },
  { roleId: 5, roleName: 'MedicalGroupManager',  description: 'Quản lý đoàn khám' },
  { roleId: 6, roleName: 'WarehouseManager',     description: 'Quản lý Kho' },
  { roleId: 7, roleName: 'MedicalStaff',         description: 'Nhân sự đi đoàn' },
  { roleId: 8, roleName: 'Customer',             description: 'Tài khoản Công ty' },
]

// ── Ma trận quyền gom theo Module ──────────────────────────────────────────────
// Format giống API response: GET /api/permissions/matrix
export const MOCK_PERMISSION_MATRIX = [
  {
    module: 'HopDong',
    permissions: [
      { permissionId: 1,  permissionKey: 'HopDong.View',    permissionName: 'Xem hợp đồng',          module: 'HopDong' },
      { permissionId: 2,  permissionKey: 'HopDong.Create',  permissionName: 'Tạo hợp đồng',          module: 'HopDong' },
      { permissionId: 3,  permissionKey: 'HopDong.Edit',    permissionName: 'Sửa hợp đồng',          module: 'HopDong' },
      { permissionId: 4,  permissionKey: 'HopDong.Approve', permissionName: 'Phê duyệt hợp đồng',    module: 'HopDong' },
      { permissionId: 5,  permissionKey: 'HopDong.Reject',  permissionName: 'Từ chối hợp đồng',      module: 'HopDong' },
      { permissionId: 6,  permissionKey: 'HopDong.Upload',  permissionName: 'Đính kèm file hợp đồng', module: 'HopDong' },
    ]
  },
  {
    module: 'DoanKham',
    permissions: [
      { permissionId: 10, permissionKey: 'DoanKham.View',        permissionName: 'Xem đoàn khám',             module: 'DoanKham' },
      { permissionId: 11, permissionKey: 'DoanKham.Create',      permissionName: 'Tạo đoàn khám',             module: 'DoanKham' },
      { permissionId: 12, permissionKey: 'DoanKham.Edit',        permissionName: 'Sửa đoàn khám',             module: 'DoanKham' },
      { permissionId: 13, permissionKey: 'DoanKham.SetPosition', permissionName: 'Thiết lập vị trí đoàn',     module: 'DoanKham' },
      { permissionId: 14, permissionKey: 'DoanKham.AssignStaff', permissionName: 'Phân công nhân sự đoàn',    module: 'DoanKham' },
      { permissionId: 15, permissionKey: 'DoanKham.ManageOwn',   permissionName: 'Quản lý đoàn mình phụ trách', module: 'DoanKham' },
      { permissionId: 16, permissionKey: 'DoanKham.Lock',        permissionName: 'Khóa sổ đoàn khám',         module: 'DoanKham' },
    ]
  },
  {
    module: 'LichKham',
    permissions: [
      { permissionId: 20, permissionKey: 'LichKham.ViewOwn', permissionName: 'Xem lịch của mình',      module: 'LichKham' },
      { permissionId: 21, permissionKey: 'LichKham.ViewAll', permissionName: 'Xem toàn bộ lịch khám',  module: 'LichKham' },
    ]
  },
  {
    module: 'ChamCong',
    permissions: [
      { permissionId: 30, permissionKey: 'ChamCong.QR',         permissionName: 'Mở QR chấm công',        module: 'ChamCong' },
      { permissionId: 31, permissionKey: 'ChamCong.CheckInOut', permissionName: 'Check-in/out nhân viên',  module: 'ChamCong' },
      { permissionId: 32, permissionKey: 'ChamCong.ViewAll',    permissionName: 'Xem toàn bộ chấm công',  module: 'ChamCong' },
    ]
  },
  {
    module: 'BaoCao',
    permissions: [
      { permissionId: 40, permissionKey: 'BaoCao.View',        permissionName: 'Xem báo cáo',              module: 'BaoCao' },
      { permissionId: 41, permissionKey: 'BaoCao.QC',          permissionName: 'Thực hiện QC hồ sơ',       module: 'BaoCao' },
      { permissionId: 64, permissionKey: 'BaoCao.ViewFinance', permissionName: 'Xem báo cáo tài chính P&L', module: 'BaoCao' },
      { permissionId: 65, permissionKey: 'BaoCao.Export',      permissionName: 'Xuất báo cáo',             module: 'BaoCao' },
    ]
  },
  {
    module: 'Kho',
    permissions: [
      { permissionId: 50,  permissionKey: 'Kho.View',    permissionName: 'Xem kho vật tư',       module: 'Kho' },
      { permissionId: 51,  permissionKey: 'Kho.Edit',    permissionName: 'Nhập xuất kho',        module: 'Kho' },
      { permissionId: 140, permissionKey: 'Kho.Reports', permissionName: 'Xem báo cáo tồn kho', module: 'Kho' },
      { permissionId: 141, permissionKey: 'Kho.Import',  permissionName: 'Import phiếu nhập kho', module: 'Kho' },
      { permissionId: 142, permissionKey: 'Kho.Export',  permissionName: 'Export phiếu xuất kho', module: 'Kho' },
    ]
  },
  {
    module: 'TaiChinh',
    permissions: [
      { permissionId: 60, permissionKey: 'QuyetToan.Edit',      permissionName: 'Sửa phát sinh quyết toán', module: 'TaiChinh' },
      { permissionId: 62, permissionKey: 'QuyetToan.Calculate', permissionName: 'Tính toán quyết toán',     module: 'TaiChinh' },
      { permissionId: 63, permissionKey: 'QuyetToan.Finalize',  permissionName: 'Chốt xác nhận quyết toán', module: 'TaiChinh' },
    ]
  },
  {
    module: 'HeThong',
    permissions: [
      { permissionId: 100, permissionKey: 'HeThong.UserManage', permissionName: 'Quản lý người dùng',     module: 'HeThong' },
      { permissionId: 101, permissionKey: 'HeThong.RoleManage', permissionName: 'Quản lý vai trò & quyền', module: 'HeThong' },
    ]
  },
  {
    module: 'PhongBan',
    permissions: [
      { permissionId: 110, permissionKey: 'PhongBan.View', permissionName: 'Xem phòng ban', module: 'PhongBan' },
      { permissionId: 111, permissionKey: 'PhongBan.Edit', permissionName: 'Sửa phòng ban', module: 'PhongBan' },
    ]
  },
  {
    module: 'WorkRule',
    permissions: [
      { permissionId: 120, permissionKey: 'WorkRule.View', permissionName: 'Xem bảng rule chức năng', module: 'WorkRule' },
      { permissionId: 121, permissionKey: 'WorkRule.Edit', permissionName: 'Sửa bảng rule chức năng', module: 'WorkRule' },
    ]
  },
  {
    module: 'AI',
    permissions: [
      { permissionId: 130, permissionKey: 'AI.SuggestStaff', permissionName: 'AI gợi ý phân công nhân sự', module: 'AI' },
    ]
  },
]

// ── Mock: Quyền đang bật của từng Role ─────────────────────────────────────────
// Format giống API response: GET /api/roles/{id}/permissions → [permissionId, ...]
export const MOCK_ROLE_PERMISSIONS = {
  1: [1,2,3,4,5,6, 10,11,12,13,14,15,16, 20,21, 30,31,32, 40,41,64,65, 50,51,140,141,142, 60,62,63, 100,101, 110,111, 120,121, 130], // Admin: tất cả
  2: [14, 21, 32, 100, 110],                            // Quản lý Nhân sự
  3: [1, 2, 3, 4, 5, 6, 110, 120],                       // Quản lý Hợp đồng
  4: [1, 40, 60, 62, 63, 64, 65, 110],                   // Quản lý tính lương
  5: [10, 11, 12, 13, 14, 15, 16, 21, 30, 130],          // Quản lý đoàn khám
  6: [50, 51, 140, 141, 142],                             // Quản lý Kho
  7: [10, 20, 15, 30],                                    // Nhân sự đi đoàn
  8: [1, 10, 40],                                         // Tài khoản Công ty (xem only)
}

// ── Bản đồ tên Module (Tiếng Việt) ────────────────────────────────────────────
export const MODULE_LABELS = {
  HopDong:  'Hợp đồng',
  DoanKham: 'Đoàn khám',
  LichKham: 'Lịch khám',
  ChamCong: 'Chấm công',
  BaoCao:   'Thống kê & Báo cáo',
  Kho:      'Kho Vật tư',
  TaiChinh: 'Quyết toán',
  HeThong:  'Tài khoản & Hệ thống',
  PhongBan: 'Nhân sự & Phòng ban',
  WorkRule: 'Phân quyền & Work Rule',
  AI:       'Gợi ý AI',
}

// ── Bản đồ icon Module ─────────────────────────────────────────────────────────
export const MODULE_ICONS = {
  HopDong:  '📄',
  DoanKham: '🩺',
  LichKham: '📅',
  ChamCong: '✅',
  BaoCao:   '📊',
  Kho:      '📦',
  TaiChinh: '💰',
  HeThong:  '⚙️',
  PhongBan: '👥',
  WorkRule: '🔐',
  AI:       '🤖',
}
