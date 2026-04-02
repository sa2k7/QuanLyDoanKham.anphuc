import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import apiClient from '@/services/apiClient'

export const useAuthStore = defineStore('auth', () => {
  // ──────────────────────────────────────────────────────────────────
  // STATE
  // ──────────────────────────────────────────────────────────────────
  const token = ref(localStorage.getItem('token') || null)
  const refreshToken = ref(localStorage.getItem('refreshToken') || null)
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

  // ──────────────────────────────────────────────────────────────────
  // GETTERS
  // ──────────────────────────────────────────────────────────────────
  const isLoggedIn = computed(() => !!token.value)

  /** Primary role name (backward compat) */
  const userRole = computed(() => user.value?.role || '')

  /** Tất cả roles của user */
  const userRoles = computed(() => user.value?.roles || [])

  /** Tất cả permission keys */
  const userPermissions = computed(() => user.value?.permissions || [])

  const fullName = computed(() => user.value?.fullName || '')
  const username = computed(() => user.value?.username || '')
  const companyId = computed(() => user.value?.companyId || null)
  const departmentId = computed(() => user.value?.departmentId || null)
  const departmentName = computed(() => user.value?.departmentName || '')
  const avatarPath = computed(() => user.value?.avatarPath || null)

  // ──────────────────────────────────────────────────────────────────
  // PERMISSION HELPERS
  // ──────────────────────────────────────────────────────────────────

  /** Kiểm tra user có role cụ thể không */
  const hasRole = (roleName) => {
    if (!roleName) return false
    return userRoles.value.includes(roleName) || userRole.value === roleName
  }

  /** Kiểm tra có ít nhất 1 trong danh sách roles */
  const hasAnyRole = (...roles) => roles.some(r => hasRole(r))

  /** Admin luôn có mọi quyền */
  const isAdmin = computed(() => hasRole('Admin'))

  /** Kiểm tra permission key cụ thể (ví dụ: 'HopDong.Approve') */
  const hasPermission = (permissionKey) => {
    if (!permissionKey) return true
    if (isAdmin.value) return true
    return userPermissions.value.includes(permissionKey)
  }

  /** Kiểm tra có ít nhất 1 permission trong danh sách */
  const hasAnyPermission = (...keys) => keys.some(k => hasPermission(k))

  /** Kiểm tra có TẤT CẢ permissions trong danh sách */
  const hasAllPermissions = (...keys) => keys.every(k => hasPermission(k))

  // ──────────────────────────────────────────────────────────────────
  // ACTIONS
  // ──────────────────────────────────────────────────────────────────
  const login = async (credentials) => {
    const response = await apiClient.post('/Auth/login', credentials)
    const data = response.data

    token.value = data.token
    refreshToken.value = data.refreshToken

    // Lưu đầy đủ thông tin user kể cả roles[] và permissions[]
    user.value = {
      username: data.username,
      fullName: data.fullName,
      role: data.role,           // primary (backward compat)
      roles: data.roles || [],   // tất cả roles
      permissions: data.permissions || [], // tất cả permissions
      companyId: data.companyId,
      departmentId: data.departmentId,
      departmentName: data.departmentName,
      avatarPath: data.avatarPath
    }

    // Persist
    localStorage.setItem('token', token.value)
    localStorage.setItem('refreshToken', refreshToken.value)
    localStorage.setItem('user', JSON.stringify(user.value))

    return data
  }

  const logout = () => {
    token.value = null
    refreshToken.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  const refreshAccessToken = async () => {
    try {
      if (!refreshToken.value) throw new Error('No refresh token')
      const response = await apiClient.post('/Auth/refresh', {
        refreshToken: refreshToken.value
      })
      const data = response.data
      token.value = data.token
      refreshToken.value = data.refreshToken
      user.value = {
        ...user.value,
        role: data.role,
        roles: data.roles || [],
        permissions: data.permissions || [],
        avatarPath: data.avatarPath
      }
      localStorage.setItem('token', token.value)
      localStorage.setItem('refreshToken', refreshToken.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      return data.token
    } catch (err) {
      logout()
      throw err
    }
  }

  const updateProfile = (profileData) => {
    user.value = { ...user.value, ...profileData }
    localStorage.setItem('user', JSON.stringify(user.value))
  }

  return {
    // State
    token,
    refreshToken,
    user,
    // Getters
    isLoggedIn,
    isAdmin,
    userRole,
    userRoles,
    userPermissions,
    fullName,
    username,
    companyId,
    departmentId,
    departmentName,
    avatarPath,
    // Permission helpers
    hasRole,
    hasAnyRole,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    // Actions
    login,
    logout,
    refreshAccessToken,
    updateProfile
  }
})
