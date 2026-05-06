<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 scrollbar-premium overflow-y-auto font-sans">
    <div class="max-w-[1600px] mx-auto w-full p-4 space-y-4">

      <!-- ── HEADER ──────────────────────────────────────────────────────── -->
      <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3">
        <div>
          <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2.5">
            <div class="w-9 h-9 bg-primary text-white rounded-xl flex items-center justify-center shadow-lg">
              <TrendingUp class="w-4.5 h-4.5" />
            </div>
            Báo Cáo Tài Chính
          </h2>
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-0.5">
            Nguồn dữ liệu: CostSnapshot (Quyết toán) — không tính lại từ bảng thô
          </p>
        </div>

        <!-- Controls -->
        <div class="flex flex-wrap items-center gap-2">
          <!-- Granularity toggle -->
          <div class="flex bg-slate-100 p-0.5 rounded-lg border border-slate-200">
            <button
              v-for="g in granularityOptions" :key="g.value"
              @click="granularity = g.value; reload()"
              :class="['px-3 py-1.5 rounded-md text-[9px] font-black uppercase tracking-widest transition-all',
                granularity === g.value
                  ? 'bg-white text-primary shadow-sm'
                  : 'text-slate-400 hover:text-slate-600']"
            >{{ g.label }}</button>
          </div>

          <!-- Date range -->
          <div class="flex items-center gap-1.5 bg-white px-3 py-1.5 rounded-lg border border-slate-200 shadow-sm">
            <Calendar class="w-3.5 h-3.5 text-slate-400" />
            <input type="date" v-model="fromDate"
              class="bg-transparent border-none outline-none text-[10px] font-black text-slate-600 cursor-pointer" />
            <span class="text-slate-300 text-[10px] font-black">→</span>
            <input type="date" v-model="toDate"
              class="bg-transparent border-none outline-none text-[10px] font-black text-slate-600 cursor-pointer" />
          </div>

          <button @click="reload"
            class="w-8 h-8 flex items-center justify-center bg-white border border-slate-200 rounded-lg hover:bg-slate-50 transition-all shadow-sm">
            <RefreshCw class="w-3.5 h-3.5 text-slate-500" :class="{ 'animate-spin': loading }" />
          </button>

          <button @click="exportExcel"
            class="h-8 px-3 bg-primary text-white rounded-lg font-black text-[9px] uppercase shadow-md shadow-primary/20 hover:bg-primary/90 transition-all flex items-center gap-1.5">
            <Download class="w-3.5 h-3.5" /> Excel
          </button>
        </div>
      </div>

      <!-- ── ALERTS BANNER ───────────────────────────────────────────────── -->
      <div v-if="lossAlerts.length || lowMarginAlerts.length"
        class="flex flex-wrap gap-2">
        <div v-for="a in lossAlerts" :key="a.contractId"
          class="flex items-center gap-2 px-3 py-1.5 bg-rose-50 border border-rose-200 rounded-lg text-[9px] font-black text-rose-700 uppercase tracking-widest">
          <AlertTriangle class="w-3 h-3 flex-shrink-0" />
          LỖ: {{ a.contractName ?? 'HĐ #' + a.contractId }}
          ({{ a.value?.toFixed(1) }}%)
        </div>
        <div v-for="a in lowMarginAlerts" :key="a.contractId"
          class="flex items-center gap-2 px-3 py-1.5 bg-amber-50 border border-amber-200 rounded-lg text-[9px] font-black text-amber-700 uppercase tracking-widest">
          <AlertTriangle class="w-3 h-3 flex-shrink-0" />
          BIÊN THẤP: {{ a.contractName ?? 'HĐ #' + a.contractId }}
          ({{ a.value?.toFixed(1) }}%)
        </div>
      </div>

      <!-- ── KPI CARDS ───────────────────────────────────────────────────── -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <!-- Revenue -->
        <div class="premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm">
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-1">
            <DollarSign class="w-3 h-3" /> Doanh Thu
          </p>
          <p class="text-xl font-black text-slate-900 tabular-nums leading-none">
            {{ fmt(overview.financial.totalRevenue) }}
          </p>
          <p class="text-[8px] text-slate-400 mt-1 font-bold">
            Gói: {{ fmt(summary.summary.packageRevenue) }}
            · Phát sinh: {{ fmt(summary.summary.extraRevenue) }}
          </p>
        </div>

        <!-- Cost -->
        <div class="premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm">
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-1">
            <TrendingDown class="w-3 h-3" /> Chi Phí
          </p>
          <p class="text-xl font-black text-slate-900 tabular-nums leading-none">
            {{ fmt(overview.financial.totalCost) }}
          </p>
          <div class="flex gap-2 mt-1">
            <span class="text-[7px] font-black px-1.5 py-0.5 bg-blue-50 text-blue-600 rounded">
              Lương {{ pct(summary.summary.laborCost, summary.summary.totalRevenue) }}%
            </span>
            <span class="text-[7px] font-black px-1.5 py-0.5 bg-amber-50 text-amber-600 rounded">
              VT {{ pct(summary.summary.supplyCost, summary.summary.totalRevenue) }}%
            </span>
          </div>
        </div>

        <!-- Gross Profit -->
        <div class="premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm">
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-1">
            <BarChart3 class="w-3 h-3" /> Lợi Nhuận Gộp
          </p>
          <p :class="['text-xl font-black tabular-nums leading-none',
            overview.financial.grossProfit >= 0 ? 'text-emerald-600' : 'text-rose-600']">
            {{ fmt(overview.financial.grossProfit) }}
          </p>
          <p class="text-[8px] text-slate-400 mt-1 font-bold">
            {{ summary.summary.contractCount }} hợp đồng
            · {{ summary.summary.lowMarginCount }} biên thấp
          </p>
        </div>

        <!-- Profit Margin -->
        <div class="premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm">
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-1">
            <Percent class="w-3 h-3" /> Biên Lợi Nhuận
          </p>
          <p :class="['text-xl font-black tabular-nums leading-none', marginColor]">
            {{ overview.financial.profitMargin?.toFixed(1) }}%
          </p>
          <div class="mt-2 w-full bg-slate-100 rounded-full h-1.5 overflow-hidden">
            <div :class="['h-full rounded-full transition-all duration-700', marginBarColor]"
              :style="{ width: Math.min(Math.max(overview.financial.profitMargin, 0), 100) + '%' }" />
          </div>
        </div>
      </div>

      <!-- ── CHARTS ROW ──────────────────────────────────────────────────── -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">

        <!-- Revenue / Cost / Profit trend (Line chart) -->
        <div class="lg:col-span-2 premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm">
          <div class="flex items-center justify-between mb-3">
            <div>
              <h3 class="font-black text-slate-800 uppercase tracking-tight text-[11px] italic">
                Xu Hướng Doanh Thu & Lợi Nhuận
              </h3>
              <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">
                Nguồn: CostSnapshot (SETTLE) · {{ granularity === 'month' ? 'Theo tháng' : 'Theo quý' }}
              </p>
            </div>
            <div v-if="loadingTrend" class="w-4 h-4 border-2 border-primary border-t-transparent rounded-full animate-spin" />
          </div>

          <div class="h-56 relative">
            <Line v-if="trendChartData.labels.length" :data="trendChartData" :options="lineOptions" />
            <div v-else class="h-full flex flex-col items-center justify-center opacity-30">
              <BarChart3 class="w-10 h-10 mb-2" />
              <p class="text-[9px] font-black uppercase tracking-widest">Chưa có dữ liệu quyết toán</p>
            </div>
          </div>
        </div>

        <!-- Cost breakdown (Doughnut) -->
        <div class="premium-card p-4 bg-white border border-slate-100 rounded-xl shadow-sm flex flex-col">
          <div class="mb-3">
            <h3 class="font-black text-slate-800 uppercase tracking-tight text-[11px] italic">
              Cơ Cấu Chi Phí
            </h3>
            <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">% trên tổng doanh thu</p>
          </div>

          <div class="flex-1 flex items-center justify-center">
            <div class="w-44 h-44 relative">
              <Doughnut v-if="costChartData.datasets[0].data.some(v => v > 0)"
                :data="costChartData" :options="doughnutOptions" />
              <div v-else class="w-full h-full flex items-center justify-center opacity-20">
                <PieChart class="w-12 h-12" />
              </div>
            </div>
          </div>

          <!-- Legend -->
          <div class="mt-3 space-y-1.5">
            <div v-for="(item, i) in costLegend" :key="i"
              class="flex items-center justify-between text-[9px] font-black">
              <div class="flex items-center gap-1.5">
                <div class="w-2.5 h-2.5 rounded-sm" :style="{ background: item.color }" />
                <span class="text-slate-600 uppercase tracking-widest">{{ item.label }}</span>
              </div>
              <span class="text-slate-800 tabular-nums">{{ item.pct }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ── OPERATIONS ROW ──────────────────────────────────────────────── -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <div v-for="op in operationCards" :key="op.label"
          class="premium-card p-3 bg-white border border-slate-100 rounded-xl shadow-sm flex items-center gap-3">
          <div :class="['w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0', op.bg]">
            <component :is="op.icon" class="w-4 h-4" :class="op.iconColor" />
          </div>
          <div>
            <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest leading-none mb-0.5">
              {{ op.label }}
            </p>
            <p class="text-lg font-black text-slate-800 tabular-nums leading-none">{{ op.value }}</p>
          </div>
        </div>
      </div>

      <!-- ── CONTRACT TABLE ──────────────────────────────────────────────── -->
      <div class="premium-card bg-white border border-slate-100 rounded-xl shadow-sm overflow-hidden">
        <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
          <div>
            <h3 class="font-black text-slate-800 uppercase tracking-tight text-[11px] italic">
              Chi Tiết Theo Hợp Đồng
            </h3>
            <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">
              Chỉ hiển thị hợp đồng đã quyết toán (SETTLE snapshot)
            </p>
          </div>
          <!-- Table tab: top / worst -->
          <div class="flex bg-slate-100 p-0.5 rounded-lg border border-slate-200">
            <button v-for="t in tableTabs" :key="t.value"
              @click="activeTab = t.value"
              :class="['px-3 py-1.5 rounded-md text-[9px] font-black uppercase tracking-widest transition-all',
                activeTab === t.value
                  ? 'bg-white text-primary shadow-sm'
                  : 'text-slate-400 hover:text-slate-600']">
              {{ t.label }}
            </button>
          </div>
        </div>

        <div class="overflow-x-auto scrollbar-premium">
          <table class="w-full text-left">
            <thead>
              <tr class="bg-slate-50/50 text-[8px] font-black text-slate-400 uppercase tracking-widest">
                <th class="p-3 pl-5">Hợp Đồng</th>
                <th class="p-3">Công Ty</th>
                <th class="p-3 text-right">Doanh Thu</th>
                <th class="p-3 text-right">Chi Phí</th>
                <th class="p-3 text-right">Lợi Nhuận</th>
                <th class="p-3 text-center">Biên (%)</th>
                <th class="p-3 text-center">SL Thực / KH</th>
                <th class="p-3 pr-5 text-center">Trạng Thái</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-if="tableRows.length === 0">
                <td colspan="8" class="py-12 text-center">
                  <div class="flex flex-col items-center opacity-30">
                    <FileText class="w-10 h-10 mb-2" />
                    <p class="text-[9px] font-black uppercase tracking-widest">Chưa có dữ liệu quyết toán</p>
                  </div>
                </td>
              </tr>
              <tr v-for="row in tableRows" :key="row.contractId"
                class="hover:bg-slate-50/50 transition-all group">
                <td class="p-3 pl-5">
                  <p class="font-black text-slate-800 text-[11px] uppercase tracking-tight">
                    {{ row.contractCode ?? 'HĐ-' + row.contractId }}
                  </p>
                  <p class="text-[8px] text-slate-400 font-bold mt-0.5">
                    {{ row.finalizedAt ? fmtDate(row.finalizedAt) : '—' }}
                  </p>
                </td>
                <td class="p-3">
                  <p class="font-bold text-slate-700 text-[10px] truncate max-w-[140px]">
                    {{ row.companyShortName ?? row.companyName }}
                  </p>
                </td>
                <td class="p-3 text-right font-black text-slate-800 tabular-nums text-[11px]">
                  {{ fmt(row.totalRevenue) }}
                </td>
                <td class="p-3 text-right font-bold text-slate-600 tabular-nums text-[11px]">
                  {{ fmt(row.totalCost) }}
                </td>
                <td class="p-3 text-right">
                  <span :class="['font-black tabular-nums text-[11px]',
                    row.grossProfit >= 0 ? 'text-emerald-600' : 'text-rose-600']">
                    {{ fmt(row.grossProfit) }}
                  </span>
                </td>
                <td class="p-3 text-center">
                  <span :class="['px-2 py-0.5 rounded-md text-[8px] font-black uppercase tracking-widest',
                    marginBadgeClass(row.profitMargin)]">
                    {{ row.profitMargin?.toFixed(1) }}%
                  </span>
                </td>
                <td class="p-3 text-center">
                  <span class="font-black text-slate-700 tabular-nums text-[11px]">
                    {{ row.actualQuantity }}
                  </span>
                  <span class="text-slate-300 text-[9px] mx-0.5">/</span>
                  <span class="font-bold text-slate-400 tabular-nums text-[10px]">
                    {{ row.expectedQuantity }}
                  </span>
                  <span v-if="row.isQuantityVarianceHigh"
                    class="ml-1 text-amber-500" title="Lệch số lượng cao">⚠</span>
                </td>
                <td class="p-3 pr-5 text-center">
                  <span :class="['px-2 py-0.5 rounded-md text-[8px] font-black uppercase tracking-widest',
                    row.marginStatus === 'OK'         ? 'bg-emerald-50 text-emerald-700' :
                    row.marginStatus === 'LOW_MARGIN' ? 'bg-amber-50 text-amber-700' :
                                                        'bg-rose-50 text-rose-700']">
                    {{ row.marginStatus === 'OK' ? 'Tốt' :
                       row.marginStatus === 'LOW_MARGIN' ? 'Thấp' : 'Lỗ' }}
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
import { ref, computed, onMounted, watch } from 'vue'
import {
  Chart as ChartJS,
  CategoryScale, LinearScale, PointElement, LineElement,
  ArcElement, Tooltip, Legend, Filler,
} from 'chart.js'
import { Line, Doughnut } from 'vue-chartjs'
import {
  TrendingUp, TrendingDown, DollarSign, BarChart3, Calendar,
  RefreshCw, Download, AlertTriangle, Percent, PieChart,
  FileText, Users, CheckCircle2, ClipboardList, Building2,
} from 'lucide-vue-next'
import { useFinancialReport } from '../composables/useFinancialReport'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'

