<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in p-3 scrollbar-premium overflow-y-auto font-sans">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3 mb-3">
      <div>
        <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
          <div class="w-8 h-8 bg-primary text-white rounded-lg flex items-center justify-center shadow-md">
            <BarChart3 class="w-4.5 h-4.5" />
          </div>
          Trung Tâm Phân Tích
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[7.5px] mt-0.5">Thống kê hiệu suất & Vận hành (BI Analytics)</p>
      </div>

      <!-- Filter Controls -->
      <div class="flex items-center gap-2 bg-white p-1.5 rounded-xl border border-slate-200 shadow-sm">
        <div class="flex items-center gap-2 px-3 border-r border-slate-200/50 py-0.5">
           <Calendar class="w-4 h-4 text-slate-400" />
           <div class="flex items-center gap-1.5">
             <input type="date" v-model="filters.startDate" @change="fetchReportData" class="bg-transparent border-none outline-none text-[10px] font-black text-slate-600 cursor-pointer focus:ring-0 uppercase tracking-tighter" />
             <span class="text-slate-300 font-black text-[10px]">→</span>
             <input type="date" v-model="filters.endDate" @change="fetchReportData" class="bg-transparent border-none outline-none text-[10px] font-black text-slate-600 cursor-pointer focus:ring-0 uppercase tracking-tighter" />
           </div>
        </div>
        <button @click="fetchReportData" class="w-8 h-8 flex items-center justify-center hover:bg-slate-50 rounded-lg transition-all group shadow-sm bg-white" title="Làm mới dữ liệu">
          <RefreshCw class="w-4 h-4 text-slate-500 group-active:rotate-180 transition-transform duration-500" />
        </button>
      </div>

      <div class="flex items-center gap-2">
        <button @click="handleExport('PDF')" class="h-8 px-3 bg-white border border-slate-200 text-slate-600 rounded-lg font-black text-[9px] uppercase hover:bg-slate-50 transition-all shadow-sm flex items-center gap-1.5">
          <FileDown class="w-3.5 h-3.5" /> XUẤT PDF
        </button>
        <button @click="handleExport('Excel')" class="h-8 px-4 bg-primary text-white rounded-lg font-black text-[9px] uppercase shadow-md shadow-primary/20 hover:bg-primary/90 transition-all flex items-center gap-1.5">
          <Download class="w-3.5 h-3.5" /> TẢI EXCEL
        </button>
      </div>
    </div>

    <!-- MAIN CONTENT START -->
    <div class="pb-10 max-w-[1600px] mx-auto w-full">
      <!-- 1. KPI WIDGETS -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
        <StatCard 
          title="LÃI/LỖ RÒNG (P&L)" 
          :value="formatCurrency(kpis.netProfit)" 
          :icon="DollarSign" 
          :trend="kpis.netProfit > 0 ? 'Tín hiệu tốt' : 'Cần tối ưu'" 
          :trendColor="kpis.netProfit > 0 ? 'emerald' : 'rose'" 
          subtext="Doanh thu - Chi phí vận hành" 
          variant="indigo" 
        />
        
        <StatCard 
          title="HIỆU SUẤT NHÂN SỰ" 
          :value="(kpis.hrPerformance || 0) + '%'" 
          :icon="Users" 
          trend="Utilization" 
          subtext="Tỷ lệ tham gia thực tế" 
          :progress="kpis.hrPerformance || 0" 
          variant="emerald"
        />

        <StatCard 
          title="ĐỘ LỆCH VẬT TƯ" 
          :value="formatCurrency(kpis.materialDeviation)" 
          :icon="Package" 
          :trend="Math.abs(kpis.materialDeviation) < 5000000 ? 'Ổn định' : 'Cao'" 
          :trendColor="kpis.materialDeviation <= 0 ? 'emerald' : 'amber'" 
          subtext="Thực tế vs. Định mức (10%)" 
          variant="sky"
        />

        <StatCard 
          title="ĐOÀN KHÁM (THÁNG)" 
          :value="operationalSummary?.totalMedicalGroupsThisMonth || 0" 
          :icon="CheckCircle2" 
          :trend="(operationalSummary?.totalStaffDeployedThisMonth || 0) + ' lượt NS'" 
          subtext="Tiến độ hoàn thành" 
          :progress="kpis.completionRate || 0" 
          variant="rose"
        />
      </div>

      <!-- 2. Charts & Performance Matrix -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-4">
        <!-- Revenue Trend Analysis -->
        <div class="lg:col-span-2 premium-card p-4 relative overflow-hidden group bg-white border border-slate-100 shadow-sm rounded-xl">
          <div class="flex items-center justify-between mb-4">
             <div>
                <h3 class="font-black text-slate-800 uppercase tracking-tight text-sm italic">Xu hướng Doanh thu</h3>
                <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">Dữ liệu 6 tháng gần nhất</p>
             </div>
             <div class="flex gap-1.5 items-center">
                <span class="w-2 h-2 rounded-full bg-primary animate-pulse"></span>
                <span class="text-[8px] font-black text-primary uppercase tracking-widest">Sync</span>
             </div>
          </div>
          
          <div class="h-48 flex items-end justify-between gap-3 px-2 relative mt-2">
             <div v-for="(point, i) in kpis.revenueTrend" :key="i" class="flex-grow flex flex-col items-center group/bar cursor-default">
                <div class="relative w-full flex flex-col items-center">
                   <div class="absolute -top-10 scale-0 group-hover/bar:scale-100 transition-all duration-300 bg-slate-900 text-white text-[8px] px-2 py-1 rounded-lg font-black whitespace-nowrap z-10 shadow-xl tabular-nums">
                      {{ formatFullCurrency(point.value) }}
                   </div>
                   <div class="w-full bg-slate-50/50 rounded-lg h-36 flex items-end overflow-hidden group-hover/bar:bg-slate-100 transition-colors border border-slate-50">
                      <div class="w-full bg-primary/20 group-hover/bar:bg-primary transition-all duration-500" :style="{ height: (point.value / (maxRevenue || 1) * 100) + '%' }"></div>
                   </div>
                </div>
                <span class="mt-2 text-[8px] font-black text-slate-400 uppercase tracking-tighter">{{ point.label }}</span>
             </div>
          </div>
        </div>

        <!-- 2.2 Operation Deadlines Widget -->
        <div class="premium-card p-4 flex flex-col group/alerts bg-white border border-slate-100 shadow-sm rounded-xl">
          <div class="flex items-center justify-between mb-3">
             <h3 class="font-black text-slate-800 uppercase tracking-tight text-sm italic">Deadline & Cảnh báo</h3>
             <div class="p-1.5 bg-indigo-50 rounded-lg">
               <BellRing class="text-indigo-500 w-4 h-4" />
             </div>
          </div>
          <div class="flex-grow space-y-2.5">
             <div v-if="!kpis.upcomingDeadlines || kpis.upcomingDeadlines.length === 0" class="flex flex-col items-center justify-center h-full opacity-30 py-8">
                <FileText class="w-8 h-8 mb-2 opacity-20" />
                <p class="font-bold text-[10px] text-slate-400">Không có deadline</p>
             </div>
             <div v-for="item in kpis.upcomingDeadlines" :key="item.id" class="p-3 rounded-xl bg-slate-50 border border-slate-100 hover:border-indigo-200 transition-all group scale-100 hover:scale-[1.01] duration-300">
                <div class="flex justify-between items-start mb-1">
                   <p class="text-[9px] font-black text-slate-700 uppercase tracking-tight leading-none">{{ item.name }}</p>
                   <span class="px-1.5 py-0.5 rounded bg-indigo-500 text-white text-[7px] font-black uppercase tracking-widest">{{ item.daysRemaining }}d</span>
                </div>
                <p class="text-[8px] text-slate-400 font-bold uppercase tracking-widest">{{ item.company }}</p>
             </div>
          </div>
          <button class="mt-4 py-2 bg-slate-50 rounded-lg text-[8px] font-black uppercase tracking-widest text-slate-500 hover:bg-slate-100 transition-all border border-slate-100">XEM TẤT CẢ</button>
        </div>
      </div>

      <!-- 3. Financial & Efficiency Grid -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <!-- 3.1 Top Performers (HR Analysis) -->
        <div class="premium-card overflow-hidden flex flex-col bg-white border border-slate-100 shadow-sm rounded-xl">
          <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/20">
            <div>
              <h3 class="font-black text-slate-800 uppercase tracking-tight text-sm italic">Hiệu suất Nhân sự</h3>
              <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">Phân tích tần suất & thu nhập</p>
            </div>
            <div class="p-1.5 bg-indigo-50 rounded-lg">
              <Users2 class="w-4 h-4 text-indigo-500" />
            </div>
          </div>
          <div class="p-1 flex-grow">
            <div class="overflow-x-auto">
              <table class="w-full text-left">
                <thead>
                  <tr class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-50">
                     <th class="px-3 py-2">Nhân sự</th>
                     <th class="px-3 py-2 text-center">Số đoàn</th>
                     <th class="px-3 py-2 text-center">Ngày công</th>
                     <th class="px-3 py-2 text-right">Tổng thu nhập</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                  <tr v-for="staff in staffEfficiency.slice(0, 5)" :key="staff.staffName" class="hover:bg-slate-50/50 transition-all group">
                     <td class="px-3 py-2">
                        <div class="flex items-center gap-2">
                          <div class="w-6 h-6 rounded bg-slate-100 flex items-center justify-center font-black text-slate-400 text-[9px] shadow-inner">
                            {{ staff.staffName.charAt(0) }}
                          </div>
                          <div>
                            <p class="font-black text-slate-800 text-[10px] leading-none mb-0.5 group-hover:text-primary transition-colors uppercase italic">{{ staff.staffName }}</p>
                            <p class="text-[7px] font-bold text-slate-400 uppercase tracking-widest">{{ staff.role }}</p>
                          </div>
                        </div>
                     </td>
                     <td class="px-3 py-2 text-center font-black text-slate-700 tabular-nums text-[10px]">{{ staff.totalGroups }}</td>
                     <td class="px-3 py-2 text-center font-bold text-slate-500 tabular-nums text-[10px]">{{ staff.daysWorked }}<span class="text-[7px] text-slate-300 ml-0.5">D</span></td>
                     <td class="px-3 py-2 text-right">
                        <span class="font-black text-emerald-600 text-[10px] tracking-tighter tabular-nums px-2 py-0.5 bg-emerald-50 rounded border border-emerald-100">{{ staff.totalSalary?.toLocaleString() }}</span>
                     </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- 3.2 Finance Overview (CEO Analysis) -->
        <div class="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden flex flex-col group/finance">
          <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/20">
            <div>
              <h3 class="font-black text-slate-800 uppercase tracking-tight text-sm italic">Phân tích Lợi nhuận</h3>
              <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">Cơ cấu doanh thu & chi phí trọng điểm</p>
            </div>
            <div class="p-1.5 bg-emerald-50 rounded-lg shadow-sm">
              <TrendingUp class="w-4 h-4 text-emerald-500" />
            </div>
          </div>
          
          <div class="flex-grow p-3">
             <div class="grid grid-cols-3 gap-2 mb-4">
                <div class="text-center p-2 bg-slate-50 rounded-lg border border-slate-100 relative group overflow-hidden shadow-sm">
                   <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest mb-0.5 italic">Lãi gộp</p>
                   <p class="text-sm font-black text-slate-900 tabular-nums">{{ financialReport.margin?.toFixed(1) || '0.0' }}%</p>
                </div>
                <div class="text-center p-2 bg-rose-50 rounded-lg border border-rose-100 group shadow-sm">
                   <p class="text-[7px] font-black text-rose-400 uppercase tracking-widest mb-0.5 italic">CP Nhân sự</p>
                   <p class="text-sm font-black text-rose-600 tabular-nums">{{ calculatePercentage(financialReport.staffCost, financialReport.revenue) }}%</p>
                </div>
                <div class="text-center p-2 bg-amber-50 rounded-lg border border-amber-100 group shadow-sm">
                   <p class="text-[7px] font-black text-amber-500 uppercase tracking-widest mb-0.5 italic">Lượt mới</p>
                   <p class="text-sm font-black text-amber-600 tabular-nums">{{ kpis.newPatientsThisMonth || 0 }}</p>
                </div>
             </div>

             <div class="space-y-2">
                <div v-for="contract in financialReport.topContracts || []" :key="contract.companyName" class="relative group/item">
                   <div class="flex justify-between text-[8px] font-black text-slate-700 uppercase tracking-tight mb-1">
                      <span class="group-hover/item:text-primary transition-colors truncate max-w-[150px] italic">{{ contract.companyName }}</span>
                      <span class="text-emerald-600 tabular-nums">{{ formatCurrency(contract.amount) }}</span>
                   </div>
                   <div class="w-full bg-slate-100 rounded-full h-1 flex overflow-hidden shadow-inner">
                      <div class="bg-primary h-full rounded-full shadow-lg shadow-primary/20 transition-all duration-1000" :style="{ width: (contract.amount / (financialReport.topContracts?.[0]?.amount || 1) * 100) + '%' }"></div>
                   </div>
                </div>
             </div>
          </div>
          <button class="mx-3 mb-3 py-2 bg-slate-900 text-white rounded-lg font-black text-[9px] uppercase tracking-widest hover:bg-slate-800 transition-all shadow-md active:scale-95 flex items-center justify-center gap-2">
            <FileText class="w-3.5 h-3.5 text-indigo-400" />
            BÁO CÁO TÀI CHÍNH (PDF)
          </button>
        </div>
      </div>
    </div>

    <!-- EXPORT MODAL & OVERLAYS (Simulated) -->
    <div v-if="exporting" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-md flex items-center justify-center p-3">
       <div class="bg-white rounded-xl p-8 max-w-sm w-full text-center shadow-2xl animate-scale-up border border-white/20">
          <div class="relative w-24 h-24 mx-auto mb-6">
             <div class="absolute inset-0 border-[6px] border-slate-100 rounded-full shadow-inner"></div>
             <div class="absolute inset-0 border-[6px] border-primary rounded-full border-t-transparent animate-spin"></div>
             <div class="absolute inset-0 flex items-center justify-center">
                <FileText class="w-8 h-8 text-primary" />
             </div>
          </div>
          <h3 class="text-lg font-black text-slate-800 mb-1">Đang khởi tạo báo cáo...</h3>
          <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest mb-6">Vui lòng không đóng cửa sổ này</p>
          <div class="w-full bg-slate-100 rounded-full h-2 mb-2 shadow-inner overflow-hidden p-0.5">
             <div class="bg-primary h-full rounded-full transition-all duration-300 shadow-lg shadow-primary/40" :style="{ width: exportProgress + '%' }"></div>
          </div>
          <p class="text-[8px] font-black text-primary uppercase tracking-widest">{{ exportProgress }}% Complete</p>
       </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, reactive } from 'vue'
