<template>
  <div class="schedule-page">
    <div class="page-header">
      <div>
        <h1 class="page-title"><i class="fas fa-calendar-check"></i> Lịch khám của tôi</h1>
        <p class="page-subtitle">Tháng {{ currentMonth }}/{{ currentYear }}</p>
      </div>
      <div class="month-nav">
        <button class="nav-btn" @click="prevMonth"><i class="fas fa-chevron-left"></i></button>
        <span class="month-label">{{ currentMonth }}/{{ currentYear }}</span>
        <button class="nav-btn" @click="nextMonth"><i class="fas fa-chevron-right"></i></button>
      </div>
    </div>

    <!-- Tổng kết tháng -->
    <div class="summary-cards">
      <div class="summary-card">
        <i class="fas fa-calendar-check"></i>
        <div>
          <span class="summary-value">{{ summary.totalGroups }}</span>
          <span class="summary-label">Đoàn tham gia</span>
        </div>
      </div>
      <div class="summary-card">
        <i class="fas fa-sun"></i>
        <div>
          <span class="summary-value">{{ summary.totalActualDays }}</span>
          <span class="summary-label">Công thực tế</span>
        </div>
      </div>
      <div class="summary-card accent">
        <i class="fas fa-money-bill-wave"></i>
        <div>
          <span class="summary-value">{{ formatMoney(summary.estimatedSalary) }}</span>
          <span class="summary-label">Lương ước tính</span>
        </div>
      </div>
    </div>

    <!-- Chi tiết lịch -->
    <div class="schedule-list" v-if="!loading && details.length > 0">
      <div v-for="item in details" :key="item.groupId" class="schedule-item"
        :class="item.workStatus === 'Đủ công' ? 'done' : item.workStatus === 'Chờ check-out' ? 'partial' : 'absent'">
        <div class="item-date">
          <span class="day">{{ new Date(item.examDate).getDate() }}</span>
          <span class="month">Th{{ new Date(item.examDate).getMonth() + 1 }}</span>
        </div>
        <div class="item-info">
          <h4 class="group-name">{{ item.groupName }}</h4>
          <div class="item-meta">
            <span v-if="item.checkInTime">
              <i class="fas fa-sign-in-alt"></i> {{ formatTime(item.checkInTime) }}
            </span>
            <span v-if="item.checkOutTime">
              <i class="fas fa-sign-out-alt"></i> {{ formatTime(item.checkOutTime) }}
            </span>
          </div>
        </div>
        <div class="item-right">
          <span class="shift-badge" :class="item.shiftType >= 1 ? 'full' : 'half'">
            {{ item.shiftType >= 1 ? '1 công' : '½ công' }}
          </span>
          <span class="status-chip" :class="getStatusCls(item.workStatus)">
            {{ item.workStatus }}
          </span>
        </div>
      </div>
    </div>

    <div v-else-if="!loading && details.length === 0" class="empty-state">
      <i class="fas fa-calendar-times"></i>
      <p>Không có lịch khám trong tháng {{ currentMonth }}/{{ currentYear }}</p>
    </div>

    <div v-if="loading" class="loading-overlay"><div class="spinner"></div></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
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
  if (s === 'Đủ công') return 'done'
  if (s === 'Chờ check-out') return 'partial'
  return 'absent'
}

const loadSchedule = async () => {
  if (!staffId.value) return
  loading.value = true
  try {
    const res = await apiClient.get(`/Attendance/summary/${staffId.value}`, {
      params: { month: currentMonth.value, year: currentYear.value }
    })
    details.value = res.data.details || []
  } catch { details.value = [] }
  finally { loading.value = false }
}

const loadStaffId = async () => {
  try {
    const res = await apiClient.get('/Auth/profile')
    staffId.value = res.data.staffId || null
    if (!staffId.value) {
      // Fallback: tìm staff theo employeeCode = username
      const sRes = await apiClient.get('/Staffs', { params: { search: auth.username } })
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
