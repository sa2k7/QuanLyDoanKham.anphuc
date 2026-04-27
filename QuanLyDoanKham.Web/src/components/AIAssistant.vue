<template>
  <div class="ai-assistant-container">
    <button 
      @click="toggleAssistant"
      class="flex items-center space-x-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl transition-all shadow-lg"
      :disabled="loading"
    >
      <Sparkles v-if="!loading" class="w-5 h-5" />
      <Loader2 v-else class="w-5 h-5 animate-spin" />
      <span>{{ loading ? 'AI đang phân tích...' : 'AI Phân tích hồ sơ' }}</span>
    </button>

    <!-- AI Result Modal/Panel -->
    <div v-if="showPanel" class="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div class="bg-white w-full max-w-2xl rounded-3xl shadow-2xl overflow-hidden animate-in fade-in zoom-in duration-300">
        <div class="p-6 border-b border-slate-100 flex items-center justify-between bg-indigo-50/50">
          <div class="flex items-center space-x-3">
            <div class="p-2 bg-indigo-600 rounded-xl text-white">
              <BrainCircuit class="w-6 h-6" />
            </div>
            <div>
              <h3 class="font-black text-indigo-900">Trợ lý AI An Phúc</h3>
              <p class="text-xs text-indigo-600 font-medium">Phân tích dữ liệu y tế thời gian thực</p>
            </div>
          </div>
          <button @click="showPanel = false" class="p-2 hover:bg-white rounded-xl transition-all">
            <X class="w-6 h-6 text-slate-400" />
          </button>
        </div>

        <div class="p-8 max-h-[70vh] overflow-y-auto">
          <div v-if="analysis" class="space-y-6">
            <!-- Summary -->
            <section>
              <h4 class="flex items-center space-x-2 text-slate-900 font-bold mb-3">
                <FileText class="w-5 h-5 text-indigo-500" />
                <span>Tóm tắt bệnh lý</span>
              </h4>
              <div class="p-4 bg-slate-50 rounded-2xl text-slate-700 leading-relaxed">
                {{ analysis.summary }}
              </div>
            </section>

            <!-- Advice -->
            <section>
              <h4 class="flex items-center space-x-2 text-slate-900 font-bold mb-3">
                <HeartPulse class="w-5 h-5 text-rose-500" />
                <span>Lời khuyên y tế</span>
              </h4>
              <div class="p-4 bg-rose-50/50 rounded-2xl text-slate-700 leading-relaxed border border-rose-100">
                {{ analysis.healthAdvice }}
              </div>
            </section>

            <!-- Risk Level -->
            <section class="flex items-center justify-between p-4 bg-slate-50 rounded-2xl">
              <div class="flex items-center space-x-2">
                <ShieldAlert class="w-5 h-5 text-amber-500" />
                <span class="font-bold text-slate-900">Mức độ rủi ro:</span>
              </div>
              <span :class="['px-4 py-1 rounded-full font-black text-sm uppercase tracking-wider', riskClass]">
                {{ analysis.riskLevel }}
              </span>
            </section>
          </div>

          <div v-else-if="error" class="text-center py-10">
            <div class="inline-flex p-4 bg-rose-100 text-rose-600 rounded-full mb-4">
              <AlertTriangle class="w-8 h-8" />
            </div>
            <p class="text-rose-600 font-bold">{{ error }}</p>
            <button @click="summarize" class="mt-4 text-indigo-600 font-bold hover:underline">Thử lại</button>
          </div>
        </div>

        <div class="p-6 bg-slate-50 border-t border-slate-100 text-center">
          <p class="text-[10px] text-slate-400 italic">
            * Lưu ý: Kết quả do AI tạo ra chỉ mang tính chất tham khảo, không thay thế chẩn đoán của bác sĩ chuyên khoa.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { 
  Sparkles, Loader2, BrainCircuit, X, 
  FileText, HeartPulse, ShieldAlert, AlertTriangle 
} from 'lucide-vue-next'
import apiClient from '@/services/apiClient'

const props = defineProps({
  recordId: {
    type: [Number, String],
    required: true
  }
})

const loading = ref(false)
const showPanel = ref(false)
const analysis = ref(null)
const error = ref(null)

const riskClass = computed(() => {
  if (!analysis.value) return ''
  const risk = analysis.value.riskLevel?.toLowerCase() || ''
  if (risk.includes('cao')) return 'bg-rose-500 text-white'
  if (risk.includes('trung bình')) return 'bg-amber-500 text-white'
  return 'bg-teal-500 text-white'
})

const summarize = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await apiClient.post(`/api/AI/summarize-record/${props.recordId}`)
    // AI response might be a JSON string inside a field
    let data = response.data.aiAnalysis
    if (typeof data === 'string') {
      // Remove markdown code blocks if present
      const cleanJson = data.replace(/```json|```/g, '').trim()
      analysis.value = JSON.parse(cleanJson)
    } else {
      analysis.value = data
    }
    showPanel.value = true
  } catch (err) {
    console.error('AI Error:', err)
    error.value = err.response?.data || 'Không thể kết nối với trí tuệ nhân tạo.'
    showPanel.value = true
  } finally {
    loading.value = false
  }
}

const toggleAssistant = () => {
  if (analysis.value) {
    showPanel.value = true
  } else {
    summarize()
  }
}
</script>
