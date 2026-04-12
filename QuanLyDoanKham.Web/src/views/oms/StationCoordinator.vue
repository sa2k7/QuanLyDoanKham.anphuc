<template>
  <div class="station-coordinator">
    <aside class="sidebar">
      <div class="station-header">
        <h2>{{ stationName }}</h2>
        <p>Bảng Điều Phối Trạm</p>
      </div>

      <nav class="queue-list">
        <header>HÀNG CHỜ ({{ queue.length }})</header>
        <div v-if="queue.length === 0" class="empty-queue">Chưa có bệnh nhân</div>
        <div 
          v-for="patient in queue" 
          :key="patient.taskId" 
          class="queue-item"
          :class="{ active: currentTask?.taskId === patient.taskId }"
          @click="selectPatient(patient)"
        >
          <span class="q-no">{{ patient.queueNo }}</span>
          <div class="q-info">
            <span class="name">{{ patient.fullName }}</span>
            <span class="time">{{ formatWaiting(patient.waitingSince) }}</span>
          </div>
        </div>
      </nav>
    </aside>

    <main class="active-area">
      <div v-if="currentTask" class="current-card">
        <header>
          <div class="p-basic">
            <h1>{{ currentTask.fullName }}</h1>
            <span class="p-meta">STT: {{ currentTask.queueNo }} • Giới tính: {{ currentTask.gender }}</span>
          </div>
          <div class="p-status">
            <span class="status-badge" :class="currentTask.status.toLowerCase()">
              {{ currentTask.status }}
            </span>
          </div>
        </header>

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
                   TẠM BỎ QUA BỆNH NHÂN NÀY
                </button>
             </div>
          </div>
        </section>
      </div>
      <div v-else class="welcome-screen flex flex-col items-center justify-center h-full text-center">
        <div class="icon-glow mb-8">
            <div class="pulse-ring"></div>
            <Hospital :size="64" class="text-indigo-500 relative z-10" />
        </div>
        <h2 class="text-2xl font-black text-slate-800 mb-2">CHÀO MỪNG ĐẾN TRẠM {{ stationDisplayName.toUpperCase() }}</h2>
        <p class="text-slate-500 max-w-sm font-medium">Vui lòng chọn một bệnh nhân từ danh sách hàng chờ bên trái để bắt đầu quá trình thăm khám.</p>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { api } from '@/services/apiClient'
import queueHub from '@/services/queueHub'
import { Mic, Hospital, User, Clock, ChevronRight } from 'lucide-vue-next'
import ExamForm from '@/components/examination/ExamForm.vue'
import ExamHistory from '@/components/examination/ExamHistory.vue'

const route = useRoute()
const stationCode = route.params.code
const queue = ref([])
const currentTask = ref(null)

// Map station code to display name (In real app, fetch from master data)
const stationDisplayName = computed(() => {
  const map = {
    'CHECKIN':        'Tiếp Đón & Cấp Số',
    'SINH_HIEU':      'Đo Sinh Hiệu',
    'LAY_MAU':        'Lấy Mẫu Xét Nghiệm',
    'XQUANG':         'Chụp X-Quang',
    'SIEU_AM':        'Siêu Âm',
    'NOI_KHOA':       'Khám Nội Khoa',
    'MAT_TAI_MUI_HONG': 'Mắt - Tai Mũi Họng',
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
      // Implement skip logic if needed
      currentTask.value = null
    }
    await fetchQueue()
  } catch (err) {
    alert('Lỗi: ' + (err.response?.data?.message || err.message))
  }
}

const onExamSuccess = async () => {
    // Backend service Step 2 already handles updating Task to COMPLETED 
    // and potentially updating Record status to QC_PENDING.
    currentTask.value = null
    await fetchQueue()
}

const formatWaiting = (date) => {
  if (!date) return 'Vừa xong'
  const diff = Math.floor((new Date() - new Date(date)) / 60000)
  return diff <= 0 ? 'Vừa xong' : `${diff} m`
}

let unsubscribeHub = null

onMounted(async () => {
  await fetchQueue()
  await queueHub.start()
  await queueHub.joinStation(stationCode)

  unsubscribeHub = queueHub.onUpdate((event, payload) => {
    if (event === 'PatientStatusChanged' && payload?.stationCode === stationCode) {
      // Phase 2: In-place update — mutate the queue array without an API call.
      // This is the fast path: O(n) scan instead of a full network round-trip.
      const taskIndex = queue.value.findIndex(t => t.medicalRecordId === payload.medicalRecordId)
      if (taskIndex !== -1) {
        queue.value[taskIndex] = { ...queue.value[taskIndex], status: payload.newStatus }
        // Sync the currentTask view if this patient is active
        if (currentTask.value?.medicalRecordId === payload.medicalRecordId) {
          currentTask.value = { ...currentTask.value, status: payload.newStatus }
        }
      }
    } else if (
      event === 'StationQueueUpdated' && (payload === stationCode || payload === 'ALL')
    ) {
      // Full re-fetch for structural changes (new patient arrived, patient removed)
      fetchQueue()
    } else if (event === 'QueueUpdated' && payload === 'ALL') {
      // Legacy global signal — only re-fetch if no richer event was received
      fetchQueue()
    }
  })
})

onUnmounted(async () => {
  if (unsubscribeHub) unsubscribeHub() // clean up listener to prevent memory leak
  await queueHub.leaveStation(stationCode)
})
</script>

<style scoped>
.icon-glow {
  position: relative;
  width: 120px;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pulse-ring {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: rgba(79, 70, 229, 0.1);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(0.8); opacity: 0.8; }
  100% { transform: scale(1.5); opacity: 0; }
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

.sidebar {
  width: 320px;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(20px);
  border-right: 1px solid rgba(226, 232, 240, 0.8);
  display: flex;
  flex-direction: column;
}

.queue-item.active {
  background: white;
  margin: 0.5rem;
  border-radius: 16px;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  border-left: 6px solid #6366f1;
}

.q-no {
  font-size: 1.5rem;
  font-weight: 900;
  background: linear-gradient(to bottom, #6366f1, #4f46e5);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
</style>
