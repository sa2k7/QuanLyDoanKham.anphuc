<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in pb-12 p-6 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-7xl mx-auto w-full">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8 glass-header p-8 rounded-[2.5rem] shadow-glass border border-white/40">
        <div class="flex items-center gap-6">
          <div class="w-16 h-16 bg-white/40 backdrop-blur-xl rounded-[1.5rem] flex items-center justify-center shadow-inner border border-white/40">
            <ShieldAlert class="w-8 h-8 text-primary" />
          </div>
          <div>
            <h1 class="text-4xl font-black text-slate-900 tracking-tighter italic uppercase leading-none">Phân Quyền Hệ Thống</h1>
            <p class="text-[11px] font-black text-slate-400 uppercase tracking-[0.2em] mt-2 ml-1 opacity-70">Quản trị vai trò & Quyền hạn truy cập (RBAC Matrix)</p>
          </div>
        </div>
      </div>

    <!-- Role Tabs -->
    <div class="role-tabs flex flex-wrap gap-3 mb-8">
      <button v-for="role in roles" :key="role.roleId"
        class="role-tab px-6 py-2 rounded-full font-black text-xs uppercase tracking-widest transition-all border-2" 
        :class="selectedRoleId === role.roleId ? 'bg-indigo-600 text-white border-indigo-600 shadow-lg' : 'bg-white text-slate-400 border-slate-100 hover:border-indigo-200'"
        @click="selectRole(role.roleId)">
        {{ getRoleLabel(role.roleName) }}
      </button>
    </div>

    <!-- Permission Matrix -->
    <div v-if="selectedRole" class="permission-matrix premium-card bg-white rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] overflow-hidden p-8">
      <div class="matrix-header flex justify-between items-center mb-8 pb-6 border-b border-slate-50">
        <div>
            <h3 class="text-xl font-black text-slate-800 uppercase tracking-widest">{{ getRoleLabel(selectedRole.roleName) }}</h3>
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ i18n.t('permissions.list') }}</p>
        </div>
        <div class="matrix-actions flex gap-3">
          <button class="btn-premium variant-indigo scale-90" @click="toggleAll(true)">
            <CheckCheck class="w-4 h-4" /> {{ i18n.t('permissions.selectAll') }}
          </button>
          <button class="btn-premium variant-slate scale-90" @click="toggleAll(false)">
            <X class="w-4 h-4" /> {{ i18n.t('permissions.deselect') }}
          </button>
          <button class="btn-premium bg-emerald-600 text-white hover:bg-emerald-700 scale-90 px-6 transition-colors" @click="restoreDefaults" :disabled="saving || restoring">
            <RefreshCcw v-if="!restoring" class="w-4 h-4" />
            <Loader2 v-else class="w-4 h-4 animate-spin" />
            {{ restoring ? i18n.t('permissions.restoring') : i18n.t('permissions.restore') }}
          </button>
          <button class="btn-premium bg-slate-900 text-white scale-90 px-8" @click="savePermissions" :disabled="saving || restoring">
            <Loader2 v-if="saving" class="w-4 h-4 animate-spin" />
            <Save v-else class="w-4 h-4" />
            {{ saving ? i18n.t('permissions.saving') : i18n.t('permissions.save') }}
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
        <div v-for="(modulePerms, module) in permsByModule" :key="module" class="perm-module bg-slate-50/30 p-6 rounded-3xl border border-slate-100">
          <div class="module-header flex items-center justify-between mb-6 pb-4 border-b border-slate-100">
            <div class="flex items-center gap-3">
                <span class="module-icon text-xl">{{ getModuleIcon(module) }}</span>
                <span class="module-name font-black text-slate-700 uppercase tracking-widest text-sm">{{ getModuleLabel(module) }}</span>
            </div>
            <label class="toggle-all-module flex items-center gap-2 cursor-pointer group">
              <input type="checkbox"
                class="w-4 h-4 accent-indigo-600 rounded"
                :checked="isModuleAllChecked(module)"
                :indeterminate.prop="isModuleIndeterminate(module)"
                @change="toggleModule(module, $event.target.checked)" />
              <span class="text-[10px] font-black text-slate-400 group-hover:text-indigo-600 uppercase tracking-widest transition-colors">{{ i18n.t('permissions.all') }}</span>
            </label>
          </div>
          <div class="perm-list grid grid-cols-1 gap-2">
            <label v-for="perm in modulePerms" :key="perm.permissionId" 
              class="perm-item flex items-center gap-3 p-4 rounded-xl border-2 transition-all cursor-pointer"
              :class="checkedPerms.includes(perm.permissionId) ? 'bg-white border-slate-900 shadow-[2px_2px_0px_#0f172a]' : 'bg-transparent border-transparent text-slate-400 hover:bg-white hover:border-slate-100'">
              <input type="checkbox"
                class="w-4 h-4 accent-indigo-600 rounded"
                :value="perm.permissionId"
                v-model="checkedPerms" />
              <div class="perm-info flex flex-col">
                <span class="perm-name text-xs font-black uppercase tracking-widest" :class="checkedPerms.includes(perm.permissionId) ? 'text-slate-800' : 'text-slate-400'">{{ perm.permissionName }}</span>
                <code class="perm-key text-[9px] font-mono mt-0.5" :class="checkedPerms.includes(perm.permissionId) ? 'text-indigo-500' : 'text-slate-300'">{{ perm.permissionKey }}</code>
              </div>
            </label>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
        <Loader2 class="w-10 h-10 animate-spin text-slate-200" />
    </div>

    <!-- Toast notification -->
    <Teleport to="body">
        <div v-if="toast.show" class="fixed bottom-10 right-10 z-[200] animate-fade-in-up">
            <div class="px-8 py-4 rounded-2xl border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex items-center gap-4"
                 :class="toast.type === 'success' ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600'">
                <CheckCircle2 v-if="toast.type === 'success'" class="w-6 h-6" />
                <AlertCircle v-else class="w-6 h-6" />
                <div class="font-black uppercase tracking-widest text-xs">{{ toast.message }}</div>
            </div>
        </div>
    </Teleport>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '../services/apiClient'