import apiClient from '../services/apiClient'
import { parseApiError } from '../services/errorHelper'
import { useAuthStore } from '../stores/auth'
import StatCard from '../components/StatCard.vue'
import { useToast } from '../composables/useToast'
import { 
  Users, DollarSign, BarChart3, TrendingUp, Calendar, RefreshCw,
  FileDown, Download, AlertTriangle, CheckCircle2, BellRing, Users2,
  Package, FileText
} from 'lucide-vue-next'

const authStore = useAuthStore()
const toast = useToast()

// Filters state
const filters = reactive({
  startDate: new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1).toISOString().split('T')[0],
  endDate: new Date().toISOString().split('T')[0]
})

// Initializing state with defaults to avoid undefined errors
const kpis = ref({
  totalPatients: 0,
  netProfit: 0,
  completionRate: 0,
  activeGroupsCount: 0,
  pendingContractsCount: 0,
  newPatientsThisMonth: 0,
  revenueTrend: [],
  upcomingDeadlines: []
})

const financialReport = ref({
  margin: 0,
  staffCost: 0,
  supplyCost: 0,
  revenue: 0,
  topContracts: []
})

const staffEfficiency = ref([])
const inventoryAlerts = ref([])
const operationalSummary = ref(null)
const exporting = ref(false)
const exportProgress = ref(0)
const showAllAlerts = ref(false)

