/**
 * team.js - Pinia store cho module Quản lý Đoàn Khám (MedicalGroups)
 * Wraps các API calls tới /api/MedicalGroups
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useTeamStore = defineStore('team', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const teams = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy danh sách tất cả đoàn khám */
  const fetchTeams = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/MedicalGroups')
      teams.value = res.data
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy chi tiết một đoàn khám theo ID */
  const getTeamById = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/MedicalGroups/${id}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải chi tiết đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tạo đoàn khám mới thủ công */
  const createTeam = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/MedicalGroups', payload)
      await fetchTeams()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Cập nhật thông tin đoàn khám */
  const updateTeam = async (id, payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/MedicalGroups/${id}`, payload)
      await fetchTeams()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi cập nhật đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tự động tạo đoàn khám từ hợp đồng (mỗi ngày một đoàn) */
  const generateFromContract = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post(`/api/MedicalGroups/generate-from-contract/${contractId}`)
      await fetchTeams()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo đoàn khám từ hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Cập nhật trạng thái đoàn khám */
  const updateStatus = async (id, status, note = '') => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/MedicalGroups/${id}/status`, { status, note })
      await fetchTeams()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi cập nhật trạng thái đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách nhân viên trong đoàn khám */
  const getTeamStaffs = async (teamId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/MedicalGroups/${teamId}/staffs`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách nhân viên đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Thêm nhân viên vào đoàn khám */
  const addStaffToTeam = async (teamId, payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post(`/api/MedicalGroups/${teamId}/staffs`, payload)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi thêm nhân viên vào đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xóa nhân viên khỏi đoàn khám */
  const removeStaff = async (detailId) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/MedicalGroups/staffs/${detailId}`)
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xóa nhân viên khỏi đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch làm việc của nhân viên hiện tại */
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

  return {
    // State
    teams,
    loading,
    error,
    // Actions
    fetchTeams,
    getTeamById,
    createTeam,
    updateTeam,
    generateFromContract,
    updateStatus,
    getTeamStaffs,
    addStaffToTeam,
    removeStaff,
    getMySchedule,
  }
})
