<template>
  <div class="qc-page min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50">

    <!-- Header -->
    <header class="qc-header sticky top-0 z-30 backdrop-blur-xl bg-white/80 border-b border-slate-100 px-8 py-4 flex items-center justify-between shadow-sm">
      <div class="flex items-center gap-4">
        <div class="p-3 bg-orange-500/10 rounded-2xl text-orange-600">
          <ShieldCheck :size="28" />
        </div>
        <div>
          <h1 class="text-xl font-black text-slate-800 uppercase tracking-wide">Duyệt Hồ Sơ QC</h1>
          <p class="text-xs text-slate-500 font-medium">Kiểm soát chất lượng kết quả thăm khám</p>
        </div>
      </div>
      <div class="flex items-center gap-3">
        <div class="px-4 py-2 bg-orange-50 border border-orange-100 rounded-2xl">
          <span class="text-xs font-black text-orange-600 uppercase">Đang chờ QC: </span>
          <span class="text-lg font-black text-orange-600">{{ pendingRecords.length }}</span>
        </div>
        <button @click="fetchPendingRecords" class="p-2 text-slate-400 hover:text-indigo-500 transition-colors">
          <RotateCcw :size="18" :class="{ 'animate-spin': isLoading }" />
        </button>
      </div>
    </header>

    <div class="flex h-[calc(100vh-80px)]">

      <!-- LEFT: Danh sách hồ sơ chờ QC -->
      <aside class="w-80 bg-white/80 border-r border-slate-100 flex flex-col overflow-hidden backdrop-blur-xl">
        <div class="p-4 border-b border-slate-50 bg-slate-50/80">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ pendingRecords.length }} HỒ SƠ CHỜ DUYỆT</p>
        </div>

        <div v-if="isLoading" class="flex items-center justify-center py-16 text-slate-400">
          <Loader2 class="animate-spin" :size="32" />
        </div>

        <div v-else-if="pendingRecords.length === 0" class="flex flex-col items-center justify-center py-16 text-center px-6">
          <div class="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mb-4">
            <CheckCircle2 :size="32" class="text-emerald-500" />
          </div>
          <p class="font-black text-slate-400 text-sm uppercase">Tất cả đã được duyệt!</p>
          <p class="text-slate-400 text-xs mt-1">Không có hồ sơ nào đang chờ QC.</p>
        </div>

        <ul v-else class="flex-1 overflow-y-auto divide-y divide-slate-50">
          <li
            v-for="record in pendingRecords"
            :key="record.medicalRecordId"
            @click="selectRecord(record)"
            class="p-4 cursor-pointer hover:bg-indigo-50/50 transition-all group"
            :class="{ 'bg-indigo-50 border-l-4 border-indigo-500': selectedRecord?.medicalRecordId === record.medicalRecordId }"
          >
            <div class="flex items-center justify-between">
              <div>
                <p class="font-black text-slate-800 text-sm">{{ record.fullName }}</p>
                <p class="text-[11px] text-slate-500 mt-0.5">STT: <span class="font-bold text-indigo-500">#{{ record.queueNo }}</span></p>
              </div>
              <ChevronRight :size="16" class="text-slate-300 group-hover:text-indigo-400 transition-colors" />
            </div>
            <div class="mt-2 flex gap-1.5 flex-wrap">
              <span
                v-for="task in record.stationTasks" :key="task.stationCode"
                class="text-[10px] font-bold px-2 py-0.5 rounded-full"
                :class="task.status === 'STATION_DONE' ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'"
              >
                {{ task.stationCode }}
              </span>
            </div>
          </li>
        </ul>
      </aside>

      <!-- RIGHT: Chi tiết + Thao tác QC -->
      <main class="flex-1 overflow-y-auto p-8">

        <!-- Trạng thái chưa chọn -->
        <div v-if="!selectedRecord" class="h-full flex flex-col items-center justify-center text-center">
          <div class="relative mb-8">
            <div class="w-24 h-24 bg-orange-50 rounded-full flex items-center justify-center">
              <ClipboardCheck :size="48" class="text-orange-400" />
            </div>
          </div>
          <h2 class="text-2xl font-black text-slate-700 mb-2">Chọn một hồ sơ để duyệt</h2>
          <p class="text-slate-400 max-w-sm">Chọn hồ sơ từ danh sách bên trái để xem kết quả khám và thực hiện phê duyệt QC.</p>
        </div>

        <!-- Khu vực duyệt hồ sơ -->
        <div v-else class="max-w-3xl mx-auto space-y-6 animate-fade-in">

          <!-- Patient info card -->
          <div class="patient-card bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
            <div class="flex items-start justify-between">
              <div>
                <h2 class="text-2xl font-black text-slate-800">{{ selectedRecord.fullName }}</h2>
                <div class="flex items-center gap-4 mt-2">
                  <span class="text-xs font-bold text-slate-500">STT: <span class="text-indigo-600 text-base">#{{ selectedRecord.queueNo }}</span></span>
                  <span class="px-3 py-1 bg-orange-100 text-orange-700 rounded-full text-xs font-black uppercase">Chờ QC</span>
                </div>
              </div>
              <div class="text-right">
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-wider">Mã hồ sơ</p>
                <p class="text-2xl font-black text-slate-700">{{ selectedRecord.medicalRecordId }}</p>
              </div>
            </div>
          </div>

          <!-- Exam results from all stations -->
          <div class="bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
            <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-5">KẾT QUẢ THĂM KHÁM TẠI CÁC TRẠM</h3>

            <div v-if="isLoadingHistory" class="text-center py-8 text-slate-400">
              <Loader2 class="animate-spin mx-auto mb-2" :size="24" />
            </div>

            <div v-else-if="examHistory.length === 0" class="text-center py-8">
              <p class="text-slate-400 text-sm font-bold">Chưa có kết quả khám nào được ghi nhận.</p>
            </div>

            <div v-else class="space-y-4">
              <div
                v-for="result in examHistory" :key="result.examResultId"
                class="result-item border border-slate-100 rounded-2xl p-4 hover:border-indigo-200 hover:bg-indigo-50/30 transition-all"
              >
                <div class="flex items-center justify-between mb-3">
                  <span class="text-indigo-600 font-black text-sm uppercase tracking-wide">{{ result.examType }}</span>
                  <span class="text-[10px] text-slate-400 font-bold italic">{{ formatDate(result.examDate) }}</span>
                </div>
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Kết quả</p>
                    <p class="text-slate-700 font-medium text-sm">{{ result.result }}</p>
                  </div>
                  <div>
                    <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Chẩn đoán</p>
                    <p class="text-slate-800 font-black text-sm text-orange-600">{{ result.diagnosis }}</p>
                  </div>
                </div>
                <p class="text-[11px] text-slate-400 mt-3 pt-3 border-t border-slate-50">
                  Bác sĩ: <span class="font-bold text-slate-600">{{ result.doctorName }}</span>
                </p>
              </div>
            </div>
          </div>

          <!-- QC Action -->
          <div class="bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
            <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-5">QUYẾT ĐỊNH QC</h3>

            <!-- Rework reason (shown if isRejecting) -->
            <div v-if="isRejecting" class="mb-4 animate-fade-in">
              <label class="block text-xs font-black text-slate-500 uppercase tracking-wider mb-2">Lý do trả lại</label>
              <textarea
                v-model="reworkReason"
                placeholder="Nhập lý do cụ thể để bác sĩ chỉnh sửa lại..."
                class="w-full bg-red-50 border-2 border-red-200 rounded-2xl p-4 text-slate-700 font-medium outline-none focus:border-red-400 min-h-[100px] resize-none"
              ></textarea>
            </div>

            <div class="flex gap-4">
              <button
                v-if="!isRejecting"
                @click="handleQcPass"
                :disabled="actionLoading"
                class="flex-1 bg-emerald-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-emerald-700 active:scale-95 transition-all shadow-xl shadow-emerald-200 disabled:opacity-50"
              >
                <CheckCircle2 v-if="!actionLoading" :size="20" />
                <Loader2 v-else class="animate-spin" :size="20" />
                QC ĐẠT — DUYỆT HỒ SƠ
              </button>

              <button
                v-if="isRejecting"
                @click="handleQcRework"
                :disabled="actionLoading || !reworkReason.trim()"
                class="flex-1 bg-red-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-red-700 active:scale-95 transition-all shadow-xl shadow-red-200 disabled:opacity-50"
              >
                <Send v-if="!actionLoading" :size="20" />
                <Loader2 v-else class="animate-spin" :size="20" />
                GỬI TRẢ VỀ BÁC SĨ
              </button>

              <button
                @click="isRejecting = !isRejecting; reworkReason = ''"
                :disabled="actionLoading"
                class="px-6 py-4 rounded-2xl font-black border-2 transition-all active:scale-95"
                :class="isRejecting ? 'border-slate-200 text-slate-500 hover:bg-slate-50' : 'border-red-200 text-red-500 hover:bg-red-50'"
              >
                {{ isRejecting ? 'HỦY' : 'TRẢ LẠI' }}
              </button>
            </div>

            <p v-if="actionError" class="text-center text-red-500 text-xs font-bold bg-red-50 py-2 px-4 rounded-xl mt-4 border border-red-100">
              ⚠️ {{ actionError }}
            </p>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { api } from '@/services/apiClient'
