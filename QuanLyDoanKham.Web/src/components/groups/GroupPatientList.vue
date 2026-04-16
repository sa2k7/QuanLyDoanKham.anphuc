<template>
  <div class="group-patient-list animate-fade-in">
    <!-- Actions Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
        <div class="flex items-center gap-2">
            <div class="w-1.5 h-6 bg-primary rounded-full"></div>
            <h3 class="text-sm font-black text-slate-800 uppercase tracking-widest italic">DANH SÁCH BỆNH NHÂN TRONG ĐOÀN</h3>
        </div>
        <div class="flex gap-2" v-if="status === 'Open' && can('DoanKham.Edit')">
            <button @click="openAddModal" class="h-10 px-4 rounded-xl bg-white border-2 border-slate-100 text-slate-600 font-black text-[10px] uppercase tracking-widest hover:border-primary hover:text-primary transition-all shadow-sm flex items-center gap-2">
                <UserPlus class="w-3.5 h-3.5" /> Thêm thủ công
            </button>
            <button @click="triggerImport" class="h-10 px-4 rounded-xl bg-slate-900 text-white font-black text-[10px] uppercase tracking-widest hover:bg-primary transition-all shadow-lg flex items-center gap-2">
                <Upload class="w-3.5 h-3.5" /> Import Excel
            </button>
        </div>
    </div>

    <!-- Stats Mini -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng bệnh nhân</p>
            <p class="text-xl font-black text-slate-900 tabular-nums">{{ patients.length }}</p>
        </div>
        <div class="bg-emerald-50 p-4 rounded-2xl border border-emerald-100">
            <p class="text-[9px] font-black text-emerald-600 uppercase tracking-widest mb-1">Đã Check-in</p>
            <p class="text-xl font-black text-emerald-700 tabular-nums">{{ patients.filter(p => p.status !== 'CREATED' && p.status !== 'READY').length }}</p>
        </div>
        <div class="bg-indigo-50 p-4 rounded-2xl border border-indigo-100">
            <p class="text-[9px] font-black text-indigo-600 uppercase tracking-widest mb-1">Đang thực hiện</p>
            <p class="text-xl font-black text-indigo-700 tabular-nums">{{ patients.filter(p => p.status === 'IN_PROGRESS' || p.status === 'STATION_DONE').length }}</p>
        </div>
        <div class="bg-amber-50 p-4 rounded-2xl border border-amber-100">
            <p class="text-[9px] font-black text-amber-600 uppercase tracking-widest mb-1">Hoàn thành</p>
            <p class="text-xl font-black text-amber-700 tabular-nums">{{ patients.filter(p => p.status === 'QC_PASSED' || p.status === 'COMPLETED').length }}</p>
        </div>
    </div>

    <!-- List Table -->
    <div class="bg-white rounded-3xl border border-slate-100 shadow-sm overflow-hidden min-h-[300px] relative">
        <div v-if="loading" class="absolute inset-0 bg-white/50 backdrop-blur-sm z-10 flex flex-col items-center justify-center">
            <Loader2 class="w-8 h-8 animate-spin text-primary opacity-20" />
            <span class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-2">Đang nạp danh sách...</span>
        </div>

        <div v-if="patients.length === 0 && !loading" class="py-20 text-center">
            <UserRound class="w-12 h-12 text-slate-200 mx-auto mb-4" />
            <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Chưa có bệnh nhân nào trong đoàn này</p>
        </div>

        <table v-else class="w-full text-left">
            <thead class="bg-slate-50 border-b border-slate-100">
                <tr>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest w-16 text-center">#</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Bệnh nhân</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Định danh</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Phòng ban</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Trạng thái</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-center">Thao tác</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
                <tr v-for="(p, i) in patients" :key="p.medicalRecordId" class="hover:bg-slate-50/50 transition-all group">
                    <td class="px-6 py-4 text-[11px] font-black text-slate-700 text-center italic">{{ i + 1 }}</td>
                    <td class="px-6 py-4">
                        <div class="flex items-center gap-3">
                            <div class="w-8 h-8 rounded-xl bg-slate-100 flex items-center justify-center text-[10px] font-black text-slate-500 uppercase border border-white shadow-sm">
                                {{ p.fullName?.charAt(0) }}
                            </div>
                            <div>
                                <p class="text-xs font-black text-slate-700 uppercase">{{ p.fullName }}</p>
                                <p class="text-[10px] text-slate-400 font-bold">{{ p.gender }} • {{ formatDate(p.dateOfBirth) }}</p>
                            </div>
                        </div>
                    </td>
                    <td class="px-6 py-4 font-mono text-[11px] text-slate-500">{{ p.iDCardNumber || '---' }}</td>
                    <td class="px-6 py-4">
                        <span class="text-[11px] font-bold text-slate-500 uppercase tracking-tighter">{{ p.department || '---' }}</span>
                    </td>
                    <td class="px-6 py-4">
                        <span :class="['px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border', getStatusColor(p.status)]">
                            {{ formatStatus(p.status) }}
                        </span>
                    </td>
                    <td class="px-6 py-4 text-center">
                        <button @click="removePatient(p)" class="p-2 opacity-0 group-hover:opacity-100 text-rose-400 hover:text-rose-600 transition-all" title="Gỡ khỏi đoàn">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>

    <!-- Import Hidden Input -->
    <input type="file" ref="importInput" @change="handleImportFile" style="display:none" accept=".xlsx,.xls" />

    <!-- Manual Add Modal (Enrollment) -->
    <Teleport to="body">
        <div v-if="showAddModal" class="modal-overlay">
            <div class="modal-content max-w-lg border-indigo-500">
                <button @click="showAddModal = false" class="btn-close-modal">
                    <X class="w-5 h-5" />
                </button>

                <div class="modal-body">
                    <div class="flex items-center gap-4 mb-8">
                        <div class="icon-box bg-indigo-50 text-indigo-600">
                            <UserPlus class="w-7 h-7" />
                        </div>
                        <div>
                            <h3 class="modal-title italic">THÊM BỆNH NHÂN</h3>
                            <p class="modal-subtitle">Ghi danh thủ công vào đoàn khám</p>
                        </div>
                    </div>

                    <div class="space-y-6">
                        <div class="space-y-2">
                            <label class="text-[10px] font-black uppercase text-slate-400 ml-1">Tìm bệnh nhân hiện có</label>
                            <div class="relative">
                                <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                                <input type="text" v-model="enrollSearch" @input="searchPatient" placeholder="Tìm theo tên hoặc CCCD..." class="input-premium pl-12" />
                            </div>
                            
                            <!-- Search Results -->
                            <div v-if="searchResults.length > 0" class="mt-2 bg-white border-2 border-slate-900 rounded-2xl shadow-[4px_4px_0px_#0f172a] max-h-48 overflow-y-auto">
                                <div v-for="p in searchResults" :key="p.patientId" @click="selectPatient(p)" 
                                    class="p-3 hover:bg-slate-50 cursor-pointer flex items-center gap-3 border-b border-slate-50 last:border-0 transition-all">
                                    <div class="w-8 h-8 bg-indigo-50 text-indigo-600 rounded-xl flex items-center justify-center text-[10px] font-black">{{ p.fullName?.charAt(0) }}</div>
                                    <div>
                                        <p class="text-xs font-black text-slate-800 uppercase italic">{{ p.fullName }}</p>
                                        <p class="text-[9px] text-slate-400 font-bold uppercase tracking-widest">{{ p.iDCardNumber || '---' }}</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div v-if="selectedEnroll" class="bg-indigo-50 p-6 rounded-2xl border-2 border-indigo-200">
                            <p class="text-[10px] font-black text-indigo-600 uppercase tracking-widest mb-3">Thông tin bệnh nhân chọn</p>
                            <div class="flex items-center gap-4">
                                <div class="w-12 h-12 rounded-2xl bg-white flex items-center justify-center text-xl font-black text-indigo-600 border border-indigo-100 shadow-sm">
                                    {{ selectedEnroll.fullName?.charAt(0) }}
                                </div>
                                <div>
                                    <p class="text-sm font-black text-slate-800 uppercase">{{ selectedEnroll.fullName }}</p>
                                    <p class="text-xs font-bold text-slate-400 italic">{{ selectedEnroll.department || 'Không có phòng ban' }}</p>
                                </div>
                                <button @click="selectedEnroll = null" class="ml-auto text-slate-300 hover:text-rose-500 transition-all">
                                    <Trash2 class="w-4 h-4" />
                                </button>
                            </div>
                        </div>

                        <div v-else class="p-8 text-center bg-slate-50 rounded-2xl border-2 border-dashed border-slate-200 opacity-60">
                            <UserRound class="w-10 h-10 text-slate-300 mx-auto mb-2" />
                            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">Hãy tìm và chọn bệnh nhân từ danh sách tổng</p>
                        </div>
                    </div>

                    <div class="flex gap-4 mt-10">
                        <button @click="showAddModal = false" class="flex-1 py-4 font-black text-slate-400 uppercase text-[10px] tracking-widest">Hủy bỏ</button>
                        <button @click="submitEnroll" :disabled="!selectedEnroll || enrolling" 
                                class="flex-[2] py-4 rounded-2xl bg-slate-900 text-white font-black text-xs uppercase tracking-widest hover:bg-indigo-600 transition-all shadow-[4px_4px_0px_#cbd5e1] flex items-center justify-center gap-2">
                            <Loader2 v-if="enrolling" class="w-4 h-4 animate-spin" />
                            <Calendar class="w-4 h-4" />
                            <span>Xác nhận ghi danh</span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { 
    UserPlus, Upload, Trash2, UserRound, Loader2, Search, X, Calendar
} from 'lucide-vue-next'
import apiClient from '@/services/apiClient'
import { useToast } from '@/composables/useToast'
import { parseApiError } from '@/services/errorHelper'

