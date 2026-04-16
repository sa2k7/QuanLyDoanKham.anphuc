<template>
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
    <!-- Tổng đoàn -->
    <div class="premium-card p-6 flex items-center gap-4">
      <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center shadow-inner">
        <Stethoscope class="w-6 h-6" />
      </div>
      <div>
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng đoàn khám</p>
        <p class="text-2xl font-black text-slate-900 tabular-nums">{{ totalCount }}</p>
      </div>
    </div>

    <!-- Đang thực hiện -->
    <div class="premium-card p-6 flex items-center gap-4">
      <div class="w-12 h-12 bg-blue-100 text-blue-600 rounded-2xl flex items-center justify-center shadow-inner">
        <Zap class="w-6 h-6" />
      </div>
      <div>
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Đang thực hiện</p>
        <p class="text-2xl font-black text-slate-900 tabular-nums">{{ openCount }}</p>
      </div>
    </div>

    <!-- Hoàn tất -->
    <div class="premium-card p-6 flex items-center gap-4">
      <div class="w-12 h-12 bg-emerald-100 text-emerald-600 rounded-2xl flex items-center justify-center shadow-inner">
        <CheckCircle2 class="w-6 h-6" />
      </div>
      <div>
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hoàn tất</p>
        <p class="text-2xl font-black text-slate-900 tabular-nums">{{ finishedCount }}</p>
      </div>
    </div>

    <!-- Khóa sổ -->
    <div class="premium-card p-6 flex items-center gap-4">
      <div class="w-12 h-12 bg-purple-100 text-purple-600 rounded-2xl flex items-center justify-center shadow-inner">
        <Lock class="w-6 h-6" />
      </div>
      <div>
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Khóa sổ</p>
        <p class="text-2xl font-black text-slate-900 tabular-nums">{{ lockedCount }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Stethoscope, Zap, CheckCircle2, Lock } from 'lucide-vue-next'

defineProps({
  totalCount: Number,
  openCount: Number,
  finishedCount: Number,
  lockedCount: Number
})
</script>

<style scoped>
.premium-card {
  background: white;
  border-radius: 2rem;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  transition: all 0.3s;
}

.premium-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
</style>
