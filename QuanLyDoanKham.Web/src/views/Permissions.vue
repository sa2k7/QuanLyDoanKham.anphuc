<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in p-3 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-full mx-auto w-full">
      <!-- Header Section -->
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3 mb-3">
        <div>
          <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
            <div class="w-8 h-8 bg-primary text-white rounded-lg flex items-center justify-center shadow-sm">
              <ShieldCheck class="w-4.5 h-4.5" />
            </div>
            Phân Quyền Hệ Thống
          </h2>
          <p class="text-slate-400 font-bold uppercase tracking-[0.2em] text-[7.5px] mt-0.5">Quản trị vai trò & Quyền hạn truy cập (RBAC Matrix)</p>
        </div>
      </div>

      <div class="flex flex-col lg:flex-row gap-4">
        <!-- Left Column: Roles List -->
        <div class="w-full lg:w-56 flex-shrink-0">
          <div class="premium-card bg-white rounded-lg shadow-sm overflow-hidden border border-slate-100">
            <div class="p-2.5 border-b border-slate-50 bg-slate-50/50">
              <h3 class="text-[7.5px] font-black text-slate-400 uppercase tracking-[0.2em] italic">Danh Sách Vai Trò</h3>
            </div>
            <div class="p-1.5 flex flex-col gap-1">
              <button v-for="role in roles" :key="role.roleId"
                @click="selectRole(role)"
                class="text-left px-2.5 py-1.5 rounded-lg transition-all duration-300 font-bold text-[10px] flex items-center justify-between group"
                :class="selectedRole?.roleId === role.roleId
                  ? 'bg-primary/10 text-primary border border-primary/20 shadow-sm'
                  : 'text-slate-500 hover:bg-slate-50 hover:text-slate-700 border border-transparent'">
                <span class="truncate uppercase">{{ role.description || role.roleName }}</span>
                <ChevronRight class="w-3 h-3 transition-all duration-300 flex-shrink-0"
                  :class="selectedRole?.roleId === role.roleId
                    ? 'opacity-100 translate-x-0'
                    : 'opacity-0 -translate-x-1 group-hover:opacity-60 group-hover:translate-x-0'" />
              </button>
            </div>
          </div>
        </div>

        <!-- Right Column: Permission Matrix -->
        <div class="flex-1 min-w-0">
          <div class="premium-card bg-white rounded-xl shadow-sm overflow-hidden relative border border-slate-100" style="min-height: 520px;">
            <!-- Loading Overlay -->
            <div v-if="loadingPerms" class="absolute inset-0 bg-white/80 backdrop-blur-sm flex items-center justify-center z-10 rounded-xl">
              <Loader2 class="w-8 h-8 text-primary animate-spin" />
            </div>

            <!-- Empty State -->
            <div v-if="!selectedRole && !loadingPerms" class="absolute inset-0 flex flex-col items-center justify-center text-slate-300">
              <ShieldAlert class="w-16 h-16 mb-3" />
              <p class="font-bold text-sm text-slate-400">Chọn một vai trò bên trái</p>
              <p class="text-[11px] text-slate-300">để xem và chỉnh sửa phân quyền.</p>
            </div>

            <!-- Matrix Content -->
            <template v-if="selectedRole && !loadingPerms">
              <!-- Sticky Header Bar -->
              <div class="p-3 border-b border-slate-100 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 bg-white/80 backdrop-blur-md sticky top-0 z-[5]">
                <div>
                  <h3 class="text-[11px] font-bold text-slate-800 italic uppercase">
                    Phân quyền: <span class="text-primary">{{ selectedRole.description || selectedRole.roleName }}</span>
                  </h3>
                  <p class="text-[7.5px] font-bold text-slate-400 uppercase tracking-widest mt-0.5">
                    {{ checkedPerms.length }} / {{ totalPermissionCount }} quyền active
                  </p>
                </div>
                <div class="flex gap-1.5">
                  <button class="h-7 px-2.5 rounded-lg border border-slate-200 text-slate-500 text-[8.5px] font-black uppercase tracking-wider hover:border-primary hover:text-primary transition-all shadow-sm"
                    @click="toggleAll(true)">
                    Chọn hết
                  </button>
                  <button class="h-7 px-2.5 rounded-lg border border-slate-200 text-slate-500 text-[8.5px] font-black uppercase tracking-wider hover:border-rose-400 hover:text-rose-500 transition-all shadow-sm"
                    @click="toggleAll(false)">
                    Bỏ chọn
                  </button>
                  <button class="h-7 px-3 rounded-lg bg-primary text-white text-[8.5px] font-black uppercase tracking-wider hover:shadow-md hover:bg-primary/90 transition-all disabled:opacity-50 flex items-center gap-1.5"
                    @click="savePermissions" :disabled="saving">
                    <Save class="w-3 h-3" />
                    {{ saving ? 'LƯU...' : 'LƯU' }}
                  </button>
                </div>
              </div>

              <div class="p-3 grid grid-cols-1 xl:grid-cols-2 gap-2.5">
                <div v-for="(modulePerms, module) in permsByModule" :key="module"
                  class="bg-slate-50/50 rounded-lg border border-slate-100 overflow-hidden hover:border-slate-200 transition-colors">
                  <!-- Module Header -->
                  <div class="px-3 py-1.5 border-b border-slate-100 flex items-center justify-between bg-white/60">
                    <div class="flex items-center gap-1.5">
                      <span class="text-sm italic opacity-80">{{ getModuleIcon(module) }}</span>
                      <span class="font-black text-[9px] text-slate-700 uppercase tracking-wider">{{ getModuleLabel(module) }}</span>
                    </div>
                    <label class="flex items-center gap-1.5 cursor-pointer group">
                      <input type="checkbox" class="sr-only peer"
                        :checked="isModuleAllChecked(module)"
                        @change="toggleModule(module, $event.target.checked)" />
                      <div class="relative w-6 h-3.5 bg-slate-200 peer-checked:bg-primary rounded-full transition-colors duration-300 after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-2.5 after:w-2.5 after:transition-transform after:duration-300 peer-checked:after:translate-x-2.5 shadow-inner"></div>
                      <span class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest group-hover:text-primary transition-colors">Tất cả</span>
                    </label>
                  </div>
                  <!-- Permission Items -->
                  <div class="p-1.5 flex flex-col gap-0.5">
                    <label v-for="perm in modulePerms" :key="perm.permissionId"
                      class="flex items-center gap-2 px-2 py-1 rounded cursor-pointer transition-all duration-200"
                      :class="checkedPerms.includes(perm.permissionId)
                        ? 'bg-white border border-primary/20 shadow-sm'
                        : 'hover:bg-white/60 border border-transparent'">
                      <input type="checkbox" class="sr-only peer"
                        :value="perm.permissionId"
                        v-model="checkedPerms" />
                      <!-- Toggle Switch -->
                      <div class="relative w-7 h-3.5 bg-slate-200 peer-checked:bg-emerald-500 rounded-full transition-colors duration-300 flex-shrink-0 after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-2.5 after:w-2.5 after:transition-transform after:duration-300 peer-checked:after:translate-x-3.5 shadow-inner"></div>
                      <div class="flex flex-col min-w-0">
                        <span class="text-[10px] font-bold transition-colors duration-200 leading-tight"
                          :class="checkedPerms.includes(perm.permissionId) ? 'text-slate-800' : 'text-slate-400'">
                          {{ perm.permissionName }}
                        </span>
                        <code class="text-[6.5px] font-mono truncate transition-colors duration-200 mt-0.5"
                          :class="checkedPerms.includes(perm.permissionId) ? 'text-primary/60' : 'text-slate-300'">
                          {{ perm.permissionKey }}
                        </code>
                      </div>
                    </label>
                  </div>
                </div>
              </div>
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast notification -->
    <Teleport to="body">
      <Transition name="toast-slide">
        <div v-if="toastState.show" class="fixed bottom-8 right-8 z-[200]">
          <div class="px-6 py-4 rounded-2xl shadow-2xl flex items-center gap-3 border"
               :class="toastState.type === 'success'
                 ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
                 : 'bg-rose-50 text-rose-700 border-rose-200'">
            <CheckCircle2 v-if="toastState.type === 'success'" class="w-5 h-5 flex-shrink-0" />
            <AlertCircle v-else class="w-5 h-5 flex-shrink-0" />
            <span class="font-bold text-sm">{{ toastState.message }}</span>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '../services/apiClient'