// ── Chart.js registration ─────────────────────────────────────────────────────
ChartJS.register(
  CategoryScale, LinearScale, PointElement, LineElement,
  ArcElement, Tooltip, Legend, Filler,
)

// ── Composable ────────────────────────────────────────────────────────────────
const {
  loading, loadingTrend, overview, summary, trend,
  marginStatusColor, lossAlerts, lowMarginAlerts,
  topContracts, worstContracts, loadAll,
} = useFinancialReport()

const toast = useToast()

// ── Filter state ──────────────────────────────────────────────────────────────
const today     = new Date()
const firstOfMonth = new Date(today.getFullYear(), today.getMonth() - 5, 1)
const fromDate  = ref(firstOfMonth.toISOString().split('T')[0])
const toDate    = ref(today.toISOString().split('T')[0])
const granularity = ref('month')
const activeTab   = ref('top')

const granularityOptions = [
  { value: 'month',   label: 'Tháng' },
  { value: 'quarter', label: 'Quý'   },
]

const tableTabs = [
  { value: 'top',   label: 'Top 5 Lợi Nhuận' },
  { value: 'worst', label: 'Rủi Ro Cao'       },
  { value: 'all',   label: 'Tất Cả'           },
]

// ── Reload ────────────────────────────────────────────────────────────────────
async function reload() {
  await loadAll(fromDate.value, toDate.value, granularity.value)
}

