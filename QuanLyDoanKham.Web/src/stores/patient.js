/**
 * patient.js - Pinia store cho module Quản lý Bệnh nhân
 * Wraps các API calls tới /api/Patients
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import apiClient from '@/services/apiClient'

export const usePatientStore = defineStore('patient', () => {
  // ── STATE ────────────────────────────────────────────────────────────────
  const patients = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ── ACTIONS ──────────────────────────────────────────────────────────────

  /** Lấy danh sách bệnh nhân (hỗ trợ filter: contractId, search, page, pageSize) */
  const fetchPatients = async (params = {}) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Patients', { params })
      patients.value = res.data
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải danh sách bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách bệnh nhân theo đoàn khám (qua MedicalRecords) */
  const getPatientsByGroup = async (groupId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Patients/by-group/${groupId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải bệnh nhân theo đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Lấy danh sách bệnh nhân theo hợp đồng */
  const getPatientsByContract = async (contractId) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get(`/api/Patients/by-contract/${contractId}`)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tải bệnh nhân theo hợp đồng'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Tạo bệnh nhân mới */
  const createPatient = async (payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.post('/api/Patients', payload)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi tạo bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Cập nhật thông tin bệnh nhân */
  const updatePatient = async (id, payload) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.put(`/api/Patients/${id}`, payload)
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi cập nhật bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xóa bệnh nhân */
  const deletePatient = async (id) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete(`/api/Patients/${id}`)
      patients.value = patients.value.filter((p) => p.patientId !== id && p.id !== id)
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xóa bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Import danh sách bệnh nhân từ file Excel DSCN vào đoàn khám */
  const importFromExcel = async (groupId, file) => {
    loading.value = true
    error.value = null
    try {
      const formData = new FormData()
      formData.append('MedicalGroupId', groupId)
      formData.append('File', file)
      const res = await apiClient.post('/api/Patients/import-group', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi import danh sách bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xuất danh sách bệnh nhân ra file Excel */
  const exportToExcel = async (params = {}) => {
    loading.value = true
    error.value = null
    try {
      const res = await apiClient.get('/api/Patients/export', {
        params,
        responseType: 'blob',
      })
      return res.data
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xuất danh sách bệnh nhân'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** Xóa bệnh nhân khỏi đoàn khám */
  const removeFromGroup = async (groupId, patientId) => {
    loading.value = true
    error.value = null
    try {
      await apiClient.delete('/api/Patients/group-patient', {
        params: { groupId, patientId },
      })
    } catch (err) {
      error.value = err.response?.data?.message || 'Lỗi khi xóa bệnh nhân khỏi đoàn khám'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    patients,
    loading,
    error,
    // Actions
    fetchPatients,
    getPatientsByGroup,
    getPatientsByContract,
    createPatient,
    updatePatient,
    deletePatient,
    importFromExcel,
    exportToExcel,
    removeFromGroup,
  }
})
