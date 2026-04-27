<template>
  <div class="h-full flex flex-col bg-slate-50 min-h-screen pb-12 overflow-y-auto font-sans">

    <!-- ── Top Bar ──────────────────────────────────────────────────── -->
    <div class="sticky top-0 z-40 bg-white/80 backdrop-blur-xl border-b border-slate-100 px-8 py-5 flex flex-wrap items-center justify-between gap-4 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="p-3 bg-violet-50 rounded-2xl shadow-inner border border-violet-100">
          <ClipboardCheck class="w-7 h-7 text-violet-600" />
        </div>
        <div>
          <h1 class="text-2xl font-black text-slate-800 tracking-tight leading-none mb-0.5">Kiểm Tra Chấm Công</h1>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng hợp nhân sự đã đi làm theo tháng</p>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <!-- Period selector -->
        <div class="flex items-center gap-2 bg-white px-4 py-2 rounded-2xl border border-slate-200 shadow-sm">
          <select v-model="selectedYear" @change="loadData" class="text-xs font-black text-slate-600 bg-transparent border-none outline-none cursor-pointer">
            <option v-for="y in yearOptions" :key="y" :value="y">Năm {{ y }}</option>
          </select>
          <div class="w-px h-5 bg-slate-200"></div>
          <select v-model="selectedMonth" @change="loadData" class="text-xs font-black text-violet-600 bg-transparent border-none outline-none cursor-pointer">
            <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }}</option>
          </select>
        </div>

        <button @click="loadData" class="w-10 h-10 bg-white hover:bg-slate-50 border border-slate-200 rounded-2xl transition-all flex items-center justify-center shadow-sm">
          <RefreshCw class="w-4 h-4 text-slate-500" :class="{ 'animate-spin': loading }" />
        </button>
      </div>
    </div>

    <!-- ── Body ──────────────────────────────────────────────────────── -->
    <div class="px-8 max-w-7xl mx-auto w-full mt-8">

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-6 mb-8">
        <div class="bg-slate-900 p-7 rounded-[2rem] shadow-xl relative overflow-hidden group">
          <Wallet class="absolute -right-3 -bottom-3 w-28 h-28 text-violet-500/10 group-hover:scale-110 transition-transform duration-500 rotate-12" />
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng quỹ lương</p>
          <h3 class="text-3xl font-black text-white tabular-nums">{{ formatCurrency(summary.totalPayroll) }}</h3>
          <p class="text-xs text-violet-300 font-bold mt-1">Tháng {{ selectedMonth }}/{{ selectedYear }}</p>
        </div>
        <div class="bg-white border border-slate-100 p-7 rounded-[2rem] shadow-sm">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Số nhân sự đã đi làm</p>
          <div class="flex items-baseline gap-2">
            <h3 class="text-3xl font-black text-slate-800 tabular-nums">{{ summary.totalStaff }}</h3>
            <span class="text-sm font-bold text-slate-400">người</span>
          </div>
        </div>
        <div class="bg-white border border-slate-100 p-7 rounded-[2rem] shadow-sm">
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng ngày công toàn tuyến</p>
          <div class="flex items-baseline gap-2">
            <h3 class="text-3xl font-black text-violet-600 tabular-nums">{{ totalDays }}</h3>
            <span class="text-sm font-bold text-slate-400">ngày</span>
          </div>
        </div>
      </div>

      <!-- Main Table -->
      <div class="bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
        <!-- Table Header Controls -->
        <div class="px-8 py-6 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
          <div>
            <h3 class="font-black text-slate-800 text-lg tracking-tight italic uppercase">Danh Sách Chi Tiết</h3>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Nhấn vào tên nhân sự để xem lịch đi khám chi tiết</p>
          </div>
          <div class="relative w-60">
            <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input v-model="searchQuery" type="text" placeholder="Tìm nhân viên..."
              class="w-full bg-white border border-slate-200 rounded-xl py-2.5 pl-10 pr-4 text-xs font-bold text-slate-700 outline-none focus:border-violet-400 focus:ring-2 focus:ring-violet-50 transition-all shadow-sm" />
          </div>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="py-20 flex flex-col items-center gap-3">
          <Loader2 class="w-8 h-8 animate-spin text-violet-400" />
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Đang tải dữ liệu chấm công...</p>
        </div>

        <!-- Empty -->
        <div v-else-if="filteredStaffs.length === 0" class="py-20 text-center">
          <ClipboardCheck class="w-12 h-12 text-slate-200 mx-auto mb-3" />
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Không có dữ liệu chấm công trong tháng này</p>
        </div>

        <!-- Data -->
        <div v-else>
          <table class="w-full text-left">
            <thead class="bg-slate-50/60">
              <tr>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest w-14 text-center">STT</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest">Nhân sự</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-center">Số đoàn</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-center">Ngày công</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">Đơn giá/ngày</th>
                <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest text-right">Tiền công</th>
                <th class="px-6 py-4 w-12"></th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <template v-for="(staff, idx) in filteredStaffs" :key="staff.staffId">
                <!-- Staff Row -->
                <tr @click="toggleExpand(staff.staffId)"
                  class="hover:bg-slate-50/50 transition-colors cursor-pointer group">
                  <td class="px-6 py-4 text-center text-[11px] font-black text-slate-400 italic">{{ idx + 1 }}</td>
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <div class="w-9 h-9 rounded-xl bg-violet-50 border border-violet-100 flex items-center justify-center text-sm font-black text-violet-600">
                        {{ staff.staffName?.charAt(0) }}
                      </div>
                      <div>
                        <p class="text-sm font-black text-slate-700 uppercase group-hover:text-violet-600 transition-colors">{{ staff.staffName }}</p>
                        <p class="text-[10px] font-bold text-slate-400">{{ staff.employeeCode }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 text-center">
                    <span class="inline-flex items-center justify-center w-10 h-7 bg-slate-100 text-slate-600 rounded-lg text-xs font-black">
                      {{ staff.workdays?.length || 0 }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-center">
                    <span class="inline-flex items-center justify-center px-3 h-7 bg-violet-50 text-violet-600 rounded-lg text-xs font-black border border-violet-100">
                      {{ staff.totalDays }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-right text-xs font-bold text-slate-400 tabular-nums">
                    {{ formatCurrency(staff.dailyRate) }}
                  </td>
                  <td class="px-6 py-4 text-right">
                    <span class="font-black text-emerald-600 text-sm tabular-nums px-3 py-1.5 rounded-xl bg-emerald-50 border border-emerald-100">
                      {{ formatCurrency(staff.totalEarned) }}
                    </span>
                  </td>
                  <td class="px-6 py-4">
                    <ChevronDown class="w-4 h-4 text-slate-400 transition-transform" :class="{ 'rotate-180': expandedStaff === staff.staffId }" />
                  </td>
                </tr>

                <!-- Expanded Detail Rows -->
                <tr v-if="expandedStaff === staff.staffId">
                  <td colspan="7" class="bg-slate-50/70 px-6 py-4">
                    <div class="rounded-2xl border border-slate-100 overflow-hidden">
                      <table class="w-full text-left">
                        <thead>
                          <tr class="bg-white border-b border-slate-100">
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest">Đoàn khám</th>
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest">Ngày khám</th>
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest text-center">Check-in</th>
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest text-center">Check-out</th>
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest text-center">Ca làm</th>
                            <th class="px-5 py-3 text-[9px] font-black text-slate-400 uppercase tracking-widest text-center">Trạng thái</th>
                          </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-50">
                          <tr v-for="day in staff.workdays" :key="day.groupId + day.examDate" class="bg-white hover:bg-slate-50/50 transition-colors">
                            <td class="px-5 py-3 text-xs font-black text-slate-700">{{ day.groupName }}</td>
                            <td class="px-5 py-3 text-xs text-slate-500 font-bold">{{ formatDate(day.examDate) }}</td>
                            <td class="px-5 py-3 text-center">
                              <span class="text-xs font-black text-emerald-600">{{ formatTime(day.checkInTime) }}</span>
                            </td>
                            <td class="px-5 py-3 text-center">
                              <span class="text-xs font-black text-rose-500">{{ formatTime(day.checkOutTime) }}</span>
                            </td>
                            <td class="px-5 py-3 text-center">
                              <span class="px-2.5 py-1 rounded-lg text-[9px] font-black uppercase tracking-widest"
                                :class="day.shiftType >= 1 ? 'bg-indigo-50 text-indigo-600 border border-indigo-100' : 'bg-amber-50 text-amber-600 border border-amber-100'">
                                {{ day.shiftType >= 1 ? 'Cả ngày' : 'Nửa ngày' }}
                              </span>
                            </td>
                            <td class="px-5 py-3 text-center">
                              <span class="px-2.5 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border"
                                :class="day.workStatus === 'Đủ công' ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100'">
                                {{ day.workStatus }}
                              </span>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ClipboardCheck, RefreshCw, Search, Loader2, Wallet, ChevronDown } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'
import { parseApiError } from '../services/errorHelper'

const toast = useToast()
const loading = ref(false)

// Period state
const now = new Date()
const selectedMonth = ref(now.getMonth() + 1)
const selectedYear = ref(now.getFullYear())
const yearOptions = Array.from({ length: 4 }, (_, i) => now.getFullYear() - i)

// Data
const summary = ref({ totalStaff: 0, totalPayroll: 0, staffs: [] })
const searchQuery = ref('')
const expandedStaff = ref(null)

const filteredStaffs = computed(() => {
  if (!searchQuery.value) return summary.value.staffs || []
  const q = searchQuery.value.toLowerCase()
  return (summary.value.staffs || []).filter(s =>
    s.staffName?.toLowerCase().includes(q) || s.employeeCode?.toLowerCase().includes(q)
  )
})

const totalDays = computed(() =>
  (summary.value.staffs || []).reduce((sum, s) => sum + (s.totalDays || 0), 0)
)

const loadData = async () => {
  loading.value = true
  expandedStaff.value = null
  try {
    const res = await apiClient.get('/api/Attendance/summary-all', {
      params: { month: selectedMonth.value, year: selectedYear.value }
    })
    summary.value = res.data
  } catch (err) {
    toast.show(parseApiError(err), 'error')
  } finally {
    loading.value = false
  }
}

const toggleExpand = (staffId) => {
  expandedStaff.value = expandedStaff.value === staffId ? null : staffId
}

// Utils
const formatCurrency = (val) => val ? val.toLocaleString('vi-VN') + ' đ' : '0 đ'
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'
const formatTime = (t) => {
  if (!t) return '---'
  const d = new Date(t)
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}

onMounted(loadData)
</script>
