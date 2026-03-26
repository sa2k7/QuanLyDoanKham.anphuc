<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in-up pb-12 pr-4">
    <!-- Page Header (In-Module) -->
    <div class="flex-shrink-0 mb-8 p-10 bg-white rounded-[3rem] shadow-sm border-2 border-slate-50 flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-black text-slate-800 tracking-tight leading-none mb-2">Báo Cáo Phân Tích</h2>
        <p class="text-xs font-bold text-slate-400 uppercase tracking-widest ">Hệ thống phân tích sức khỏe & vận hành thông minh</p>
      </div>
      <div class="flex items-center gap-3">
        <button class="px-6 py-4 bg-primary/10 text-primary rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-primary/20 transition-all flex items-center gap-2">
          <FileDown class="w-4 h-4" /> Xuất PDF
        </button>
        <button class="px-6 py-4 bg-slate-900 text-white rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-slate-800 transition-all flex items-center gap-2">
          <Download class="w-4 h-4" /> Tải Excel
        </button>
      </div>
    </div>

    <div class="flex-grow space-y-8">
      <!-- Dynamic Stat Cards by Role -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <StatCard 
            v-for="(card, index) in currentRoleConfig.cards"
            :key="index"
            :title="card.title"
            :value="card.value"
            :icon="card.icon"
            :trend="card.trend"
            :trendColor="card.trendColor"
            :subtext="card.subtext"
            :progress="card.progress"
            :variant="card.variant"
        />
      </div>

      <!-- Main Visualizations (Dynamic based on Role) -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
        <!-- Visualization 1: Main Chart Area -->
        <div class="lg:col-span-2 bg-white p-6 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a]">
          <div class="flex items-center justify-between mb-6">
            <h3 class="font-bold text-slate-800">
              <span v-if="authStore.role === 'PersonnelManager'">Phân bổ Nhân sự theo Chuyên môn</span>
              <span v-else-if="authStore.role === 'WarehouseManager'">Cơ cấu Danh mục Vật tư</span>
              <span v-else>Phân bố Phân loại Sức khỏe</span>
            </h3>
          </div>
          <div class="flex flex-col md:flex-row items-center gap-8">
            <div class="relative w-48 h-48 flex-shrink-0">
              <canvas id="healthDistributionChart" width="200" height="200"></canvas>
              <div class="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
                <span class="text-2xl font-black">
                   {{ authStore.role === 'WarehouseManager' ? warehouseGrades.reduce((sum, g) => sum + g.value, 0) : '1.2k' }}
                </span>
                <span class="text-[10px] text-slate-400 uppercase font-bold tracking-widest">
                   {{ authStore.role === 'WarehouseManager' ? 'Mặt hàng' : 'Tổng số' }}
                </span>
              </div>
            </div>
            <div class="flex-grow grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div v-for="(item, i) in (authStore.role === 'WarehouseManager' ? warehouseGrades : healthGrades)" :key="i" class="flex items-center gap-3">
                <div class="w-3 h-3 rounded-full" :style="{ backgroundColor: item.color }"></div>
                <div class="flex-grow">
                  <div class="flex justify-between text-[10px] font-bold uppercase tracking-widest mb-1">
                    <span class="text-slate-600">{{ item.label }}</span>
                    <span class="text-slate-900">{{ item.value }}%</span>
                  </div>
                  <div class="w-full bg-slate-100 rounded-full h-1">
                    <div class="h-1 rounded-full" :style="{ width: item.value + '%', backgroundColor: item.color }"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Visualization 2: Status / Progress -->
        <div class="bg-white p-6 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex flex-col">
          <h3 class="font-bold text-slate-800 mb-6 italic">Sẵn sàng vận hành</h3>
          <div class="flex-grow flex flex-col justify-center space-y-6">
            <div v-for="(progress, name) in completionProgress" :key="name">
              <div class="flex justify-between text-xs font-bold uppercase tracking-widest mb-2">
                <span class="text-slate-500">{{ name }}</span>
                <span class="text-blue-600">{{ progress }}%</span>
              </div>
              <div class="w-full bg-slate-100 rounded-full h-1.5">
                <div class="bg-blue-600 h-1.5 rounded-full" :style="{ width: progress + '%' }"></div>
              </div>
            </div>
          </div>
          <button class="mt-8 text-center text-[10px] font-black uppercase tracking-widest text-blue-600 hover:text-blue-700">Chi tiết vận hành →</button>
        </div>
      </div>

      <!-- MỤC BÁO CÁO MỚI: Revenue & Financial Performance -->
      <div v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager' || authStore.role === 'PayrollManager'" 
           class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
        <div class="bg-white p-6 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] relative overflow-hidden group">
          <div class="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
            <DollarSign class="w-32 h-32 text-emerald-600" />
          </div>
          <h3 class="font-black text-slate-800 mb-6 flex items-center gap-3 uppercase tracking-tighter italic">
            <TrendingUp class="w-5 h-5 text-emerald-600" /> Phân tích Doanh thu & Lợi nhuận (Mới)
          </h3>
          <div class="space-y-6 relative z-10">
            <div class="grid grid-cols-2 gap-4">
              <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Doanh thu dự kiến</p>
                <p class="text-xl font-black text-slate-900 tabular-nums">{{ formatCurrency(stats.totalRevenue) }} <span class="text-[10px] text-slate-400 uppercase">VND</span></p>
              </div>
              <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Chi phí nhân sự</p>
                <p class="text-xl font-black text-rose-500 tabular-nums">{{ formatCurrency(stats.staffCost) }} <span class="text-[10px] text-slate-400 uppercase">VND</span></p>
              </div>
            </div>
            <div class="bg-emerald-50/50 p-6 rounded-2xl border border-emerald-100">
              <div class="flex justify-between items-end mb-4">
                <div>
                  <p class="text-[10px] font-black text-emerald-600 uppercase tracking-[0.2em] mb-1">Tỷ suất lợi nhuận ròng</p>
                  <p class="text-3xl font-black text-emerald-700">~ {{ stats.totalRevenue > 0 ? (((stats.totalRevenue - stats.staffCost) / stats.totalRevenue) * 100).toFixed(1) : 0 }}%</p>
                </div>
                <div class="text-right">
                  <span class="inline-flex items-center px-3 py-1 bg-emerald-100 text-emerald-700 rounded-full text-[10px] font-black uppercase tracking-widest">Premium Level</span>
                </div>
              </div>
              <div class="w-full bg-emerald-100/50 rounded-full h-3">
                <div class="bg-emerald-500 h-3 rounded-full" :style="{ width: (stats.totalRevenue > 0 ? ((stats.totalRevenue - stats.staffCost) / stats.totalRevenue * 100) : 0) + '%' }"></div>
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white p-6 rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a]">
          <h3 class="font-black text-slate-800 mb-6 flex items-center gap-3 uppercase tracking-tighter italic">
             <LayoutGrid class="w-5 h-5 text-blue-600" /> Hiệu suất Phân bổ Nhân sự (Mới)
          </h3>
          <div class="space-y-5">
            <div v-for="(metric, name) in staffEfficiency" :key="name" class="flex items-center justify-between p-3 bg-slate-50 rounded-xl border border-slate-100">
                <div class="flex items-center gap-4">
                   <div class="w-10 h-10 rounded-xl bg-white flex items-center justify-center shadow-sm">
                      <component :is="metric.icon" class="w-5 h-5" :class="metric.color" />
                   </div>
                   <div>
                      <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ name }}</p>
                      <p class="text-sm font-black text-slate-800">{{ metric.label }}</p>
                   </div>
                </div>
                <div class="text-right">
                   <p class="text-lg font-black text-slate-900">{{ metric.value }}</p>
                </div>
            </div>
          </div>
        </div>
      </div>
      <!-- 4. Secondary Analysis Grid -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-8">
        <!-- 4.1 Role-Specific Data Lists -->
        <div class="bg-white rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] overflow-hidden">
          <div class="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
            <h3 class="font-bold text-slate-800 italic">
               <span v-if="authStore.role === 'PersonnelManager'">Dương tính hiệu suất - Top Nhân viên</span>
               <span v-else-if="authStore.role === 'WarehouseManager'">Cảnh báo Tồn kho Thấp</span>
               <span v-else-if="authStore.role === 'MedicalGroupManager'">Các đoàn khám đang mở</span>
               <span v-else>Top Bệnh lý / Vấn đề phát hiện</span>
            </h3>
            <span class="text-[9px] font-black text-slate-500 bg-white border border-slate-200 px-3 py-1 rounded-lg uppercase tracking-widest tabular-nums">Real-time Data</span>
          </div>
          
          <div class="overflow-x-auto">
            <!-- Table for HR: Top Performers -->
            <table v-if="authStore.role === 'PersonnelManager'" class="w-full text-left text-sm">
              <thead>
                <tr class="bg-white border-b border-slate-100">
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400">Nhân viên</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-center">Số đoàn</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-right">Đánh giá</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="p in topPerformers" :key="p.name" class="hover:bg-blue-50/30 transition-colors cursor-pointer">
                  <td class="px-6 py-4">
                     <p class="font-bold text-slate-700">{{ p.name }}</p>
                     <p class="text-[10px] text-slate-400 uppercase font-black">{{ p.role }}</p>
                  </td>
                  <td class="px-6 py-4 text-center font-black text-blue-600">{{ p.participation }}</td>
                  <td class="px-6 py-4 text-right">
                     <span class="px-3 py-1 bg-emerald-100 text-emerald-700 rounded-full text-[10px] font-black italic">🌟 {{ p.rating }}</span>
                  </td>
                </tr>
              </tbody>
            </table>

            <!-- Table for Warehouse: Low Stock -->
            <table v-else-if="authStore.role === 'WarehouseManager'" class="w-full text-left text-sm">
              <thead>
                <tr class="bg-white border-b border-slate-100">
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400">Vật tư</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-center">Tồn kho</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-right">Tình trạng</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="s in lowStockSupplies" :key="s.supplyId" class="hover:bg-rose-50/30 transition-colors">
                  <td class="px-6 py-4">
                     <p class="font-bold text-slate-700">{{ s.supplyName }}</p>
                     <p class="text-[10px] text-slate-400 uppercase font-bold italic">{{ s.category }}</p>
                  </td>
                  <td class="px-6 py-4 text-center font-black text-rose-600">{{ s.quantity }} {{ s.unit }}</td>
                  <td class="px-6 py-4 text-right italic font-black text-[10px] text-amber-500 uppercase">Ưu tiên nhập</td>
                </tr>
              </tbody>
            </table>

            <!-- Table for MedicalGroupManager: Active Groups -->
            <table v-else-if="authStore.role === 'MedicalGroupManager'" class="w-full text-left text-sm">
              <thead>
                <tr class="bg-white border-b border-slate-100">
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400">Đoàn khám</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-center">Tiến độ</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="g in activeGroupsList" :key="g.name" class="hover:bg-emerald-50/30 transition-colors cursor-pointer">
                  <td class="px-6 py-4">
                     <p class="font-bold text-slate-700">{{ g.name }}</p>
                     <p class="text-[10px] text-slate-400 uppercase font-black">{{ g.company }}</p>
                  </td>
                  <td class="px-6 py-4 text-center">
                     <div class="flex items-center gap-2 justify-center">
                        <span class="font-black text-emerald-600 tabular-nums">{{ g.progress }}%</span>
                        <div class="w-12 bg-slate-100 rounded-full h-1">
                           <div class="bg-emerald-500 h-1 rounded-full" :style="{ width: g.progress + '%' }"></div>
                        </div>
                     </div>
                  </td>
                  <td class="px-6 py-4 text-right">
                    <button class="text-blue-600 hover:underline font-bold text-[10px] uppercase tracking-widest">Giám sát →</button>
                  </td>
                </tr>
              </tbody>
            </table>

            <!-- Default: Health Findings -->
            <table v-else class="w-full text-left text-sm">
              <thead>
                <tr class="bg-white border-b border-slate-100">
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400">Tình trạng / Vấn đề</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400">Tần suất</th>
                  <th class="px-6 py-4 font-black uppercase tracking-widest text-[9px] text-slate-400 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="finding in healthFindings" :key="finding.name" class="hover:bg-slate-50 transition-colors">
                  <td class="px-6 py-4 font-bold text-slate-700">{{ finding.name }}</td>
                  <td class="px-6 py-4 font-medium text-slate-600 tabular-nums">{{ finding.frequency }}</td>
                  <td class="px-6 py-4 text-right">
                    <button class="text-blue-600 hover:underline font-bold text-[10px] uppercase tracking-widest tabular-nums">Phân tích sâu →</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- 4.2 Ready Reports -->
        <div class="bg-white rounded-[2rem] border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex flex-col">
          <div class="px-6 py-4 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
            <h3 class="font-bold text-slate-800">Báo cáo sẵn sàng xuất file</h3>
            <button class="text-[9px] font-black uppercase tracking-widest text-blue-600">Xem tất cả</button>
          </div>
          <div class="p-4 space-y-4">
            <div v-for="report in readyReports" :key="report.title" class="flex items-center justify-between p-4 rounded-2xl border border-slate-100 hover:border-blue-200 hover:bg-blue-50/20 transition group">
              <div class="flex items-center gap-4">
                <div class="p-3 bg-slate-100 rounded-xl text-slate-500 group-hover:bg-blue-100 group-hover:text-blue-600 transition">
                  <Building2 v-if="report.type === 'company'" class="h-6 w-6" />
                  <FileText v-else class="h-6 w-6" />
                </div>
                <div>
                  <h4 class="text-sm font-black text-slate-800">{{ report.title }}</h4>
                  <p class="text-[10px] font-medium text-slate-500 italic uppercase tracking-widest text-ellipsis overflow-hidden">{{ report.subtitle }}</p>
                </div>
              </div>
              <div class="flex items-center gap-2">
                <span v-if="report.status === 'Verified'" class="text-[9px] font-black text-emerald-600 bg-emerald-100 px-3 py-1 rounded-lg uppercase tracking-widest">OK</span>
                <button v-if="report.status === 'Verified'" class="p-1 hover:text-blue-600 transition">
                  <Download class="h-5 w-5" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'
