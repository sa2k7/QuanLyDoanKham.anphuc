<template>
  <div class="p-8 bg-slate-50 min-h-screen">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-10">
        <div>
            <h1 class="text-3xl font-black text-slate-900 tracking-tighter uppercase italic flex items-center gap-4">
                <div class="w-12 h-12 bg-primary rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20 rotate-3">
                    <Building2 class="text-white w-6 h-6" />
                </div>
                Quản lý Đoàn khám
            </h1>
            <p class="text-[11px] font-black text-slate-400 uppercase tracking-[0.2em] mt-2 ml-1">Điều phối nhân sự & Theo dõi vận hành thực tế</p>
        </div>
        <div class="flex gap-4">
            <button v-if="can('DoanKham.Create')" @click="showForm = !showForm" 
                    class="h-12 px-8 rounded-2xl bg-primary text-white font-black text-[11px] uppercase tracking-widest hover:bg-primary/90 transition-all shadow-xl shadow-primary/20 active:scale-95 flex items-center gap-3">
                <Plus class="w-4 h-4" /> THÊM ĐOÀN MỚI
            </button>
            <button @click="exportGroups" class="h-12 px-6 rounded-2xl bg-white border-2 border-slate-200 text-slate-600 font-black text-[11px] uppercase tracking-widest hover:border-emerald-500 hover:text-emerald-600 transition-all shadow-sm flex items-center gap-3">
                <Download class="w-4 h-4" /> Xuất Excel
            </button>
        </div>
    </div>

    <!-- Wizard: Group Creation -->
    <GroupCreationWizard 
        v-if="showForm"
        :approved-contracts="approvedContracts"
        :is-loading="isAutoAssignLoading"
        @manual-create="onManualCreate"
        @auto-create="onAutoCreate"
        @close="showForm = false"
    />

    <!-- Stats Summary Section -->
    <GroupStatsSummary 
        :total-count="allGroups.length"
        :open-count="openGroups.length"
        :finished-count="closedGroups.filter(g => g.status === 'Finished').length"
        :locked-count="closedGroups.filter(g => g.status === 'Locked').length"
    />

    <!-- Main List Container -->
    <div class="premium-card bg-white rounded-[2.5rem] p-4 border border-slate-100 shadow-sm relative overflow-hidden">
        <div class="flex gap-2 p-1 bg-slate-50 rounded-2xl border border-slate-100 mb-8 max-w-fit">
            <button @click="activeTab = 'Open'" :class="['px-8 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all', activeTab === 'Open' ? 'bg-white text-primary shadow-lg' : 'text-slate-400 hover:text-slate-600']">
                Đang thực hiện ({{ openGroups.length }})
            </button>
            <button @click="activeTab = 'Closed'" :class="['px-8 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all', activeTab === 'Closed' ? 'bg-white text-slate-600 shadow-lg' : 'text-slate-400 hover:text-slate-600']">
                Lưu trữ / Đã khóa ({{ closedGroups.length }})
            </button>
        </div>

        <div v-if="loading" class="flex flex-col items-center justify-center py-32 gap-4">
            <Loader2 class="w-10 h-10 animate-spin text-primary opacity-20" />
            <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Đang nạp dữ liệu đoàn...</span>
        </div>

        <div v-else-if="filteredGroups.length === 0" class="py-32 text-center">
            <div class="w-20 h-20 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <Search class="w-8 h-8 text-slate-200" />
            </div>
            <h3 class="text-sm font-black text-slate-400 uppercase tracking-widest">Không tìm thấy đoàn khám nào</h3>
        </div>

        <div v-else class="grid gap-0">
            <GroupInformationCard 
                v-for="g in filteredGroups" 
                :key="g.groupId"
                :group="g"
                :staff-count="staffDetails[g.groupId]?.length || 0"
                :lock-status="lockStatuses[g.groupId]"
                :can="can"
                @update-status="updateStatus"
                @lock-group="handleLockGroup"
                @trigger-import="triggerImport"
                @open-supplies="openSuppliesTab"
                @edit-group="openEditGroup"
            >
                <!-- Slots for Table Content -->
                <template #staffs>
                    <GroupStaffAssignment 
                        :group-id="g.groupId"
                        :group-exam-date="g.examDate"
                        :group-start-time="g.startTime"
                        :staff="staffDetails[g.groupId] || []"
                        :positions="groupPositions[g.groupId] || []"
                        :can="can"
                        :is-ai-loading="isAiLoading && currentAiGroupId === g.groupId"
                        :status="g.status"
                        @open-modal="openModalAction"
                        @ai-suggest="handleAiSuggest"
                        @export-staff="(id) => exportGroupStaff(id, g.groupName)"
                        @check-in="checkIn"
                        @check-out="checkOut"
                        @remove-staff="removeStaffAssignment"
                    />
                </template>

                <template #patients>
                    <GroupPatientList 
                        :group-id="g.groupId"
                        :status="g.status"
                        :can="can"
                    />
                </template>

                <template #supplies>
                    <GroupSuppliesConsumption 
                        :group-id="g.groupId"
                        :supplies-data="suppliesData[g.groupId]"
                        :is-loading="loadingSupplies[g.groupId]"
                    />
                </template>
            </GroupInformationCard>
        </div>
    </div>

    <!-- Modals Section -->
    <GroupOperationalModals 
        :modals="modals"
        :group-positions="groupPositions[modals.staff.groupId] || []"
        :recommended-staff="recommendedStaff"
        :other-staff="otherStaff"
        :qr-data="qrData"
        :is-role-mismatch="isRoleMismatch"
        :current-position-name="currentPositionName"
        @close="closeModalAction"
        @add-staff="addStaff"
        @add-position="addPosition"
        @position-change="onPositionChange"
        @copy-qr-url="copyQrUrl"
        @update-group="updateGroup"
    />

    <!-- AI Suggestion Result Modal -->
    <Teleport to="body">
        <div v-if="showAiModal" class="modal-overlay">
            <div class="modal-content max-w-2xl border-indigo-600">
                <div class="modal-body">
                    <div class="flex items-center gap-4 mb-8">
                        <div class="icon-box bg-indigo-50 text-indigo-600">
                            <Sparkles class="w-7 h-7" />
                        </div>
                        <div>
                            <h3 class="modal-title italic">AI Đề xuất nhân sự</h3>
                            <p class="modal-subtitle">Dựa trên chuyên môn & Kết quả khám trung bình</p>
                        </div>
                    </div>
                    
                    <div class="bg-indigo-50/50 p-6 rounded-2xl border border-indigo-100 mb-8">
                        <div class="max-h-[400px] overflow-y-auto">
                            <table class="w-full text-left">
                                <thead class="text-[9px] font-black uppercase text-indigo-400">
                                    <tr>
                                        <th class="pb-4">Nhân viên</th>
                                        <th class="pb-4">Vị trí gợi ý</th>
                                        <th class="pb-4">Lý do</th>
                                        <th class="pb-4 text-center">Ca làm</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="s in aiSuggestions" :key="s.staffId" class="text-xs border-t border-indigo-100/50">
                                        <td class="py-4 font-black text-slate-700 uppercase">
                                            {{ staffList.find(st => st.staffId === s.staffId)?.fullName || 'Không xác định' }}
                                        </td>
                                        <td class="py-4">
                                            <span class="px-3 py-1 bg-white text-indigo-600 rounded-lg font-black uppercase tracking-widest text-[9px] border border-indigo-100/50">
                                                {{ s.workPosition }}
                                            </span>
                                        </td>
                                        <td class="py-4 text-slate-500 italic text-[11px] leading-relaxed">"{{ s.reason }}"</td>
                                        <td class="py-4 text-center font-black">{{ s.shiftType === 1 ? 'Cả ngày' : 'Nửa ngày' }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div class="flex gap-4">
                         <button @click="applyAiSuggestions" class="flex-1 bg-slate-900 text-white py-4 rounded-2xl font-black hover:bg-indigo-600 transition-all shadow-xl shadow-slate-200 flex items-center justify-center gap-3">
                            <Sparkles class="w-5 h-5" />
                            ÁP DỤNG TOÀN BỘ GỢI Ý
                        </button>
                        <button @click="showAiModal = false" class="px-8 py-4 bg-white border-2 border-slate-100 rounded-2xl font-black text-slate-400 hover:bg-slate-50 transition-all uppercase tracking-widest text-[10px]">
                            HỦY BỎ
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>

    <!-- Import Hidden Input -->
    <input type="file" ref="importInput" @change="handleImportFile" style="display:none" accept=".xlsx,.xls" />
    
    <!-- Confirm Dialog -->
    <ConfirmDialog 
        v-if="confirmData.show"
        :title="confirmData.title"
        :message="confirmData.message"
        :variant="confirmData.variant"
        @confirm="confirmData.onConfirm(); confirmData.show = false"
        @cancel="confirmData.show = false"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import apiClient from '../services/apiClient'
import { 
    Building2, Plus, Download, Loader2, Search, Sparkles, X
} from 'lucide-vue-next'

// Atomic Components
import GroupStatsSummary from '../components/groups/GroupStatsSummary.vue'
import GroupCreationWizard from '../components/groups/GroupCreationWizard.vue'
import GroupInformationCard from '../components/groups/GroupInformationCard.vue'
import GroupStaffAssignment from '../components/groups/GroupStaffAssignment.vue'
import GroupPatientList from '../components/groups/GroupPatientList.vue'
import GroupSuppliesConsumption from '../components/groups/GroupSuppliesConsumption.vue'
import GroupOperationalModals from '../components/groups/GroupOperationalModals.vue'

import { useAuthStore } from '../stores/auth'
import { parseApiError } from '../services/errorHelper'
import { usePermission } from '@/composables/usePermission'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const { can } = usePermission()
const toast = useToast()

// State
const groups = ref([])
const allGroups = computed(() => groups.value)
const loading = ref(false)
const contracts = ref([])
const staffList = ref([])
const staffDetails = ref({})
const showForm = ref(false)
const activeTab = ref('Open')
const importInput = ref(null)
const currentGroupId = ref(null)
const lockStatuses = ref({})
const isAutoAssignLoading = ref(false)

// Modals State
const modals = ref({
    staff: { show: false, groupId: null, data: { staffId: null, shiftType: 1.0, positionId: null, workPosition: '', workStatus: 'Đã tham gia', pickupLocation: '' } },
    position: { show: false, groupId: null, data: { positionName: '', requiredCount: 1 } },
    qr: { show: false },
    editGroup: { show: false, groupId: null, data: { groupName: '', examDate: '', status: '', startTime: '', departureTime: '', examContent: '' } }
})
const qrData = ref(null)

// AI Suggestion State
const isAiLoading = ref(false)
const aiSuggestions = ref([])
const showAiModal = ref(false)
const currentAiGroupId = ref(null)

// Supplies State
const loadingSupplies = ref({})
const suppliesData = ref({})
const groupPositions = ref({})

// Computed
const openGroups = computed(() => groups.value.filter(g => g.status === 'Open'))
const closedGroups = computed(() => groups.value.filter(g => g.status !== 'Open'))
const filteredGroups = computed(() => activeTab.value === 'Open' ? openGroups.value : closedGroups.value)
const approvedContracts = computed(() => contracts.value.filter(c => c.status === 'Approved' || c.status === 'Active'))

const currentPositionName = computed(() => {
    const gid = modals.value.staff.groupId
    const pid = modals.value.staff.data.positionId
    if (!gid || !pid) return ''
    return groupPositions.value[gid]?.find(p => p.positionId === pid)?.positionName || ''
})

const getRequiredStaffType = (posName) => {
    if (!posName) return null
    const clinicalKeywords = ['Khám', 'Siêu âm', 'Sản', 'Tai mũi họng', 'Bác sĩ', 'Chẩn đoán']
    const technicalKeywords = ['Lấy máu', 'X-Quang', 'Xét nghiệm', 'Xét nghiệm máu', 'Kỹ thuật']
    
    if (clinicalKeywords.some(k => posName.toLowerCase().includes(k.toLowerCase()))) return 'BacSi'
    if (technicalKeywords.some(k => posName.toLowerCase().includes(k.toLowerCase()))) return 'KyThuatVien'
    return 'DieuDuong'
}

const requiredTypeForSelectedPos = computed(() => getRequiredStaffType(currentPositionName.value))
const recommendedStaff = computed(() => {
    if (!requiredTypeForSelectedPos.value) return []
    return staffList.value.filter(s => s.staffType === requiredTypeForSelectedPos.value)
})
const otherStaff = computed(() => {
    if (!requiredTypeForSelectedPos.value) return staffList.value
    return staffList.value.filter(s => s.staffType !== requiredTypeForSelectedPos.value)
})

const isRoleMismatch = computed(() => {
    const sid = modals.value.staff.data.staffId
    const reqType = requiredTypeForSelectedPos.value
    if (!sid || !reqType) return false
    const staff = staffList.value.find(s => s.staffId === sid)
    if (!staff || !staff.staffType) return false
    if (reqType === 'BacSi' && staff.staffType !== 'BacSi') return true
    if (staff.staffType === 'BacSi' && reqType !== 'BacSi') return true
    return false
})

// Auto-fill workPosition when staffId changes
watch(() => modals.value.staff.data.staffId, (newId) => {
    if (!newId) return
    const staff = staffList.value.find(s => s.staffId === newId)
    if (staff) {
        // Ưu tiên JobTitle, nếu không có thì lấy StaffType
        modals.value.staff.data.workPosition = staff.jobTitle || staff.staffType || ''
    }
})

const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

// Actions
const onManualCreate = async (formData) => {
    try {
        isAutoAssignLoading.value = true
        await apiClient.post('/api/MedicalGroups', { ...formData, status: 'Open' })
        toast.success("Khởi tạo đoàn thành công!")
        showForm.value = false
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
    finally { isAutoAssignLoading.value = false }
}

const onAutoCreate = async (formData) => {
    try {
        isAutoAssignLoading.value = true
        await apiClient.post('/api/MedicalGroups', { ...formData, status: 'Open' })
        await fetchData()
        showForm.value = false
        toast.success("Hệ thống đã tự động tạo đoàn khám thành công!")
    } catch (e) { toast.error(parseApiError(e)) }
    finally { isAutoAssignLoading.value = false }
}

const fetchData = async () => {
    try {
        loading.value = true
        const res = await apiClient.get('/api/MedicalGroups')
        groups.value = res.data
        
        const cRes = await apiClient.get('/api/Contracts')
        contracts.value = cRes.data

        const sRes = await apiClient.get('/api/Staffs')
        staffList.value = sRes.data

        groups.value.forEach(g => {
            fetchGroupStaff(g.groupId)
            fetchGroupPositions(g.groupId)
            if (g.status === 'Finished' || g.status === 'Locked') {
                fetchLockStatus(g.groupId)
            }
        })
    } catch (e) { toast.error("Lỗi khi tải dữ liệu") }
    finally { loading.value = false }
}

const fetchGroupStaff = async (id) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/staffs`)
        staffDetails.value[id] = res.data
    } catch (e) { console.error(e) }
}

const fetchGroupPositions = async (id) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/positions`)
        groupPositions.value[id] = res.data
    } catch (e) { console.error(e) }
}