const maxRevenue = computed(() => {
  if (!kpis.value || !kpis.value.revenueTrend || kpis.value.revenueTrend.length === 0) return 1
  const values = kpis.value.revenueTrend.map(p => p.value || 0)
  return Math.max(...values, 1)
})

const formatCurrency = (val) => {
  if (val === undefined || val === null || isNaN(val)) return '0 ₫'
  if (val >= 1000000000) return (val / 1000000000).toFixed(2) + ' tỷ'
  if (val >= 1000000) return (val / 1000000).toFixed(1) + ' tr'
  return val.toLocaleString() + ' ₫'
}

const formatFullCurrency = (val) => {
  if (val === undefined || val === null || isNaN(val)) return '0 VNĐ'
  return val.toLocaleString() + ' VNĐ'
}

const calculatePercentage = (part, total) => {
  if (!total || total === 0) return '0.0'
  return ((part / total) * 100).toFixed(1)
}

// FETCH LOGIC
const fetchReportData = async () => {
  const params = { startDate: filters.startDate, endDate: filters.endDate }
  
  // Tải KPI tổng quát (Dành cho tất cả Manager)
  try {
    const res = await apiClient.get('/api/Reports/dashboard-kpis', { params })
    if (res.data) kpis.value = res.data
  } catch (err) {
    console.error("Lỗi KPI:", err)
    toast.error(parseApiError(err))
    // Giữ nguyên giá trị mặc định đã init
  }

  // Tải báo cáo Vận hành
  try {
    const endDt = new Date(filters.endDate);
    const resOps = await apiClient.get('/api/Reports/operational-summary', { 
        params: { year: endDt.getFullYear(), month: endDt.getMonth() + 1 } 
    })
    if (resOps.data) operationalSummary.value = resOps.data
  } catch (err) {
    console.error("Lỗi lấy Operational Summary:", err)
  }

  // Tải báo cáo Tài chính (Chỉ Admin/Payroll/Contract)
  try {
    const res = await apiClient.get('/api/Reports/financial', { params })
    if (res.data) financialReport.value = res.data
  } catch (err) {
    console.warn("Hạn chế quyền truy cập Tài chính hoặc lỗi server:", err.response?.status)
    toast.error(parseApiError(err))
    financialReport.value = { revenue: 0, staffCost: 0, supplyCost: 0, margin: 0, topContracts: [] }
  }

  // Tải hiệu suất nhân sự (Chỉ Admin/Personnel)
  try {
    const res = await apiClient.get('/api/Reports/staff-efficiency', { params })
    if (res.data) staffEfficiency.value = res.data
  } catch (err) {
    console.warn("Hạn chế quyền truy cập Nhân sự:", err.response?.status)
    toast.error(parseApiError(err))
    staffEfficiency.value = []
  }

  // Tải cảnh báo hệ thống (Chỉ Admin/Manager)
  /* Removed Inventory Alerts call */
}