onMounted(reload)

// ── Formatters ────────────────────────────────────────────────────────────────
function fmt(val) {
  if (val == null || isNaN(val)) return '0 đ'
  const n = Number(val)
  if (Math.abs(n) >= 1_000_000_000) return (n / 1_000_000_000).toFixed(2) + ' tỷ'
  if (Math.abs(n) >= 1_000_000)     return (n / 1_000_000).toFixed(1) + ' tr'
  return n.toLocaleString('vi-VN') + ' đ'
}

function fmtDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('vi-VN')
}

function pct(part, total) {
  if (!total || total === 0) return '0.0'
  return ((part / total) * 100).toFixed(1)
}

// ── Margin helpers ────────────────────────────────────────────────────────────
const marginColor = computed(() => {
  const m = overview.value.financial.profitMargin
  if (m < 0)  return 'text-rose-600'
  if (m < 20) return 'text-amber-600'
  if (m < 35) return 'text-sky-600'
  return 'text-emerald-600'
})

const marginBarColor = computed(() => {
  const m = overview.value.financial.profitMargin
  if (m < 0)  return 'bg-rose-500'
  if (m < 20) return 'bg-amber-500'
  if (m < 35) return 'bg-sky-500'
  return 'bg-emerald-500'
})

function marginBadgeClass(m) {
  if (m == null) return 'bg-slate-100 text-slate-500'
  if (m < 0)  return 'bg-rose-100 text-rose-700'
  if (m < 20) return 'bg-amber-100 text-amber-700'
  return 'bg-emerald-100 text-emerald-700'
}

