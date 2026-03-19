<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in-up">
    <!-- Header Mềm mại bo tròn -->
    <div class="flex-shrink-0 mb-8 p-10 bg-white rounded-[3rem] shadow-sm border-2 border-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-black text-slate-800 tracking-tight leading-none mb-2">{{ i18n.locale === 'vi' ? 'Báo Cáo Thống Kê' : 'Analytics & Reports' }}</h2>
        <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">{{ i18n.locale === 'vi' ? 'TỔNG QUAN HIỆU SUẤT VÀ DOANH THU' : 'OVERVIEW OF PERFORMANCE AND REVENUE' }}</p>
      </div>
      <button @click="fetchStats" class="px-6 py-4 bg-primary/10 text-primary rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-primary/20 transition-all flex items-center gap-2">
        <RefreshCw :class="{'animate-spin': isRefreshing}" class="w-4 h-4" />
        {{ i18n.locale === 'vi' ? 'Cập Nhật' : 'Refresh' }}
      </button>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-10">
        <!-- Revenue Card -->
        <div class="bg-indigo-600 rounded-[3rem] p-6 lg:p-8 text-white relative overflow-hidden shadow-2xl shadow-indigo-200 group flex flex-col justify-between min-h-[16rem]">
            <div class="absolute -right-4 -bottom-4 w-32 h-32 bg-white/10 rounded-full blur-2xl group-hover:bg-white/20 transition-all duration-500"></div>
            <div class="w-16 h-16 bg-white/20 rounded-3xl flex items-center justify-center mb-4">
                <CircleDollarSign class="w-8 h-8 text-white" />
            </div>
            <div>
                <p class="text-[10px] sm:text-[11px] font-black uppercase tracking-wider text-indigo-200 mb-2 leading-tight whitespace-nowrap">Tổng Doanh Thu (Dự kiến)</p>
                <h3 class="text-2xl sm:text-3xl lg:text-4xl font-black break-words leading-none">{{ formatCurrency(totalRevenue) }}</h3>
            </div>
        </div>

        <!-- Contracts Card -->
        <div class="bg-white rounded-[3rem] p-8 border-2 border-slate-50 relative overflow-hidden premium-card group">
            <div class="w-16 h-16 bg-teal-50 text-teal-600 rounded-3xl flex items-center justify-center mb-6 group-hover:-translate-y-2 transition-transform duration-500">
                <FileCheck2 class="w-8 h-8" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Hợp Đồng Khám</p>
            <h3 class="text-3xl font-black text-slate-800">{{ totalContracts }} <span class="text-sm text-slate-300 font-bold ml-1">bản</span></h3>
        </div>

        <!-- Groups Card -->
        <div class="bg-white rounded-[3rem] p-8 border-2 border-slate-50 relative overflow-hidden premium-card group">
            <div class="w-16 h-16 bg-sky-50 text-sky-600 rounded-3xl flex items-center justify-center mb-6 group-hover:-translate-y-2 transition-transform duration-500">
                <Stethoscope class="w-8 h-8" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Đoàn Đã Tổ Chức</p>
            <h3 class="text-3xl font-black text-slate-800">{{ totalGroups }} <span class="text-sm text-slate-300 font-bold ml-1">đoàn</span></h3>
        </div>

        <!-- Patients Card -->
        <div class="bg-white rounded-[3rem] p-8 border-2 border-slate-50 relative overflow-hidden premium-card group">
            <div class="w-16 h-16 bg-rose-50 text-rose-600 rounded-3xl flex items-center justify-center mb-6 group-hover:-translate-y-2 transition-transform duration-500">
                <Users2 class="w-8 h-8" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Lượt Người Khám</p>
            <h3 class="text-3xl font-black text-slate-800">{{ totalPatients }} <span class="text-sm text-slate-300 font-bold ml-1">lượt</span></h3>
        </div>
    </div>

    <!-- Detailed Charts Section -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 flex-1">
        <!-- Recent Contracts Overview -->
        <div class="bg-white rounded-[3rem] border-2 border-slate-50 p-10 shadow-sm flex flex-col premium-card">
            <div class="flex justify-between items-center mb-8">
                <h3 class="font-black text-lg text-slate-800">Tình Trạng Hợp Đồng</h3>
                <div class="p-3 bg-slate-50 rounded-2xl">
                    <PieChart class="w-5 h-5 text-slate-400" />
                </div>
            </div>
            <div class="flex-1 flex flex-col justify-center">
                <div class="space-y-6">
                    <div>
                        <div class="flex justify-between text-xs font-black uppercase tracking-widest mb-3">
                            <span class="text-slate-500">Đã Hoàn Thành (Finished)</span>
                            <span class="text-emerald-600">{{ contractStats.finished }}</span>
                        </div>
                        <div class="w-full bg-slate-100 h-3 rounded-full overflow-hidden">
                            <div class="bg-emerald-500 h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.finished / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                    <div>
                        <div class="flex justify-between text-xs font-black uppercase tracking-widest mb-3">
                            <span class="text-slate-500">Đang Tiến Hành (Active)</span>
                            <span class="text-primary">{{ contractStats.active }}</span>
                        </div>
                        <div class="w-full bg-slate-100 h-3 rounded-full overflow-hidden">
                            <div class="bg-primary h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.active / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                    <div>
                        <div class="flex justify-between text-xs font-black uppercase tracking-widest mb-3">
                            <span class="text-slate-500">Chờ Xử Lý (Pending)</span>
                            <span class="text-amber-500">{{ contractStats.pending }}</span>
                        </div>
                        <div class="w-full bg-slate-100 h-3 rounded-full overflow-hidden">
                            <div class="bg-amber-400 h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.pending / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Inventory Warning -->
        <div class="bg-white rounded-[3rem] border-2 border-slate-50 p-10 shadow-sm flex flex-col premium-card">
            <div class="flex justify-between items-center mb-6">
                <h3 class="font-black text-lg text-slate-800">Cảnh Báo Vật Tư (Sắp Hết)</h3>
                <div class="p-3 bg-rose-50 text-rose-500 rounded-2xl">
                    <AlertTriangle class="w-5 h-5" />
                </div>
            </div>
            <div class="flex-1 overflow-y-auto custom-scrollbar pr-4">
                <div v-if="lowSupplies.length === 0" class="h-full flex flex-col items-center justify-center text-center opacity-50">
                    <ShieldCheck class="w-16 h-16 text-slate-300 mb-4" />
                    <p class="text-xs font-black uppercase tracking-widest text-slate-400">Kho hàng an toàn, không có cảnh báo</p>
                </div>
                <div v-else class="space-y-4">
                    <div v-for="item in lowSupplies" :key="item.supplyId" class="p-5 bg-rose-50/50 rounded-2xl border border-rose-100 flex justify-between items-center group">
                        <div class="flex items-center gap-4">
                            <div class="w-12 h-12 bg-white rounded-xl shadow-sm text-rose-500 flex items-center justify-center font-black">
                                <PackageX class="w-6 h-6" />
                            </div>
                            <div>
                                <h4 class="font-black text-slate-800">{{ item.supplyName }}</h4>
                                <p class="text-[9px] font-black uppercase tracking-widest text-slate-500 mt-1">Lô: {{ item.lotNumber || 'N/A' }} • Nhóm: {{ item.category }}</p>
                            </div>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-black text-rose-600">{{ item.totalStock }} <span class="text-[9px] text-rose-400">{{ item.unit }}</span></p>
                            <p class="text-[9px] font-black uppercase tracking-widest text-slate-400 mt-1">Tối thiểu: {{ item.minStockLevel }}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useI18nStore } from '../stores/i18n'