import { 
    ShieldAlert, CheckCheck, X, Save, Loader2, CheckCircle2, AlertCircle, RefreshCcw 
} from 'lucide-vue-next'
import { usePermission } from '../composables/usePermission'
import { useI18nStore } from '../stores/i18n'
import { useAuthStore } from '../stores/auth'

const i18n = useI18nStore()
const auth = useAuthStore()
const { can } = usePermission()
const roles = ref([])
const permissions = ref([])
const rolePermissions = ref({}) // { roleId: [permissionId, ...] }
const selectedRoleId = ref(null)
const checkedPerms = ref([])
const loading = ref(false)
const saving = ref(false)
const restoring = ref(false)
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
  return i18n.t('roles.' + name)
}

const getModuleLabel = (m) => {
  const map = {
    HopDong: '📄 Hợp đồng & Doanh nghiệp', DoanKham: '🩺 Vận hành Đoàn khám', LichKham: '📅 Lịch khám & Điều phối',
    ChamCong: '✅ Chấm công & Lương', Kho: '📦 Kho dược & Vật tư', Luong: '💰 Kế toán Tài chính',
    NhanSu: '👤 Quản trị Nhân lực', BaoCao: '📊 Hệ thống Báo cáo', HeThong: '⚙️ Cấu hình Hệ thống'
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
      const res = await apiClient.get(`/api/Auth/role-permissions/${roleId}`)
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
    await apiClient.put(`/api/Auth/role-permissions/${selectedRoleId.value}`, {
      permissionIds: checkedPerms.value
    })
    rolePermissions.value[selectedRoleId.value] = [...checkedPerms.value]
    
    // Nếu đang sửa quyền của chính role hiện tại, cần làm mới JWT
    if (auth.userRole === selectedRole.value?.roleName || selectedRole.value?.roleName === 'Admin') {
      try {
        await auth.refreshAccessToken()
      } catch (e) {
        console.warn('Could not refresh token automatically', e)
      }
    }

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

const restoreDefaults = async () => {
  if (!confirm('Bạn có chắc chắn muốn khôi phục phân quyền của vai trò này về mặc định ban đầu không? (Quyền hiện tại sẽ bị ghi đè)')) return;
  restoring.value = true;
  try {
    const res = await apiClient.post(`/api/Auth/restore-default-permissions/${selectedRoleId.value}`);
    // Tải lại các quyền sau khi khôi phục
    const permRes = await apiClient.get(`/api/Auth/role-permissions/${selectedRoleId.value}`);
    rolePermissions.value[selectedRoleId.value] = permRes.data.map(p => p.permissionId);
    checkedPerms.value = [...rolePermissions.value[selectedRoleId.value]];
    showToast('success', res.data?.message || 'Khôi phục mặc định thành công!');
  } catch (e) {
    showToast('error', e.response?.data?.message || 'Lỗi khi khôi phục quyền mặc định.');
  } finally {
    restoring.value = false;
  }
}

onMounted(async () => {
  loading.value = true
  try {
    const [rolesRes, permsRes] = await Promise.all([
      apiClient.get('/api/Auth/roles'),
      apiClient.get('/api/Auth/permissions')
    ])
    roles.value = rolesRes.data
    permissions.value = permsRes.data
    if (roles.value.length > 0) await selectRole(roles.value[0].roleId)
  } catch (e) {
      console.error("Lỗi s khởi tạo phân quyền:", e)
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
