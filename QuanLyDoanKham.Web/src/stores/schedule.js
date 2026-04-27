/**
 * schedule.js - Pinia store cho module Lịch & Lịch trực
 * Wraps các API calls tới /api/Calendar và /api/MedicalGroups (schedule)
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useScheduleStore = defineStore('schedule', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy lịch theo tháng/năm */
  // GET /api/Calendar?month=X&year=Y
  const getCalendar = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Calendar', {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch cá nhân theo tháng/năm */
  // GET /api/Calendar/my-schedule?month=X&year=Y
  const getMySchedule = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Calendar/my-schedule', {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch cá nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch đoàn khám của tôi (toàn bộ) */
  // GET /api/MedicalGroups/my-schedule
  const getMyGroupSchedule = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/MedicalGroups/my-schedule')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy phân công hôm nay */
  // GET /api/MedicalGroups/today-assignment
  const getTodayAssignment = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/MedicalGroups/today-assignment')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải phân công hôm nay'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    getCalendar,
    getMySchedule,
    getMyGroupSchedule,
    getTodayAssignment,
  }
})