import { useAuthStore } from '../stores/auth'
import StatCard from '../components/StatCard.vue'
import { 
  Users, Check, Zap, Building2, DollarSign, TrendingUp, LayoutGrid, 
  Briefcase, ArrowUp, ArrowDown, Download, FileDown, FileText,
  Clock, ShieldAlert, ClipboardList, PackageSearch, UserCheck, History, Percent
} from 'lucide-vue-next'

const authStore = useAuthStore()

const stats = ref({
  totalPatients: 0,
  completionRate: 94.2,
  criticalAlerts: 28,
  totalCompanies: 0,
  totalRevenue: 0,
  staffCost: 0,
  pendingContracts: 3,
  activeGroups: 0,
  totalSupplies: 0,
  lowStockItems: 12,
  avgGroupsPerStaff: 4.2
})

const currentRoleConfig = computed(() => {
  const role = authStore.role
  const configs = {
    Admin: {
      cards: [
        { title: 'Tổng lượt khám', value: stats.value.totalPatients.toLocaleString(), icon: Users, trend: '+12%', subtext: 'So với kỳ trước', variant: 'indigo' },
        { title: 'Doanh thu tháng', value: formatCurrency(stats.value.totalRevenue), icon: DollarSign, trend: '+8.5%', subtext: 'Dòng tiền dương', variant: 'emerald' },
        { title: 'Tỷ lệ hoàn thành', value: stats.value.completionRate + '%', icon: Check, trend: 'Tối ưu', progress: stats.value.completionRate, variant: 'sky' },
        { title: 'Cảnh báo khẩn', value: stats.value.criticalAlerts, icon: Zap, trend: '+4', trendColor: 'rose', subtext: 'Cần xử lý ngay', variant: 'rose' }
      ]
    },
    PersonnelManager: {
      cards: [
        { title: 'Tổng quân số', value: staffCount.value, icon: Users, trend: 'Ổn định', subtext: 'Y bác sĩ hệ thống', variant: 'indigo' },
        { title: 'Đang trực chiến', value: 18, icon: UserCheck, trend: '+3', subtext: 'Tham gia đoàn hôm nay', variant: 'emerald' },
        { title: 'Hiệu suất TB', value: stats.value.avgGroupsPerStaff, icon: Zap, trend: 'Cải thiện', subtext: 'Số đoàn/Nhân sự', variant: 'amber' },
        { title: 'Hồ sơ sắp hạn', value: 5, icon: ShieldAlert, trend: 'Cần cập nhật', trendColor: 'rose', subtext: 'Chứng chỉ hành nghề', variant: 'rose' }
      ]
    },
    ContractManager: {
      cards: [
        { title: 'Hợp đồng mới', value: 7, icon: FileText, trend: '+2', subtext: 'Trong tháng này', variant: 'indigo' },
        { title: 'Doanh thu dự kiến', value: formatCurrency(stats.value.totalRevenue), icon: TrendingUp, trend: '+15%', subtext: 'Tăng trưởng mục tiêu', variant: 'emerald' },
        { title: 'Hợp đồng chờ ký', value: stats.value.pendingContracts, icon: Clock, trend: 'Cần xử lý', trendColor: 'amber', variant: 'amber' },
        { title: 'Đối tác đã thuê', value: stats.value.totalCompanies, icon: Building2, trend: 'Mở rộng', subtext: 'Các doanh nghiệp', variant: 'sky' }
      ]
    },
    WarehouseManager: {
      cards: [
        { title: 'Mặt hàng trong kho', value: stats.value.totalSupplies || 0, icon: LayoutGrid, trend: 'Đầy đủ', subtext: 'Vật tư thiết bị', variant: 'indigo' },
        { title: 'Tồn kho thấp', value: stats.value.lowStockItems, icon: PackageSearch, trend: 'Cần nhập hàng', trendColor: 'rose', subtext: 'Liên hệ NCC ngay', variant: 'rose' },
        { title: 'Phiếu kho tháng', value: 24, icon: History, trend: '+8', subtext: 'Giao dịch mới', variant: 'sky' },
        { title: 'Sắp hết hạn', value: 3, icon: Clock, trend: 'Cần thanh lý', trendColor: 'amber', variant: 'amber' }
      ]
    },
    MedicalGroupManager: {
      cards: [
        { title: 'Đoàn đang mở', value: stats.value.activeGroups, icon: Briefcase, trend: 'Đang khám', variant: 'indigo' },
        { title: 'Nhân sự tham gia', value: staffCount.value, icon: Users, trend: 'Phân bổ đủ', subtext: 'Đội ngũ tại hiện trường', variant: 'emerald' },
        { title: 'Tiến độ khám', value: '75%', icon: Percent, trend: 'Đúng kế hoạch', progress: 75, variant: 'sky' },
        { title: 'Sẵn sàng vật tư', value: '98%', icon: Check, trend: 'Tốt', variant: 'emerald' }
      ]
    }
  }
  return configs[role] || configs.Admin
})

