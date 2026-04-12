<template>
  <div class="exam-form-container bg-white/10 backdrop-blur-xl border border-white/20 p-6 rounded-3xl shadow-2xl">
    <header class="flex items-center gap-3 mb-6">
      <div class="icon-box bg-indigo-500/20 text-indigo-400 p-3 rounded-2xl">
        <Stethoscope :size="24" />
      </div>
      <div>
        <h3 class="text-xl font-black text-slate-800">NHẬP KẾT QUẢ KHÁM</h3>
        <p class="text-slate-500 text-xs font-bold uppercase tracking-wider">Trạm: {{ stationName }}</p>
      </div>
    </header>

    <div class="space-y-6">
      <!-- Exam Type Input -->
      <div class="form-group">
        <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Loại khám / Chỉ định</label>
        <div class="relative">
          <Activity class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
          <input 
            v-model="formData.examType" 
            placeholder="Ví dụ: Khám nội tổng quát, Siêu âm..."
            class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 pl-12 pr-4 text-slate-800 font-bold focus:border-indigo-500 focus:bg-white outline-none transition-all"
          />
        </div>
      </div>

      <!-- Result Textarea -->
      <div class="form-group">
        <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Kết quả chi tiết</label>
        <textarea 
          v-model="formData.result" 
          placeholder="Nhập ghi chú kết quả tại đây..."
          class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-4 px-4 text-slate-800 font-medium focus:border-indigo-500 focus:bg-white outline-none transition-all min-h-[120px]"
        ></textarea>
      </div>

      <!-- Diagnosis Textarea -->
      <div class="form-group">
        <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Chẩn đoán / Kết luận</label>
        <div class="relative">
          <ClipboardCheck class="absolute left-4 top-5 text-slate-400" :size="18" />
          <textarea 
            v-model="formData.diagnosis" 
            placeholder="Kết luận của bác sĩ..."
            class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-4 pl-12 pr-4 text-slate-800 font-bold focus:border-emerald-500 focus:bg-white outline-none transition-all min-h-[100px]"
          ></textarea>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex gap-4 pt-4">
        <button 
          @click="submitResult" 
          :disabled="loading || !canSubmit"
          class="flex-1 bg-indigo-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed shadow-xl shadow-indigo-200 active:scale-95 transition-all"
        >
          <Save v-if="!loading" :size="20" />
          <Loader2 v-else class="animate-spin" :size="20" />
          {{ loading ? 'ĐANG LƯU...' : 'LƯU & HOÀN THÀNH' }}
        </button>
      </div>
      
      <p v-if="error" class="text-center text-red-500 text-xs font-bold bg-red-50 py-2 rounded-lg border border-red-100 italic">
          ⚠️ {{ error }}
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { Stethoscope, Activity, ClipboardCheck, Save, Loader2 } from 'lucide-vue-next'
import { api } from '@/services/apiClient'

const props = defineProps({
  recordId: { type: Number, required: true },
  stationCode: { type: String, required: true },
  stationName: { type: String, default: '' },
  defaultExamType: { type: String, default: '' }
})

const emit = defineEmits(['success'])

const loading = ref(false)
const error = ref('')

const formData = reactive({
  examType: props.defaultExamType || '',
  result: '',
  diagnosis: ''
})

const canSubmit = computed(() => {
  return formData.examType.trim() !== '' && 
         formData.result.trim() !== '' && 
         formData.diagnosis.trim() !== ''
})

const submitResult = async () => {
  if (!canSubmit.value) return
  
  loading.value = true
  error.value = ''
  
  try {
    const payload = {
      medicalRecordId: props.recordId,
      stationCode: props.stationCode,
      examType: formData.examType,
      result: formData.result,
      diagnosis: formData.diagnosis,
      doctorStaffId: null // Backend will take from JWT if null, or we can fetch current user ID
    }
    
    await api.post('/api/ExamResults/save', payload)
    
    // Reset form if needed, or just emit success
    emit('success')
  } catch (err) {
    error.value = err.response?.data?.message || 'Không thể lưu kết quả. Vui lòng thử lại.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.exam-form-container {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
textarea {
  resize: none;
}
</style>
