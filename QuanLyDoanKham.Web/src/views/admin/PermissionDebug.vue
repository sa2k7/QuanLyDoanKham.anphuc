<template>
  <div class="p-6 max-w-6xl mx-auto">
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-800">🔐 Permission Debug Tool</h1>
      <p class="text-gray-500 text-sm mt-1">Kiểm tra và quản lý phân quyền người dùng</p>
    </div>

    <!-- Token Invalidation -->
    <div class="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="font-semibold text-amber-800">Force Re-login All Users</h3>
          <p class="text-sm text-amber-600 mt-1">
            Vô hiệu hóa tất cả refresh tokens. Users sẽ cần đăng nhập lại để nhận JWT mới với permissions đúng.
          </p>
        </div>
        <button
          @click="invalidateAllTokens"
          :disabled="invalidating"
          class="px-4 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 disabled:opacity-50 font-medium text-sm whitespace-nowrap ml-4"
        >
          {{ invalidating ? 'Đang xử lý...' : '🔄 Invalidate All Tokens' }}
        </button>
      </div>
      <div v-if="invalidateResult" class="mt-3 text-sm font-medium" :class="invalidateResult.success ? 'text-green-700' : 'text-red-700'">
        {{ invalidateResult.message }}
      </div>
    </div>

    <!-- User Selector -->
    <div class="bg-white rounded-xl border border-gray-200 p-4 mb-6">
      <label class="block text-sm font-medium text-gray-700 mb-2">Chọn người dùng để kiểm tra:</label>
      <div class="flex gap-3">
        <select
          v-model="selectedUserId"
          class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        >
          <option value="">-- Chọn user --</option>
          <option v-for="u in users" :key="u.userId" :value="u.userId">
            {{ u.username }} ({{ u.primaryRole }})
          </option>
        </select>
        <button
          @click="loadUserPermissions"
          :disabled="!selectedUserId || loadingPerms"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 text-sm font-medium"
        >
          {{ loadingPerms ? 'Đang tải...' : 'Xem quyền' }}
        </button>
      </div>
    </div>

    <!-- Permission Matrix -->
    <div v-if="userPermData" class="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div class="p-4 border-b border-gray-100 bg-gray-50">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="font-semibold text-gray-800">{{ userPermData.username }}</h3>
            <p class="text-sm text-gray-500">
              Roles: <span class="font-medium text-blue-600">{{ userPermData.allRoles?.join(', ') }}</span>
            </p>
          </div>
          <div class="text-right">
            <span class="text-2xl font-bold text-blue-600">{{ userPermData.permissionCount }}</span>
            <p class="text-xs text-gray-500">permissions</p>
          </div>
        </div>
      </div>

      <!-- Group by module -->
      <div class="divide-y divide-gray-100">
        <div v-for="(perms, module) in groupedPermissions" :key="module" class="p-4">
          <h4 class="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">{{ module }}</h4>
          <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2">
            <div
              v-for="perm in perms"
              :key="perm.key"
              class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm"
              :class="perm.hasAccess ? 'bg-green-50 text-green-800' : 'bg-gray-50 text-gray-400'"
            >
              <span>{{ perm.hasAccess ? '✅' : '❌' }}</span>
              <span class="font-mono text-xs">{{ perm.key.split('.')[1] }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else-if="!loadingPerms" class="text-center py-12 text-gray-400">
      <div class="text-4xl mb-3">🔍</div>
      <p>Chọn một user để xem ma trận phân quyền</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/apiClient'

const users = ref([])
const selectedUserId = ref('')
const userPermData = ref(null)
const loadingPerms = ref(false)
const invalidating = ref(false)
const invalidateResult = ref(null)

// All known permissions grouped by module
const allPermissions = [
  { key: 'HopDong.View', module: 'HopDong' },
  { key: 'HopDong.Create', module: 'HopDong' },
  { key: 'HopDong.Edit', module: 'HopDong' },
  { key: 'HopDong.Approve', module: 'HopDong' },
  { key: 'HopDong.Reject', module: 'HopDong' },
  { key: 'HopDong.Upload', module: 'HopDong' },
  { key: 'DoanKham.View', module: 'DoanKham' },
  { key: 'DoanKham.Create', module: 'DoanKham' },
  { key: 'DoanKham.Edit', module: 'DoanKham' },
  { key: 'DoanKham.SetPosition', module: 'DoanKham' },
  { key: 'DoanKham.AssignStaff', module: 'DoanKham' },
  { key: 'DoanKham.ManageOwn', module: 'DoanKham' },
  { key: 'DoanKham.Lock', module: 'DoanKham' },
  { key: 'LichKham.ViewOwn', module: 'LichKham' },
  { key: 'LichKham.ViewAll', module: 'LichKham' },
  { key: 'ChamCong.QR', module: 'ChamCong' },
  { key: 'ChamCong.CheckInOut', module: 'ChamCong' },
  { key: 'ChamCong.ViewAll', module: 'ChamCong' },
  { key: 'BaoCao.View', module: 'BaoCao' },
  { key: 'BaoCao.QC', module: 'BaoCao' },
  { key: 'BaoCao.Export', module: 'BaoCao' },
  { key: 'BaoCao.ViewFinance', module: 'BaoCao' },
  { key: 'Kho.View', module: 'Kho' },
  { key: 'Kho.Edit', module: 'Kho' },
  { key: 'Kho.Import', module: 'Kho' },
  { key: 'Kho.Export', module: 'Kho' },
  { key: 'Kho.Reports', module: 'Kho' },
  { key: 'Luong.View', module: 'Luong' },
  { key: 'Luong.Manage', module: 'Luong' },
  { key: 'NhanSu.View', module: 'NhanSu' },
  { key: 'NhanSu.Manage', module: 'NhanSu' },
  { key: 'KetQua.Write', module: 'KetQua' },
  { key: 'KetQua.QCApprove', module: 'KetQua' },
  { key: 'BenhNhan.View', module: 'BenhNhan' },
  { key: 'QuyetToan.Edit', module: 'QuyetToan' },
  { key: 'QuyetToan.Calculate', module: 'QuyetToan' },
  { key: 'QuyetToan.Finalize', module: 'QuyetToan' },
  { key: 'DieuPhoi.Edit', module: 'DieuPhoi' },
  { key: 'HeThong.UserManage', module: 'HeThong' },
  { key: 'HeThong.RoleManage', module: 'HeThong' },
  { key: 'HeThong.AuditLog', module: 'HeThong' },
  { key: 'AI.SuggestStaff', module: 'AI' },
]

const groupedPermissions = computed(() => {
  if (!userPermData.value) return {}
  const userPerms = new Set(userPermData.value.permissions || [])
  const groups = {}
  for (const p of allPermissions) {
    if (!groups[p.module]) groups[p.module] = []
    groups[p.module].push({ key: p.key, hasAccess: userPerms.has(p.key) })
  }
  return groups
})

async function loadUsers() {
  try {
    const res = await apiClient.get('/api/Users')
    users.value = res.data || []
  } catch (e) {
    console.error('Failed to load users', e)
  }
}

async function loadUserPermissions() {
  if (!selectedUserId.value) return
  loadingPerms.value = true
  userPermData.value = null
  try {
    const res = await apiClient.get(`/api/admin/users/${selectedUserId.value}/permissions`)
    userPermData.value = res.data
  } catch (e) {
    console.error('Failed to load permissions', e)
  } finally {
    loadingPerms.value = false
  }
}

async function invalidateAllTokens() {
  invalidating.value = true
  invalidateResult.value = null
  try {
    const res = await apiClient.post('/api/admin/invalidate-all-tokens')
    invalidateResult.value = {
      success: true,
      message: `✅ ${res.data.message}`
    }
  } catch (e) {
    invalidateResult.value = {
      success: false,
      message: `❌ Lỗi: ${e.response?.data?.message || e.message}`
    }
  } finally {
    invalidating.value = false
  }
}

onMounted(loadUsers)
</script>
