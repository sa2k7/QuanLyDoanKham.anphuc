<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm animate-fade-in">
    <div class="bg-white rounded-3xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col max-h-[90vh]">
      
      <!-- Header -->
      <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between bg-gradient-to-r from-blue-50 to-indigo-50">
        <div class="flex items-center gap-3">
          <div class="p-2 bg-gradient-to-r from-blue-500 to-cyan-500 text-white rounded-xl">
            <Sparkles :size="24" />
          </div>
          <div>
            <h3 class="font-black text-slate-800 uppercase tracking-widest text-lg">AI Clinical Summary</h3>
            <p class="text-xs text-slate-500 font-medium">Tự động kết luận bệnh án bằng AI (#{{ medicalRecordId }})</p>
          </div>
        </div>
        <button @click="$emit('close')" class="p-2 text-slate-400 hover:text-red-500 hover:bg-slate-100 rounded-full transition-colors">
          <X :size="20" />
        </button>
      </div>

      <!-- Body -->
      <div class="p-6 flex-1 overflow-y-auto">
        <div v-if="isLoading" class="flex flex-col items-center justify-center py-12 text-slate-400">
          <Loader2 :size="48" class="animate-spin text-cyan-400 mb-4" />
          <p class="font-bold text-slate-600">Đang phân tích kết quả khám...</p>
          <p class="text-xs">AI đang tổng hợp và đưa ra kết luận. Vui lòng đợi trong giây lát.</p>
        </div>

        <div v-else-if="error" class="bg-red-50 text-red-600 p-4 rounded-2xl border border-red-100 flex gap-3 text-sm font-medium">
          <AlertCircle :size="20" class="shrink-0" />
          <p>{{ error }}</p>
        </div>

        <div v-else class="prose prose-sm md:prose-base prose-slate max-w-none">
          <!-- Render markdown-like text naturally -->
          <div class="whitespace-pre-wrap text-slate-700 font-medium leading-relaxed">{{ summaryText }}</div>
        </div>
      </div>

      <!-- Footer -->
      <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex justify-end gap-3">
        <button 
          @click="$emit('close')"
          class="px-5 py-2.5 rounded-xl text-sm font-bold text-slate-500 hover:bg-slate-200 transition-colors"
        >
          Đóng
        </button>
        <button 
          v-if="!isLoading && !error"
          @click="copyToClipboard"
          class="px-5 py-2.5 rounded-xl text-sm font-bold bg-slate-800 text-white hover:bg-slate-700 transition-colors flex items-center gap-2"
        >
          <Copy :size="16" />
          Sao chép
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Sparkles, X, Loader2, AlertCircle, Copy } from 'lucide-vue-next'
import { api } from '@/services/apiClient'

const props = defineProps({
  medicalRecordId: {
    type: [Number, String],
    required: true
  }
})

const emit = defineEmits(['close'])

const isLoading = ref(true)
const error = ref(null)
const summaryText = ref('')

const generateSummary = async () => {
  isLoading.value = true
  error.value = null
  try {
    const res = await api.post(`/api/MedicalRecords/${props.medicalRecordId}/ai-clinical-summary`)
    summaryText.value = res.data.summary
  } catch (err) {
    console.error('Lỗi khi tạo kết luận AI:', err)
    error.value = err.response?.data?.message || 'Có lỗi xảy ra khi tạo kết luận bằng AI.'
  } finally {
    isLoading.value = false
  }
}

const copyToClipboard = async () => {
  try {
    await navigator.clipboard.writeText(summaryText.value)
    alert('Đã sao chép vào bộ nhớ tạm!')
  } catch (err) {
    console.error('Không thể sao chép', err)
  }
}

onMounted(() => {
  generateSummary()
})
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; }
  to   { opacity: 1; }
}
</style>