const handleExport = async (type) => {
  exporting.value = true
  exportProgress.value = 0
  
  // Hiệu ứng progress giả mượt mà trong khi chờ API
  const progressInterval = setInterval(() => {
    if (exportProgress.value < 90) {
      exportProgress.value += Math.floor(Math.random() * 10) + 2
    }
  }, 300)

  try {
    const endpoint = type === 'PDF' ? '/api/Reports/export-pdf' : '/api/Reports/export-excel'
    const response = await apiClient.get(endpoint, {
      params: { 
        startDate: filters.startDate, 
        endDate: filters.endDate 
      },
      responseType: 'blob'
    })

    // Xử lý hoàn tất progress
    clearInterval(progressInterval)
    exportProgress.value = 100

    // Tạo link download
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    const extension = type === 'PDF' ? 'pdf' : 'xlsx'
    link.setAttribute('download', `BaoCao_HeThong_${new Date().toISOString().split('T')[0]}.${extension}`)
    document.body.appendChild(link)
    link.click()
    
    // Cleanup
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    setTimeout(() => {
      exporting.value = false
      toast.show(`Xuất báo cáo ${type} thành công!`, 'success')
    }, 500)

  } catch (err) {
    clearInterval(progressInterval)
    exporting.value = false
    console.error("Lỗi xuất báo cáo:", err)
    toast.error("Không thể xuất báo cáo. Vui lòng thử lại sau.")
  }
}

onMounted(() => {
  fetchReportData()
})
</script>

<style scoped>
@reference "tailwindcss";

.scrollbar-premium::-webkit-scrollbar {
  width: 6px;
}
.scrollbar-premium::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-premium::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.scrollbar-premium::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

@keyframes scaleUp {
  from { transform: scale(0.9); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.animate-scale-up {
  animation: scaleUp 0.4s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
}

.animate-bounce-slow {
  animation: bounce 3s infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(-5%); animation-timing-function: cubic-bezier(0.8,0,1,1); }
  50% { transform: none; animation-timing-function: cubic-bezier(0,0,0.2,1); }
}
</style>
