<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-7xl mx-auto w-full p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8 glass-header p-6 rounded-[2rem] shadow-sm">
        <div>
          <h1 class="text-3xl font-black text-slate-900 tracking-tight italic uppercase">Thống Kê Tổng Hợp (Master Stats)</h1>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Tổng hợp doanh thu, chi phí và lợi nhuận thực tế toàn hệ thống.</p>
        </div>
        <div class="flex gap-3">
          <input type="date" v-model="startDate" class="px-4 py-2 border border-slate-200 rounded-xl text-sm font-bold text-slate-700 focus:outline-none focus:border-primary" />
          <span class="px-2 py-2 text-slate-400">→</span>
          <input type="date" v-model="endDate" class="px-4 py-2 border border-slate-200 rounded-xl text-sm font-bold text-slate-700 focus:outline-none focus:border-primary" />
          <button @click="loadStats" class="w-12 h-12 bg-white rounded-2xl shadow-sm border border-slate-100 text-slate-400 hover:text-primary transition-all flex items-center justify-center">
            <RefreshCw class="w-5 h-5" :class="{ 'animate-spin': loading }" />
          </button>
        </div>
      </div>

      <!-- Main KPI Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <!-- Total Revenue -->
        <div class="bg-gradient-to-br from-emerald-50 to-emerald-100 border border-emerald-200 p-8 rounded-[2rem] shadow-lg relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 opacity-10 text-emerald-600 group-hover:scale-110 transition-transform">
            <TrendingUp class="w-32 h-32" />
          </div>
          <p class="text-[10px] font-black text-emerald-600 uppercase tracking-widest mb-2">Tổng Doanh Thu</p>
          <h3 class="text-3xl font-black text-emerald-900 tabular-nums mb-2">{{ formatCurrency(stats.totalRevenue) }}</h3>
          <div class="flex items-center gap-2">
            <span class="text-[10px] text-emerald-700 font-bold uppercase tracking-wide">Gói: {{ formatCurrency(stats.totalPackageRevenue) }}</span>
            <span class="text-[10px] text-emerald-600 font-bold">| Phát sinh: {{ formatCurrency(stats.totalExtraRevenue) }}</span>
          </div>
        </div>

        <!-- Total Cost -->
        <div class="bg-gradient-to-br from-orange-50 to-orange-100 border border-orange-200 p-8 rounded-[2rem] shadow-lg relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 opacity-10 text-orange-600 group-hover:scale-110 transition-transform">
            <TrendingDown class="w-32 h-32" />
          </div>
          <p class="text-[10px] font-black text-orange-600 uppercase tracking-widest mb-2">Tổng Chi Phí</p>
          <h3 class="text-3xl font-black text-orange-900 tabular-nums mb-2">{{ formatCurrency(stats.totalCost) }}</h3>
          <div class="flex items-center gap-2 text-[10px] text-orange-700 font-bold uppercase tracking-wide">
            <span>Lương: {{ stats.staffCostPercentage }}%</span>
            <span>| Vật tư: {{ stats.materialCostPercentage }}%</span>
          </div>
        </div>

        <!-- Net Profit -->
        <div :class="[stats.profitMargin >= 20 ? 'from-blue-50 to-blue-100 border-blue-200' : 'from-rose-50 to-rose-100 border-rose-200']" class="bg-gradient-to-br p-8 rounded-[2rem] shadow-lg relative overflow-hidden group">
          <div :class="[stats.profitMargin >= 20 ? 'text-blue-600' : 'text-rose-600']" class="absolute -right-4 -bottom-4 opacity-10 group-hover:scale-110 transition-transform">
            <DollarSign class="w-32 h-32" />
          </div>
          <p :class="[stats.profitMargin >= 20 ? 'text-blue-600' : 'text-rose-600']" class="text-[10px] font-black uppercase tracking-widest mb-2">Lợi Nhuận Ròng</p>
          <h3 :class="[stats.profitMargin >= 20 ? 'text-blue-900' : 'text-rose-900']" class="text-3xl font-black tabular-nums mb-2">{{ formatCurrency(stats.netProfit) }}</h3>
          <div class="flex items-center gap-2">
            <span :class="[stats.profitMargin >= 20 ? 'bg-blue-200 text-blue-700' : 'bg-rose-200 text-rose-700']" class="px-3 py-1 rounded-xl text-xs font-black">
              {{ stats.profitMargin.toFixed(1) }}%
            </span>
            <span :class="[stats.profitMargin >= 20 ? 'text-blue-600' : 'text-rose-600']" class="text-[10px] font-black uppercase tracking-widest">Margin</span>
          </div>
        </div>

        <!-- Contracts & Records -->
        <div class="bg-gradient-to-br from-indigo-50 to-indigo-100 border border-indigo-200 p-8 rounded-[2rem] shadow-lg relative overflow-hidden group">
          <div class="absolute -right-4 -bottom-4 opacity-10 text-indigo-600 group-hover:scale-110 transition-transform">
            <FileText class="w-32 h-32" />
          </div>
          <p class="text-[10px] font-black text-indigo-600 uppercase tracking-widest mb-2">Hợp Đồng & Ca Khám</p>
          <h3 class="text-3xl font-black text-indigo-900 tabular-nums mb-2">{{ stats.totalContracts }} / {{ stats.totalCompletedRecords }}</h3>
          <div class="text-[10px] text-indigo-700 font-bold uppercase tracking-wide">Hợp đồng / Ca hoàn thành</div>
        </div>
      </div>

      <!-- Cost Breakdown -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- Cost Distribution Chart -->
        <div class="premium-card p-8">
          <h3 class="font-black text-slate-800 uppercase tracking-tighter text-lg italic mb-6">Cấu Trúc Chi Phí (%)</h3>
          <div class="space-y-4">
            <div class="flex items-center gap-4">
              <div class="w-full bg-slate-100 rounded-full h-3 overflow-hidden">
                <div class="bg-blue-500 h-full" :style="{ width: stats.staffCostPercentage + '%' }"></div>
              </div>
              <div class="w-32 text-right">
                <p class="text-xs font-black text-slate-600">Lương: {{ stats.staffCostPercentage }}%</p>
                <p class="text-xs text-slate-500">{{ formatCurrency(stats.totalStaffCost) }}</p>
              </div>
            </div>
            <div class="flex items-center gap-4">
              <div class="w-full bg-slate-100 rounded-full h-3 overflow-hidden">
                <div class="bg-amber-500 h-full" :style="{ width: stats.materialCostPercentage + '%' }"></div>
              </div>
              <div class="w-32 text-right">
                <p class="text-xs font-black text-slate-600">Vật tư: {{ stats.materialCostPercentage }}%</p>
                <p class="text-xs text-slate-500">{{ formatCurrency(stats.totalMaterialCost) }}</p>
              </div>
            </div>
            <div class="flex items-center gap-4">
              <div class="w-full bg-slate-100 rounded-full h-3 overflow-hidden">
                <div class="bg-purple-500 h-full" :style="{ width: stats.overheadCostPercentage + '%' }"></div>
              </div>
              <div class="w-32 text-right">
                <p class="text-xs font-black text-slate-600">Overhead: {{ stats.overheadCostPercentage }}%</p>
                <p class="text-xs text-slate-500">{{ formatCurrency(stats.totalOverheadCost) }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Staff Statistics -->
        <div class="premium-card p-8">
          <h3 class="font-black text-slate-800 uppercase tracking-tighter text-lg italic mb-6">Thống Kê Nhân Sự</h3>
          <div class="space-y-4">
            <div class="flex justify-between items-center p-4 bg-slate-50 rounded-2xl">
              <span class="font-bold text-slate-600">Tổng lượt điều động</span>
              <span class="font-black text-slate-800 text-lg">{{ stats.totalStaffDeployed }}</span>
            </div>
            <div class="flex justify-between items-center p-4 bg-slate-50 rounded-2xl">
              <span class="font-bold text-slate-600">Số nhân sự duy nhất</span>
              <span class="font-black text-slate-800 text-lg">{{ stats.uniqueStaffCount }}</span>
            </div>
            <div class="flex justify-between items-center p-4 bg-emerald-50 rounded-2xl border border-emerald-100">
              <span class="font-bold text-emerald-600">Hợp đồng biên lợi nhuận thấp</span>
              <span class="font-black text-emerald-700 text-lg">{{ stats.lowMarginContracts }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Reconciliation List -->
      <div class="premium-card overflow-hidden flex flex-col">
        <div class="p-8 border-b border-slate-50 flex justify-between items-center bg-slate-50/20">
          <div>
            <h3 class="font-black text-slate-800 uppercase tracking-tighter text-lg italic">Danh Sách Đối Soát</h3>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Các ca khám lệch cần xử lý</p>
          </div>
          <button @click="loadReconciliation" class="px-4 py-2 bg-primary text-white rounded-xl font-bold text-xs hover:bg-primary/90 transition-all">
            Tải Danh Sách
          </button>
        </div>
        
        <div class="overflow-x-auto">
          <table class="w-full text-left">
            <thead class="bg-slate-50/50">
              <tr>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Loại</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Hợp Đồng</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100">Mô Tả</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-center">Số Lượng</th>
                <th class="px-6 py-5 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] border-b border-slate-100 text-right">Số Tiền</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-if="reconciliationList.length === 0">
                <td colspan="5" class="py-16 text-center">
                  <FileText class="w-12 h-12 text-slate-200 mx-auto mb-3" />
                  <p class="text-xs font-black text-slate-400 uppercase tracking-widest">Không có dữ liệu đối soát</p>
                </td>
              </tr>
              <tr v-for="item in reconciliationList" :key="item.contractId + item.type" class="hover:bg-slate-50/50 transition-colors group">
                <td class="px-6 py-4">
                  <span :class="getTypeClass(item.type)" class="px-3 py-1 rounded-lg text-xs font-black">
                    {{ getTypeLabel(item.type) }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <p class="font-black text-slate-800 text-sm">{{ item.contractName }}</p>
                </td>
                <td class="px-6 py-4">
                  <p class="text-xs text-slate-600">{{ item.description }}</p>
                </td>
                <td class="px-6 py-4 text-center">
                  <span class="inline-flex items-center justify-center px-3 py-1 bg-slate-100 text-slate-600 rounded-lg text-xs font-black">
                    {{ item.quantity }}
                  </span>
                </td>
                <td class="px-6 py-4 text-right">
                  <span :class="item.amount > 0 ? 'text-emerald-600' : 'text-rose-600'" class="font-black text-sm">
                    {{ formatCurrency(item.amount) }}
                  </span>
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
import { ref, onMounted } from 'vue'
import { RefreshCw, TrendingUp, TrendingDown, DollarSign, FileText } from 'lucide-vue-next'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'

const toast = useToast()
const loading = ref(false)

// Date filters
const today = new Date()
const startDate = ref(new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0])
const endDate = ref(today.toISOString().split('T')[0])

// Data
const stats = ref({
  totalRevenue: 0,
  totalPackageRevenue: 0,
  totalExtraRevenue: 0,
  totalCost: 0,
  totalStaffCost: 0,
  totalMaterialCost: 0,
  totalOverheadCost: 0,
  staffCostPercentage: 0,
  materialCostPercentage: 0,
  overheadCostPercentage: 0,
  netProfit: 0,
  profitMargin: 0,
  totalCompletedRecords: 0,
  totalContracts: 0,
  lowMarginContracts: 0,
  totalStaffDeployed: 0,
  uniqueStaffCount: 0
})

const reconciliationList = ref([])

const loadStats = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/api/Settlement/master-stats', {
      params: {
        startDate: startDate.value,
        endDate: endDate.value
      }
    })
    stats.value = res.data
  } catch (err) {
    console.error('Lỗi tải thống kê', err)
    toast.show('Lỗi tải thống kê tổng hợp', 'error')
  } finally {
    loading.value = false
  }
}

const loadReconciliation = async () => {
  try {
    const res = await apiClient.get('/api/Settlement/reconciliation-list', {
      params: {
        startDate: startDate.value,
        endDate: endDate.value
      }
    })
    reconciliationList.value = res.data
  } catch (err) {
    console.error('Lỗi tải danh sách đối soát', err)
    toast.show('Lỗi tải danh sách đối soát', 'error')
  }
}

const formatCurrency = (val) => {
  if (!val) return '0 đ'
  return val.toLocaleString('vi-VN') + ' đ'
}

const getTypeLabel = (type) => {
  const labels = {
    'EXTRA_COMPLETED': 'Khám Thêm',
    'INCOMPLETE': 'Khám Thiếu',
    'CANCELLED': 'Hủy'
  }
  return labels[type] || type
}

const getTypeClass = (type) => {
  const classes = {
    'EXTRA_COMPLETED': 'bg-emerald-100 text-emerald-700',
    'INCOMPLETE': 'bg-orange-100 text-orange-700',
    'CANCELLED': 'bg-rose-100 text-rose-700'
  }
  return classes[type] || 'bg-slate-100 text-slate-700'
}

onMounted(() => {
  loadStats()
  loadReconciliation()
})
</script>
