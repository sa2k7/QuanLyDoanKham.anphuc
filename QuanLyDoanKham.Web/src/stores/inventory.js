/**
 * inventory.js - Pinia store cho module Quản lý Vật tư / Kho
 * Wraps các API calls tới /api/Supplies
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const useInventoryStore = defineStore('inventory', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const items = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy danh sách tất cả vật tư — GET /api/Supplies */
  const fetchItems = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Supplies')
      items.value = res.data
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách vật tư'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tạo vật tư mới — POST /api/Supplies */
  const createItem = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Supplies', payload)
      await fetchItems()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo vật tư'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Nhập kho — POST /api/Supplies/import */
  const importStock = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Supplies/import', payload)
      await fetchItems()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi nhập kho'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xuất kho — POST /api/Supplies/export */
  const exportStock = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Supplies/export', payload)
      await fetchItems()
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xuất kho'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy tất cả lịch sử nhập/xuất — GET /api/Supplies/movements */
  const getMovements = async () => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Supplies/movements')
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch sử nhập/xuất'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy lịch sử nhập/xuất của một vật tư — GET /api/Supplies/movements/{supplyId} */
  const getItemMovements = async (supplyId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Supplies/movements/${supplyId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải lịch sử vật tư'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    items,
    loading,
    error,
    // Actions
    fetchItems,
    createItem,
    importStock,
    exportStock,
    getMovements,
    getItemMovements,
  }
})
