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

      <!-- Dynamic Exam Component -->
      <div class="dynamic-form-area bg-slate-50/50 p-4 rounded-2xl border border-slate-100">
        <component 
          :is="currentFormComponent" 
          v-model="formData.resultData"
        />
      </div>

      <!-- Attachments / Clinical Images -->
      <div class="form-group border-t border-slate-100 pt-4">
        <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2 flex justify-between">
          <span>Hình ảnh Cận Lâm Sàng</span>
          <span class="text-indigo-400 cursor-pointer hover:underline relative overflow-hidden">
            + Tải ảnh lên
            <input type="file" multiple accept="image/*" @change="uploadImages" class="absolute inset-0 opacity-0 cursor-pointer" />
          </span>
        </label>
        
        <div v-if="uploadError" class="text-red-500 text-xs mb-2 italic">⚠️ {{ uploadError }}</div>
        <div v-if="uploading" class="text-indigo-500 text-xs flex items-center gap-2 mb-2"><Loader2 class="animate-spin" :size="12" /> Đang tải ảnh lên...</div>
        
        <div class="grid grid-cols-4 gap-3 mt-2" v-if="formData.attachments.length > 0">
          <div v-for="(url, index) in formData.attachments" :key="index" class="relative group aspect-square rounded-xl overflow-hidden border-2 border-slate-100">
            <img :src="api.defaults.baseURL + url" class="w-full h-full object-cover" />
            <button @click="removeImage(index)" class="absolute top-1 right-1 bg-red-500 text-white p-1 rounded-full opacity-0 group-hover:opacity-100 transition-opacity transform scale-75 hover:scale-100 shadow-md">
              <X :size="16" />
            </button>
          </div>
        </div>
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
import { Stethoscope, Activity, ClipboardCheck, Save, Loader2, X } from 'lucide-vue-next'
import { api } from '@/services/apiClient'
import VitalsForm from './forms/VitalsForm.vue'
import EyeExamForm from './forms/EyeExamForm.vue'
import GeneralExamForm from './forms/GeneralExamForm.vue'

const props = defineProps({
  recordId: { type: Number, required: true },
  stationCode: { type: String, required: true },
  stationName: { type: String, default: '' },
  defaultExamType: { type: String, default: '' }
})

const emit = defineEmits(['success'])

const loading = ref(false)
const error = ref('')
const uploading = ref(false)
const uploadError = ref('')

const formData = reactive({
  examType: props.defaultExamType || '',
  resultData: {},
  diagnosis: '',
  attachments: []
})

const currentFormComponent = computed(() => {
  switch (props.stationCode) {
    case 'SINH_HIEU':
      return VitalsForm
    case 'MAT_TAI_MUI_HONG':
      return EyeExamForm
    default:
      return GeneralExamForm
  }
})

const canSubmit = computed(() => {
  // Validate that diagnosis and exam type are not empty
  if (formData.examType.trim() === '' || formData.diagnosis.trim() === '') return false
  
  // Basic validation that SOME data was entered in the dynamic form
  return Object.keys(formData.resultData).length > 0 && Object.values(formData.resultData).some(v => v !== '')
})

const uploadImages = async (event) => {
  const files = event.target.files
  if (!files || files.length === 0) return
  
  uploading.value = true
  uploadError.value = ''
  
  for (let i = 0; i < files.length; i++) {
    const file = files[i]
    const fd = new FormData()
    fd.append('file', file)
    
    try {
      const res = await api.post('/api/UploadStore/clinical', fd, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      formData.attachments.push(res.data.url)
    } catch (err) {
      uploadError.value = err.response?.data?.message || 'Có lỗi khi tải ảnh lên.'
      console.error(err)
    }
  }
  
  uploading.value = false
  event.target.value = '' // Reset input
}

const removeImage = (index) => {
  formData.attachments.splice(index, 1)
}

const submitResult = async () => {
  if (!canSubmit.value) return
  
  loading.value = true
  error.value = ''
  
  try {
    const finalResultData = { ...formData.resultData }
    if (formData.attachments.length > 0) {
      finalResultData.Attachments = formData.attachments // Will be serialized into JSON
    }

    const payload = {
      medicalRecordId: props.recordId,
      stationCode: props.stationCode,
      examType: formData.examType,
      resultData: finalResultData,
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
