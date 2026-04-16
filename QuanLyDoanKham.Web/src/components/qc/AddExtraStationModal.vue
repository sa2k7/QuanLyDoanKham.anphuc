<template>
  <Teleport to="body">
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
      @click.self="$emit('close')"
    >
      <div class="bg-white rounded-3xl shadow-2xl w-full max-w-lg mx-4 overflow-hidden animate-grow-in">
        <!-- Header -->
        <header class="bg-gradient-to-r from-indigo-600 to-violet-600 p-6 text-white">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="bg-white/20 p-2.5 rounded-2xl">
                <PlusCircle :size="22" />
              </div>
              <div>
                <h2 class="text-lg font-black">CHỈ ĐỊNH THÊM TRẠM KHÁM</h2>
                <p class="text-indigo-200 text-xs font-medium mt-0.5">Bổ sung dịch vụ khám cho bệnh nhân</p>
              </div>
            </div>
            <button @click="$emit('close')" class="bg-white/20 hover:bg-white/30 rounded-xl p-2 transition-colors">
              <X :size="18" />
            </button>
          </div>
        </header>

        <!-- Body -->
        <div class="p-6 space-y-5">
          <!-- Station selector -->
          <div>
            <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">
              Chọn Trạm Khám
            </label>
            <div class="grid grid-cols-2 gap-2">
              <button
                v-for="station in availableStations"
                :key="station.code"
                @click="selectedStation = station.code"
                :class="[
                  'flex items-center gap-2 px-3 py-3 rounded-xl border-2 text-left transition-all font-bold text-sm',
                  selectedStation === station.code
                    ? 'border-indigo-500 bg-indigo-50 text-indigo-700'
                    : 'border-slate-100 bg-slate-50 text-slate-600 hover:border-indigo-200'
                ]"
              >
                <component :is="station.icon" :size="16" />
                <span>{{ station.name }}</span>
              </button>
            </div>
          </div>

          <!-- Notes -->
          <div>
            <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">
              Ghi chú / Chỉ định (tuỳ chọn)
            </label>
            <textarea
              v-model="notes"
              rows="3"
              placeholder="Ví dụ: Nghi ngờ bất thường siêu âm tim, cần khám lại..."
              class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl p-4 text-slate-800 font-medium text-sm focus:border-indigo-400 focus:bg-white outline-none transition-all resize-none"
            />
          </div>

          <p v-if="error" class="text-red-500 text-xs font-bold bg-red-50 rounded-xl px-3 py-2 border border-red-100">
            ⚠️ {{ error }}
          </p>
        </div>

        <!-- Footer -->
        <footer class="px-6 pb-6 flex gap-3">
          <button
            @click="$emit('close')"
            class="flex-1 border-2 border-slate-100 text-slate-600 font-black py-3.5 rounded-2xl hover:bg-slate-50 transition-all"
          >
            Huỷ bỏ
          </button>
          <button
            @click="confirm"
            :disabled="!selectedStation || loading"
            class="flex-1 bg-indigo-600 text-white font-black py-3.5 rounded-2xl hover:bg-indigo-700 disabled:opacity-40 disabled:cursor-not-allowed shadow-lg shadow-indigo-200 flex items-center justify-center gap-2 transition-all"
          >
            <Loader2 v-if="loading" class="animate-spin" :size="18" />
            <PlusCircle v-else :size="18" />
            {{ loading ? 'Đang gửi...' : 'Chỉ định' }}
          </button>
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { ref } from 'vue'
import { PlusCircle, X, Loader2, Activity, Eye, Scan, Zap, Heart, Stethoscope } from 'lucide-vue-next'
import { api } from '@/services/apiClient'
import { useToast } from '@/composables/useToast'

const props = defineProps({
  medicalRecordId: { type: Number, required: true }
})

const emit = defineEmits(['close', 'assigned'])
const toast = useToast()

const selectedStation = ref('')
const notes = ref('')
const loading = ref(false)
const error = ref('')

const availableStations = [
  { code: 'SINH_HIEU',         name: 'Đo sinh hiệu',    icon: Activity },
  { code: 'NOI_KHOA',          name: 'Nội khoa',         icon: Stethoscope },
  { code: 'MAT_TAI_MUI_HONG', name: 'Mắt / TMH',        icon: Eye },
  { code: 'XQUANG',            name: 'X-Quang',          icon: Scan },
  { code: 'SIEU_AM',           name: 'Siêu âm',          icon: Zap },
  { code: 'TIM_MACH',          name: 'Điện tim (ECG)',   icon: Heart },
]

const confirm = async () => {
  if (!selectedStation.value) return
  loading.value = true
  error.value = ''

  try {
    const notesParam = notes.value.trim() ? `&notes=${encodeURIComponent(notes.value.trim())}` : ''
    await api.post(`/api/Oms/station/${props.medicalRecordId}/assign-extra?stationCode=${selectedStation.value}${notesParam}`)

    const stationName = availableStations.find(s => s.code === selectedStation.value)?.name || selectedStation.value
    toast.success(`✅ Đã chỉ định thêm trạm: ${stationName}`)
    emit('assigned')
    emit('close')
  } catch (err) {
    error.value = err.response?.data?.message || 'Có lỗi xảy ra khi chỉ định trạm.'
  } finally {
    loading.value = false
  }
}
</script>