// ── Operation cards ───────────────────────────────────────────────────────────
const operationCards = computed(() => [
  {
    label: 'Đoàn Đang Mở',
    value: overview.value.operations.activeGroups,
    icon: ClipboardList,
    bg: 'bg-sky-50',
    iconColor: 'text-sky-600',
  },
  {
    label: 'Đoàn Hoàn Thành',
    value: overview.value.operations.completedGroups,
    icon: CheckCircle2,
    bg: 'bg-emerald-50',
    iconColor: 'text-emerald-600',
  },
  {
    label: 'HĐ Chờ Duyệt',
    value: overview.value.operations.pendingContracts,
    icon: Building2,
    bg: 'bg-amber-50',
    iconColor: 'text-amber-600',
  },
  {
    label: 'Nhân Sự Điều Động',
    value: overview.value.operations.totalStaffDeployed,
    icon: Users,
    bg: 'bg-indigo-50',
    iconColor: 'text-indigo-600',
  },
])

// ── Table rows (tab-driven) ───────────────────────────────────────────────────
const tableRows = computed(() => {
  if (activeTab.value === 'top')   return topContracts.value
  if (activeTab.value === 'worst') return worstContracts.value
  return [...(summary.value.byContract ?? [])].sort((a, b) => b.grossProfit - a.grossProfit)
})

