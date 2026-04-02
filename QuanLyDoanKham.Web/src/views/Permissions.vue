<template>
  <div class="permissions-page">
    <div class="page-header">
      <div>
        <h1 class="page-title"><i class="fas fa-shield-alt"></i> Quản lý Phân quyền</h1>
        <p class="page-subtitle">Cấu hình permissions cho từng Role</p>
      </div>
    </div>

    <!-- Role Tabs -->
    <div class="role-tabs">
      <button v-for="role in roles" :key="role.roleId"
        class="role-tab" :class="{ active: selectedRoleId === role.roleId }"
        @click="selectRole(role.roleId)">
        {{ getRoleLabel(role.roleName) }}
      </button>
    </div>

    <!-- Permission Matrix -->
    <div v-if="selectedRole" class="permission-matrix">
      <div class="matrix-header">
        <h3>{{ getRoleLabel(selectedRole.roleName) }}</h3>
        <div class="matrix-actions">
          <button class="btn btn-sm btn-outline" @click="toggleAll(true)">
            <i class="fas fa-check-double"></i> Chọn tất cả
          </button>
          <button class="btn btn-sm btn-outline" @click="toggleAll(false)">
            <i class="fas fa-times"></i> Bỏ chọn
          </button>
          <button class="btn btn-sm btn-primary" @click="savePermissions" :disabled="saving">
            <i v-if="saving" class="fas fa-spinner fa-spin"></i>
            <i v-else class="fas fa-save"></i>
            {{ saving ? 'Đang lưu...' : 'Lưu thay đổi' }}
          </button>
        </div>
      </div>

      <div v-if="selectedRole.roleName === 'Admin'" class="admin-notice">
        <i class="fas fa-crown"></i>
        Admin tự động có <strong>tất cả quyền</strong> trong hệ thống.
      </div>

      <div v-else>
        <div v-for="(modulePerms, module) in permsByModule" :key="module" class="perm-module">
          <div class="module-header">
            <span class="module-icon">{{ getModuleIcon(module) }}</span>
            <span class="module-name">{{ getModuleLabel(module) }}</span>
            <label class="toggle-all-module">
              <input type="checkbox"
                :checked="isModuleAllChecked(module)"
                :indeterminate.prop="isModuleIndeterminate(module)"
                @change="toggleModule(module, $event.target.checked)" />
              Tất cả
            </label>
          </div>
          <div class="perm-list">
            <label v-for="perm in modulePerms" :key="perm.permissionId" class="perm-item"
              :class="{ active: checkedPerms.includes(perm.permissionId) }">
              <input type="checkbox"
                :value="perm.permissionId"
                v-model="checkedPerms" />
              <div class="perm-info">
                <span class="perm-name">{{ perm.permissionName }}</span>
                <code class="perm-key">{{ perm.permissionKey }}</code>
              </div>
            </label>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-overlay"><div class="spinner"></div></div>

    <!-- Toast notification -->
    <div v-if="toast.show" class="toast" :class="toast.type">
      <i :class="toast.type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle'"></i>
      {{ toast.message }}
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/apiClient'

const roles = ref([])
const permissions = ref([])
const rolePermissions = ref({}) // { roleId: [permissionId, ...] }
const selectedRoleId = ref(null)
const checkedPerms = ref([])
const loading = ref(false)
const saving = ref(false)
const toast = ref({ show: false, type: 'success', message: '' })

const selectedRole = computed(() => roles.value.find(r => r.roleId === selectedRoleId.value))

const permsByModule = computed(() => {
  const groups = {}
  permissions.value.forEach(p => {
    if (!groups[p.module]) groups[p.module] = []
    groups[p.module].push(p)
  })
  return groups
})

const isModuleAllChecked = (module) =>
  permsByModule.value[module].every(p => checkedPerms.value.includes(p.permissionId))

const isModuleIndeterminate = (module) => {
  const perms = permsByModule.value[module]
  const checked = perms.filter(p => checkedPerms.value.includes(p.permissionId))
  return checked.length > 0 && checked.length < perms.length
}

const toggleModule = (module, val) => {
  const ids = permsByModule.value[module].map(p => p.permissionId)
  if (val) {
    checkedPerms.value = [...new Set([...checkedPerms.value, ...ids])]
  } else {
    checkedPerms.value = checkedPerms.value.filter(id => !ids.includes(id))
  }
}

const toggleAll = (val) => {
  checkedPerms.value = val ? permissions.value.map(p => p.permissionId) : []
}

const getRoleLabel = (name) => {
  const map = {
    Admin: '👑 Admin', PersonnelManager: '👤 Nhân sự', ContractManager: '📄 Hợp đồng',
    PayrollManager: '💰 Kế toán lương', MedicalGroupManager: '🩺 Quản lý đoàn',
    WarehouseManager: '📦 Kho vật tư', GroupLeader: '🎯 Trưởng đoàn',
    MedicalStaff: '💊 Nhân viên khám', Accountant: '📊 Kế toán', Customer: '🏢 Khách hàng'
  }
  return map[name] || name
}

const getModuleLabel = (m) => {
  const map = {
    HopDong: '📄 Hợp đồng', DoanKham: '🩺 Đoàn khám', LichKham: '📅 Lịch khám',
    ChamCong: '✅ Chấm công', Kho: '📦 Kho vật tư', Luong: '💰 Lương',
    NhanSu: '👤 Nhân sự', BaoCao: '📊 Báo cáo', HeThong: '⚙️ Hệ thống'
  }
  return map[m] || m
}

const getModuleIcon = (m) => {
  const map = { HopDong:'📄', DoanKham:'🩺', LichKham:'📅', ChamCong:'✅',
    Kho:'📦', Luong:'💰', NhanSu:'👤', BaoCao:'📊', HeThong:'⚙️' }
  return map[m] || '🔧'
}

