<template>
  <div class="h-screen flex flex-col bg-slate-900 text-white overflow-hidden font-sans">
    <!-- Header -->
    <header class="p-6 bg-slate-800 border-b border-slate-700 flex justify-between items-center shadow-2xl relative z-10">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 bg-primary rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20">
          <Monitor class="w-7 h-7 text-white" />
        </div>
        <div>
          <h1 class="text-2xl font-black tracking-tight italic uppercase">Màn Hình Gọi Số</h1>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hệ thống điều phối bệnh nhân tự động</p>
        </div>
      </div>
      <div class="text-right">
        <div class="text-3xl font-black tabular-nums tracking-tighter">{{ currentTime }}</div>
        <div class="text-[10px] font-black text-slate-500 uppercase tracking-widest">{{ currentDate }}</div>
      </div>
    </header>

    <!-- Main Grid -->
    <main class="flex-1 p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 overflow-hidden">
      <div v-for="station in stationQueues" :key="station.stationCode" 
           class="bg-slate-800/50 rounded-[2rem] border border-slate-700/50 flex flex-col overflow-hidden shadow-xl hover:border-primary/30 transition-all group">
        
        <!-- Station Header -->
        <div class="p-5 bg-slate-800 border-b border-slate-700 flex justify-between items-center">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 bg-slate-700 rounded-lg flex items-center justify-center group-hover:bg-primary transition-colors">
              <User class="w-4 h-4 text-slate-400 group-hover:text-white" />
            </div>
            <h3 class="font-black text-slate-200 uppercase tracking-tighter text-sm italic">{{ station.stationName }}</h3>
          </div>
          <div class="px-3 py-1 bg-slate-700 rounded-full text-[10px] font-black text-slate-400 uppercase tracking-widest">
            Đợi: {{ station.waitingCount }}
          </div>
        </div>

        <!-- Serving Now -->
        <div class="p-6 border-b border-slate-700/30 bg-primary/5 relative overflow-hidden">
          <div class="absolute -right-4 -bottom-4 opacity-5 text-primary">
            <Activity class="w-24 h-24" />
          </div>
          <p class="text-[10px] font-black text-primary uppercase tracking-widest mb-3">Đang khám</p>
          <div v-if="station.servingNow.length > 0" class="space-y-3">
            <div v-for="patient in station.servingNow" :key="patient.medicalRecordId" 
                 class="flex items-center justify-between animate-pulse">
              <span class="text-3xl font-black text-white tabular-nums tracking-tighter">#{{ patient.queueNo }}</span>
              <span class="text-sm font-bold text-slate-300 truncate max-w-[120px]">{{ patient.fullName }}</span>
            </div>
          </div>
          <div v-else class="py-4 text-center">
            <p class="text-xs font-bold text-slate-500 italic uppercase tracking-widest">Đang trống</p>
          </div>
        </div>

        <!-- Up Next -->
        <div class="flex-1 p-5 overflow-y-auto scrollbar-none">
          <p class="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-4">Chuẩn bị</p>
          <div v-if="station.upNext.length > 0" class="space-y-4">
            <div v-for="(patient, index) in station.upNext.slice(0, 5)" :key="patient.medicalRecordId" 
                 class="flex items-center justify-between group/item">
              <div class="flex items-center gap-3">
                <span class="text-lg font-black text-slate-400 group-hover/item:text-primary transition-colors tabular-nums">#{{ patient.queueNo }}</span>
                <span class="text-xs font-bold text-slate-400 group-hover/item:text-slate-200 transition-colors">{{ patient.fullName }}</span>
              </div>
              <div v-if="index === 0" class="w-2 h-2 bg-primary rounded-full animate-ping"></div>
            </div>
          </div>
          <div v-else class="h-full flex items-center justify-center opacity-20 grayscale">
            <Users class="w-12 h-12 text-slate-600" />
          </div>
        </div>
      </div>
    </main>

    <!-- Footer / News Ticker -->
    <footer class="h-14 bg-primary flex items-center overflow-hidden relative shadow-2xl">
      <div class="px-6 bg-sky-600 h-full flex items-center font-black italic uppercase text-xs tracking-widest relative z-10 shadow-xl">
        Thông báo
      </div>
      <div class="flex-1 whitespace-nowrap animate-ticker">
        <span class="mx-12 font-bold text-sm uppercase tracking-wider">Chào mừng quý khách đến với buổi khám sức khỏe định kỳ. Vui lòng chuẩn bị mã QR hoặc mã hồ sơ để báo danh tại quầy tiếp đón.</span>
        <span class="mx-12 font-bold text-sm uppercase tracking-wider">Hệ thống đang điều phối bệnh nhân tự động để giảm thời gian chờ đợi. Xin cảm ơn sự hợp tác của quý khách.</span>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { Monitor, User, Users, Activity } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import queueHub from '../services/queueHub'

const currentTime = ref('')
const currentDate = ref('')
const stationQueues = ref([])
const groupId = ref(null) // Lấy từ URL hoặc config

const updateTime = () => {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('vi-VN', { hour12: false })
  currentDate.value = now.toLocaleDateString('vi-VN', { weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric' })
}

const loadQueueData = async () => {
  try {
    // 1. Lấy danh sách các trạm đang hoạt động
    const overviewRes = await apiClient.get('/api/MedicalRecords/queue/overview', {
      params: { groupId: groupId.value }
    })
    
    const stations = overviewRes.data
    const newQueues = []

    // 2. Lấy chi tiết hàng đợi cho mỗi trạm
    for (const s of stations) {
      const queueRes = await apiClient.get(`/api/MedicalRecords/queue/${s.stationCode}`)
      const queue = queueRes.data
      
      newQueues.push({
        stationCode: s.stationCode,
        stationName: s.stationName,
        waitingCount: s.waitingCount,
        servingNow: queue.filter(p => p.status === 'STATION_IN_PROGRESS'),
        upNext: queue.filter(p => p.status === 'WAITING')
      })
    }
    
    stationQueues.value = newQueues
  } catch (err) {
    console.error('Lỗi tải dữ liệu hàng đợi', err)
  }
}

let timer = null

onMounted(() => {
  updateTime()
  timer = setInterval(updateTime, 1000)
  
  // Lấy groupId từ URL query nếu có
  const urlParams = new URLSearchParams(window.location.search)
  groupId.value = urlParams.get('groupId')
  
  loadQueueData()

  // Kết nối SignalR để cập nhật thời gian thực
  queueHub.start().then(() => {
    queueHub.onUpdate((type) => {
      if (type === 'ALL' || type === 'QUEUE') {
        loadQueueData()
      }
    })
    
    queueHub.onStationUpdate(() => {
      loadQueueData()
    })
  })
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<style scoped>
.animate-ticker {
  display: inline-block;
  animation: ticker 30s linear infinite;
}

@keyframes ticker {
  0% { transform: translateX(100%); }
  100% { transform: translateX(-100%); }
}

.scrollbar-none::-webkit-scrollbar {
  display: none;
}
.scrollbar-none {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