// ── Line chart: Revenue / Cost / Profit trend ─────────────────────────────────
const CHART_COLORS = {
  revenue: { line: '#0ea5e9', fill: 'rgba(14,165,233,0.08)' },
  cost:    { line: '#f97316', fill: 'rgba(249,115,22,0.06)' },
  profit:  { line: '#10b981', fill: 'rgba(16,185,129,0.08)' },
}

const trendChartData = computed(() => {
  const series = trend.value.series ?? []
  return {
    labels: series.map(s => s.periodLabel ?? s.label ?? ''),
    datasets: [
      {
        label: 'Doanh Thu',
        data: series.map(s => s.totalRevenue ?? 0),
        borderColor: CHART_COLORS.revenue.line,
        backgroundColor: CHART_COLORS.revenue.fill,
        borderWidth: 2,
        pointRadius: 3,
        pointHoverRadius: 5,
        tension: 0.4,
        fill: true,
      },
      {
        label: 'Chi Phí',
        data: series.map(s => s.totalCost ?? 0),
        borderColor: CHART_COLORS.cost.line,
        backgroundColor: CHART_COLORS.cost.fill,
        borderWidth: 2,
        pointRadius: 3,
        pointHoverRadius: 5,
        tension: 0.4,
        fill: true,
      },
      {
        label: 'Lợi Nhuận',
        data: series.map(s => s.totalProfit ?? 0),
        borderColor: CHART_COLORS.profit.line,
        backgroundColor: CHART_COLORS.profit.fill,
        borderWidth: 2.5,
        pointRadius: 4,
        pointHoverRadius: 6,
        tension: 0.4,
        fill: true,
      },
    ],
  }
})

const lineOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: {
      position: 'top',
      labels: {
        font: { size: 9, weight: 'bold', family: 'inherit' },
        usePointStyle: true,
        pointStyleWidth: 8,
        padding: 12,
      },
    },
    tooltip: {
      backgroundColor: '#0f172a',
      titleFont: { size: 9, weight: 'bold' },
      bodyFont: { size: 9 },
      padding: 10,
      callbacks: {
        label(ctx) {
          const v = ctx.parsed.y
          if (Math.abs(v) >= 1_000_000_000) return ` ${ctx.dataset.label}: ${(v/1e9).toFixed(2)} tỷ`
          if (Math.abs(v) >= 1_000_000)     return ` ${ctx.dataset.label}: ${(v/1e6).toFixed(1)} tr`
          return ` ${ctx.dataset.label}: ${v.toLocaleString('vi-VN')} đ`
        },
      },
    },
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { font: { size: 8, weight: 'bold' }, color: '#94a3b8' },
    },
    y: {
      grid: { color: 'rgba(148,163,184,0.1)' },
      ticks: {
        font: { size: 8 },
        color: '#94a3b8',
        callback(v) {
          if (Math.abs(v) >= 1e9) return (v/1e9).toFixed(1) + 'B'
          if (Math.abs(v) >= 1e6) return (v/1e6).toFixed(0) + 'M'
          return v
        },
      },
    },
  },
}

// ── Doughnut chart: cost breakdown ────────────────────────────────────────────
const COST_COLORS = ['#3b82f6', '#f59e0b', '#8b5cf6']

const costChartData = computed(() => {
  const s = summary.value.summary
  return {
    labels: ['Lương', 'Vật Tư', 'Overhead'],
    datasets: [{
      data: [s.laborCost ?? 0, s.supplyCost ?? 0, s.overheadCost ?? 0],
      backgroundColor: COST_COLORS,
      borderWidth: 0,
      hoverOffset: 6,
    }],
  }
})

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '72%',
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: '#0f172a',
      titleFont: { size: 9, weight: 'bold' },
      bodyFont: { size: 9 },
      callbacks: {
        label(ctx) {
          const v = ctx.parsed
          if (v >= 1e9) return ` ${(v/1e9).toFixed(2)} tỷ`
          if (v >= 1e6) return ` ${(v/1e6).toFixed(1)} tr`
          return ` ${v.toLocaleString('vi-VN')} đ`
        },
      },
    },
  },
}

const costLegend = computed(() => {
  const s = summary.value.summary
  const total = s.totalRevenue || 1
  return [
    { label: 'Lương',    color: COST_COLORS[0], pct: pct(s.laborCost,    total) },
    { label: 'Vật Tư',   color: COST_COLORS[1], pct: pct(s.supplyCost,   total) },
    { label: 'Overhead', color: COST_COLORS[2], pct: pct(s.overheadCost, total) },
  ]
})

// ── Export ────────────────────────────────────────────────────────────────────
async function exportExcel() {
  try {
    const res = await apiClient.get('/api/reports/financial/export', {
      params: { from: fromDate.value, to: toDate.value },
      responseType: 'blob',
    })
    const url  = URL.createObjectURL(new Blob([res.data]))
    const link = document.createElement('a')
    link.href  = url
    link.setAttribute('download', `BaoCao_TaiChinh_${fromDate.value}_${toDate.value}.xlsx`)
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
    toast.success('Xuất Excel thành công')
  } catch {
    toast.error('Không thể xuất Excel. Vui lòng thử lại.')
  }
}
</script>

<style scoped>
.scrollbar-premium::-webkit-scrollbar { width: 5px; }
.scrollbar-premium::-webkit-scrollbar-track { background: transparent; }
.scrollbar-premium::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.scrollbar-premium::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }
</style>
