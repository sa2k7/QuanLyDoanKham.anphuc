<template>
  <div class="exam-history-container h-full flex flex-direction-column">
    <header class="flex items-center justify-between mb-6">
      <div class="flex items-center gap-3">
        <div class="icon-box bg-emerald-500/20 text-emerald-500 p-2 rounded-xl">
          <History :size="20" />
        </div>
        <h3 class="text-sm font-black text-slate-700 uppercase tracking-wider">LỊCH SỬ THĂM KHÁM</h3>
      </div>
      <button @click="fetchHistory" class="text-slate-400 hover:text-indigo-500 transition-colors">
        <RotateCcw :size="16" :class="{ 'animate-spin': loading }" />
      </button>
    </header>

    <div class="history-content flex-1 overflow-y-auto pr-2 custom-scrollbar">
      <div v-if="loading" class="flex flex-col items-center justify-center py-12 text-slate-400">
        <Loader2 class="animate-spin mb-2" :size="32" />
        <p class="text-xs font-bold">ĐANG TẢI DỮ LIỆU...</p>
      </div>

      <div v-else-if="history.length === 0" class="empty-state text-center py-12 glass-panel rounded-2xl border-2 border-dashed border-slate-100">
        <div class="mx-auto w-12 h-12 bg-slate-50 rounded-full flex items-center justify-center text-slate-300 mb-3">
          <ClipboardList :size="24" />
        </div>
        <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Chưa có kết quả khám nào</p>
      </div>

      <div v-else class="timeline space-y-4 relative pl-8 before:content-[''] before:absolute before:left-3 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-100">
        <div v-for="item in history" :key="item.examResultId" class="timeline-item relative animate-fade-in">
          <!-- Timeline point -->
          <div class="absolute -left-[25px] top-1 w-4 h-4 rounded-full bg-white border-4 border-emerald-500 shadow-sm z-10"></div>
          
          <div class="result-card bg-slate-50 hover:bg-white border border-slate-100 hover:border-emerald-200 p-4 rounded-2xl transition-all group">
            <header class="flex justify-between items-start mb-2">
              <span class="text-indigo-600 font-black text-xs uppercase">{{ item.examType }}</span>
              <span class="text-[10px] font-bold text-slate-400 italic">{{ formatDate(item.examDate) }}</span>
            </header>
            
            <div class="result-body mb-3">
              <p class="text-slate-700 text-sm font-medium line-clamp-3 group-hover:line-clamp-none transition-all">{{ item.result }}</p>
            </div>
            
            <footer class="pt-3 border-t border-slate-100 flex items-center justify-between">
              <div class="diagnosis flex items-start gap-2 max-w-[70%]">
                <div class="text-orange-500 mt-0.5"><AlertCircle :size="14" /></div>
                <p class="text-[11px] font-black text-slate-800 uppercase leading-tight">{{ item.diagnosis }}</p>
              </div>
              <div class="doctor flex items-center gap-1.5 opacity-60">
                <div class="w-5 h-5 rounded-full bg-indigo-100 flex items-center justify-center text-[10px] font-bold text-indigo-600">
                  {{ item.doctorName?.charAt(0) || 'D' }}
                </div>
                <span class="text-[10px] font-bold text-slate-500">{{ item.doctorName }}</span>
              </div>
            </footer>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { History, RotateCcw, Loader2, ClipboardList, AlertCircle } from 'lucide-vue-next'
import { api } from '@/services/apiClient'

const props = defineProps({
  recordId: { type: Number, required: true }
})

const history = ref([])
const loading = ref(false)

const fetchHistory = async () => {
  if (!props.recordId) return
  loading.value = true
  try {
    const res = await api.get(`/api/ExamResults/record/${props.recordId}`)
    history.value = res.data
  } catch (err) {
    console.error('History fetch error:', err)
  } finally {
    loading.value = false
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) + ' - ' + 
         date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
}

watch(() => props.recordId, () => {
  fetchHistory()
}, { immediate: true })

onMounted(() => {
  fetchHistory()
})
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

.timeline-item {
  animation: slideIn 0.3s ease-out forwards;
}

@keyframes slideIn {
  from { opacity: 0; transform: translateX(10px); }
  to { opacity: 1; transform: translateX(0); }
}

.glass-panel {
  background: rgba(255, 255, 255, 0.4);
  backdrop-filter: blur(4px);
}
</style>
