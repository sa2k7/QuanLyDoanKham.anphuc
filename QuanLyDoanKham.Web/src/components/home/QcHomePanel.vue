<template>
  <div class="space-y-10 animate-fade-in">

    <!-- ══ HERO GREETING ══════════════════════════════════════════ -->
    <div class="relative overflow-hidden bg-gradient-to-br from-emerald-600 to-teal-500 rounded-[3rem] p-10 md:p-14 text-white shadow-2xl shadow-emerald-200">
      <div class="absolute -right-10 -top-10 w-60 h-60 bg-white/5 rounded-full blur-2xl"></div>
      <div class="absolute right-10 bottom-0 opacity-10">
        <ShieldCheck class="w-48 h-48" />
      </div>
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-[0.25em] mb-6">
          <span class="w-2 h-2 rounded-full bg-amber-400 animate-pulse"></span>
          Giám sát vận hành
        </div>
        <h1 class="text-4xl md:text-5xl font-black leading-tight mb-3">
          Trung tâm Giám sát, <span class="text-emerald-200">{{ authStore.fullName || authStore.username }}!</span>
        </h1>
        <p class="text-white/70 font-bold">{{ todayDateStr }}</p>
      </div>
    </div>

    <!-- ══ QUICK ACTIONS ══════════════════════════════════════════ -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <button @click="$emit('navigate', 'patients')"
              class="group bg-white border-2 border-slate-100 hover:border-sky-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-sky-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-sky-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <UserRound class="w-7 h-7 text-sky-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Duyệt kết quả (QC)</h3>
        <p class="text-xs text-slate-400 font-bold">Xem xét và phê duyệt hồ sơ bệnh nhân</p>
        <div class="mt-4 flex items-center gap-2 text-sky-500 font-black text-xs uppercase tracking-widest">
          QC Ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'groups')"
              class="group bg-white border-2 border-slate-100 hover:border-emerald-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-emerald-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-emerald-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Stethoscope class="w-7 h-7 text-emerald-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Quản lý đoàn khám</h3>
        <p class="text-xs text-slate-400 font-bold">Quản lý tiến độ và thông tin các đoàn khám</p>
        <div class="mt-4 flex items-center gap-2 text-emerald-500 font-black text-xs uppercase tracking-widest">
          Vào ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'analytics')"
              class="group bg-white border-2 border-slate-100 hover:border-violet-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-violet-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-violet-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <BarChart3 class="w-7 h-7 text-violet-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Báo cáo hiệu suất</h3>
        <p class="text-xs text-slate-400 font-bold">Thống kê theo đoàn và tổng quát</p>
        <div class="mt-4 flex items-center gap-2 text-violet-500 font-black text-xs uppercase tracking-widest">
          Phân tích <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { UserRound, ArrowRight, BarChart3, ShieldCheck, Stethoscope } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

defineEmits(['navigate'])

const authStore = useAuthStore()

const todayDateStr = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)
</script>
