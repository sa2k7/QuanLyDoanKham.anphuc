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

      <!-- LEFT: Danh sách hồ sơ -->
      <QcSidebar
        :records="pendingRecords"
        :selectedId="selectedRecord?.medicalRecordId"
        :isLoading="isLoading"
        @select="selectRecord"
      />

      <!-- RIGHT: Chi tiết + Thao tác -->
      <main class="flex-1 overflow-y-auto p-8">
        <div v-if="!selectedRecord" class="h-full flex flex-col items-center justify-center text-center">
          <div class="relative mb-8">
            <div class="w-24 h-24 bg-orange-50 rounded-full flex items-center justify-center">
              <ClipboardCheck :size="48" class="text-orange-400" />
            </div>
          </div>
          <h2 class="text-2xl font-black text-slate-700 mb-2">Chọn một hồ sơ để duyệt</h2>
          <p class="text-slate-400 max-w-sm">Chọn hồ sơ từ danh sách bên trái để xem kết quả khám và thực hiện phê duyệt QC.</p>
        </div>

        <div v-else class="max-w-3xl mx-auto space-y-6 animate-fade-in">
          <QcPatientCard
            :fullName="selectedRecord.fullName"
            :medicalRecordId="selectedRecord.medicalRecordId"
          />

          <QcExamHistory
            :history="examHistory"
            :isLoading="isLoadingHistory"
          />

          <QcActionBox
            :actionLoading="actionLoading"
            :actionError="actionError"
            @pass="handleQcPass"
            @rework="handleQcRework"
            @download-pdf="handleDownloadPdf"
            @ai-summary="showAiSummaryModal = true"
          />

          <!-- AI Clinical Summary Modal -->
          <AiClinicalSummaryModal
            v-if="showAiSummaryModal"
            :medicalRecordId="selectedRecord.medicalRecordId"
            @close="showAiSummaryModal = false"
          />
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { api } from '@/services/apiClient'
import { ShieldCheck, RotateCcw, ClipboardCheck } from 'lucide-vue-next'
import AiClinicalSummaryModal from '@/components/qc/AiClinicalSummaryModal.vue'

// Sub-components
import QcSidebar from '@/components/qc/QcSidebar.vue'
import QcPatientCard from '@/components/qc/QcPatientCard.vue'
import QcExamHistory from '@/components/qc/QcExamHistory.vue'
import QcActionBox from '@/components/qc/QcActionBox.vue'

const showAiSummaryModal = ref(false)

// ─── State ───────────────────────────────────────────────────────────────────
const pendingRecords  = ref([])
const selectedRecord  = ref(null)
const examHistory     = ref([])
const isLoading       = ref(false)
const isLoadingHistory = ref(false)
const actionLoading   = ref(false)
const actionError     = ref('')

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
  actionError.value = ''
  fetchExamHistory(record.medicalRecordId)
}

const handleQcPass = async () => {
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/MedicalRecords/qc/${selectedRecord.value.medicalRecordId}/pass`)
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

const handleDownloadPdf = async () => {
  if (!selectedRecord.value) return;
  
  actionLoading.value = true;
  actionError.value = '';
  try {
    const res = await api.get(`/api/MedicalRecords/${selectedRecord.value.medicalRecordId}/export-pdf`, {
      responseType: 'blob'
    });
    
    // Create a Blob from the PDF Stream
    const file = new Blob([res.data], { type: 'application/pdf' });
    const fileURL = URL.createObjectURL(file);
    
    // Auto download
    const link = document.createElement('a');
    link.href = fileURL;
    link.download = `KhamSucKhoe_${selectedRecord.value.medicalRecordId}_${selectedRecord.value.fullName}.pdf`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
  } catch (err) {
    actionError.value = 'Lỗi hệ thống khi tạo Báo cáo Y Tế PDF.';
    console.error(err);
  } finally {
    actionLoading.value = false;
  }
}

const handleQcRework = async (reason) => {
  actionLoading.value = true
  actionError.value = ''
  try {
    await api.post(`/api/MedicalRecords/qc/${selectedRecord.value.medicalRecordId}/rework`, {
      reason: reason
    })
    pendingRecords.value = pendingRecords.value.filter(
      r => r.medicalRecordId !== selectedRecord.value.medicalRecordId
    )
    selectedRecord.value = null
  } catch (err) {
    actionError.value = err.response?.data?.message || 'Không thể trả hồ sơ.'
  } finally {
    actionLoading.value = false
  }
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
