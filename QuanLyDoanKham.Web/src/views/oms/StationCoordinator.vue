<template>
  <div class="station-coordinator">
    <!-- LEFT: Sidebar with Queue -->
    <StationSidebar
      :stationName="stationDisplayName"
      :queue="queue"
      :selectedId="currentTask?.taskId"
      @select="selectPatient"
    />

    <!-- RIGHT: Active Area -->
    <main class="active-area">
      <div v-if="currentTask" class="current-card">
        <StationPatientBanner :patient="currentTask" />

        <section class="task-actions">
          <button 
            v-if="currentTask.status === 'WAITING'" 
            class="btn-call-in animate-pulse-subtle" 
            @click="handleAction('start')"
          >
            <div class="flex items-center justify-center gap-3">
              <Mic :size="24" />
              <span>GỌI BỆNH NHÂN VÀO KHÁM</span>
            </div>
          </button>
          
          <div v-if="currentTask.status === 'STATION_IN_PROGRESS'" class="exam-workspace animate-grow-in">
             <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <!-- FORM NHẬP KẾT QUẢ -->
                <ExamForm 
                  :record-id="currentTask.medicalRecordId" 
                  :station-code="stationCode"
                  :station-name="stationDisplayName"
                  :default-exam-type="stationDisplayName"
                  @success="onExamSuccess" 
                />

                <!-- LỊCH SỬ KHÁM -->
                <div class="history-panel bg-white/40 backdrop-blur-md rounded-3xl p-6 border border-white/40 shadow-xl overflow-hidden flex flex-col max-h-[600px]">
                   <ExamHistory :record-id="currentTask.medicalRecordId" :key="currentTask.medicalRecordId" />
                </div>
             </div>

             <div class="flex justify-end mt-6">
                <button class="btn-skip text-slate-400 font-bold px-6 py-2 hover:text-orange-500 transition-colors" @click="handleAction('skip')">
                   TẠM BỎ QUA BỆNH NHÂN LẠI
                </button>
             </div>
          </div>
        </section>
      </div>

      <!-- Welcome screen if no patient selected -->
      <StationWelcome v-else :stationName="stationDisplayName" />
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { api } from '@/services/apiClient'
import queueHub from '@/services/queueHub'
import { Mic } from 'lucide-vue-next'

// Sub-components
import StationSidebar from '@/components/station/StationSidebar.vue'
import StationPatientBanner from '@/components/station/StationPatientBanner.vue'
import StationWelcome from '@/components/station/StationWelcome.vue'
import ExamForm from '@/components/examination/ExamForm.vue'
import ExamHistory from '@/components/examination/ExamHistory.vue'

const route = useRoute()
const stationCode = route.params.code
const queue = ref([])
const currentTask = ref(null)

// Map station code to display name
const stationDisplayName = computed(() => {
  const map = {
    'CHECKIN':        'Tiếp Đón & Cấp Số',
    'SINH_HIEU':      'Đo Sinh Hiệu',
    'LAY_MAU':        'Lấy Mẫu Xét Nghiệm',
    'XQUANG':         'Chụp X-Quang',
    'SIEU_AM':        'Siêu Âm',
    'NOI_KHOA':       'Khám Nội Khoa',
    'MAT_TAI_MUI_HONG': 'Mắt - Tai Mùi Họng',
    'QC':             'Kiểm Tra Hồ Sơ (QC)',
  }
  return map[stationCode] || stationCode
})

const fetchQueue = async () => {
  try {
    const res = await api.get(`/api/MedicalRecords/queue/${stationCode}`)
    queue.value = res.data
    
    const inProgress = queue.value.find(t => t.status === 'STATION_IN_PROGRESS')
    if (inProgress) currentTask.value = inProgress
  } catch (err) {
    console.error('Fetch error:', err)
  }
}

const selectPatient = (patient) => {
  if (currentTask.value?.status === 'STATION_IN_PROGRESS' && currentTask.value.medicalRecordId !== patient.medicalRecordId) {
     alert('Vui lòng hoàn thành bệnh nhân hiện tại trước.')
     return
  }
  currentTask.value = patient
}

const handleAction = async (action) => {
  try {
    if (action === 'start') {
      await api.post(`/api/Oms/station/${currentTask.value.medicalRecordId}/start?stationCode=${stationCode}`)
    } else if (action === 'skip') {
      currentTask.value = null
    }
    await fetchQueue()
  } catch (err) {
    alert('Lỗi: ' + (err.response?.data?.message || err.message))
  }
}

const onExamSuccess = async () => {
    currentTask.value = null
    await fetchQueue()
}

let unsubscribeHub = null

onMounted(async () => {
  await fetchQueue()
  await queueHub.start()
  await queueHub.joinStation(stationCode)

  unsubscribeHub = queueHub.onUpdate((event, payload) => {
    if (event === 'PatientStatusChanged' && payload?.stationCode === stationCode) {
      const taskIndex = queue.value.findIndex(t => t.medicalRecordId === payload.medicalRecordId)
      if (taskIndex !== -1) {
        queue.value[taskIndex] = { ...queue.value[taskIndex], status: payload.newStatus }
        if (currentTask.value?.medicalRecordId === payload.medicalRecordId) {
          currentTask.value = { ...currentTask.value, status: payload.newStatus }
        }
      }
    } else if (
      event === 'StationQueueUpdated' && (payload === stationCode || payload === 'ALL')
    ) {
      fetchQueue()
    } else if (event === 'QueueUpdated' && payload === 'ALL') {
      fetchQueue()
    }
  })
})

onUnmounted(async () => {
  if (unsubscribeHub) unsubscribeHub()
  await queueHub.leaveStation(stationCode)
})
</script>

<style scoped>
.station-coordinator {
  display: flex;
  height: 100vh;
  background: #f8fafc;
}

.active-area {
  flex: 1;
  overflow-y: auto;
  padding: 2rem;
}

.btn-call-in {
  width: 100%;
  padding: 2rem;
  background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
  color: white;
  border: none;
  border-radius: 24px;
  font-size: 1.25rem;
  font-weight: 900;
  cursor: pointer;
  box-shadow: 0 20px 40px -10px rgba(79, 70, 229, 0.4);
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.btn-call-in:hover {
  transform: translateY(-5px) scale(1.02);
  box-shadow: 0 30px 60px -15px rgba(79, 70, 229, 0.5);
}

.animate-pulse-subtle {
  animation: pulse-subtle 3s infinite;
}

@keyframes pulse-subtle {
  0% { transform: scale(1); }
  50% { transform: scale(1.01); }
  100% { transform: scale(1); }
}

.animate-grow-in {
  animation: growIn 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards;
}

@keyframes growIn {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
</style>
