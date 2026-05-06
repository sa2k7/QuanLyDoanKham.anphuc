<template>
  <div class="group-patient-list animate-fade-in">
    <!-- Actions Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
        <div class="flex items-center gap-2">
            <div class="w-1.5 h-6 bg-primary rounded-full"></div>
            <h3 class="text-sm font-black text-slate-800 uppercase tracking-widest italic">DANH SÁCH BỆNH ÁN TRONG ĐOÀN</h3>
        </div>
        <div class="flex gap-2" v-if="status === 'Open' && can('DoanKham.Edit')">
            <button @click="triggerImport" class="h-10 px-4 rounded-xl bg-slate-900 text-white font-black text-[10px] uppercase tracking-widest hover:bg-primary transition-all shadow-lg flex items-center gap-2">
                <Upload class="w-3.5 h-3.5" /> Import Excel
            </button>
            <button @click="syncFromContract" class="h-10 px-4 rounded-xl bg-indigo-50 border-2 border-indigo-100 text-indigo-600 font-black text-[10px] uppercase tracking-widest hover:border-indigo-500 hover:bg-indigo-100 transition-all shadow-sm flex items-center gap-2">
                <RefreshCw class="w-3.5 h-3.5" /> Đồng bộ từ hợp đồng
            </button>
        </div>
    </div>

    <!-- Stats Mini -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng bệnh án</p>
            <p class="text-xl font-black text-slate-900 tabular-nums">{{ patients.length }}</p>
        </div>
        <div class="bg-emerald-50 p-4 rounded-2xl border border-emerald-100">
            <p class="text-[9px] font-black text-emerald-600 uppercase tracking-widest mb-1">Đã Check-in</p>
            <p class="text-xl font-black text-emerald-700 tabular-nums">{{ patients.filter(p => p.status !== 'CREATED' && p.status !== 'READY').length }}</p>
        </div>
        <div class="bg-indigo-50 p-4 rounded-2xl border border-indigo-100">
            <p class="text-[9px] font-black text-indigo-600 uppercase tracking-widest mb-1">Đang khám</p>
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
            <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Chưa có bệnh án nào trong đoàn này</p>
        </div>

        <table v-else class="w-full text-left">
            <thead class="bg-slate-50 border-b border-slate-100">
                <tr>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest w-16 text-center">#</th>
                    <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Họ và tên</th>
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
                    <td class="px-6 py-4 font-mono text-[11px] text-slate-500">{{ p.iDCardNumber || p.idCardNumber || '---' }}</td>
                    <td class="px-6 py-4">
                        <span class="text-[11px] font-bold text-slate-500 uppercase tracking-tighter">{{ p.department || '---' }}</span>
                    </td>
                    <td class="px-6 py-4">
                        <span :class="['px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border', getStatusColor(p.status)]">
                            {{ formatStatus(p.status) }}
                        </span>
                    </td>
                    <td class="px-6 py-4 text-center">
                        <button v-if="p.status === 'CREATED' || p.status === 'READY'" @click="removePatient(p)" class="p-2 opacity-0 group-hover:opacity-100 text-rose-400 hover:text-rose-600 transition-all" title="Gỡ khỏi đoàn">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>

    <!-- Import Hidden Input -->
    <input type="file" ref="importInput" @change="handleImportFile" style="display:none" accept=".xlsx,.xls" />


  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { 
    Upload, Trash2, UserRound, Loader2, X, RefreshCw
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
// Import State
const importInput = ref(null)

const fetchData = async () => {
  try {
    loading.value = true
    // Gọi API lấy bệnh án theo đoàn khám
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
    CREATED: 'Mới tạo', READY: 'Sẵn sàng', CHECKED_IN: 'Đã tiếp đón',
    IN_PROGRESS: 'Đang khám', STATION_DONE: 'Xong trạm',
    QC_PENDING: 'Chờ QC', QC_PASSED: 'QC đạt', QC_REWORK: 'QC trả về',
    COMPLETED: 'Hoàn thành', NO_SHOW: 'Vắng mặt', CANCELLED: 'Đã hủy'
  }
  return map[s] || s
}

const getStatusColor = (s) => {
  if (['COMPLETED', 'QC_PASSED'].includes(s)) return 'bg-emerald-50 text-emerald-600 border-emerald-100'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'bg-blue-50 text-blue-600 border-blue-100'
  if (s === 'CHECKED_IN') return 'bg-indigo-50 text-indigo-600 border-indigo-100'
  if (s === 'QC_PENDING' || s === 'QC_REWORK') return 'bg-pink-50 text-pink-600 border-pink-100'
  if (s === 'NO_SHOW') return 'bg-amber-50 text-amber-600 border-amber-100'
  if (s === 'CANCELLED') return 'bg-red-50 text-red-500 border-red-100'
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
    formData.append('groupId', props.groupId)
    try {
        loading.value = true
        // Gọi đúng route import-group → lưu vào MedicalGroupPatient
        await apiClient.post(`/api/Patients/import-group`, formData)
        toast.success("Đã Import danh sách bệnh án vào đoàn!")
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
    finally { loading.value = false; e.target.value = '' }
}

const syncFromContract = async () => {
    try {
        loading.value = true
        const res = await apiClient.post(`/api/MedicalGroups/${props.groupId}/sync-patients`)
        toast.success(res.data.message)
        fetchData()
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        loading.value = false
    }
}


const removePatient = async (p) => {
    if (!confirm(`Bạn có chắc muốn gỡ bệnh án ${p.fullName} khỏi đoàn này?`)) return
    try {
        // Xóa bệnh án khỏi đoàn
        await apiClient.delete(`/api/MedicalRecords/${p.medicalRecordId}`)
        toast.success("Đã gỡ bệnh án khỏi đoàn")
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
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
