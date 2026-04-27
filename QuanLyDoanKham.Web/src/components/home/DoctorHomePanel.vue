<template>
  <div class="space-y-10 animate-fade-in">

    <!-- ══ HERO GREETING ══════════════════════════════════════════ -->
    <div class="relative overflow-hidden bg-gradient-to-br from-sky-600 to-indigo-600 rounded-[3rem] p-10 md:p-14 text-white shadow-2xl shadow-sky-200">
      <div class="absolute -right-10 -top-10 w-60 h-60 bg-white/5 rounded-full blur-2xl"></div>
      <div class="absolute right-10 bottom-0 opacity-10">
        <Stethoscope class="w-48 h-48" />
      </div>
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-[0.25em] mb-6">
          <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
          Phiên đang hoạt động
        </div>
        <h1 class="text-4xl md:text-5xl font-black leading-tight mb-3">
          Chào bác sĩ, <span class="text-sky-200">{{ authStore.fullName || authStore.username }}!</span>
        </h1>
        <p class="text-white/70 font-bold">{{ todayDateStr }}</p>
      </div>
    </div>

    <!-- ══ TODAY'S ASSIGNMENT ══════════════════════════════════════ -->
    <div v-if="todayAssignment" class="bg-white rounded-[2.5rem] border-2 border-emerald-100 shadow-xl shadow-emerald-50 p-8 md:p-10 flex flex-col md:flex-row items-center gap-8">
      <div class="w-20 h-20 bg-emerald-50 rounded-[2rem] flex items-center justify-center flex-shrink-0">
        <CalendarCheck class="w-10 h-10 text-emerald-500" />
      </div>
      <div class="flex-1 text-center md:text-left">
        <div class="text-[10px] font-black uppercase tracking-widest text-emerald-500 mb-2">Nhiệm vụ hôm nay</div>
        <h2 class="text-2xl font-black text-slate-800">{{ todayAssignment.groupName }}</h2>
        <p class="text-slate-400 font-bold mt-1">{{ todayAssignment.companyName }} · {{ todayAssignment.workPosition }}</p>
      </div>
      <div class="flex-shrink-0">
        <template v-if="!todayAssignment.checkInTime">
          <button @click="$emit('check-in')"
                  :disabled="isCheckingIn"
                  class="px-8 py-4 bg-emerald-500 text-white rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-emerald-200 hover:scale-105 active:scale-95 transition-all flex items-center gap-3">
            <span class="w-2 h-2 rounded-full bg-white animate-pulse"></span>
            {{ isCheckingIn ? 'Đang xử lý...' : 'VÀO ĐOÀN NGAY' }}
          </button>
        </template>
        <template v-else-if="!todayAssignment.checkOutTime">
          <div class="text-center space-y-2">
            <div class="flex items-center gap-2 text-emerald-600 font-black text-sm">
              <CheckCircle2 class="w-5 h-5" />
              VÀO LÚC {{ formatTime(todayAssignment.checkInTime) }}
            </div>
            <button @click="$emit('check-in')"
                    :disabled="isCheckingIn"
                    class="px-6 py-3 bg-slate-100 text-slate-600 rounded-xl font-black text-xs uppercase tracking-widest hover:bg-slate-200 transition-all">
              Kết thúc & Rời đoàn
            </button>
          </div>
        </template>
        <template v-else>
          <div class="flex items-center gap-2 px-6 py-4 bg-emerald-50 text-emerald-600 rounded-2xl font-black text-sm">
            <ShieldCheck class="w-5 h-5" />
            HOÀN THÀNH HÔM NAY
          </div>
        </template>
      </div>
    </div>

    <!-- ══ QUICK ACCESS ══════════════════════════════ -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Go to Groups -->
      <button @click="$emit('navigate', 'groups')"
              class="group bg-white border-2 border-slate-100 hover:border-sky-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-sky-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-sky-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <Stethoscope class="w-7 h-7 text-sky-500" />
        </div>
        <h3 class="text-lg font-black text-slate-800 mb-1">Thông tin đoàn khám</h3>
        <p class="text-sm text-slate-400 font-bold">Xem danh mục khám và gói dịch vụ của đoàn</p>
        <div class="mt-4 flex items-center gap-2 text-sky-500 font-black text-xs uppercase tracking-widest">
          Vào ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>

      <!-- View Patients -->
      <button @click="$emit('navigate', 'patients')"
              class="group bg-white border-2 border-slate-100 hover:border-indigo-200 rounded-[2rem] p-8 text-left transition-all hover:shadow-xl hover:shadow-indigo-50 hover:-translate-y-1">
        <div class="w-14 h-14 bg-indigo-50 rounded-2xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
          <UserRound class="w-7 h-7 text-indigo-500" />
        </div>
        <h3 class="text-lg font-black text-slate-800 mb-1">Danh sách bệnh nhân</h3>
        <p class="text-sm text-slate-400 font-bold">Tra cứu hồ sơ và thực hiện nhập kết quả khám</p>
        <div class="mt-4 flex items-center gap-2 text-indigo-500 font-black text-xs uppercase tracking-widest">
          Xem ngay <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
        </div>
      </button>
    </div>

    <!-- ══ NO ASSIGNMENT ══════════════════════════════════════════ -->
    <div v-if="!todayAssignment" class="bg-amber-50 border-2 border-amber-100 rounded-[2rem] p-8 flex items-center gap-6">
      <div class="w-12 h-12 bg-amber-100 rounded-2xl flex items-center justify-center flex-shrink-0">
        <AlertCircle class="w-6 h-6 text-amber-500" />
      </div>
      <div>
        <h3 class="font-black text-slate-800">Chưa có lịch hôm nay</h3>
        <p class="text-sm text-slate-500 font-bold mt-1">Bạn chưa được phân công vào đoàn khám nào ngày hôm nay. Liên hệ quản lý để xác nhận.</p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue'
import {
  Stethoscope, CalendarCheck, CheckCircle2, ShieldCheck,
  UserRound, ArrowRight, AlertCircle
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({
  todayAssignment: { type: Object, default: null },
  isCheckingIn: { type: Boolean, default: false }
})

defineEmits(['check-in', 'navigate'])

const authStore = useAuthStore()

const todayDateStr = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const formatTime = (dateStr) =>
  new Date(dateStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
</script>
