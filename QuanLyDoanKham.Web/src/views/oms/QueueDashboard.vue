<template>
  <div class="oms-dashboard">
    <header class="dashboard-header">
      <div class="brand">
        <h1>OMS MONITORING</h1>
        <p>Hệ thống Điều phối Hàng chờ Thời gian thực</p>
      </div>
      <div class="status-badge" :class="{ connected: isConnected }">
        <span class="dot"></span>
        {{ isConnected ? 'REAL-TIME CONNECTED' : 'RECONNECTING...' }}
      </div>
    </header>

    <div class="dashboard-grid">
      <div v-for="station in stations" :key="station.code" class="station-card">
        <div class="station-header">
          <div class="station-info">
            <h3>{{ station.name }}</h3>
            <span class="station-type">{{ station.type }}</span>
          </div>
          <div class="header-actions">
            <router-link :to="'/oms/station/' + station.code" class="manage-btn">ĐIỀU PHỐI</router-link>
            <div class="count-badge">{{ queueData[station.code]?.length || 0 }}</div>
          </div>
        </div>

        <div class="now-serving" v-if="servingNow[station.code]">
          <label>ĐANG PHỤC VỤ</label>
          <div class="record-info" style="display: flex; justify-content: space-between; align-items: center; width: 100%">
            <div style="display: flex; gap: 1rem; align-items: center;">
              <span class="queue-no">{{ servingNow[station.code].queueNo }}</span>
              <span class="full-name">{{ servingNow[station.code].fullName }}</span>
            </div>
            <button v-if="can('DieuPhoi.Edit')" @click="cancelRecord(servingNow[station.code].medicalRecordId)" class="cancel-btn" title="Hủy / Bỏ cuộc"><XCircle class="w-4 h-4 text-rose-500" /></button>
          </div>
        </div>
        <div class="now-serving empty" v-else>
          <p>ĐANG TRỐNG</p>
        </div>

        <div class="up-next">
          <label>TIẾP THEO</label>
          <ul v-if="queueData[station.code]?.length > 0">
            <li v-for="item in queueData[station.code].slice(0, 3)" :key="item.taskId" style="display: flex; justify-content: space-between; align-items: center;">
              <div style="display: flex; gap: 0.5rem; align-items: center;">
                <span class="q-num">{{ item.queueNo }}</span>
                <span class="q-name">{{ item.fullName }}</span>
              </div>
              <button v-if="can('DieuPhoi.Edit')" @click="cancelRecord(item.medicalRecordId)" class="cancel-btn" title="Hủy / Bỏ cuộc"><XCircle class="w-4 h-4 text-rose-500" /></button>
            </li>
          </ul>
          <p v-else class="empty-msg">Chưa có bệnh nhân</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { api } from '@/services/apiClient'
import queueHub from '@/services/queueHub'
import { XCircle } from 'lucide-vue-next'
import { useToast } from '@/composables/useToast'
import { usePermission } from '@/composables/usePermission'

const { can } = usePermission()
const toast = useToast()
const isConnected = ref(false)
const stations = ref([
  { code: 'SINH_HIEU', name: 'Đo sinh hiệu', type: 'Lâm sàng' },
  { code: 'XQUANG', name: 'X-Quang', type: 'Chẩn đoán hình ảnh' },
  { code: 'SIEU_AM', name: 'Siêu âm', type: 'Chẩn đoán hình ảnh' },
  { code: 'NOI_KHOA', name: 'Nội khoa', type: 'Khám chuyên khoa' },
  { code: 'MAT', name: 'Mắt / Tai Mũi Họng', type: 'Khám chuyên khoa' }
])

const queueData = ref({})
const servingNow = ref({})

const fetchQueue = async (stationCode) => {
  try {
    const res = await api.get(`/api/MedicalRecords/queue/${stationCode}`)
    const allItems = res.data
    servingNow.value[stationCode] = allItems.find(i => i.status === 'STATION_IN_PROGRESS')
    queueData.value[stationCode] = allItems.filter(i => i.status === 'WAITING')
  } catch (err) {
    console.error(`Error fetching queue for ${stationCode}:`, err)
  }
}

