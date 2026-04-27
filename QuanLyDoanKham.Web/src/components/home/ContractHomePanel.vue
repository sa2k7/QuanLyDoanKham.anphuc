<template>
  <div class="space-y-10 animate-fade-in">

    <!-- ══ HERO ══════════════════════════════════════════════════ -->
    <div class="relative overflow-hidden bg-gradient-to-br from-teal-600 to-emerald-500 rounded-[3rem] p-10 md:p-14 text-white shadow-2xl shadow-teal-200">
      <div class="absolute -right-10 -top-10 w-60 h-60 bg-white/5 rounded-full blur-2xl"></div>
      <div class="absolute right-10 bottom-0 opacity-10">
        <FileText class="w-48 h-48" />
      </div>
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-[0.25em] mb-6">
          <span class="w-2 h-2 rounded-full bg-yellow-300 animate-pulse"></span>
          Quản lý hợp đồng
        </div>
        <h1 class="text-4xl md:text-5xl font-black leading-tight mb-3">
          Trung tâm hợp đồng, <span class="text-teal-200">{{ authStore.fullName || authStore.username }}!</span>
        </h1>
        <p class="text-white/70 font-bold">{{ todayDateStr }}</p>
      </div>
    </div>

    <!-- ══ CONTRACT SUMMARY ═══════════════════════════════════════ -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <div v-for="card in contractCards" :key="card.label"
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
      <button @click="$emit('navigate', 'companies')"
              class="group bg-white border-2 border-slate-100 hover:border-sky-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-sky-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-sky-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Building2 class="w-7 h-7 text-sky-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Danh sách công ty</h3>
        <p class="text-xs text-slate-400 font-bold">Quản lý thông tin đối tác khám sức khỏe</p>
        <div class="mt-4 flex items-center gap-2 text-sky-500 font-black text-xs uppercase tracking-widest">
          Xem ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'contracts')"
              class="group bg-white border-2 border-slate-100 hover:border-teal-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-teal-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-teal-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <FileText class="w-7 h-7 text-teal-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Hợp đồng dịch vụ</h3>
        <p class="text-xs text-slate-400 font-bold">Ký kết, theo dõi và phê duyệt hợp đồng</p>
        <div class="mt-4 flex items-center gap-2 text-teal-500 font-black text-xs uppercase tracking-widest">
          Quản lý <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <button @click="$emit('navigate', 'groups')"
              class="group bg-white border-2 border-slate-100 hover:border-emerald-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-emerald-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-emerald-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Stethoscope class="w-7 h-7 text-emerald-500" />
        </div>
        <h3 class="text-base font-black text-slate-800 mb-1">Đoàn khám</h3>
        <p class="text-xs text-slate-400 font-bold">Theo dõi triển khai đoàn theo hợp đồng</p>
        <div class="mt-4 flex items-center gap-2 text-emerald-500 font-black text-xs uppercase tracking-widest">
          Xem tiến độ <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import { FileText, Building2, Stethoscope, ArrowRight, CheckCircle2, Clock, TrendingUp, AlertCircle } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

defineEmits(['navigate'])

const authStore = useAuthStore()

const todayDateStr = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const contractCards = [
  {
    label: 'Hiệu lực', value: '—',
    icon: CheckCircle2, bgClass: 'bg-emerald-50', borderClass: 'border-emerald-100',
    iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600', valueColor: 'text-emerald-700'
  },
  {
    label: 'Chờ ký duyệt', value: '—',
    icon: Clock, bgClass: 'bg-amber-50', borderClass: 'border-amber-100',
    iconBg: 'bg-amber-100', iconColor: 'text-amber-600', valueColor: 'text-amber-700'
  },
  {
    label: 'Đoàn đang chạy', value: '—',
    icon: TrendingUp, bgClass: 'bg-sky-50', borderClass: 'border-sky-100',
    iconBg: 'bg-sky-100', iconColor: 'text-sky-600', valueColor: 'text-sky-700'
  },
  {
    label: 'Sắp hết hạn', value: '—',
    icon: AlertCircle, bgClass: 'bg-rose-50', borderClass: 'border-rose-100',
    iconBg: 'bg-rose-100', iconColor: 'text-rose-600', valueColor: 'text-rose-700'
  },
]
</script>
