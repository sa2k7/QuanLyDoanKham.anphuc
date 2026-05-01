<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in-up p-3 overflow-y-auto">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3 mb-4">
      <div>
        <h2 class="text-xl font-bold text-slate-800 flex items-center gap-2">
          <div class="w-9 h-9 bg-primary text-white rounded-xl flex items-center justify-center shadow-md">
            <BarChart3 class="w-5 h-5" />
          </div>
          {{ i18n.locale === 'vi' ? 'Báo Cáo Thống Kê' : 'Analytics & Reports' }}
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[8px] mt-1">{{ i18n.locale === 'vi' ? 'TỔNG QUAN HIỆU SUẤT VÀ DOANH THU' : 'OVERVIEW OF PERFORMANCE AND REVENUE' }}</p>
      </div>
      
      <div class="flex items-center gap-2">
        <div class="flex items-center gap-1.5 bg-white px-2.5 py-1.5 rounded-xl border border-slate-200 shadow-sm">
            <input type="date" v-model="startDate" class="bg-transparent border-none text-[10px] font-black text-slate-600 outline-none" />
            <span class="text-slate-300">→</span>
            <input type="date" v-model="endDate" class="bg-transparent border-none text-[10px] font-black text-slate-600 outline-none" />
        </div>
        <button @click="fetchStats" class="h-8.5 px-4 bg-primary/10 text-primary rounded-xl font-black text-[10px] uppercase tracking-widest hover:bg-primary/20 transition-all flex items-center gap-1.5">
            <RefreshCw :class="{'animate-spin': isRefreshing}" class="w-3.5 h-3.5" />
            {{ i18n.locale === 'vi' ? 'Cập Nhật' : 'Refresh' }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-4 gap-3 mb-4">
        <!-- Revenue Card (Estimated) -->
        <div class="bg-gradient-to-br from-indigo-600 to-indigo-800 rounded-2xl p-4 text-white relative overflow-hidden shadow-lg shadow-indigo-100 group flex flex-col justify-between">
            <div class="absolute -right-4 -bottom-4 w-16 h-16 bg-white/10 rounded-full blur-xl group-hover:bg-white/20 transition-all duration-500"></div>
            <div class="w-9 h-9 bg-white/20 rounded-xl flex items-center justify-center mb-2">
                <CircleDollarSign class="w-5 h-5 text-white" />
            </div>
            <div>
                <p class="text-[8px] font-black uppercase tracking-wider text-indigo-200 mb-0.5 leading-tight">{{ i18n.locale === 'vi' ? 'Tổng Doanh Thu' : 'Total Revenue' }}</p>
                <h3 class="text-lg font-black break-words leading-none">{{ formatCurrency(totalRevenue) }}</h3>
            </div>
        </div>

        <!-- Revenue Card (Actual) -->
        <div class="bg-gradient-to-br from-emerald-600 to-teal-800 rounded-2xl p-4 text-white relative overflow-hidden shadow-lg shadow-emerald-100 group flex flex-col justify-between">
            <div class="absolute -right-4 -bottom-4 w-16 h-16 bg-white/10 rounded-full blur-xl group-hover:bg-white/20 transition-all duration-500"></div>
            <div class="w-9 h-9 bg-white/20 rounded-xl flex items-center justify-center mb-2">
                <TrendingUp class="w-5 h-5 text-white" />
            </div>
            <div>
                <p class="text-[8px] font-black uppercase tracking-wider text-emerald-200 mb-0.5 leading-tight">{{ i18n.locale === 'vi' ? 'Lợi Nhuận Gộp' : 'Gross Profit' }}</p>
                <h3 class="text-lg font-black break-words leading-none">{{ formatCurrency(actualRevenue) }}</h3>
            </div>
        </div>

        <!-- Contracts Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-teal-50 text-teal-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <FileCheck2 class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Hợp Đồng' : 'Contracts' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalContracts }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">mục</span></h3>
        </div>

        <!-- Groups Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-sky-50 text-sky-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <Stethoscope class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Đoàn Khám' : 'Groups' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalGroups }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">đoàn</span></h3>
        </div>

        <!-- Departments Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-emerald-50 text-emerald-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <Layers class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Trạm Khám' : 'Departments' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalDepartments }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">trạm</span></h3>
        </div>

        <!-- Patients Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-indigo-50 text-indigo-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <Users2 class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Bệnh Nhân' : 'Patients' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalPatients }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">hồ sơ</span></h3>
        </div>

        <!-- Staff Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-emerald-50 text-emerald-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <ShieldCheck class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Nhân Sự' : 'Staff' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalStaff }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">người</span></h3>
        </div>

        <!-- Supplies Card -->
        <div class="bg-white rounded-2xl p-4 border border-slate-100 relative overflow-hidden premium-card group">
            <div class="w-9 h-9 bg-amber-50 text-amber-600 rounded-xl flex items-center justify-center mb-2 group-hover:-translate-y-1 transition-transform duration-500">
                <Box class="w-5 h-5" />
            </div>
            <p class="text-[8px] font-black uppercase tracking-widest text-slate-400 mb-0.5">{{ i18n.locale === 'vi' ? 'Vật Tư' : 'Supplies' }}</p>
            <h3 class="text-xl font-black text-slate-800">{{ totalSupplies }} <span class="text-[9px] text-slate-300 font-bold ml-0.5">mục</span></h3>
        </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 flex-1 overflow-hidden">
        <!-- Recent Contracts Overview -->
        <div class="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm flex flex-col premium-card overflow-hidden">
            <div class="flex justify-between items-center mb-4">
                <h3 class="font-black text-xs text-slate-800 uppercase tracking-widest">Tình Trạng Hợp Đồng</h3>
                <div class="p-1.5 bg-slate-50 rounded-lg">
                    <PieChart class="w-4 h-4 text-slate-400" />
                </div>
            </div>
            <div class="flex-1 flex flex-col justify-center overflow-y-auto">
                <div class="space-y-4">
                    <div>
                        <div class="flex justify-between text-[8px] font-black uppercase tracking-widest mb-1.5">
                            <span class="text-slate-500">Đã Hoàn Thành (Finished)</span>
                            <span class="text-emerald-600">{{ contractStats.finished }}</span>
                        </div>
                        <div class="w-full bg-slate-50 h-2 rounded-full overflow-hidden">
                            <div class="bg-emerald-500 h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.finished / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                    <div>
                        <div class="flex justify-between text-[8px] font-black uppercase tracking-widest mb-1.5">
                            <span class="text-slate-500">Đang Tiến Hành (Active)</span>
                            <span class="text-primary">{{ contractStats.active }}</span>
                        </div>
                        <div class="w-full bg-slate-50 h-2 rounded-full overflow-hidden">
                            <div class="bg-primary h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.active / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                    <div>
                        <div class="flex justify-between text-[8px] font-black uppercase tracking-widest mb-1.5">
                            <span class="text-slate-500">Chờ Xử Lý (Pending)</span>
                            <span class="text-amber-500">{{ contractStats.pending }}</span>
                        </div>
                        <div class="w-full bg-slate-50 h-2 rounded-full overflow-hidden">
                            <div class="bg-amber-400 h-full rounded-full transition-all duration-1000" :style="`width: ${(contractStats.pending / totalContracts) * 100 || 0}%`"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Inventory Warning -->
        <div class="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm flex flex-col premium-card overflow-hidden">
            <div class="flex justify-between items-center mb-4">
                <h3 class="font-black text-xs text-slate-800 uppercase tracking-widest">Cảnh Báo Vật Tư</h3>
                <div class="p-1.5 bg-rose-50 text-rose-500 rounded-lg">
                    <AlertTriangle class="w-4 h-4" />
                </div>
            </div>
            <div class="flex-1 overflow-y-auto custom-scrollbar pr-2">
                <div v-if="lowSupplies.length === 0" class="h-full flex flex-col items-center justify-center text-center opacity-50">
                    <ShieldCheck class="w-10 h-10 text-slate-300 mb-2" />
                    <p class="text-[8px] font-black uppercase tracking-widest text-slate-400">Kho hàng an toàn</p>
                </div>
                <div v-else class="space-y-2">
                    <div v-for="item in lowSupplies" :key="item.supplyId" class="p-2.5 bg-rose-50/50 rounded-xl border border-rose-100 flex justify-between items-center group">
                        <div class="flex items-center gap-3">
                            <div class="w-8 h-8 bg-white rounded-lg shadow-sm text-rose-500 flex items-center justify-center font-black">
                                <PackageX class="w-4 h-4" />
                            </div>
                            <div>
                                <h4 class="font-black text-[10px] text-slate-800 leading-tight">{{ item.supplyName }}</h4>
                                <p class="text-[7px] font-black uppercase tracking-widest text-slate-500 mt-0.5">Lô: {{ item.lotNumber || 'N/A' }} • Nhóm: {{ item.category }}</p>
                            </div>
                        </div>
                        <div class="text-right">
                            <p class="text-[10px] font-black text-rose-600">{{ item.totalStock }} <span class="text-[7px] text-rose-400">{{ item.unit }}</span></p>
                            <p class="text-[7px] font-black uppercase tracking-widest text-slate-400 mt-0.5">Tối thiểu: {{ item.minStockLevel }}</p>
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
import apiClient from '../services/apiClient'
import { 
    RefreshCw, CircleDollarSign, FileCheck2, Stethoscope, 
    Users2, PieChart, AlertTriangle, ShieldCheck, PackageX,
    Layers, Box, TrendingUp
} from 'lucide-vue-next'
import { parseApiError } from '../services/errorHelper'
import { useToast } from '../composables/useToast'

const i18n = useI18nStore()
const toast = useToast()
const isRefreshing = ref(false)

const startDate = ref(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0])
const endDate = ref(new Date().toISOString().split('T')[0])

