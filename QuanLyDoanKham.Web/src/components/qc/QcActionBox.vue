<template>
  <div class="bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
    <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-5">QUYẾT ĐỊNH QC</h3>

    <!-- Rework reason (shown if isRejecting) -->
    <div v-if="isRejecting" class="mb-4 animate-fade-in">
      <label class="block text-xs font-black text-slate-500 uppercase tracking-wider mb-2">Lý do trả lại</label>
      <textarea
        v-model="reworkReason"
        placeholder="Nhập lý do cụ thể để bác sĩ chỉnh sửa lại..."
        class="w-full bg-red-50 border-2 border-red-200 rounded-2xl p-4 text-slate-700 font-medium outline-none focus:border-red-400 min-h-[100px] resize-none"
      ></textarea>
    </div>

    <div class="flex gap-4">
      <button
        v-if="!isRejecting"
        @click="$emit('pass')"
        :disabled="actionLoading"
        class="flex-1 bg-emerald-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-emerald-700 active:scale-95 transition-all shadow-xl shadow-emerald-200 disabled:opacity-50"
      >
        <CheckCircle2 v-if="!actionLoading" :size="20" />
        <Loader2 v-else class="animate-spin" :size="20" />
        QC ĐẠT — DUYỆT HỒ SƠ
      </button>

      <button
        v-if="!isRejecting"
        @click="$emit('download-pdf')"
        :disabled="actionLoading"
        class="flex-1 bg-indigo-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-indigo-700 active:scale-95 transition-all shadow-xl shadow-indigo-200 disabled:opacity-50"
      >
        <Printer v-if="!actionLoading" :size="20" />
        <Loader2 v-else class="animate-spin" :size="20" />
        XUẤT BÁO CÁO (PDF)
      </button>

      <button
        v-if="!isRejecting"
        @click="$emit('assign-extra')"
        :disabled="actionLoading"
        class="px-6 py-4 rounded-2xl font-black border-2 border-violet-200 text-violet-600 hover:bg-violet-50 active:scale-95 transition-all flex items-center gap-2 disabled:opacity-50"
        title="Chỉ định thêm trạm khám"
      >
        <PlusCircle :size="20" />
        THÊM TRẠM
      </button>

      <button
        v-if="isRejecting"
        @click="$emit('rework', reworkReason)"
        :disabled="actionLoading || !reworkReason.trim()"
        class="flex-1 bg-red-600 text-white py-4 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-red-700 active:scale-95 transition-all shadow-xl shadow-red-200 disabled:opacity-50"
      >
        <Send v-if="!actionLoading" :size="20" />
        <Loader2 v-else class="animate-spin" :size="20" />
        GỬI TRẢ VỀ BÁC SĨ
      </button>

      <button
        @click="toggleReject"
        :disabled="actionLoading"
        class="px-6 py-4 rounded-2xl font-black border-2 transition-all active:scale-95"
        :class="isRejecting ? 'border-slate-200 text-slate-500 hover:bg-slate-50' : 'border-red-200 text-red-500 hover:bg-red-50'"
      >
        {{ isRejecting ? 'HỦY' : 'TRẢ LẠI' }}
      </button>
    </div>

    <p v-if="actionError" class="text-center text-red-500 text-xs font-bold bg-red-50 py-2 px-4 rounded-xl mt-4 border border-red-100">
      ⚠️ {{ actionError }}
    </p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { CheckCircle2, Loader2, Send, Printer, PlusCircle } from 'lucide-vue-next'

defineProps({
  actionLoading: {
    type: Boolean,
    default: false
  },
  actionError: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['pass', 'rework', 'download-pdf', 'assign-extra'])

const isRejecting = ref(false)
const reworkReason = ref('')

const toggleReject = () => {
  isRejecting.value = !isRejecting.value
  reworkReason.value = ''
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.35s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to   { opacity: 1; transform: translateY(0); }
}
</style>
