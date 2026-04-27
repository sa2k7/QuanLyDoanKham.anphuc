/**
 * payroll.js - Pinia store cho module Lương & Chấm công
 * Wraps các API calls tới /api/Attendance và /api/Staffs (payroll)
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const usePayrollStore = defineStore('payroll', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy tổng hợp chấm công của một nhân viên theo tháng/năm */
  // GET /api/Attendance/summary/{staffId}?month=X&year=Y
  const getAttendanceSummary = async (staffId, month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Attendance/summary/${staffId}`, {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tổng hợp chấm công'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tổng hợp chấm công toàn bộ nhân viên theo tháng/năm */
  // GET /api/Attendance/summary-all?month=X&year=Y
  const getAllAttendanceSummary = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Attendance/summary-all', {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tổng hợp chấm công toàn bộ'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tóm tắt lương của một nhân viên */
  // GET /api/Staffs/{id}/payroll-summary
  const getStaffPayrollSummary = async (staffId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Staffs/${staffId}/payroll-summary`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải tóm tắt lương nhân viên'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tính lương cho một nhân viên theo tháng/năm */
  // POST /api/Staffs/{id}/calculate-payroll?month=X&year=Y
  const calculatePayroll = async (staffId, month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post(`/api/Staffs/${staffId}/calculate-payroll`, null, {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tính lương nhân viên'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tính lương cho toàn bộ nhân viên theo tháng/năm */
  // POST /api/Staffs/calculate-payroll-all?month=X&year=Y
  const calculateAllPayroll = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Staffs/calculate-payroll-all', null, {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tính lương toàn bộ nhân viên'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy báo cáo tổng hợp lương theo tháng/năm */
  // GET /api/Reports/payroll-summary?month=X&year=Y
  const getPayrollReport = async (month, year) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Reports/payroll-summary', {
        params: { month, year },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải báo cáo lương'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    getAttendanceSummary,
    getAllAttendanceSummary,
    getStaffPayrollSummary,
    calculatePayroll,
    calculateAllPayroll,
    getPayrollReport,
  }
})
