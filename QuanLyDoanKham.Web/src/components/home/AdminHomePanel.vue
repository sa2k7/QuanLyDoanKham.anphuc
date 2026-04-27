<template>
  <div class="space-y-10 animate-fade-in">

    <!-- ══ HERO GREETING ══════════════════════════════════════════ -->
    <div class="relative overflow-hidden bg-gradient-to-br from-slate-800 to-slate-700 rounded-[3rem] p-10 md:p-14 text-white shadow-2xl shadow-slate-300">
      <div class="absolute -right-10 -top-10 w-64 h-64 bg-white/5 rounded-full blur-2xl"></div>
      <div class="absolute right-10 bottom-0 opacity-5">
        <LayoutDashboard class="w-64 h-64" />
      </div>
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/10 rounded-full text-[10px] font-black uppercase tracking-[0.25em] mb-6">
          <ShieldCheck class="w-3 h-3 text-emerald-400" />
          Quản trị hệ thống
        </div>
        <h1 class="text-4xl md:text-5xl font-black leading-tight mb-3">
          Chào, <span class="text-slate-300">{{ authStore.fullName || authStore.username }}!</span>
        </h1>
        <p class="text-white/50 font-bold">{{ todayDateStr }}</p>
      </div>
    </div>

    <!-- ══ SYSTEM STATS ═══════════════════════════════════════════ -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <div v-for="stat in systemStats" :key="stat.label"
           :class="['rounded-[2rem] p-7 border-2 cursor-pointer hover:-translate-y-1 transition-all', stat.borderClass, stat.bgClass]"
           @click="$emit('navigate', stat.target)">
        <div :class="['w-12 h-12 rounded-2xl flex items-center justify-center mb-4', stat.iconBg]">
          <component :is="stat.icon" :class="['w-6 h-6', stat.iconColor]" />
        </div>
        <div :class="['text-2xl font-black', stat.valueColor]">{{ stat.value }}</div>
        <div class="text-[10px] font-black uppercase tracking-widest text-slate-400 mt-1">{{ stat.label }}</div>
      </div>
    </div>


    <!-- ══ QUICK NAVIGATION GRID ══════════════════════════════════ -->
    <div>
      <h2 class="text-base font-black text-slate-800 uppercase tracking-widest mb-6">Truy cập nhanh</h2>
      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        <button v-for="item in quickNavItems" :key="item.id"
                @click="$emit('navigate', item.id)"
                :class="['group rounded-[1.5rem] p-5 text-left border-2 transition-all hover:-translate-y-1', item.borderClass, item.bgClass, 'hover:shadow-lg']">
          <div :class="['w-10 h-10 rounded-xl flex items-center justify-center mb-3 group-hover:scale-110 transition-transform', item.iconBg]">
            <component :is="item.icon" :class="['w-5 h-5', item.iconColor]" />
          </div>
          <p class="font-black text-slate-800 text-sm">{{ item.name }}</p>
          <p class="text-[10px] font-black text-slate-400 mt-1 uppercase tracking-widest">{{ item.desc }}</p>
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import {
  LayoutDashboard, ShieldCheck, Building2, FileText, Stethoscope,
  UserRound, Activity, Calculator, Users as UsersIcon, Wallet,
  BarChart3, User, Package, Server
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

const emit = defineEmits(['navigate'])

const authStore = useAuthStore()

const todayDateStr = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const systemStats = [
  {
    label: 'Đoàn khám', value: '—', target: 'groups',
    icon: Stethoscope, bgClass: 'bg-primary/5', borderClass: 'border-primary/10',
    iconBg: 'bg-primary/10', iconColor: 'text-primary', valueColor: 'text-primary'
  },
  {
    label: 'Hợp đồng HĐ', value: '—', target: 'contracts',
    icon: FileText, bgClass: 'bg-teal-50', borderClass: 'border-teal-100',
    iconBg: 'bg-teal-100', iconColor: 'text-teal-600', valueColor: 'text-teal-700'
  },
  {
    label: 'Nhân sự', value: '—', target: 'staff',
    icon: UsersIcon, bgClass: 'bg-rose-50', borderClass: 'border-rose-100',
    iconBg: 'bg-rose-100', iconColor: 'text-rose-600', valueColor: 'text-rose-700'
  },
  {
    label: 'Người dùng HT', value: '—', target: 'users',
    icon: User, bgClass: 'bg-slate-50', borderClass: 'border-slate-200',
    iconBg: 'bg-slate-200', iconColor: 'text-slate-600', valueColor: 'text-slate-800'
  },
]


const quickNavItems = [
  { id: 'companies',         name: 'Công ty',      desc: 'Đối tác',      icon: Building2,   bgClass: 'bg-sky-50',     borderClass: 'border-sky-100',     iconBg: 'bg-sky-100',     iconColor: 'text-sky-600' },
  { id: 'contracts',         name: 'Hợp đồng',     desc: 'Pháp lý',      icon: FileText,    bgClass: 'bg-teal-50',    borderClass: 'border-teal-100',    iconBg: 'bg-teal-100',    iconColor: 'text-teal-600' },
  { id: 'groups',            name: 'Đoàn khám',    desc: 'Vận hành',     icon: Stethoscope, bgClass: 'bg-primary/5',  borderClass: 'border-primary/10',  iconBg: 'bg-primary/10',  iconColor: 'text-primary' },
  { id: 'patients',          name: 'Bệnh nhân',    desc: 'Hồ sơ',        icon: UserRound,   bgClass: 'bg-blue-50',    borderClass: 'border-blue-100',    iconBg: 'bg-blue-100',    iconColor: 'text-blue-600' },
  { id: 'settlement-report', name: 'Quyết toán',   desc: 'Tài chính',    icon: Calculator,  bgClass: 'bg-emerald-50', borderClass: 'border-emerald-100', iconBg: 'bg-emerald-100', iconColor: 'text-emerald-600' },
  { id: 'analytics',         name: 'Thống kê',     desc: 'Báo cáo',      icon: BarChart3,   bgClass: 'bg-indigo-50',  borderClass: 'border-indigo-100',  iconBg: 'bg-indigo-100',  iconColor: 'text-indigo-600' },
  { id: 'users',             name: 'Tài khoản',    desc: 'Quản trị',     icon: User,        bgClass: 'bg-slate-50',   borderClass: 'border-slate-200',   iconBg: 'bg-slate-200',   iconColor: 'text-slate-600' },
]
</script>