import {
  ShieldCheck, RotateCcw, Loader2, CheckCircle2,
  ClipboardCheck, ChevronRight, Send
} from 'lucide-vue-next'

// ─── State ───────────────────────────────────────────────────────────────────
const pendingRecords  = ref([])
const selectedRecord  = ref(null)
const examHistory     = ref([])
const isLoading       = ref(false)
const isLoadingHistory = ref(false)
const actionLoading   = ref(false)
const actionError     = ref('')
const isRejecting     = ref(false)
const reworkReason    = ref('')

// ─── Data fetching ────────────────────────────────────────────────────────────
const fetchPendingRecords = async () => {
  isLoading.value = true
  try {
    const res = await api.get('/api/MedicalRecords/qc-pending')
    pendingRecords.value = res.data
  } catch (err) {
    console.error('Fetch QC pending error:', err)
  } finally {
    isLoading.value = false
  }
}

const fetchExamHistory = async (recordId) => {
  isLoadingHistory.value = true
  examHistory.value = []
  try {
    const res = await api.get(`/api/ExamResults/record/${recordId}`)
    examHistory.value = res.data
  } catch (err) {
    console.error('Fetch history error:', err)
  } finally {
    isLoadingHistory.value = false
  }
}

// ─── User Actions ─────────────────────────────────────────────────────────────
const selectRecord = (record) => {
  selectedRecord.value = record
  isRejecting.value = false
  reworkReason.value = ''
  actionError.value = ''
  fetchExamHistory(record.medicalRecordId)
}

