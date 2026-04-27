/**
 * finance.js - Pinia store cho module Tài chính / Quyết toán
 * Wraps các API calls tới /api/Settlement và /api/Contracts/{id}/...
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useFinanceStore = defineStore('finance', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy chi tiết quyết toán theo hợp đồng — GET /api/Settlement/detail/{contractId} */
  const getSettlementDetail = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Settlement/detail/${contractId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải chi tiết quyết toán'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy thống kê tổng hợp — GET /api/Settlement/master-stats?startDate=X&endDate=Y */
  const getMasterStats = async (params = {}) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Settlement/master-stats', { params })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải thống kê tổng hợp'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy quyết toán hợp đồng — GET /api/Contracts/{id}/settlement */
  const getContractSettlement = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Contracts/${contractId}/settlement`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải quyết toán hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy báo cáo lãi/lỗ — GET /api/Contracts/{id}/pnl-report */
  const getPnlReport = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Contracts/${contractId}/pnl-report`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải báo cáo lãi/lỗ'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Chốt quyết toán — POST /api/Contracts/{id}/costs/settle */
  const finalizeSettlement = async (contractId, note = '') => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post(`/api/Contracts/${contractId}/costs/settle`, { note })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi chốt quyết toán'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách snapshot chi phí — GET /api/Contracts/{id}/costs/snapshots */
  const getCostSnapshots = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Contracts/${contractId}/costs/snapshots`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải snapshot chi phí'
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
    getSettlementDetail,
    getMasterStats,
    getContractSettlement,
    getPnlReport,
    finalizeSettlement,
    getCostSnapshots,
  }
})
