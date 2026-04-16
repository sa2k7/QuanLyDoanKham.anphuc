<template>
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
        :class="{ active: selectedId === patient.taskId }"
        @click="$emit('select', patient)"
      >
        <span class="q-no">{{ patient.queueNo }}</span>
        <div class="q-info">
          <span class="name">{{ patient.fullName }}</span>
          <span class="time">{{ formatWaiting(patient.waitingSince) }}</span>
        </div>
      </div>
    </nav>
  </aside>
</template>

<script setup>
defineProps({
  stationName: {
    type: String,
    required: true
  },
  queue: {
    type: Array,
    required: true
  },
  selectedId: {
    type: [Number, String],
    default: null
  }
})

defineEmits(['select'])

const formatWaiting = (date) => {
  if (!date) return 'Vừa xong'
  const diff = Math.floor((new Date() - new Date(date)) / 60000)
  return diff <= 0 ? 'Vừa xong' : `${diff} m`
}
</script>

<style scoped>
.sidebar {
  width: 320px;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(20px);
  border-right: 1px solid rgba(226, 232, 240, 0.8);
  display: flex;
  flex-direction: column;
}

.station-header {
  padding: 1.5rem;
  border-bottom: 1px solid rgba(226, 232, 240, 0.8);
}

.station-header h2 {
  font-size: 1.25rem;
  font-weight: 900;
  color: #1e293b;
  text-transform: uppercase;
}

.station-header p {
  font-size: 0.75rem;
  color: #64748b;
  font-weight: 600;
  margin-top: 0.25rem;
}

.queue-list {
  flex: 1;
  overflow-y: auto;
  padding: 1rem 0;
}

.queue-list header {
  padding: 0 1.5rem;
  font-size: 0.65rem;
  font-weight: 900;
  color: #94a3b8;
  letter-spacing: 0.1em;
  margin-bottom: 1rem;
}

.empty-queue {
  padding: 2rem 1.5rem;
  text-align: center;
  color: #94a3b8;
  font-size: 0.875rem;
  font-style: italic;
}

.queue-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.5rem;
  cursor: pointer;
  transition: all 0.2s;
  border-left: 4px solid transparent;
}

.queue-item:hover {
  background: rgba(79, 70, 229, 0.05);
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

.q-info {
  display: flex;
  flex-direction: column;
}

.q-info .name {
  font-weight: 700;
  color: #334155;
  font-size: 0.9375rem;
}

.q-info .time {
  font-size: 0.75rem;
  color: #94a3b8;
  font-weight: 500;
}
</style>