const handleQcPass = async () => {
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/Oms/qc/${selectedRecord.value.medicalRecordId}/pass`)
    pendingRecords.value = pendingRecords.value.filter(
      r => r.medicalRecordId !== selectedRecord.value.medicalRecordId
    )
    selectedRecord.value = null
  } catch (err) {
    actionError.value = err.response?.data?.message || 'Không thể duyệt hồ sơ.'
  } finally {
    actionLoading.value = false
  }
}

const handleQcRework = async () => {
  if (!reworkReason.value.trim()) return
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/Oms/qc/${selectedRecord.value.medicalRecordId}/rework`, {
      reason: reworkReason.value
    })
    pendingRecords.value = pendingRecords.value.filter(
      r => r.medicalRecordId !== selectedRecord.value.medicalRecordId
    )
    selectedRecord.value = null
    isRejecting.value = false
    reworkReason.value = ''
  } catch (err) {
    actionError.value = err.response?.data?.message || 'Không thể trả hồ sơ.'
  } finally {
    actionLoading.value = false
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getHours().toString().padStart(2,'0')}:${d.getMinutes().toString().padStart(2,'0')} — ${d.getDate()}/${d.getMonth() + 1}`
}

onMounted(fetchPendingRecords)
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.35s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to   { opacity: 1; transform: translateY(0); }
}
</style>
