<template>
  <div class="patient-checkin-page min-h-screen bg-slate-900 flex items-center justify-center p-4">
    <div class="glass-card w-full max-w-md p-8 rounded-3xl animate-fade-in">
      <header class="text-center mb-8">
        <div class="icon-box mx-auto mb-4 bg-primary/10 text-primary w-16 h-16 rounded-2xl flex items-center justify-center">
          <UserCheck :size="32" />
        </div>
        <h1 class="text-2xl font-black text-slate-800">TỰ BÁO DANH</h1>
        <p class="text-slate-500 text-sm mt-2">Vui lòng nhập thông tin để xác nhận bạn đã đến</p>
      </header>

      <div v-if="!success" class="checkin-form space-y-6">
        <div class="form-group">
          <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-2">Mã hồ sơ (Medical Record ID)</label>
          <div class="relative">
            <Hash class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
            <input 
              v-model="recordId" 
              type="text" 
              placeholder="Ví dụ: BN12345" 
              class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-4 pl-12 pr-4 text-slate-800 font-bold focus:border-primary focus:bg-white outline-none transition-all"
              @keyup.enter="handleCheckIn"
            />
          </div>
        </div>

        <button 
          @click="handleCheckIn" 
          :disabled="!recordId || loading"
          class="w-full bg-primary text-white py-4 rounded-2xl font-bold flex items-center justify-center gap-2 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg shadow-primary/30 active:scale-95 transition-all"
        >
          <span v-if="!loading">XÁC NHẬN ĐẾN KHÁM</span>
          <Loader2 v-else class="animate-spin" :size="20" />
        </button>

        <p v-if="error" class="text-center text-red-500 text-sm font-bold animate-bounce-in">
          {{ error }}
        </p>
      </div>

      <div v-else class="success-view text-center py-8 animate-zoom-in">
        <div class="success-icon mx-auto mb-6 bg-green-100 text-green-600 w-24 h-24 rounded-full flex items-center justify-center shadow-xl shadow-green-100">
          <Check :size="48" />
        </div>
        <h2 class="text-2xl font-black text-slate-800 mb-2">THÀNH CÔNG!</h2>
        <p class="text-slate-600 mb-6">Bạn đã báo danh thành công. Vui lòng theo dõi số thứ tự trên bảng điện tử.</p>
        
        <div class="patient-info bg-slate-50 rounded-2xl p-4 mb-8">
           <p class="text-xs text-slate-400 font-bold uppercase mb-1">Họ và tên</p>
           <p class="text-lg font-black text-slate-800 mb-4">{{ patientName }}</p>

           <div class="grid grid-cols-2 gap-4 border-t border-slate-200 pt-4">
             <div>
               <p class="text-xs text-slate-400 font-bold uppercase mb-1">Số thứ tự</p>
               <p class="text-3xl font-black text-primary">{{ queueNo }}</p>
             </div>
             <div>
               <p class="text-xs text-slate-400 font-bold uppercase mb-1">Trạm tiếp theo</p>
               <p class="text-sm font-bold text-slate-800">{{ nextStation }}</p>
             </div>
           </div>
        </div>

        <button @click="reset" class="text-primary font-bold hover:underline">Tiếp tục báo danh cho người khác</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { UserCheck, Hash, Loader2, Check } from 'lucide-vue-next'
import apiClient from '../services/apiClient'

const recordId = ref('')
const loading = ref(false)
const success = ref(false)
const error = ref('')
const patientName = ref('')
const queueNo = ref(null)
const nextStation = ref('')

const handleCheckIn = async () => {
    if (!recordId.value) return
    loading.value = true
    error.value = ''
    
    try {
        // Giả lập logic check-in bệnh nhân (Cần API CheckIn/Self)
        const res = await apiClient.post('/api/Patients/self-checkin', { 
            recordId: recordId.value.trim() 
        })
        
        patientName.value = res.data.fullName
        queueNo.value = res.data.queueNo
        nextStation.value = res.data.nextStation
        success.value = true
    } catch (err) {
        error.value = err.response?.data?.message || 'Không tìm thấy hồ sơ hoặc đã check-in. Vui lòng liên hệ quầy tiếp đón.'
    } finally {
        loading.value = false
    }
}

const reset = () => {
    recordId.value = ''
    success.value = false
    error.value = ''
    patientName.value = ''
    queueNo.value = null
    nextStation.value = ''
}
</script>

<style scoped>
.glass-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  box-shadow: 0 50px 100px -20px rgba(0,0,0,0.5);
}

.animate-fade-in { animation: fadeIn 0.5s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }

.animate-zoom-in { animation: zoomIn 0.3s ease-out; }
@keyframes zoomIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }

.animate-bounce-in { animation: bounceIn 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55); }
@keyframes bounceIn { from { transform: scale(0.8); } to { transform: scale(1); } }
</style>
