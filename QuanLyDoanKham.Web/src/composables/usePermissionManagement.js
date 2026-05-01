/**
 * usePermissionManagement.js
 * Composable chứa toàn bộ logic & state cho module Phân Quyền Động.
 * Tách biệt hoàn toàn khỏi UI — component chỉ cần gọi và render.
 *
 * API thật: /api/roles, /api/permissions/matrix, /api/roles/{id}/permissions
 * Mock fallback: permission-mock-data.js
 */
import { ref, computed } from 'vue'
import apiClient from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'
import {
  MOCK_ROLES,
  MOCK_PERMISSION_MATRIX,
  MOCK_ROLE_PERMISSIONS,
} from '@/data/permission-mock-data'

// Cờ bật Mock (đặt false khi API đã sẵn sàng)
const USE_MOCK = false

export function usePermissionManagement() {
  const authStore = useAuthStore()

  // ── Reactive State ──────────────────────────────────────────────────────────
  const roleList            = ref([])
  const permissionGroups    = ref([])
  const selectedRole        = ref(null)
  const activePermissionIds = ref([])
  const isLoading           = ref(false)
  const isSaving            = ref(false)

  // ── Computed ────────────────────────────────────────────────────────────────
  const totalPermissionCount = computed(() => {
    let count = 0
    permissionGroups.value.forEach(group => {
      count += group.permissions.length
    })
    return count
  })

  const activeRoleId = computed(() => selectedRole.value?.roleId ?? null)

  // ── Data Loading ────────────────────────────────────────────────────────────

  const loadRoles = async () => {
    if (USE_MOCK) {
      roleList.value = MOCK_ROLES
      return
    }
    try {
      const response = await apiClient.get('/api/roles')
      roleList.value = response.data
    } catch (error) {
      console.error('Lỗi tải danh sách vai trò:', error)
      roleList.value = MOCK_ROLES // fallback
    }
  }

  const loadPermissionMatrix = async () => {
    if (USE_MOCK) {
      permissionGroups.value = MOCK_PERMISSION_MATRIX
      return
    }
    try {
      const response = await apiClient.get('/api/permissions/matrix')
      permissionGroups.value = response.data
    } catch (error) {
      console.error('Lỗi tải ma trận quyền:', error)
      permissionGroups.value = MOCK_PERMISSION_MATRIX // fallback
    }
  }

  const loadRolePermissions = async (roleId) => {
    if (USE_MOCK) {
      activePermissionIds.value = [...(MOCK_ROLE_PERMISSIONS[roleId] || [])]
      return
    }
    try {
      const response = await apiClient.get(`/api/roles/${roleId}/permissions`)
      activePermissionIds.value = [...response.data]
    } catch (error) {
      console.error('Lỗi tải quyền vai trò:', error)
      activePermissionIds.value = []
    }
  }

  // ── User Actions ────────────────────────────────────────────────────────────

  const selectRole = async (role) => {
    selectedRole.value = role
    isLoading.value = true
    await loadRolePermissions(role.roleId)
    isLoading.value = false
  }

  const togglePermission = (permissionId) => {
    const index = activePermissionIds.value.indexOf(permissionId)
    if (index >= 0) {
      activePermissionIds.value.splice(index, 1)
    } else {
      activePermissionIds.value.push(permissionId)
    }
  }

  const toggleModule = (moduleKey, isChecked) => {
    const targetGroup = permissionGroups.value.find(g => g.module === moduleKey)
    if (!targetGroup) return

    const modulePermissionIds = targetGroup.permissions.map(p => p.permissionId)

    if (isChecked) {
      const mergedIds = new Set([...activePermissionIds.value, ...modulePermissionIds])
      activePermissionIds.value = [...mergedIds]
    } else {
      activePermissionIds.value = activePermissionIds.value
        .filter(id => !modulePermissionIds.includes(id))
    }
  }

  const toggleAll = (isChecked) => {
    if (isChecked) {
      const allPermissionIds = []
      permissionGroups.value.forEach(group => {
        group.permissions.forEach(p => allPermissionIds.push(p.permissionId))
      })
      activePermissionIds.value = allPermissionIds
    } else {
      activePermissionIds.value = []
    }
  }

  const savePermissions = async () => {
    if (!selectedRole.value) return
    isSaving.value = true

    try {
      if (!USE_MOCK) {
        await apiClient.post(
          `/api/roles/${selectedRole.value.roleId}/permissions`,
          activePermissionIds.value
        )
      }

      // Refresh JWT nếu đang sửa quyền của role hiện tại hoặc Admin
      const isEditingOwnRole = authStore.userRole === selectedRole.value.roleName
      const isEditingAdmin = selectedRole.value.roleName === 'Admin'
      if (isEditingOwnRole || isEditingAdmin) {
        try { await authStore.refreshAccessToken() } catch (refreshError) {
          console.warn('Không thể tự động refresh token:', refreshError)
        }
      }

      return { success: true, message: 'Cập nhật phân quyền thành công! Lịch sử đã được ghi nhận.' }
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Có lỗi xảy ra khi lưu phân quyền.'
      return { success: false, message: errorMessage }
    } finally {
      isSaving.value = false
    }
  }

  // ── Init ────────────────────────────────────────────────────────────────────

  const initPermissionModule = async () => {
    await Promise.all([loadRoles(), loadPermissionMatrix()])
    if (roleList.value.length > 0) {
      await selectRole(roleList.value[0])
    }
  }

  // ── Public API ──────────────────────────────────────────────────────────────
  return {
    // State
    roleList,
    permissionGroups,
    selectedRole,
    activePermissionIds,
    isLoading,
    isSaving,
    totalPermissionCount,
    activeRoleId,

    // Actions
    selectRole,
    togglePermission,
    toggleModule,
    toggleAll,
    savePermissions,
    initPermissionModule,
  }
}
