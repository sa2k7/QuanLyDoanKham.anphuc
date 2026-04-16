<template>
  <aside class="w-80 bg-white/80 border-r border-slate-100 flex flex-col overflow-hidden backdrop-blur-xl">
    <div class="p-4 border-b border-slate-50 bg-slate-50/80">
      <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ records.length }} HỒ SƠ CHỜ DUYỆT</p>
    </div>

    <!-- Loading state -->
    <div v-if="isLoading" class="flex items-center justify-center py-16 text-slate-400">
      <Loader2 class="animate-spin" :size="32" />
    </div>

    <!-- Empty state -->
    <div v-else-if="records.length === 0" class="flex flex-col items-center justify-center py-16 text-center px-6">
      <div class="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mb-4">
        <CheckCircle2 :size="32" class="text-emerald-500" />
      </div>
      <p class="font-black text-slate-400 text-sm uppercase">Tất cả đã được duyệt!</p>
      <p class="text-slate-400 text-xs mt-1">Không có hồ sơ nào đang chờ QC.</p>
    </div>

    <!-- List -->
    <ul v-else class="flex-1 overflow-y-auto divide-y divide-slate-50">
      <li
        v-for="record in records"
        :key="record.medicalRecordId"
        @click="$emit('select', record)"
        class="p-4 cursor-pointer hover:bg-indigo-50/50 transition-all group"
        :class="{ 'bg-indigo-50 border-l-4 border-indigo-500': selectedId === record.medicalRecordId }"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="font-black text-slate-800 text-sm">{{ record.fullName }}</p>
            <p class="text-[11px] text-slate-500 mt-0.5">
              STT: <span class="font-bold text-indigo-500">#{{ record.queueNo }}</span>
            </p>
          </div>
          <ChevronRight :size="16" class="text-slate-300 group-hover:text-indigo-400 transition-colors" />
        </div>
        <div class="mt-2 flex gap-1.5 flex-wrap">
          <span
            v-for="task in record.stationTasks" :key="task.stationCode"
            class="text-[10px] font-bold px-2 py-0.5 rounded-full"
            :class="task.status === 'STATION_DONE' ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-500'"
          >
            {{ task.stationCode }}
          </span>
        </div>
      </li>
    </ul>
  </aside>
</template>

<script setup>
import { Loader2, CheckCircle2, ChevronRight } from 'lucide-vue-next'

defineProps({
  records: {
    type: Array,
    required: true
  },
  selectedId: {
    type: [Number, String],
    default: null
  },
  isLoading: {
    type: Boolean,
    default: false
  }
})

defineEmits(['select'])
</script>
