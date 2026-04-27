/**
 * report.js - Pinia store cho module Báo cáo & Thống kê
 * Wraps các API calls tới /api/UnifiedReports và /api/Reports
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useReportStore = defineStore('report', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy dashboard tổng hợp */
  // GET /api/UnifiedReports/dashboard?from=X&to=Y
  const getDashboard = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/dashboard', {
        params: { from, to },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải dashboard'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy biểu đồ doanh thu */
  // GET /api/UnifiedReports/charts/revenue?from=X&to=Y
  const getRevenueChart = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/charts/revenue', {
        params: { from, to },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải biểu đồ doanh thu'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy biểu đồ hợp đồng */
  // GET /api/UnifiedReports/charts/contracts?from=X&to=Y
  const getContractChart = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/charts/contracts', {
        params: { from, to },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải biểu đồ hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy biểu đồ đoàn khám */
  // GET /api/UnifiedReports/charts/medical-groups?from=X&to=Y
  const getMedicalGroupChart = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/charts/medical-groups', {
        params: { from, to },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải biểu đồ đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy thống kê top */
  // GET /api/UnifiedReports/top-stats?from=X&to=Y
  const getTopStats = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/top-stats', {
        params: { from, to },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải thống kê top'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách vật tư sắp hết */
  // GET /api/UnifiedReports/low-stock
  const getLowStock = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/low-stock')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách vật tư sắp hết'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xuất báo cáo PDF */
  // GET /api/UnifiedReports/export/pdf?from=X&to=Y
  const exportPdf = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/export/pdf', {
        params: { from, to },
        responseType: 'blob',
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xuất báo cáo PDF'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xuất báo cáo Excel */
  // GET /api/UnifiedReports/export/excel?from=X&to=Y
  const exportExcel = async (from, to) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/UnifiedReports/export/excel', {
        params: { from, to },
        responseType: 'blob',
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xuất báo cáo Excel'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy KPI dashboard */
  // GET /api/Reports/dashboard-kpis?startDate=X&endDate=Y
  const getDashboardKpis = async (startDate, endDate) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Reports/dashboard-kpis', {
        params: { startDate, endDate },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải KPI dashboard'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    getDashboard,
    getRevenueChart,
    getContractChart,
    getMedicalGroupChart,
    getTopStats,
    getLowStock,
    exportPdf,
    exportExcel,
    getDashboardKpis,
  }
})