const props = defineProps({
  groupId: { type: Number, required: true },
  status: { type: String, default: 'Open' },
  can: { type: Function, required: true }
})

const toast = useToast()
const loading = ref(false)
const patients = ref([])
const importInput = ref(null)

// Enroll State
const showAddModal = ref(false)
const enrollSearch = ref('')
const searchResults = ref([])
const selectedEnroll = ref(null)
const enrolling = ref(false)

const fetchData = async () => {
  try {
    loading.value = true
    const res = await apiClient.get(`/api/MedicalRecords/by-group/${props.groupId}`)
    patients.value = res.data
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'

const formatStatus = (s) => {
  const map = {
    CREATED: 'Mới tạo', READY: 'Sẵn sàng', CHECKED_IN: 'Đã check-in',
    IN_PROGRESS: 'Đang khám', STATION_DONE: 'Xong trạm',
    QC_PENDING: 'Chờ QC', QC_PASSED: 'QC đạt', COMPLETED: 'Hoàn thành'
  }
  return map[s] || s
}

const getStatusColor = (s) => {
  if (['COMPLETED', 'QC_PASSED'].includes(s)) return 'bg-emerald-50 text-emerald-600 border-emerald-100'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'bg-blue-50 text-blue-600 border-blue-100'
  if (s === 'CHECKED_IN') return 'bg-indigo-50 text-indigo-600 border-indigo-100'
  return 'bg-slate-50 text-slate-400 border-slate-100'
}

const triggerImport = () => {
    importInput.value.click()
}

const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        loading.value = true
        // Upload file first
        const uploadRes = await apiClient.post(`/api/MedicalGroups/upload-data`, formData)
        // Ingest data
        await apiClient.post(`/api/MedicalRecords/batch-ingest-excel?groupId=${props.groupId}&filePath=${uploadRes.data.path}`)
        toast.success("Đã Import danh sách bệnh nhân vào đoàn!")
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
    finally { loading.value = false; e.target.value = '' }
}

const removePatient = async (p) => {
    if (!confirm(`Bạn có chắc muốn gỡ bệnh nhân ${p.fullName} khỏi đoàn này?`)) return
    try {
        await apiClient.delete(`/api/MedicalRecords/${p.medicalRecordId}`)
        toast.success("Đã gỡ bệnh nhân khỏi đoàn")
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
}

const openAddModal = () => {
    enrollSearch.value = ''
    searchResults.value = []
    selectedEnroll.value = null
    showAddModal.value = true
}

const searchPatient = async () => {
    if (enrollSearch.value.length < 2) {
        searchResults.value = []
        return
    }
    try {
        const res = await apiClient.get(`/api/Patients?search=${enrollSearch.value}&pageSize=5`)
        searchResults.value = res.data.items || []
    } catch (e) { console.error(e) }
}

const selectPatient = (p) => {
    selectedEnroll.value = p
    searchResults.value = []
    enrollSearch.value = ''
}

const submitEnroll = async () => {
    if (!selectedEnroll.value) return
    enrolling.value = true
    try {
        const p = selectedEnroll.value
        const payload = {
            groupId: props.groupId,
            records: [{
                fullName: p.fullName,
                iDCardNumber: p.iDCardNumber,
                gender: p.gender,
                dateOfBirth: p.dateOfBirth,
                department: p.department
            }]
        }
        await apiClient.post('/api/MedicalRecords/batch-ingest', payload)
        toast.success(`Đã ghi danh ${p.fullName} thành công!`)
        showAddModal.value = false
        fetchData()
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        enrolling.value = false
    }
}

onMounted(fetchData)
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 1000;
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
  border-radius: 3rem;
  border-width: 2px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
  position: relative;
  overflow: hidden;
  animation: slideUp 0.3s ease-out;
}

.modal-body {
  padding: 3rem;
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

.input-premium {
  width: 100%;
  padding: 1rem 1.25rem;
  border-radius: 1.25rem;
  background: #f8fafc;
  border: 2px solid #f1f5f9;
  outline: none;
  font-weight: 900;
  color: #334155;
  transition: all 0.2s;
}

.input-premium:focus {
  border-color: #0f172a;
  background: white;
}

.btn-close-modal {
  position: absolute;
  top: 2rem;
  right: 2rem;
  padding: 0.5rem;
  background: white;
  border: 2px solid #0f172a;
  border-radius: 50%;
  color: #94a3b8;
  z-index: 10;
  box-shadow: 3px 3px 0px #0f172a;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