const healthGrades = ref([
  { label: 'Loại I (Rất tốt)', value: 35, color: '#10b981' },
  { label: 'Loại II (Khá)', value: 42, color: '#3b82f6' },
  { label: 'Loại III (Trung bình)', value: 15, color: '#f59e0b' },
  { label: 'Loại IV (Yếu)', value: 6, color: '#f97316' },
  { label: 'Loại V (Cấp cứu)', value: 2, color: '#ef4444' }
])

const warehouseGrades = ref([
  { label: 'Vật tư tiêu hao', value: 45, color: '#10b981' },
  { label: 'Thuốc cố định', value: 25, color: '#3b82f6' },
  { label: 'Dụng cụ y tế', value: 20, color: '#f59e0b' },
  { label: 'Thiết bị đo kđ', value: 10, color: '#ef4444' }
])

const completionProgress = ref({
  'Khám lâm sàng': 98,
  'Xét nghiệm máu': 92,
  'Chẩn đoán hình ảnh': 85,
  'Điện tim (ECG)': 90
})

const staffCount = ref(0)
const groupCount = ref(0)
const staffEfficiency = computed(() => ({
  'Nhân sự tham gia': { label: 'Đội ngũ Y bác sĩ', value: staffCount.value.toString(), icon: Users, color: 'text-blue-600' },
  'Số đoàn thực hiện': { label: 'Tổng cộng', value: groupCount.value.toString(), icon: Briefcase, color: 'text-emerald-600' },
  'Hiệu suất làm việc': { label: 'Lượt khám/Nhân viên', value: staffCount.value > 0 ? (stats.value.totalPatients / staffCount.value).toFixed(1) : '0', icon: Zap, color: 'text-amber-500' }
}))