import axios from 'axios'
import { 
    RefreshCw, CircleDollarSign, FileCheck2, Stethoscope, 
    Users2, PieChart, AlertTriangle, ShieldCheck, PackageX
} from 'lucide-vue-next'

const i18n = useI18nStore()
const isRefreshing = ref(false)

const totalRevenue = ref(0)
const totalContracts = ref(0)
const totalGroups = ref(0)
const totalPatients = ref(0)

const contractStats = ref({
    active: 0,
    finished: 0,
    pending: 0
})

const lowSupplies = ref([])

const formatCurrency = (value) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}

const fetchStats = async () => {
    isRefreshing.value = true
    try {
        // Fetch Contracts for Revenue and Stats
        const reqContracts = axios.get('http://localhost:5283/api/HealthContracts')
        const reqGroups = axios.get('http://localhost:5283/api/MedicalGroups')
        const reqSupplies = axios.get('http://localhost:5283/api/Supplies')

        const [resContracts, resGroups, resSupplies] = await Promise.all([reqContracts, reqGroups, reqSupplies])

        // Process Contracts
        const contracts = resContracts.data || []
        totalContracts.value = contracts.length
        
        totalRevenue.value = contracts.reduce((sum, c) => sum + (c.totalAmount || 0), 0)
        totalPatients.value = contracts.reduce((sum, c) => sum + (c.expectedQuantity || 0), 0)
        
        contractStats.value = {
            active: contracts.filter(c => c.status === 'Active').length,
            finished: contracts.filter(c => c.status === 'Finished').length,
            pending: contracts.filter(c => c.status === 'Pending').length
        }

        // Process Groups
        totalGroups.value = resGroups.data ? resGroups.data.length : 0

        // Process Supplies
        const supplies = resSupplies.data || []
        lowSupplies.value = supplies.filter(s => s.totalStock <= s.minStockLevel).sort((a,b) => a.totalStock - b.totalStock)

    } catch (error) {
        console.error("Lỗi khi lấy dữ liệu thống kê:", error)
        alert("Lỗi kết nối khi lấy dữ liệu thống kê!")
    } finally {
        isRefreshing.value = false
    }
}

onMounted(() => {
    fetchStats()
})
</script>
