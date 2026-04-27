/**
 * staff.js - Pinia store cho module Quản lý Nhân sự
 * Wraps các API calls tới /api/Staffs và /api/MedicalGroups (AI scheduler)
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useStaffStore = defineStore('staff', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const staffList = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy danh sách tất cả nhân sự đang hoạt động */
  const fetchStaff = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Staffs')
      staffList.value = res.data
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách nhân sự'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy chi tiết nhân sự theo ID */
  const getStaffById = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Staffs/${id}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải chi tiết nhân sự'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tạo nhân sự mới */
  const createStaff = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Staffs', payload)
      await fetchStaff()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo nhân sự'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Cập nhật thông tin nhân sự */
  const updateStaff = async (id, payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/Staffs/${id}`, payload)
      await fetchStaff()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi cập nhật nhân sự'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xóa nhân sự (soft delete) */
  const deleteStaff = async (id) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/Staffs/${id}`)
      staffList.value = staffList.value.filter(
        (s) => s.staffId !== id && s.id !== id
      )
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xóa nhân sự'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tổng hợp lương của nhân sự */
  const getPayrollSummary = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Staffs/${id}/payroll-summary`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tổng hợp lương'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch sử chấm công của nhân sự */
  const getAttendance = async (id, params = {}) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Staffs/${id}/attendance`, { params })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch sử chấm công'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch làm việc của người dùng hiện tại */
  const getMySchedule = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/MedicalGroups/my-schedule')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch làm việc'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** AI gợi ý nhân sự cho đoàn khám (theo vị trí nếu có) */
  const suggestStaffForGroup = async (groupId, positionName = null) => {
    loading.value = true
    error.value = null
    try {
      const params = positionName ? { positionName } : {}
      const res = await apiClient.get(`/api/MedicalGroups/${groupId}/ai-suggest-staff`, { params })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi lấy gợi ý AI'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tự động tạo đoàn và phân công nhân sự */
  const autoCreateWithStaff = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/MedicalGroups/auto-create-with-staff', payload)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tự động tạo đoàn'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    staffList,
    loading,
    error,
    // Actions
    fetchStaff,
    getStaffById,
    createStaff,
    updateStaff,
    deleteStaff,
    getPayrollSummary,
    getAttendance,
    getMySchedule,
    suggestStaffForGroup,
    autoCreateWithStaff,
  }
})
