/**
 * contract.js - Pinia store cho module Quản lý Hợp đồng
 * Wraps các API calls tới /api/Contracts
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useContractStore = defineStore('contract', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const contracts = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy danh sách tất cả hợp đồng */
  const fetchContracts = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Contracts')
      contracts.value = res.data
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy chi tiết một hợp đồng theo ID */
  const getContractById = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Contracts/${id}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải chi tiết hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tạo hợp đồng mới (trạng thái ban đầu: Draft) */
  const createContract = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Contracts', payload)
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Cập nhật thông tin hợp đồng */
  const updateContract = async (id, payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/Contracts/${id}`, payload)
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi cập nhật hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Gửi hợp đồng đi phê duyệt (Draft → PendingApproval) */
  const submitForApproval = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post(`/api/Contracts/${id}/submit`)
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi gửi phê duyệt'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Phê duyệt hợp đồng (PendingApproval → Approved) — yêu cầu quyền HopDong.Approve */
  const approveContract = async (id, note = '') => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.patch(`/api/Contracts/${id}/status`, {
        status: 'Approved',
        note,
      })
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi phê duyệt hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Từ chối hợp đồng */
  const rejectContract = async (id, note = '') => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.patch(`/api/Contracts/${id}/status`, {
        status: 'Rejected',
        note,
      })
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi từ chối hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Kích hoạt hợp đồng (Approved → Active) */
  const activateContract = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.patch(`/api/Contracts/${id}/status`, {
        status: 'Active',
        note: 'Kích hoạt hợp đồng để triển khai',
      })
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi kích hoạt hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Upload file đính kèm cho hợp đồng */
  const uploadAttachment = async (id, file) => {
    loading.value = true
    error.value = null
    try {
      const formData = new FormData()
      formData.append('file', file)
      const res = await apiClient.post(`/api/Contracts/${id}/upload`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lên file đính kèm'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xóa hợp đồng */
  const deleteContract = async (id) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/Contracts/${id}`)
      contracts.value = contracts.value.filter(
        (c) => c.healthContractId !== id && c.id !== id
      )
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xóa hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Khóa hợp đồng */
  const lockContract = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/Contracts/${id}/lock`)
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi khóa hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Mở khóa hợp đồng */
  const unlockContract = async (id) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/Contracts/${id}/unlock`)
      await fetchContracts()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi mở khóa hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    contracts,
    loading,
    error,
    // Actions
    fetchContracts,
    getContractById,
    createContract,
    updateContract,
    submitForApproval,
    approveContract,
    rejectContract,
    activateContract,
    uploadAttachment,
    deleteContract,
    lockContract,
    unlockContract,
  }
})
