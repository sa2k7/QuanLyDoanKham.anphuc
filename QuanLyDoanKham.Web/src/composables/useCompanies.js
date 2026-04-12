import { ref, computed } from 'vue'
import apiClient from '../services/apiClient'
import { useToast } from './useToast'

export function useCompanies() {
    const toast = useToast()
    const list = ref([])
    const searchQuery = ref('')
    const isLoading = ref(false)
    const showDeleteConfirm = ref(false)
    const deleteTargetId = ref(null)

    const filteredList = computed(() => {
        if (!searchQuery.value) return list.value
        const q = searchQuery.value.toLowerCase()
        return list.value.filter(c =>
            c.companyName?.toLowerCase().includes(q) ||
            (c.shortName || '').toLowerCase().includes(q) ||
            c.taxCode?.includes(q)
        )
    })

    const fetchList = async () => {
        isLoading.value = true
        try {
            const res = await apiClient.get('/api/Companies')
            list.value = res.data
        } catch (e) {
            toast.error("Lỗi tải danh mục công ty")
        } finally {
            isLoading.value = false
        }
    }

    const saveCompany = async (companyData) => {
        // Validation check for duplicates
        if (!companyData.companyId) {
            const duplicate = list.value.find(c =>
                (c.companyName && companyData.companyName && c.companyName.toLowerCase() === companyData.companyName.toLowerCase()) ||
                (c.taxCode && companyData.taxCode && c.taxCode === companyData.taxCode)
            )
            if (duplicate) {
                toast.warning(`Phát hiện trùng lặp với công ty: ${duplicate.companyName}`)
                return false // Indicate failure
            }
        }

        try {
            if (companyData.companyId) {
                await apiClient.put(`/api/Companies/${companyData.companyId}`, companyData)
            } else {
                await apiClient.post('/api/Companies', companyData)
            }
            toast.success("Đã ghi nhận dữ liệu công ty!")
            await fetchList()
            return true // Indicate success
        } catch (e) {
            toast.error(e.response?.data || "Lỗi khi lưu dữ liệu")
            return false
        }
    }

    const deleteCompany = async (companyId) => {
        try {
            await apiClient.delete(`/api/Companies/${companyId}`)
            toast.success("Đã xóa công ty!")
            await fetchList()
            return true
        } catch (e) {
            toast.error(e.response?.data || "Không thể xóa công ty này")
            return false
        }
    }

    const confirmDelete = (companyId) => {
        deleteTargetId.value = companyId
        showDeleteConfirm.value = true
    }

    const handleDeleteConfirm = async () => {
        if (deleteTargetId.value) {
            const success = await deleteCompany(deleteTargetId.value)
            if (success) {
                showDeleteConfirm.value = false
                deleteTargetId.value = null
            }
        }
    }

    return {
        list,
        filteredList,
        searchQuery,
        isLoading,
        showDeleteConfirm,
        deleteTargetId,
        fetchList,
        saveCompany,
        deleteCompany,
        confirmDelete,
        handleDeleteConfirm
    }
}
