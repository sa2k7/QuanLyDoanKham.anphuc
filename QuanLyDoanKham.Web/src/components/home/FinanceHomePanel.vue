<template>
  <div class="space-y-10 animate-fade-in">

    <!-- ══ HERO ══════════════════════════════════════════════════ -->
    <div class="relative overflow-hidden bg-gradient-to-br from-emerald-500 to-teal-400 rounded-[3rem] p-10 md:p-14 text-white shadow-2xl shadow-emerald-200">
      <div class="absolute -right-10 -top-10 w-60 h-60 bg-white/5 rounded-full blur-2xl"></div>
      <div class="absolute right-10 bottom-0 opacity-10">
        <Calculator class="w-48 h-48" />
      </div>
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-[0.25em] mb-6">
          <span class="w-2 h-2 rounded-full bg-yellow-300 animate-pulse"></span>
          Tài chính & Quyết toán
        </div>
        <h1 class="text-4xl md:text-5xl font-black leading-tight mb-3">
          Bảng tài chính, <span class="text-emerald-200">{{ authStore.fullName || authStore.username }}!</span>
        </h1>
        <p class="text-white/70 font-bold">{{ todayDateStr }}</p>
      </div>
    </div>

    <!-- ══ FINANCIAL SUMMARY CARDS ════════════════════════════════ -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <div v-for="card in financeCards" :key="card.label"
           :class="['rounded-[2rem] p-7 border-2', card.borderClass, card.bgClass]">
        <div :class="['w-12 h-12 rounded-2xl flex items-center justify-center mb-4', card.iconBg]">
          <component :is="card.icon" :class="['w-6 h-6', card.iconColor]" />
        </div>
        <div :class="['text-2xl font-black', card.valueColor]">{{ card.value }}</div>
        <div class="text-[10px] font-black uppercase tracking-widest text-slate-400 mt-1">{{ card.label }}</div>
      </div>
    </div>

    <!-- ══ QUICK NAVIGATION ══════════════════════════════════════ -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <button @click="$emit('navigate', 'settlement-report')"
              class="group bg-white border-2 border-slate-100 hover:border-emerald-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-emerald-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-emerald-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Calculator class="w-7 h-7 text-emerald-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Quyết toán đoàn</h3>
        <p class="text-xs text-slate-400 font-bold">Đối soát doanh thu theo hợp đồng</p>
        <div class="mt-4 flex items-center gap-2 text-emerald-500 font-black text-xs uppercase tracking-widest">
          Vào ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'analytics')"
              class="group bg-white border-2 border-slate-100 hover:border-indigo-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-indigo-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-indigo-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <BarChart3 class="w-7 h-7 text-indigo-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Thống kê doanh thu</h3>
        <p class="text-xs text-slate-400 font-bold">Biểu đồ phân tích theo thời gian</p>
        <div class="mt-4 flex items-center gap-2 text-indigo-500 font-black text-xs uppercase tracking-widest">
          Xem ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'payroll')"
              class="group bg-white border-2 border-slate-100 hover:border-amber-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-amber-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-amber-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Wallet class="w-7 h-7 text-amber-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Bảng lương nhân sự</h3>
        <p class="text-xs text-slate-400 font-bold">Tính và xuất bảng thanh toán thu nhập</p>
        <div class="mt-4 flex items-center gap-2 text-amber-500 font-black text-xs uppercase tracking-widest">
          Xem ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Calculator, BarChart3, Wallet, ArrowRight, TrendingUp, CreditCard, FileText, AlertTriangle } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

defineEmits(['navigate'])

const authStore = useAuthStore()

const todayDateStr = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const financeCards = [
  {
    label: 'Doanh thu tháng', value: '—',
    icon: TrendingUp, bgClass: 'bg-emerald-50', borderClass: 'border-emerald-100',
    iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', valueColor: 'text-emerald-700'
  },
  {
    label: 'Chưa quyết toán', value: '—',
    icon: AlertTriangle, bgClass: 'bg-amber-50', borderClass: 'border-amber-100',
    iconBg: 'bg-amber-100', iconColor: 'text-amber-600', valueColor: 'text-amber-700'
  },
  {
    label: 'Hợp đồng hiệu lực', value: '—',
    icon: FileText, bgClass: 'bg-sky-50', borderClass: 'border-sky-100',
    iconBg: 'bg-sky-100', iconColor: 'text-sky-600', valueColor: 'text-sky-700'
  },
  {
    label: 'Chi phí phát sinh', value: '—',
    icon: CreditCard, bgClass: 'bg-rose-50', borderClass: 'border-rose-100',
    iconBg: 'bg-rose-100', iconColor: 'text-rose-600', valueColor: 'text-rose-700'
  },
]
</script>