import {
  ShieldCheck, ShieldAlert, ChevronRight, Save, Loader2,
  CheckCheck, X, CheckCircle2, AlertCircle
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()

const roles = ref([])
const permissions = ref([])
const selectedRole = ref(null)
const checkedPerms = ref([])
const loadingPerms = ref(false)
const saving = ref(false)
const toastState = ref({ show: false, type: 'success', message: '' })

// ── Computed ──

const permsByModule = computed(() => {
  const groups = {}
  permissions.value.forEach(p => {
    if (!groups[p.module]) groups[p.module] = []
    groups[p.module].push(p)
  })
  return groups
})

const totalPermissionCount = computed(() => permissions.value.length)

const isModuleAllChecked = (module) =>
  permsByModule.value[module]?.every(p => checkedPerms.value.includes(p.permissionId)) ?? false

// ── Actions ──

const toggleModule = (module, isChecked) => {
  const modulePermIds = permsByModule.value[module].map(p => p.permissionId)
  if (isChecked) {
    checkedPerms.value = [...new Set([...checkedPerms.value, ...modulePermIds])]
  } else {
    checkedPerms.value = checkedPerms.value.filter(id => !modulePermIds.includes(id))
  }
}

const toggleAll = (isChecked) => {
  checkedPerms.value = isChecked ? permissions.value.map(p => p.permissionId) : []
}

const selectRole = async (role) => {
  selectedRole.value = role
  loadingPerms.value = true
  try {
    const res = await apiClient.get(`/api/roles/${role.roleId}/permissions`)
    checkedPerms.value = [...res.data]
  } catch (error) {
    console.error('Lỗi khi tải quyền vai trò:', error)
    checkedPerms.value = []
    showToast('error', 'Không thể tải quyền của vai trò này.')
  } finally {
    loadingPerms.value = false
  }
}

const savePermissions = async () => {
  if (!selectedRole.value) return
  saving.value = true
  try {
    await apiClient.post(`/api/roles/${selectedRole.value.roleId}/permissions`, checkedPerms.value)

    // Refresh JWT if editing own role or Admin
    if (auth.userRole === selectedRole.value.roleName || selectedRole.value.roleName === 'Admin') {
      try { await auth.refreshAccessToken() } catch (e) {
        console.warn('Không thể tự động làm mới token:', e)
      }
    }

    showToast('success', 'Cập nhật phân quyền thành công! Lịch sử đã được ghi nhận.')
  } catch (error) {
    showToast('error', error.response?.data?.message || 'Có lỗi xảy ra khi lưu phân quyền.')
  } finally {
    saving.value = false
  }
}

// ── UI Helpers ──

const showToast = (type, message) => {
  toastState.value = { show: true, type, message }
  setTimeout(() => { toastState.value.show = false }, 4000)
}

const getModuleLabel = (moduleKey) => {
  const moduleLabels = {
    HopDong: 'Hợp đồng',
    DoanKham: 'Đoàn khám',
    LichKham: 'Lịch khám',
    ChamCong: 'Chấm công',
    BaoCao: 'Thống kê & Báo cáo',
    Kho: 'Kho Vật tư',
    TaiChinh: 'Quyết toán',
    HeThong: 'Tài khoản & Hệ thống',
    PhongBan: 'Nhân sự',
    WorkRule: 'Work Rule',
    AI: 'Gợi ý AI'
  }
  return moduleLabels[moduleKey] || moduleKey
}

const getModuleIcon = (moduleKey) => {
  const moduleIcons = {
    HopDong: '📄', DoanKham: '🩺', LichKham: '📅', ChamCong: '✅',
    BaoCao: '📊', Kho: '📦', TaiChinh: '💰', HeThong: '⚙️',
    PhongBan: '👥', WorkRule: '🔧', AI: '🤖'
  }
  return moduleIcons[moduleKey] || '🔒'
}

// ── Init ──

onMounted(async () => {
  try {
    const [rolesRes, permsRes] = await Promise.all([
      apiClient.get('/api/roles'),
      apiClient.get('/api/permissions/matrix')
    ])
    roles.value = rolesRes.data

    // Flatten matrix (API returns grouped) into a flat list
    const allPermissions = []
    permsRes.data.forEach(group => {
      group.permissions.forEach(p => allPermissions.push(p))
    })
    permissions.value = allPermissions

    // Auto-select first role
    if (roles.value.length > 0) {
      await selectRole(roles.value[0])
    }
  } catch (error) {
    console.error('Lỗi khởi tạo phân quyền:', error)
  }
})
</script>

<style scoped>
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.22, 1, 0.36, 1);
}
.toast-slide-enter-from {
  transform: translateX(120%);
  opacity: 0;
}
.toast-slide-leave-to {
  transform: translateX(120%);
  opacity: 0;
}
</style>
