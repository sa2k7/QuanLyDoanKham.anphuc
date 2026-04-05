<template>
  <div class="schedule-page animate-fade-in pb-20">
    <div class="page-header flex justify-between items-center mb-8 gap-4 flex-wrap">
      <div>
        <h1 class="page-title text-3xl font-black text-slate-800 flex items-center gap-3">
            <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
                <CalendarCheck class="w-6 h-6" />
            </div>
            {{ i18n.t('schedule.title') }}
        </h1>
        <p class="page-subtitle text-slate-400 font-black uppercase tracking-widest text-[10px] mt-2">{{ i18n.t('schedule.subtitle').replace('{0}', currentMonth).replace('{1}', currentYear) }}</p>
      </div>
      <div class="month-nav flex items-center gap-4 bg-white p-2 rounded-2xl border border-slate-100 shadow-sm">
        <button class="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 hover:bg-indigo-50 hover:text-indigo-600 transition-all" @click="prevMonth">
            <ChevronLeft class="w-5 h-5" />
        </button>
        <span class="month-label font-black text-sm uppercase tracking-widest text-slate-700 min-width-[100px] text-center">{{ currentMonth }} / {{ currentYear }}</span>
        <button class="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 hover:bg-indigo-50 hover:text-indigo-600 transition-all" @click="nextMonth">
            <ChevronRight class="w-5 h-5" />
        </button>
      </div>
    </div>

    <!-- Tổng kết tháng -->
    <div class="summary-cards grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
      <div class="summary-card premium-card bg-white p-8 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex items-center gap-4 group">
        <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center group-hover:scale-110 transition-transform">
            <CalendarCheck class="w-6 h-6" />
        </div>
        <div>
          <span class="summary-value block text-2xl font-black text-slate-800 tabular-nums">{{ String(summary.totalGroups).padStart(2, '0') }}</span>
          <span class="summary-label text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ i18n.t('schedule.summary.groups') }}</span>
        </div>
      </div>
      <div class="summary-card premium-card bg-white p-8 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex items-center gap-4 group">
        <div class="w-12 h-12 bg-emerald-50 text-emerald-600 rounded-2xl flex items-center justify-center group-hover:scale-110 transition-transform">
            <Sun class="w-6 h-6" />
        </div>
        <div>
          <span class="summary-value block text-2xl font-black text-slate-800 tabular-nums">{{ summary.totalActualDays }}</span>
          <span class="summary-label text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ i18n.t('schedule.summary.days') }}</span>
        </div>
      </div>
      <div class="summary-card premium-card bg-slate-900 p-8 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#1e293b] flex items-center gap-4 group">
        <div class="w-12 h-12 bg-white/10 text-emerald-400 rounded-2xl flex items-center justify-center group-hover:scale-110 transition-transform">
            <Wallet class="w-6 h-6" />
        </div>
        <div>
          <span class="summary-value block text-2xl font-black text-emerald-400 tabular-nums">{{ formatMoney(summary.estimatedSalary) }}</span>
          <span class="summary-label text-[10px] font-black text-slate-500 uppercase tracking-widest">{{ i18n.t('schedule.summary.salary') }}</span>
        </div>
      </div>
    </div>

    <!-- Chi tiết lịch -->
    <div class="schedule-list grid grid-cols-1 gap-4" v-if="!loading && details.length > 0">
      <div v-for="item in details" :key="item.groupId" 
        class="schedule-item bg-white p-6 rounded-3xl border-2 transition-all flex items-center gap-6"
        :class="getStatusBorderCls(item.workStatus)">
        <div class="item-date flex flex-col items-center justify-center w-16 h-16 bg-slate-50 rounded-2xl border border-slate-100 shadow-inner">
          <span class="day text-xl font-black text-slate-800 leading-none">{{ new Date(item.examDate).getDate() }}</span>
          <span class="month text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ i18n.t('schedule.item.month').replace('{0}', new Date(item.examDate).getMonth() + 1) }}</span>
        </div>
        <div class="item-info flex-1">
          <h4 class="group-name text-sm font-black text-slate-800 uppercase tracking-widest mb-2">{{ item.groupName }}</h4>
          <div class="item-meta flex items-center gap-6">
            <span v-if="item.checkInTime" class="flex items-center gap-2 text-[10px] font-black text-emerald-500 uppercase tracking-widest">
              <LogIn class="w-3.5 h-3.5" /> {{ formatTime(item.checkInTime) }}
            </span>
            <span v-if="item.checkOutTime" class="flex items-center gap-2 text-[10px] font-black text-rose-500 uppercase tracking-widest">
              <LogOut class="w-3.5 h-3.5" /> {{ formatTime(item.checkOutTime) }}
            </span>
          </div>
        </div>
        <div class="item-right flex flex-col items-end gap-2">
          <span class="shift-badge px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest border" :class="item.shiftType >= 1 ? 'bg-indigo-50 text-indigo-600 border-indigo-100' : 'bg-amber-50 text-amber-600 border-amber-100'">
            {{ item.shiftType >= 1 ? i18n.t('schedule.item.shift1') : i18n.t('schedule.item.shift05') }}
          </span>
          <span class="status-chip px-4 py-1.5 rounded-full text-[9px] font-black uppercase tracking-widest" :class="getStatusCls(item.workStatus)">
            {{ item.workStatus }}
          </span>
        </div>
      </div>
    </div>

    <div v-else-if="!loading && details.length === 0" class="flex flex-col items-center justify-center py-32 text-center">
      <CalendarOff class="w-16 h-16 text-slate-200 mb-6" />
      <p class="text-slate-400 font-black uppercase tracking-widest text-sm">{{ i18n.t('schedule.empty').replace('{0}', currentMonth).replace('{1}', currentYear) }}</p>
    </div>

    <div v-if="loading" class="flex justify-center py-20">
        <Loader2 class="w-10 h-10 animate-spin text-slate-200" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '../services/apiClient'