const healthFindings = ref([
  { name: 'Rối loạn Lipid máu', frequency: '248 (19%)', trend: 'High', trendColor: 'text-rose-500' },
  { name: 'Thừa cân/Béo phì', frequency: '186 (14%)', trend: 'Moderate', trendColor: 'text-amber-500' },
  { name: 'Tăng huyết áp', frequency: '112 (9%)', trend: 'Rising', trendColor: 'text-rose-500' },
  { name: 'Tật khúc xạ', frequency: '94 (7%)', trend: 'Stable', trendColor: 'text-emerald-500' }
])

const topPerformers = ref([])
const lowStockSupplies = ref([])
const activeGroupsList = ref([])

const readyReports = ref([
  { title: 'Báo cáo tổng hợp Q3', subtitle: 'Tất cả các đoàn khám', status: 'Verified', type: 'file' }
])

const formatCurrency = (val) => {
  if (val >= 1000000000) return (val / 1000000000).toFixed(2) + 'B'
  if (val >= 1000000) return (val / 1000000).toFixed(1) + 'M'
  return val.toLocaleString()
}

const fetchData = async () => {
  try {
    const endpoints = [
      { key: 'contracts', url: 'http://localhost:5283/api/HealthContracts', roles: ['Admin', 'ContractManager', 'Customer', 'MedicalStaff'] },
      { key: 'groups', url: 'http://localhost:5283/api/MedicalGroups', roles: ['Admin', 'MedicalGroupManager', 'MedicalStaff', 'Customer'] },
      { key: 'staff', url: 'http://localhost:5283/api/Staffs', roles: ['Admin', 'PersonnelManager', 'MedicalGroupManager', 'MedicalStaff'] },
      { key: 'companies', url: 'http://localhost:5283/api/Companies', roles: ['Admin', 'ContractManager', 'MedicalStaff'] },
      { key: 'supplies', url: 'http://localhost:5283/api/Supplies', roles: ['Admin', 'WarehouseManager'] }
    ]

    const allowed = endpoints.filter(ep => ep.roles.includes(authStore.role))
    
    const results = await Promise.allSettled(allowed.map(ep => axios.get(ep.url)))
    
    const data = {}
    results.forEach((res, index) => {
      const key = allowed[index].key
      if (res.status === 'fulfilled') {
        data[key] = res.value.data
      } else {
        console.warn(`Could not load ${key} for analytics:`, res.reason)
        data[key] = []
      }
    })

    const contracts = data.contracts || []
    const groups = data.groups || []
    const staff = data.staff || []
    const companies = data.companies || []
    const supplies = data.supplies || []

    stats.value.totalPatients = contracts.reduce((sum, c) => sum + (c.expectedQuantity || 0), 0)
    stats.value.totalRevenue = contracts.reduce((sum, c) => sum + (c.totalAmount || 0), 0)
    stats.value.totalCompanies = companies.length
    stats.value.pendingContracts = contracts.filter(c => c.status === 'Draft' || c.status === 'Pending').length
    
    groupCount.value = groups.length
    stats.value.activeGroups = groups.filter(g => g.status === 'Open' || g.status === 'InProgress').length
    
    staffCount.value = staff.length
    stats.value.avgGroupsPerStaff = staff.length > 0 ? (groups.length / staff.length).toFixed(1) : 0
    stats.value.staffCost = staff.reduce((sum, s) => sum + (s.baseSalary || 0), 0) * 1.2 

    stats.value.totalSupplies = supplies.length
    stats.value.lowStockItems = supplies.filter(s => s.quantity < 50).length
    lowStockSupplies.value = supplies.filter(s => s.quantity < 30).slice(0, 5)

    activeGroupsList.value = groups.filter(g => g.status === 'Open' || g.status === 'InProgress').map(g => ({
      name: g.groupName,
      company: g.companyName,
      staffCount: 12, // Mock or fetch actual
      progress: 75
    }))

    topPerformers.value = staff.slice(0, 4).map(s => ({
      name: s.fullName,
      role: s.position,
      participation: Math.floor(Math.random() * 20) + 10,
      rating: (Math.random() * 1 + 4).toFixed(1)
    }))

    // Update ready reports with actual companies
    readyReports.value = companies.slice(0, 3).map(c => ({
      title: c.shortName || c.companyName,
      subtitle: `Đối tác chiến lược • ${c.address?.split(',').pop() || 'Việt Nam'}`,
      status: 'Verified',
      type: 'company'
    }))

    renderChart()
  } catch (err) {
    console.error("Lỗi khi tải dữ liệu dashboard:", err)
  }
}

