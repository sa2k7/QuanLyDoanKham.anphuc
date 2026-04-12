<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans">
    
    <!-- Top Bar -->
    <div class="sticky top-0 z-40 glass-header p-6 mb-8 flex flex-wrap items-center justify-between gap-6 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="p-4 bg-emerald-50 rounded-2xl shadow-inner border border-emerald-100">
          <Wallet class="w-8 h-8 text-emerald-600" />
        </div>
        <div>
          <h2 class="text-2xl font-black text-slate-800 tracking-tight leading-none mb-1">Bảng Lương Tổng Hợp</h2>
          <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Quản lý lương và thù lao nhân sự toàn tuyến</p>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <!-- Date Pickers -->
        <div class="flex items-center gap-2 bg-white p-2 rounded-2xl border border-slate-200 shadow-sm">
            <select v-model="selectedYear" @change="loadPayroll" class="px-4 py-2 bg-transparent border-none font-black text-xs text-slate-600 outline-none cursor-pointer">
                <option v-for="y in yearOptions" :key="y" :value="y">Năm {{ y }}</option>
            </select>
            <div class="w-px h-6 bg-slate-200"></div>
            <select v-model="selectedMonth" @change="loadPayroll" class="px-4 py-2 bg-transparent border-none font-black text-xs text-emerald-600 outline-none cursor-pointer">
                <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }}</option>
            </select>
        </div>
        <button @click="loadPayroll" class="w-10 h-10 bg-white hover:bg-slate-50 border border-slate-200 rounded-2xl transition-all flex items-center justify-center shadow-sm">
          <RefreshCw class="w-4 h-4 text-slate-500" :class="{ 'animate-spin': loading }" />
        </button>
        <button @click="exportExcel" class="btn-premium primary">
          <Download class="w-3.5 h-3.5" /> Xuất Excel
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div class="px-6 max-w-7xl mx-auto w-full">
      
      <!-- Summaries -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <!-- Total Payout -->
          <div class="bg-slate-900 border border-slate-800 p-8 rounded-[2rem] shadow-xl relative overflow-hidden group">
              <Wallet class="absolute -right-4 -bottom-4 w-32 h-32 text-indigo-500/10 group-hover:scale-110 transition-transform duration-500 rotate-12" />
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Tổng quỹ lương đợt này</p>
              <h3 class="text-4xl font-black text-white tabular-nums mb-2">{{ formatCurrency(totalPayout) }}</h3>
              <p class="text-xs text-indigo-300 font-black">+ Thanh toán qua chuyển khoản 100%</p>
          </div>
          <!-- Total Staff -->
          <div class="premium-card p-8 flex flex-col justify-center">
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Số lượng nhân sự</p>
              <div class="flex items-baseline gap-3">
                  <h3 class="text-4xl font-black text-slate-800 tabular-nums">{{ payoutList.length }}</h3>
                  <span class="text-sm font-bold text-slate-400">người</span>
              </div>
          </div>
          <!-- Total Days -->
          <div class="premium-card p-8 flex flex-col justify-center">
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Tổng ngày công toàn tuyến</p>
              <div class="flex items-baseline gap-3">
                  <h3 class="text-4xl font-black text-emerald-600 tabular-nums">{{ totalDaysWorked }}</h3>
                  <span class="text-sm font-bold text-slate-400">ngày</span>
              </div>
          </div>
      </div>

      <!-- Detail Table -->
      <div class="premium-card overflow-hidden flex flex-col">
          <div class="p-8 border-b border-slate-50 flex justify-between items-center bg-slate-50/20">
            <div>
              <h3 class="font-black text-slate-800 uppercase tracking-tighter text-lg italic">Bảng Chi Tiết Thù Lao</h3>
              <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Đã tự động tổng hợp từ dữ liệu chấm công</p>
            </div>
            
            <div class="relative w-64">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                <input v-model="searchQuery" type="text" placeholder="Tìm tên nhân viên..." class="w-full bg-white border border-slate-200 rounded-xl py-2.5 pl-10 pr-4 text-xs font-bold text-slate-700 outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-50 transition-all shadow-sm">
            </div>
          </div>
          
          <div class="overflow-x-auto">
              <table class="w-full text-left">
                  <thead class="bg-slate-50/50">
                      <tr>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 w-16 text-center">STT</th>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Cán bộ / Nhân sự</th>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-center">Lượt tham gia</th>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-center">Ngày công quy đổi</th>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-right">Lương cơ bản</th>
                          <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-right">Thực Lãnh</th>
                      </tr>
                  </thead>
                  <tbody class="divide-y divide-slate-100">
                      <tr v-if="loading">
                          <td colspan="6" class="py-16 text-center">
                              <Loader2 class="w-8 h-8 animate-spin text-emerald-400 mx-auto mb-3" />
                              <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Đang tải bảng lương...</p>
                          </td>
                      </tr>
                      <tr v-else-if="filteredList.length === 0">
                          <td colspan="6" class="py-16 text-center">
                              <FileText class="w-12 h-12 text-slate-200 mx-auto mb-3" />
                              <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Không có dữ liệu lương</p>
                          </td>
                      </tr>
                      <tr v-for="(staff, idx) in filteredList" :key="staff.staffId" class="hover:bg-slate-50/50 transition-colors group">
                          <td class="px-6 py-4 text-center font-black text-slate-400 text-xs">{{ idx + 1 }}</td>
                          <td class="px-6 py-4">
                              <p class="font-black text-slate-800 text-sm group-hover:text-emerald-600 transition-colors">{{ staff.staffName }}</p>
                              <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ staff.jobTitle }}</p>
                          </td>
                          <td class="px-6 py-4 text-center">
                              <span class="inline-flex items-center justify-center px-3 py-1 bg-slate-100 text-slate-600 rounded-lg text-xs font-black w-12">{{ staff.totalShifts }}</span>
                          </td>
                          <td class="px-6 py-4 text-center">
                              <span class="inline-flex items-center justify-center px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg text-xs font-black w-12">{{ staff.totalDays }}</span>
                          </td>
                          <td class="px-6 py-4 text-right font-bold text-slate-400 text-xs tabular-nums">
                              {{ formatCurrency(staff.baseSalary) }}
                          </td>
                          <td class="px-6 py-4 text-right">
                              <span class="font-black text-emerald-600 text-sm tracking-tighter tabular-nums px-3 py-1.5 rounded-xl border border-emerald-100 bg-white group-hover:bg-emerald-50 transition-colors shadow-sm">{{ formatCurrency(staff.totalSalary) }}</span>
                          </td>
                      </tr>
                  </tbody>
              </table>
          </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Wallet, Download, Search, RefreshCw, FileText, Loader2 } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'