const totalRevenue = ref(0)
const actualRevenue = ref(0)
const totalContracts = ref(0)
const totalGroups = ref(0)
const totalDepartments = ref(0)
const totalStaff = ref(0)
const totalPatients = ref(0)
const totalSupplies = ref(0)

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
        const params = { startDate: startDate.value, endDate: endDate.value }
        
        const reqContracts = apiClient.get('/api/Contracts')
        const reqGroups = apiClient.get('/api/MedicalGroups')
        const reqSupplies = apiClient.get('/api/Supplies')
        const reqStaff = apiClient.get('/api/Staffs')
        const reqPatients = apiClient.get('/api/Patients')
        const reqFinancial = apiClient.get('/api/Reports/financial', { params })

        const [resContracts, resGroups, resSupplies, resStaff, resPatients, resFinancial] = await Promise.all([
            reqContracts, reqGroups, reqSupplies, reqStaff, reqPatients, reqFinancial
        ])

        // Process Contracts
        const contracts = resContracts.data || []
        totalContracts.value = contracts.length
        
        totalRevenue.value = contracts.reduce((sum, c) => sum + (c.totalAmount || 0), 0)
        
        contractStats.value = {
            active: contracts.filter(c => c.status === 'Active').length,
            finished: contracts.filter(c => c.status === 'Finished').length,
            pending: contracts.filter(c => c.status === 'Pending').length
        }

        // Process Groups
        totalGroups.value = resGroups.data ? resGroups.data.length : 0

        // Process Staff
        const staff = resStaff.data || []
        totalStaff.value = staff.length

        // Process Patients
        const patients = Array.isArray(resPatients.data)
            ? resPatients.data
            : (resPatients.data?.items || resPatients.data?.data || [])
        totalPatients.value = patients.length
        totalDepartments.value = new Set([
            ...staff.map(s => s.departmentName).filter(Boolean),
            ...patients.map(p => p.department).filter(Boolean)
        ]).size

        // Process Financial (Gross Profit)
        actualRevenue.value = resFinancial.data?.totalGrossProfit || 0

        // Process Supplies
        const supplies = resSupplies.data || []
        totalSupplies.value = supplies.length
        lowSupplies.value = supplies.filter(s => s.totalStock <= s.minStockLevel).sort((a,b) => a.totalStock - b.totalStock)

    } catch (error) {
        console.error("Lỗi khi lấy dữ liệu thống kê:", error)
        toast.error(parseApiError(error))
    } finally {
        isRefreshing.value = false
    }
}

onMounted(() => {
    fetchStats()
})
</script>
