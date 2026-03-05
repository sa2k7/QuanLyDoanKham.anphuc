<template>
  <div class="space-y-10 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-primary text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-primary/20">
            <BarChart3 class="w-8 h-8" />
          </div>
          Phân tích Dữ liệu
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">Kinh doanh & Hiệu quả vận hành hợp nhất</p>
      </div>
      <button @click="exportReport" 
              class="btn-premium bg-slate-900 text-white hover:bg-black shadow-2xl shadow-slate-200 py-4 px-10">
        <Download class="w-6 h-6" />
        <span>XUẤT DỮ LIỆU EXCEL</span>
      </button>
    </div>

    <!-- Dashboard Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
      <div v-for="stat in statCards" :key="stat.label" 
           class="premium-card p-8 bg-white border-2 border-slate-50 group hover:border-primary/20 transition-all">
          <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4 group-hover:text-primary transition-colors">{{ stat.label }}</p>
          <div class="flex items-end gap-2">
              <span class="text-4xl font-black text-slate-900 tracking-tighter leading-none">{{ stat.value || 0 }}</span>
              <div v-if="stat.icon" class="mb-1 ml-auto text-slate-100 group-hover:text-primary/10 transition-colors">
                  <component :is="stat.icon" class="w-12 h-12" />
              </div>
          </div>
      </div>
    </div>

    <!-- Revenue vs Profit Area -->
    <div class="bg-slate-900 p-12 rounded-[3.5rem] text-white shadow-2xl shadow-slate-200 relative overflow-hidden mb-12">
        <div class="absolute right-0 top-0 bottom-0 w-1/3 bg-gradient-to-l from-primary/10 to-transparent"></div>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-16 relative z-10">
            <div class="space-y-8">
                <div>
                   <span class="px-4 py-2 bg-primary/20 text-primary text-[10px] font-black uppercase tracking-widest rounded-full border border-primary/20">Báo cáo tài chính</span>
                   <h3 class="text-5xl font-black tracking-tighter text-white mt-6 leading-tight">Chỉ số Hiệu quả<br/>Quý I / 2026</h3>
                </div>
                <p class="text-slate-400 font-bold leading-relaxed max-w-sm">Dữ liệu được tổng hợp từ doanh thu hợp đồng và chi phí vận hành (nhân sự/vật tư) theo thời gian thực.</p>
                <div class="flex items-center gap-10 pt-4">
                    <div class="flex flex-col">
                        <span class="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Phương pháp tính</span>
                        <span class="text-sm font-bold text-slate-200 italic">Accrual Accounting</span>
                    </div>
                </div>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div class="bg-white/5 border border-white/10 p-8 rounded-[2rem] backdrop-blur-md">
                    <p class="text-slate-500 text-[10px] font-black uppercase tracking-widest mb-4">Tổng doanh thu</p>
                    <p class="text-3xl font-black tracking-tighter text-white leading-none">{{ formatPrice(stats.totalRevenue) }}</p>
                </div>
                <div class="bg-white/5 border border-white/10 p-8 rounded-[2rem] backdrop-blur-md">
                    <p class="text-slate-500 text-[10px] font-black uppercase tracking-widest mb-4">Chi phí vận hành</p>
                    <p class="text-3xl font-black tracking-tighter text-white leading-none text-rose-400">{{ formatPrice(stats.totalSalaryCost + stats.totalSupplyCost) }}</p>
                </div>
                <div class="bg-indigo-600 p-10 rounded-[2.5rem] col-span-1 md:col-span-2 shadow-2xl shadow-indigo-500/20 flex flex-col justify-center">
                    <p class="text-indigo-200 text-[10px] font-black uppercase tracking-widest mb-4">Lợi nhuận ròng (EBITDA)</p>
                    <p class="text-6xl font-black tracking-tighter text-white leading-none mb-2">{{ formatPrice(stats.netProfit) }}</p>
                    <div class="flex items-center gap-2 mt-4">
                        <TrendingUp class="w-4 h-4 text-emerald-400" />
                        <span class="text-emerald-400 font-black text-xs">+12.4% so với kỳ trước</span>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-20">
        <!-- Top Supplies Table -->
        <div class="lg:col-span-1 premium-card p-10 bg-white border-2 border-slate-50">
            <h3 class="text-xl font-black mb-10 flex items-center gap-4 text-slate-800 tracking-tight">
                <div class="w-10 h-10 bg-slate-50 text-indigo-500 rounded-xl flex items-center justify-center">
                   <Package class="w-6 h-6" />
                </div>
                Tiêu thụ vật tư
            </h3>
            <div class="space-y-8">
                <div v-for="(s, idx) in stats.topSupplies" :key="s.name" class="flex items-center justify-between group">
                    <div class="flex items-center gap-4">
                        <span class="w-8 h-8 flex items-center justify-center bg-slate-50 rounded-xl text-[10px] font-black text-slate-400 group-hover:bg-primary group-hover:text-white transition-all">{{ idx + 1 }}</span>
                        <div>
                            <p class="font-black text-slate-800 text-sm group-hover:text-primary transition-colors">{{ s.name }}</p>
                            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mt-1">Xuất: {{ s.qty }} đơn vị</p>
                        </div>
                    </div>
                    <div class="text-right">
                        <p class="font-black text-slate-900 text-sm italic">{{ formatPrice(s.cost) }}</p>
                    </div>
                </div>
                <div v-if="!stats.topSupplies || stats.topSupplies.length === 0" class="text-center py-20 text-slate-300 font-black uppercase tracking-widest text-xs">Chưa có dữ liệu</div>
            </div>
        </div>

        <!-- Group Performance Table -->
        <div class="lg:col-span-2 premium-card bg-white border-2 border-slate-50 overflow-hidden">
            <div class="p-10 border-b border-slate-50 flex items-center justify-between">
                <h3 class="text-xl font-black flex items-center gap-4 text-slate-800 tracking-tight">
                    <div class="w-10 h-10 bg-slate-50 text-primary rounded-xl flex items-center justify-center">
                       <Stethoscope class="w-6 h-6" />
                    </div>
                    Vận hành Đoàn khám
                </h3>
                <span class="px-4 py-2 bg-slate-50 rounded-full text-[10px] font-black text-slate-400 uppercase tracking-widest">Thời gian thực</span>
            </div>
            <div class="overflow-x-auto">
                <table class="professional-table">
                    <thead>
                        <tr>
                            <th class="w-16">#</th>
                            <th>Đoàn khám / Công ty đối tác</th>
                            <th class="text-center">Điều phối</th>
                            <th class="text-right">Chi phí triển khai</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(g, idx) in stats.groupStats" :key="g.groupName" class="hover:bg-slate-50/50 transition-all">
                            <td>
                                <span class="text-slate-300 font-black text-xs">{{ (idx + 1).toString().padStart(2, '0') }}</span>
                            </td>
                            <td>
                                <p class="font-black text-slate-800">{{ g.groupName }}</p>
                                <p class="text-[10px] font-bold text-primary uppercase tracking-widest mt-0.5">{{ g.companyName }}</p>
                            </td>
                            <td class="text-center">
                                <span class="inline-flex items-center gap-1 font-black text-slate-700 bg-slate-100 px-3 py-1.5 rounded-xl text-xs">
                                   <UsersIcon class="w-3 h-3 text-slate-400" /> {{ g.staffCount }}
                                </span>
                            </td>
                            <td class="text-right font-black text-rose-500 tracking-tighter">
                                {{ formatPrice(g.totalCost) }}
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div v-if="!stats.groupStats || stats.groupStats.length === 0" class="text-center py-32 flex flex-col items-center gap-6">
                    <div class="w-20 h-20 bg-slate-50 rounded-full flex items-center justify-center text-slate-100">
                        <BarChart3 class="w-10 h-10" />
                    </div>
                    <p class="text-xl font-black text-slate-300 uppercase tracking-widest">Chưa có dữ liệu vận hành</p>
                </div>
            </div>
        </div>

        <!-- Row 3: Insights -->
        <div class="premium-card p-10 bg-indigo-600 text-white relative overflow-hidden">
            <div class="absolute right-0 bottom-0 opacity-10">
                <Trophy class="w-40 h-40 -rotate-12" />
            </div>
            <h3 class="text-xl font-black mb-10 flex items-center gap-4 relative z-10 tracking-tight">
                <div class="w-10 h-10 bg-white/20 text-white rounded-xl flex items-center justify-center">
                   <Trophy class="w-6 h-6" />
                </div>
                Nhân sự tiêu biểu
            </h3>
            <div class="space-y-8 relative z-10">
                <div v-for="(s, idx) in stats.topStaff" :key="s.name" class="flex items-center gap-5 group">
                    <div class="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center font-black text-xs text-indigo-200 group-hover:bg-white group-hover:text-indigo-600 transition-all">{{ idx + 1 }}</div>
                    <div>
                        <p class="font-black text-sm group-hover:translate-x-1 transition-transform">{{ s.name }}</p>
                        <p class="text-[9px] font-black text-indigo-300 uppercase tracking-widest mt-1">{{ s.sessions }} ĐOÀN ĐÃ TRIỂN KHAI</p>
                    </div>
                </div>
            </div>
        </div>

        <div class="premium-card p-10 bg-white border-2 border-slate-50 lg:col-span-2 relative overflow-hidden">
            <div v-if="stats.lowStock.length > 0" class="absolute top-0 right-0 p-3 px-8 bg-rose-500 text-white text-[9px] font-black uppercase tracking-[0.2em] rounded-bl-3xl animate-pulse shadow-lg z-10">
                Cảnh báo kho hàng
            </div>
            <h3 class="text-xl font-black mb-10 flex items-center gap-4 text-slate-800 tracking-tight">
                <div class="w-10 h-10 bg-slate-50 text-rose-500 rounded-xl flex items-center justify-center">
                   <AlertCircle class="w-6 h-6" />
                </div>
                Vật tư sắp hết hạn mức (&lt; 20)
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-6">
                <div v-for="s in stats.lowStock" :key="s.supplyName" 
                     class="flex justify-between items-center bg-rose-50/20 p-5 rounded-[2rem] border-2 border-rose-100/30 group hover:border-rose-500/20 transition-all">
                    <span class="font-black text-slate-700 text-sm group-hover:text-rose-600 transition-colors">{{ s.supplyName }}</span>
                    <span class="font-black text-rose-600 bg-white px-4 py-2 rounded-2xl border-2 border-rose-50 shadow-sm">{{ s.quantityInStock }}</span>
                </div>
                <div v-if="stats.lowStock.length === 0" class="col-span-2 text-center py-20 flex flex-col items-center gap-4">
                    <ShieldCheck class="w-12 h-12 text-emerald-100" />
                    <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Kho hàng đang ở trạng thái an toàn</p>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { 
    FileText, Stethoscope, Users, Package, BarChart3, TrendingUp, Download, Trophy, AlertCircle, ShieldCheck
} from 'lucide-vue-next'