const selectRole = async (roleId) => {
  selectedRoleId.value = roleId
  // Load permissions của role này nếu chưa có
  if (!rolePermissions.value[roleId]) {
    try {
      const res = await apiClient.get(`/Auth/role-permissions/${roleId}`)
      rolePermissions.value[roleId] = res.data.map(p => p.permissionId)
    } catch {
      rolePermissions.value[roleId] = []
    }
  }
  checkedPerms.value = [...(rolePermissions.value[roleId] || [])]
}

const savePermissions = async () => {
  saving.value = true
  try {
    await apiClient.put(`/Auth/role-permissions/${selectedRoleId.value}`, {
      permissionIds: checkedPerms.value
    })
    rolePermissions.value[selectedRoleId.value] = [...checkedPerms.value]
    showToast('success', 'Cập nhật quyền thành công!')
  } catch (e) {
    showToast('error', e.response?.data?.message || 'Lỗi khi cập nhật quyền.')
  } finally {
    saving.value = false
  }
}

const showToast = (type, message) => {
  toast.value = { show: true, type, message }
  setTimeout(() => { toast.value.show = false }, 3000)
}

onMounted(async () => {
  loading.value = true
  try {
    const [rolesRes, permsRes] = await Promise.all([
      apiClient.get('/Auth/roles'),
      apiClient.get('/Auth/permissions')
    ])
    roles.value = rolesRes.data
    permissions.value = permsRes.data
    if (roles.value.length > 0) await selectRole(roles.value[0].roleId)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.permissions-page { padding: 24px; max-width: 1100px; margin: 0 auto; }
.page-header { margin-bottom: 24px; }
.page-title { font-size: 1.6rem; font-weight: 700; margin: 0; color: var(--text-primary, #1e293b); }
.page-title i { margin-right: 10px; color: var(--primary, #6366f1); }
.page-subtitle { color: var(--text-muted, #64748b); margin: 4px 0 0; }

.role-tabs { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 24px; }
.role-tab {
  padding: 8px 16px; border: 2px solid var(--border, #e2e8f0); border-radius: 20px;
  background: var(--card-bg, #fff); cursor: pointer; font-size: 0.85rem; font-weight: 500;
  color: var(--text-secondary, #475569); transition: all .15s;
}
.role-tab.active { border-color: var(--primary, #6366f1); background: var(--primary, #6366f1); color: #fff; }
.role-tab:hover:not(.active) { border-color: var(--primary, #6366f1); color: var(--primary, #6366f1); }

.permission-matrix {
  background: var(--card-bg, #fff); border-radius: 14px; padding: 24px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08); border: 1px solid var(--border, #e2e8f0);
}
.matrix-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 20px; flex-wrap: wrap; gap: 12px;
}
.matrix-header h3 { margin: 0; font-size: 1.1rem; }
.matrix-actions { display: flex; gap: 8px; }

.admin-notice {
  padding: 16px 20px; background: linear-gradient(135deg, #fef3c7, #fde68a);
  border-radius: 10px; color: #92400e; font-size: 0.9rem; display: flex;
  align-items: center; gap: 10px;
}
.admin-notice i { font-size: 1.2rem; }

.perm-module { margin-bottom: 20px; }
.module-header {
  display: flex; align-items: center; gap: 10px; padding: 10px 14px;
  background: var(--hover, #f8fafc); border-radius: 10px; margin-bottom: 10px;
}
.module-icon { font-size: 1.2rem; }
.module-name { font-weight: 600; flex: 1; color: var(--text-primary, #1e293b); }
.toggle-all-module { display: flex; align-items: center; gap: 6px; font-size: 0.82rem; cursor: pointer; color: var(--text-muted, #64748b); }

.perm-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 8px; padding: 0 4px; }
.perm-item {
  display: flex; align-items: flex-start; gap: 10px; padding: 10px 12px;
  border: 1px solid var(--border, #e2e8f0); border-radius: 8px; cursor: pointer;
  transition: all .12s; background: var(--card-bg, #fff);
}
.perm-item.active { border-color: var(--primary, #6366f1); background: #eef2ff; }
.perm-item input[type="checkbox"] { margin-top: 2px; cursor: pointer; accent-color: var(--primary, #6366f1); }
.perm-info { display: flex; flex-direction: column; gap: 2px; }
.perm-name { font-size: 0.88rem; font-weight: 500; color: var(--text-primary, #1e293b); }
.perm-key { font-size: 0.72rem; color: var(--text-muted, #64748b); font-family: monospace; background: var(--hover, #f1f5f9); padding: 1px 4px; border-radius: 4px; }

.btn { padding: 8px 16px; border: none; border-radius: 8px; font-weight: 600; cursor: pointer; font-size: 0.85rem; transition: all .15s; display: flex; align-items: center; gap: 6px; }
.btn-sm { padding: 6px 12px; font-size: 0.8rem; }
.btn-primary { background: var(--primary, #6366f1); color: #fff; }
.btn-primary:hover:not(:disabled) { background: #4f46e5; }
.btn-outline { background: transparent; border: 1px solid var(--border, #e2e8f0); color: var(--text-secondary, #475569); }
.btn-outline:hover { border-color: var(--primary, #6366f1); color: var(--primary, #6366f1); }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }

.loading-overlay { display: flex; justify-content: center; padding: 60px; }

.toast {
  position: fixed; bottom: 24px; right: 24px; padding: 14px 20px;
  border-radius: 10px; color: #fff; font-weight: 600; display: flex;
  align-items: center; gap: 10px; z-index: 2000; animation: slideIn .3s ease;
}
.toast.success { background: #10b981; }
.toast.error { background: #ef4444; }
@keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
</style>