const fetchLockStatus = async (groupId) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${groupId}/lock-status`)
        lockStatuses.value[groupId] = res.data
    } catch (e) { console.error(e) }
}

const updateStatus = async (id, status) => {
    if (status === 'Finished') {
        confirmData.value = {
            show: true,
            title: 'Hoàn tất đoàn khám',
            message: 'Bạn có chắc chắn muốn kết thúc đoàn khám này? Sau khi hoàn tất, đoàn sẽ chuyển sang trạng thái lưu trữ.',
            variant: 'warning',
            onConfirm: async () => {
                try {
                    await apiClient.put(`/api/MedicalGroups/${id}/status`, { status: status })
                    toast.success(`Đã hoàn tất đoàn khám!`)
                    fetchData()
                } catch (e) { toast.error(parseApiError(e)) }
            }
        }
        return
    }
    try {
        await apiClient.put(`/api/MedicalGroups/${id}/status`, { status: status })
        toast.success(`Đã cập nhật trạng thái đoàn!`)
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
}

const handleLockGroup = async (groupId) => {
    confirmData.value = {
        show: true,
        title: 'Khóa sổ tài chính',
        message: 'Hệ thống sẽ tự động tính toán lương dựa trên ca làm thực tế và khóa toàn bộ dữ liệu đoàn khám này.',
        variant: 'warning',
        onConfirm: async () => {
            try {
                await apiClient.post(`/api/MedicalGroups/${groupId}/lock`)
                toast.success("Đã khóa sổ đoàn khám thành công!")
                fetchData()
            } catch (e) { toast.error(parseApiError(e)) }
        }
    }
}

const openModalAction = (type, gid) => {
    modals.value[type].groupId = gid
    if (type === 'staff') {
        modals.value.staff.data = { staffId: null, shiftType: 1.0, positionId: null, workPosition: '', workStatus: 'Đã tham gia' }
    } else if (type === 'position') {
        modals.value.position.data = { positionName: '', requiredCount: 1 }
    } else if (type === 'qr') {
        openQrModal(gid)
    }
    modals.value[type].show = true
}

const closeModalAction = (type) => {
    modals.value[type].show = false
}

const addStaff = async () => {
    try {
        const gid = modals.value.staff.groupId
        const payload = { ...modals.value.staff.data }
        const pos = groupPositions.value[gid]?.find(p => p.positionId === payload.positionId)
        if (pos) payload.workPosition = pos.positionName

        await apiClient.post(`/api/MedicalGroups/${gid}/staffs`, payload)
        toast.success("Đã phân công nhân sự!")
        modals.value.staff.show = false
        fetchGroupStaff(gid)
        fetchGroupPositions(gid)
    } catch (e) { toast.error(parseApiError(e)) }
}

const addPosition = async () => {
    try {
        const gid = modals.value.position.groupId
        await apiClient.post(`/api/MedicalGroups/${gid}/positions`, modals.value.position.data)
        toast.success("Thêm vị trí thành công!")
        modals.value.position.show = false
        fetchGroupPositions(gid)
    } catch (e) { toast.error(parseApiError(e)) }
}

const openEditGroup = (group) => {
    modals.value.editGroup.groupId = group.groupId
    modals.value.editGroup.data = { 
        groupName: group.groupName,
        examDate: group.examDate ? group.examDate.split('T')[0] : '',
        status: group.status,
        startTime: group.startTime || '',
        departureTime: group.departureTime || '',
        examContent: group.examContent || ''
    }
    modals.value.editGroup.show = true
}

const updateGroup = async () => {
    try {
        const gid = modals.value.editGroup.groupId
        await apiClient.put(`/api/MedicalGroups/${gid}`, modals.value.editGroup.data)
        toast.success("Đã cập nhật thông tin đoàn!")
        modals.value.editGroup.show = false
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
}

const removePosition = async (pid, gid) => {
    confirmData.value = {
        show: true,
        title: 'Xóa vị trí',
        message: 'Bạn có chắc chắn muốn xóa vị trí này không?',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await apiClient.delete(`/api/MedicalGroups/positions/${pid}`)
                toast.success("Đã xóa vị trí!")
                fetchGroupPositions(gid)
            } catch (e) { toast.error(parseApiError(e)) }
        }
    }
}

const handleAiSuggest = async (groupId) => {
    try {
        isAiLoading.value = true
        currentAiGroupId.value = groupId
        const res = await apiClient.post(`/api/MedicalGroups/${groupId}/ai-suggest-staff`)
        let processedData = res.data;
        if (typeof processedData === 'string') {
            processedData = processedData.replace(/,(\s*[\]\}])/g, '$1')
            aiSuggestions.value = JSON.parse(processedData)
        } else {
            aiSuggestions.value = processedData
        }
        showAiModal.value = true
    } catch (e) { toast.error("Lỗi AI: " + parseApiError(e)) }
    finally { isAiLoading.value = false }
}

const applyAiSuggestions = async () => {
    try {
        const groupId = currentAiGroupId.value
        for (const s of aiSuggestions.value) {
            await apiClient.post(`/api/MedicalGroups/${groupId}/staffs`, {
                staffId: s.staffId,
                workPosition: s.workPosition,
                shiftType: s.shiftType,
                workStatus: 'Đã tham gia'
            })
        }
        toast.success("Đã áp dụng gợi ý AI!")
        showAiModal.value = false
        fetchGroupStaff(groupId)
        fetchGroupPositions(groupId)
    } catch (e) { toast.error("Lỗi áp dụng gợi ý: " + e.message) }
}

const openSuppliesTab = async (groupId) => {
    loadingSupplies.value[groupId] = true
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${groupId}/supplies-report`)
        suppliesData.value[groupId] = res.data
    } catch (e) { toast.error(parseApiError(e)) }
    finally { loadingSupplies.value[groupId] = false }
}