import { parseApiError } from '../services/errorHelper'

const toast = useToast()
const loading = ref(false)

// Selectors
const currentDt = new Date()
const selectedMonth = ref(currentDt.getMonth() + 1)
const selectedYear = ref(currentDt.getFullYear())
const yearOptions = Array.from({ length: 5 }, (_, i) => currentDt.getFullYear() - i)

const searchQuery = ref('')
const payoutList = ref([])

const loadPayroll = async () => {
    loading.value = true
    try {
        const res = await apiClient.get('/api/Reports/payroll-summary', {
            params: { month: selectedMonth.value, year: selectedYear.value }
        })
        payoutList.value = res.data || []
    } catch (err) {
        console.error('Lỗi tải bảng lương', err)
        toast.show(parseApiError(err), 'error')
    } finally {
        loading.value = false
    }
}

// Computed
const filteredList = computed(() => {
    if (!searchQuery.value) return payoutList.value
    const q = searchQuery.value.toLowerCase()
    return payoutList.value.filter(s => s.staffName.toLowerCase().includes(q) || s.jobTitle.toLowerCase().includes(q))
})

const totalPayout = computed(() => payoutList.value.reduce((sum, s) => sum + s.totalSalary, 0))
const totalDaysWorked = computed(() => payoutList.value.reduce((sum, s) => sum + s.totalDays, 0))

// Utils
const formatCurrency = (val) => {
    if (!val) return '0 đ'
    return val.toLocaleString('vi-VN') + ' đ'
}

const exportExcel = () => {
    toast.show('Đang xuất bảng lương ra file Excel...', 'success')
    setTimeout(() => {
        // Giả lập redirect file
        window.open(`/api/Reports/export-excel?startDate=${selectedYear.value}-${String(selectedMonth.value).padStart(2,'0')}-01`, '_blank')
    }, 1000)
}

onMounted(() => {
    loadPayroll()
})

</script>

<style scoped>
@reference "tailwindcss";

.scrollbar-premium::-webkit-scrollbar { width: 6px; }
.scrollbar-premium::-webkit-scrollbar-track { background: transparent; }
.scrollbar-premium::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.scrollbar-premium::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }
</style>
