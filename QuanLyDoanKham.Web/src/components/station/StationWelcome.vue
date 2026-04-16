<template>
  <div class="welcome-screen flex flex-col items-center justify-center h-full text-center">
    <div class="icon-glow mb-8">
        <div class="pulse-ring"></div>
        <Hospital :size="64" class="text-indigo-500 relative z-10" />
    </div>
    <h2 class="text-2xl font-black text-slate-800 mb-2">CHÀO MỪNG ĐẾN TRẠM {{ stationName.toUpperCase() }}</h2>
    <p class="text-slate-500 max-w-sm font-medium">Vui lòng chọn một bệnh nhân từ danh sách hàng chờ bên trái để bắt đầu quá trình thăm khám.</p>
  </div>
</template>

<script setup>
import { Hospital } from 'lucide-vue-next'

defineProps({
  stationName: {
    type: String,
    required: true
  }
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

.welcome-screen {
    animation: fadeIn 0.5s ease-out;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