const stats = ref({
    totalContracts: 0,
    totalRevenue: 0,
    totalSalaryCost: 0,
    totalSupplyCost: 0,
    netProfit: 0,
    activeGroups: 0,
    staffParticipating: 0,
    suppliesExported: 0,
    topSupplies: [],
    groupStats: [],
    lowStock: [],
    topStaff: []
})

const fetchStats = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Reports/dashboard')
        // Defensive mapping to handle both camelCase and PascalCase
        const d = res.data
        stats.value = {
            totalContracts: d.totalContracts ?? d.TotalContracts ?? 0,
            totalRevenue: d.totalRevenue ?? d.TotalRevenue ?? 0,
            totalSalaryCost: d.totalSalaryCost ?? d.TotalSalaryCost ?? 0,
            totalSupplyCost: d.totalSupplyCost ?? d.TotalSupplyCost ?? 0,
            netProfit: d.netProfit ?? d.NetProfit ?? 0,
            activeGroups: d.activeGroups ?? d.ActiveGroups ?? 0,
            staffParticipating: d.staffParticipating ?? d.StaffParticipating ?? 0,
            suppliesExported: d.suppliesExported ?? d.SuppliesExported ?? 0,
            topSupplies: d.topSupplies ?? d.TopSupplies ?? [],
            groupStats: d.groupStats ?? d.GroupStats ?? [],
            lowStock: d.lowStock ?? d.LowStock ?? [],
            topStaff: d.topStaff ?? d.TopStaff ?? []
        }
    } catch (e) {
        console.error("Failed to fetch report stats", e)
    }
}

const statCards = computed(() => [
    { label: 'Tổng Hợp đồng', value: stats.value.totalContracts, icon: FileText, colorClass: 'bg-cyan-500' },
    { label: 'Đoàn đang vận hành', value: stats.value.activeGroups, icon: Stethoscope, colorClass: 'bg-indigo-500' },
    { label: 'Nhân sự tham gia', value: stats.value.staffParticipating, icon: Users, colorClass: 'bg-emerald-500' },
    { label: 'Vật phẩm đã định biên', value: stats.value.suppliesExported, icon: Package, colorClass: 'bg-primary' }
])

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p || 0)

const exportReport = async () => {
    try {
        const response = await axios.get('http://localhost:5283/api/Reports/export-dashboard', {
            responseType: 'blob'
        })
        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'BaoCao_TongQuan.xlsx')
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
    } catch (e) {
        console.error("Export failed", e)
    }
}

onMounted(fetchStats)
</script>