import { useAuthStore } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'
import { 
    CalendarCheck, ChevronLeft, ChevronRight, Sun, Wallet, 
    LogIn, LogOut, CalendarOff, Loader2 
} from 'lucide-vue-next'

const auth = useAuthStore()
const i18n = useI18nStore()
const now = new Date()
const currentMonth = ref(now.getMonth() + 1)
const currentYear  = ref(now.getFullYear())
const details  = ref([])
const loading  = ref(false)

// Tìm staffId từ profile — API trả kèm staffId nếu linked
const staffId = ref(null)

const summary = computed(() => ({
  totalGroups:     details.value.length,
  totalActualDays: details.value.reduce((s, d) => s + (d.shiftType || 0), 0),
  estimatedSalary: 0  // sẽ lấy từ /Payroll/my-salary
}))

const prevMonth = () => {
  if (currentMonth.value === 1) { currentMonth.value = 12; currentYear.value-- }
  else currentMonth.value--
}
const nextMonth = () => {
  if (currentMonth.value === 12) { currentMonth.value = 1; currentYear.value++ }
  else currentMonth.value++
}

const formatTime  = (d) => d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : ''
const formatMoney = (n) => n ? n.toLocaleString('vi-VN') + ' ₫' : '—'

const getStatusCls = (s) => {
  if (s === 'Đủ công') return 'bg-emerald-500 text-white'
  if (s === 'Chờ check-out') return 'bg-amber-500 text-white'
  return 'bg-rose-500 text-white'
}

const getStatusBorderCls = (s) => {
  if (s === 'Đủ công') return 'border-emerald-100 hover:border-emerald-300'
  if (s === 'Chờ check-out') return 'border-amber-100 hover:border-amber-300'
  return 'border-rose-100 hover:border-rose-300'
}

const loadSchedule = async () => {
  if (!staffId.value) return
  loading.value = true
  try {
    const res = await apiClient.get(`/api/Attendance/summary/${staffId.value}`, {
      params: { month: currentMonth.value, year: currentYear.value }
    })
    details.value = res.data.details || []
  } catch { details.value = [] }
  finally { loading.value = false }
}

const loadStaffId = async () => {
  try {
    const res = await apiClient.get('/api/Auth/profile')
    staffId.value = res.data.staffId || null
    if (!staffId.value) {
      // Fallback: tìm staff theo employeeCode = username
      const sRes = await apiClient.get('/api/Staffs', { params: { search: auth.username } })
      if (sRes.data.length > 0) staffId.value = sRes.data[0].staffId
    }
  } catch { staffId.value = null }
}