const renderChart = () => {
  const canvas = document.getElementById('healthDistributionChart')
  if (!canvas) return
  const ctx = canvas.getContext('2d')
  
  // Clear canvas
  ctx.clearRect(0, 0, canvas.width, canvas.height)
  
  const grades = authStore.role === 'WarehouseManager' ? warehouseGrades.value : healthGrades.value
  const data = grades.map(g => g.value)
  const colors = grades.map(g => g.color)
  
  let total = data.reduce((a, b) => a + b, 0)
  let startAngle = -0.5 * Math.PI
  
  const centerX = canvas.width / 2
  const centerY = canvas.height / 2
  const outerRadius = 85
  const innerRadius = 60

  data.forEach((value, i) => {
    const sliceAngle = (value / total) * (2 * Math.PI)
    
    ctx.beginPath()
    ctx.arc(centerX, centerY, outerRadius, startAngle, startAngle + sliceAngle)
    ctx.arc(centerX, centerY, innerRadius, startAngle + sliceAngle, startAngle, true)
    ctx.closePath()
    
    ctx.fillStyle = colors[i]
    ctx.fill()
    
    startAngle += sliceAngle
  })
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
@reference "tailwindcss";

.input-premium {
  @apply px-4 py-2 rounded-xl bg-slate-50 border border-slate-200 outline-none transition-all focus:border-blue-600/30 font-bold text-xs;
}

.animate-slide-up {
  animation: slideUp 0.5s ease-out forwards;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
