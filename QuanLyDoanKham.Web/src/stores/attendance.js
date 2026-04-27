/**
 * attendance.js - Pinia store cho module Điểm danh / QR Check-in
 * Wraps các API calls tới /api/Attendance
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useAttendanceStore = defineStore('attendance', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Tạo QR cho nhóm (Trưởng đoàn) — GET /api/Attendance/qr/{groupId} */
  const generateQr = async (groupId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Attendance/qr/${groupId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo mã QR'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy QR đang hoạt động hôm nay — GET /api/Attendance/active-qr-today */
  const getActiveQrToday = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Attendance/active-qr-today')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi lấy QR hôm nay'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách điểm danh của nhóm — GET /api/Attendance/group/{groupId} */
  const getGroupAttendance = async (groupId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Attendance/group/${groupId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách điểm danh'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tổng hợp điểm danh của nhân viên — GET /api/Attendance/summary/{staffId}?month=X&year=Y */
  const getAttendanceSummary = async (staffId, month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Attendance/summary/${staffId}`, {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tổng hợp điểm danh'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tổng hợp điểm danh tất cả nhân viên — GET /api/Attendance/summary-all?month=X&year=Y */
  const getAllAttendanceSummary = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Attendance/summary-all', {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tổng hợp điểm danh toàn bộ'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Điểm danh thủ công — POST /api/Attendance/manual */
  const manualCheckIn = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Attendance/manual', payload)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi điểm danh thủ công'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tự điểm danh trực tiếp (one-tap) — POST /api/Attendance/self-checkin-direct */
  const selfCheckInDirect = async (groupId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Attendance/self-checkin-direct', { groupId })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tự điểm danh'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    loading,
    error,
    // Actions
    generateQr,
    getActiveQrToday,
    getGroupAttendance,
    getAttendanceSummary,
    getAllAttendanceSummary,
    manualCheckIn,
    selfCheckInDirect,
  }
})