watch([currentMonth, currentYear], loadSchedule)
onMounted(async () => { await loadStaffId(); await loadSchedule() })
</script>

<style scoped>
.schedule-page { padding: 24px; max-width: 800px; margin: 0 auto; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; flex-wrap: wrap; gap: 12px; }
.page-title { font-size: 1.5rem; font-weight: 700; margin: 0; color: var(--text-primary, #1e293b); }
.page-title i { margin-right: 10px; color: var(--primary, #6366f1); }
.page-subtitle { color: var(--text-muted, #64748b); margin: 4px 0 0; }

.month-nav { display: flex; align-items: center; gap: 12px; }
.nav-btn { width: 36px; height: 36px; border: 1px solid var(--border, #e2e8f0); border-radius: 8px; background: var(--card-bg, #fff); cursor: pointer; display: flex; align-items: center; justify-content: center; color: var(--text-secondary, #475569); transition: all .15s; }
.nav-btn:hover { background: var(--primary, #6366f1); color: #fff; border-color: var(--primary, #6366f1); }
.month-label { font-weight: 700; font-size: 1rem; color: var(--text-primary, #1e293b); min-width: 70px; text-align: center; }

.summary-cards { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 24px; }
.summary-card {
  background: var(--card-bg, #fff); border-radius: 12px; padding: 18px;
  border: 1px solid var(--border, #e2e8f0); display: flex; align-items: center;
  gap: 14px; box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.summary-card i { font-size: 1.6rem; color: var(--primary, #6366f1); }
.summary-card.accent { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: #fff; }
.summary-card.accent i { color: rgba(255,255,255,0.8); }
.summary-value { display: block; font-size: 1.5rem; font-weight: 700; }
.summary-label { font-size: 0.78rem; opacity: 0.75; }

.schedule-list { display: flex; flex-direction: column; gap: 12px; }
.schedule-item {
  display: flex; align-items: center; gap: 16px; padding: 14px 18px;
  background: var(--card-bg, #fff); border-radius: 12px;
  border: 1px solid var(--border, #e2e8f0); border-left: 4px solid #e2e8f0;
  transition: transform .15s; box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}
.schedule-item:hover { transform: translateX(4px); }
.schedule-item.done    { border-left-color: #10b981; }
.schedule-item.partial { border-left-color: #f59e0b; }
.schedule-item.absent  { border-left-color: #ef4444; }

.item-date { text-align: center; min-width: 44px; }
.day   { display: block; font-size: 1.5rem; font-weight: 800; color: var(--text-primary, #1e293b); line-height: 1; }
.month { font-size: 0.72rem; color: var(--text-muted, #64748b); }

.item-info { flex: 1; }
.group-name { margin: 0 0 6px; font-weight: 600; font-size: 0.95rem; color: var(--text-primary, #1e293b); }
.item-meta { display: flex; gap: 14px; font-size: 0.8rem; color: var(--text-muted, #64748b); }
.item-meta i { margin-right: 4px; }

.item-right { display: flex; flex-direction: column; align-items: flex-end; gap: 6px; }
.shift-badge { padding: 3px 10px; border-radius: 20px; font-size: 0.78rem; font-weight: 600; }
.shift-badge.full { background: #dcfce7; color: #059669; }
.shift-badge.half { background: #fef3c7; color: #d97706; }

.status-chip { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
.status-chip.done    { background: #dcfce7; color: #059669; }
.status-chip.partial { background: #fef3c7; color: #d97706; }
.status-chip.absent  { background: #fef2f2; color: #ef4444; }

.empty-state { text-align: center; padding: 60px; color: var(--text-muted, #64748b); }
.empty-state i { font-size: 3rem; opacity: 0.3; margin-bottom: 16px; }
.loading-overlay { display: flex; justify-content: center; padding: 60px; }
.spinner { width: 40px; height: 40px; border: 3px solid #e2e8f0; border-top-color: var(--primary, #6366f1); border-radius: 50%; animation: spin .7s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 600px) {
  .summary-cards { grid-template-columns: 1fr; }
}
</style>
