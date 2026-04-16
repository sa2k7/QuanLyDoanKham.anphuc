<template>
  <div class="bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
    <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-5">KẾT QUẢ THĂM KHÁM TẠI CÁC TRẠM</h3>

    <div v-if="isLoading" class="text-center py-8 text-slate-400">
      <Loader2 class="animate-spin mx-auto mb-2" :size="24" />
    </div>

    <div v-else-if="history.length === 0" class="text-center py-8">
      <p class="text-slate-400 text-sm font-bold">Chưa có kết quả khám nào được ghi nhận.</p>
    </div>

    <div v-else class="space-y-4">
      <div
        v-for="result in history" :key="result.examResultId"
        class="result-item border border-slate-100 rounded-2xl p-4 hover:border-indigo-200 hover:bg-indigo-50/30 transition-all"
      >
        <div class="flex items-center justify-between mb-3">
          <span class="text-indigo-600 font-black text-sm uppercase tracking-wide">{{ result.examType }}</span>
          <span class="text-[10px] text-slate-400 font-bold italic">{{ formatDate(result.examDate) }}</span>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Kết quả</p>
            <div v-if="typeof result.resultData === 'object' && result.resultData !== null" class="space-y-1">
              <div v-for="(val, key) in result.resultData" :key="key">
                <div v-if="key !== 'Attachments'" class="flex gap-2 text-sm">
                  <span class="text-[10px] font-black text-slate-400 uppercase w-1/3">{{ key }}</span>
                  <span class="font-bold text-slate-800">{{ val }}</span>
                </div>
              </div>
            </div>
            <p v-else class="text-slate-700 font-medium text-sm">{{ result.resultData || result.result }}</p>
            
            <div v-if="result.resultData?.Attachments?.length > 0" class="mt-3">
              <p class="text-[10px] font-black text-indigo-400 uppercase mb-2">Đính kèm</p>
              <div class="flex flex-wrap gap-2">
                <a v-for="(img, idx) in result.resultData.Attachments" :key="idx" :href="api.defaults.baseURL + img" target="_blank" class="w-16 h-16 rounded-lg overflow-hidden border-2 border-indigo-100 hover:border-indigo-400 transition-all block relative group">
                  <img :src="api.defaults.baseURL + img" class="w-full h-full object-cover" />
                  <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-all">
                    <ExternalLink :size="12" class="text-white" />
                  </div>
                </a>
              </div>
            </div>
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
</template>

<script setup>
import { Loader2, ExternalLink } from 'lucide-vue-next'
import { api } from '@/services/apiClient'

defineProps({
  history: {
    type: Array,
    required: true
  },
  isLoading: {
    type: Boolean,
    default: false
  }
})

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getHours().toString().padStart(2,'0')}:${d.getMinutes().toString().padStart(2,'0')} — ${d.getDate()}/${d.getMonth() + 1}`
}
</script>