const fetchAll = () => {
  stations.value.forEach(s => fetchQueue(s.code))
}

const cancelRecord = async (medicalRecordId) => {
  const reason = prompt('Nhập lý do Hủy / Bỏ cuộc:')
  if (reason === null) return

  try {
    const response = await api.post(`/api/Oms/cancel/${medicalRecordId}`, { reason: reason || 'Bỏ cuộc' })
    toast.success(response.data?.message || 'Đã hủy hồ sơ thành công')
    fetchAll() // Refresh the queue manually, signalR will also fire, but just to be sure
  } catch (err) {
    toast.error('Lỗi khi hủy: ' + (err.response?.data?.message || err.message))
  }
}

onMounted(async () => {
  fetchAll()
  
  await queueHub.start()
  isConnected.value = true

  queueHub.onUpdate((event, payload) => {
    if (event === 'QueueUpdated') {
      fetchAll()
    } else if (event === 'StationQueueUpdated') {
      fetchQueue(payload)
    }
  })
})

onUnmounted(() => {
  // Logic to stop hub if needed
})
</script>

<style scoped>
.oms-dashboard {
  padding: 2rem;
  background: #0f172a;
  min-height: 100vh;
  color: #f8fafc;
  font-family: 'Inter', sans-serif;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
  border-bottom: 1px solid #1e293b;
  padding-bottom: 1rem;
}

.brand h1 {
  font-size: 1.8rem;
  font-weight: 800;
  letter-spacing: 2px;
  color: #38bdf8;
  margin: 0;
}

.brand p {
  color: #94a3b8;
  margin: 0;
  font-size: 0.9rem;
}

.status-badge {
  background: #1e293b;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #ef4444;
}

.status-badge.connected {
  color: #10b981;
}

.status-badge .dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
  box-shadow: 0 0 8px currentColor;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
}

.station-card {
  background: #1e293b;
  border-radius: 16px;
  padding: 1.5rem;
  border: 1px solid #334155;
  transition: transform 0.3s ease;
}

.station-card:hover {
  transform: translateY(-5px);
  border-color: #38bdf8;
}

.station-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}

.station-info h3 {
  margin: 0;
  font-size: 1.2rem;
  color: #fff;
}

.station-type {
  font-size: 0.75rem;
  color: #38bdf8;
  text-transform: uppercase;
  font-weight: 700;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.manage-btn {
  font-size: 0.65rem;
  font-weight: 800;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  background: #38bdf8;
  color: #0f172a;
  text-decoration: none;
  transition: all 0.2s;
}

.manage-btn:hover {
  background: #fff;
}

.count-badge {
  background: rgba(56, 189, 248, 0.1);
  color: #38bdf8;
  padding: 0.2rem 0.6rem;
  border-radius: 8px;
  font-weight: 700;
}

.now-serving {
  background: #0f172a;
  border-radius: 12px;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-left: 4px solid #10b981;
}

.now-serving label, .up-next label {
  font-size: 0.7rem;
  font-weight: 800;
  color: #64748b;
  display: block;
  margin-bottom: 0.5rem;
}

.record-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.cancel-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  opacity: 0.5;
  transition: opacity 0.2s;
  padding: 4px;
}

.cancel-btn:hover {
  opacity: 1;
}

.queue-no {
  font-size: 2rem;
  font-weight: 900;
  color: #10b981;
}

.full-name {
  font-size: 1.1rem;
  font-weight: 600;
}

.empty {
  border-left-color: #475569;
  text-align: center;
  color: #475569;
  font-weight: 700;
}

.up-next ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.up-next li {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  padding: 0.6rem 0;
  border-bottom: 1px solid #334155;
}

.up-next li:last-child {
  border: none;
}

.q-num {
  font-weight: 800;
  color: #38bdf8;
}

.q-name {
  font-size: 0.9rem;
}

.empty-msg {
  color: #475569;
  font-style: italic;
  font-size: 0.9rem;
}
</style>