const openQrModal = async (gid) => {
    qrData.value = null
    try {
        const res = await apiClient.get(`/api/Attendance/qr/${gid}`)
        qrData.value = res.data
    } catch (e) { toast.error(parseApiError(e)) }
}

const copyQrUrl = () => {
    if (qrData.value?.qrUrl) {
        navigator.clipboard.writeText(qrData.value.qrUrl)
        toast.success("Đã sao chép link!")
    }
}

const exportGroups = async () => {
    try {
        const res = await apiClient.get('/api/MedicalGroups/export', { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'DanhSachDoanKham.xlsx')
        document.body.appendChild(link)
        link.click()
        toast.success("Đã xuất Excel!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const exportGroupStaff = async (id, name) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/export-staff`, { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `NhanSu_${name}.xlsx`)
        document.body.appendChild(link)
        link.click()
        toast.success(`Đã xuất nhân sự đoàn ${name}!`)
    } catch (e) { toast.error("Lỗi xuất file") }
}

const triggerImport = (id) => {
    currentGroupId.value = id
    importInput.value.click()
}

const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        const res = await apiClient.post(`/api/MedicalGroups/upload-data`, formData)
        await apiClient.put(`/api/MedicalGroups/${currentGroupId.value}`, { importFilePath: res.data.path })
        toast.success("Đã Import dữ liệu!")
        fetchData()
    } catch (e) { toast.error("Lỗi Import file") }
}

const checkIn = async (sid, gid) => {
    try {
        await apiClient.post(`/api/MedicalGroups/staffs/${sid}/checkin`)
        toast.success("Đã điểm danh vào!")
        fetchGroupStaff(gid)
    } catch (e) { toast.error(parseApiError(e)) }
}

const checkOut = async (sid, gid) => {
    try {
        await apiClient.post(`/api/MedicalGroups/staffs/${sid}/checkout`)
        toast.success("Đã điểm danh ra!")
        fetchGroupStaff(gid)
    } catch (e) { toast.error(parseApiError(e)) }
}

const removeStaffAssignment = async (sid, gid) => {
    confirmData.value = {
        show: true,
        title: 'Xóa phân công',
        message: 'Bạn có chắc chắn muốn gỡ nhân sự này khỏi đoàn khám?',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await apiClient.delete(`/api/MedicalGroups/staffs/${sid}`)
                toast.success("Đã xóa phân công nhân sự!")
                fetchGroupStaff(gid)
            } catch (e) { toast.error(parseApiError(e)) }
        }
    }
}

const onPositionChange = () => {}

onMounted(fetchData)
</script>

<style scoped>
.dashboard-gradient {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
}

.premium-card {
  background: white;
  border-radius: 2rem;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(8px);
  padding: 1rem;
}

.modal-content {
  background: white;
  width: 100%;
  border-radius: 2.5rem;
  border-width: 2px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
  position: relative;
  overflow: hidden;
  animation: slideUp 0.3s ease-out;
}

.modal-body {
  padding: 2.5rem;
}

.modal-title {
  font-size: 1.5rem;
  font-weight: 900;
  color: #1e293b;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.modal-subtitle {
  font-size: 10px;
  font-weight: 900;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  margin-top: 0.25rem;
}

.icon-box {
  width: 3.5rem;
  height: 3.5rem;
  border-radius: 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
