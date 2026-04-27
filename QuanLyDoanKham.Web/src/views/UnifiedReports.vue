<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in-up">
    <!-- Header -->
    <div class="flex-shrink-0 mb-4 p-5 bg-white rounded-3xl shadow-sm border border-slate-100 flex flex-wrap items-center justify-between gap-4">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-2xl flex items-center justify-center text-white shadow-lg shadow-indigo-200">
          <BarChart3 class="w-6 h-6" />
        </div>
        <div>
          <h2 class="text-2xl font-black text-slate-800 tracking-tight">Thống Kê Tổng Hợp</h2>
          <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Báo cáo hiệu suất & Phân tích kinh doanh</p>
        </div>
      </div>
      
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-2 bg-slate-50 p-2 rounded-2xl border border-slate-100">
          <input type="date" v-model="filters.from" class="bg-transparent border-none text-xs font-bold text-slate-600 outline-none" />
          <span class="text-slate-300">→</span>
          <input type="date" v-model="filters.to" class="bg-transparent border-none text-xs font-bold text-slate-600 outline-none" />
        </div>
        <button @click="refreshData" class="p-3 bg-primary/10 text-primary rounded-2xl hover:bg-primary/20 transition-all" :class="{'animate-spin': loading}">
          <RefreshCw class="w-4 h-4" />
        </button>
        <button @click="exportData('pdf')" class="px-4 py-3 bg-rose-50 text-rose-600 rounded-2xl text-xs font-bold hover:bg-rose-100 transition-all flex items-center gap-2">
          <FileDown class="w-4 h-4" /> PDF
        </button>
        <button @click="exportData('excel')" class="px-4 py-3 bg-emerald-50 text-emerald-600 rounded-2xl text-xs font-bold hover:bg-emerald-100 transition-all flex items-center gap-2">
          <Download class="w-4 h-4" /> Excel
        </button>
      </div>
    </div>

    <!-- Tab Navigation -->
    <div class="flex-shrink-0 flex gap-2 mb-4">
      <button v-for="tab in tabs" :key="tab.id"
        @click="activeTab = tab.id"
        :id="'tab-' + tab.id"
        class="flex items-center gap-2 px-4 py-2.5 rounded-2xl text-xs font-bold transition-all"
        :class="activeTab === tab.id
          ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-200'
          : 'bg-white text-slate-500 border border-slate-100 hover:border-indigo-200 hover:text-indigo-600'">
        <component :is="tab.icon" class="w-4 h-4" />
        {{ tab.label }}
      </button>
    </div>

    <!-- KPI Cards -->
    <div v-if="activeTab === 'dashboard'" class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <!-- Doanh thu -->
      <div class="bg-gradient-to-br from-indigo-500 to-indigo-700 rounded-2xl p-5 text-white relative overflow-hidden shadow-lg shadow-indigo-200">
        <div class="absolute -right-4 -bottom-4 w-20 h-20 bg-white/10 rounded-full blur-xl"></div>
        <div class="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center mb-3">
          <DollarSign class="w-5 h-5" />
        </div>
        <p class="text-[10px] font-bold uppercase tracking-wider text-indigo-200 mb-1">Tổng Doanh Thu</p>
        <h3 class="text-xl font-black">{{ formatCurrency(dashboard?.financial?.totalRevenue || 0) }}</h3>
      </div>

      <!-- Lợi nhuận -->
      <div class="bg-gradient-to-br from-emerald-500 to-emerald-700 rounded-2xl p-5 text-white relative overflow-hidden shadow-lg shadow-emerald-200">
        <div class="absolute -right-4 -bottom-4 w-20 h-20 bg-white/10 rounded-full blur-xl"></div>
        <div class="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center mb-3">
          <TrendingUp class="w-5 h-5" />
        </div>
        <p class="text-[10px] font-bold uppercase tracking-wider text-emerald-200 mb-1">Lợi Nhuận Gộp</p>
        <h3 class="text-xl font-black">{{ formatCurrency(dashboard?.financial?.grossProfit || 0) }}</h3>
        <p class="text-[10px] text-emerald-200 mt-1">{{ formatPercent(dashboard?.financial?.profitMargin || 0) }} biên lợi nhuận</p>
      </div>

      <!-- Hợp đồng -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
        <div class="w-10 h-10 bg-sky-50 text-sky-600 rounded-xl flex items-center justify-center mb-3">
          <FileCheck class="w-5 h-5" />
        </div>
        <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1">Hợp Đồng</p>
        <h3 class="text-xl font-black text-slate-800">{{ dashboard?.contracts?.total || 0 }}</h3>
        <div class="flex gap-2 mt-2 text-[10px]">
          <span class="px-2 py-1 bg-emerald-50 text-emerald-600 rounded-full font-bold">{{ dashboard?.contracts?.active || 0 }} active</span>
          <span class="px-2 py-1 bg-amber-50 text-amber-600 rounded-full font-bold">{{ dashboard?.contracts?.pending || 0 }} pending</span>
        </div>
      </div>

      <!-- Đoàn khám -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
        <div class="w-10 h-10 bg-violet-50 text-violet-600 rounded-xl flex items-center justify-center mb-3">
          <Stethoscope class="w-5 h-5" />
        </div>
        <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1">Đoàn Khám</p>
        <h3 class="text-xl font-black text-slate-800">{{ dashboard?.medicalGroups?.total || 0 }}</h3>
        <div class="flex gap-2 mt-2 text-[10px]">
          <span class="px-2 py-1 bg-indigo-50 text-indigo-600 rounded-full font-bold">{{ dashboard?.medicalGroups?.inPeriod || 0 }} trong kỳ</span>
        </div>
      </div>
    </div>

    <!-- Charts Row -->
    <div v-if="activeTab === 'dashboard'" class="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6 flex-1 min-h-0">
      <!-- Revenue Chart -->
      <div class="lg:col-span-2 bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-col">
        <div class="flex justify-between items-center mb-4">
          <div>
            <h3 class="font-black text-slate-800">Biểu Đồ Doanh Thu</h3>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Theo tháng (6 tháng gần nhất)</p>
          </div>
          <div class="flex gap-2">
            <span class="flex items-center gap-1 text-[10px] font-bold text-slate-500">
              <span class="w-2 h-2 rounded-full bg-indigo-500"></span> Doanh thu
            </span>
            <span class="flex items-center gap-1 text-[10px] font-bold text-slate-500">
              <span class="w-2 h-2 rounded-full bg-rose-400"></span> Chi phí
            </span>
          </div>
        </div>
        
        <div class="flex-1 flex items-end gap-2 min-h-[200px]">
          <div v-for="(item, i) in revenueChart.data" :key="i" class="flex-1 flex flex-col items-center group cursor-pointer">
            <!-- Tooltip -->
            <div class="opacity-0 group-hover:opacity-100 transition-opacity mb-2 bg-slate-800 text-white text-[10px] px-2 py-1 rounded-lg font-bold whitespace-nowrap">
              {{ formatCurrency(item.revenue) }}
            </div>
            <!-- Bars -->
            <div class="w-full flex flex-col items-center gap-1">
              <div class="w-full bg-indigo-500 rounded-t-lg transition-all hover:bg-indigo-600" :style="{ height: (item.revenue / maxRevenue * 150) + 'px' }"></div>
              <div class="w-full bg-rose-400 rounded-t-lg transition-all hover:bg-rose-500" :style="{ height: (item.cost / maxRevenue * 150) + 'px' }"></div>
            </div>
            <span class="mt-2 text-[9px] font-bold text-slate-400 uppercase">{{ item.month }}</span>
          </div>
        </div>
      </div>

      <!-- Contract Distribution -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-col">
        <h3 class="font-black text-slate-800 mb-4">Phân Bổ Hợp Đồng</h3>
        
        <div class="flex-1 flex flex-col justify-center gap-4">
          <div v-for="(item, i) in contractChart.byStatus" :key="i">
            <div class="flex justify-between text-xs font-bold mb-2">
              <span class="text-slate-600">{{ getStatusLabel(item.status) }}</span>
              <span :class="getStatusColor(item.status)">{{ item.count }}</span>
            </div>
            <div class="w-full bg-slate-100 h-2 rounded-full overflow-hidden">
              <div class="h-full rounded-full transition-all duration-500" :class="getStatusBgColor(item.status)" :style="{ width: (item.count / (dashboard?.contracts?.total || 1) * 100) + '%' }"></div>
            </div>
          </div>
        </div>

        <div class="mt-4 pt-4 border-t border-slate-100">
          <h4 class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-2">Theo tháng</h4>
          <div class="space-y-2">
            <div v-for="(item, i) in contractChart.byMonth.slice(-3)" :key="i" class="flex justify-between text-xs">
              <span class="text-slate-500">{{ item.month }}</span>
              <span class="font-bold text-slate-700">{{ item.count }} hợp đồng</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Row -->
    <div v-if="activeTab === 'dashboard'" class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Top Companies -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-8 h-8 bg-amber-50 text-amber-600 rounded-lg flex items-center justify-center">
            <Building2 class="w-4 h-4" />
          </div>
          <h3 class="font-black text-slate-800">Top Công Ty</h3>
        </div>
        <div class="space-y-3">
          <div v-for="(company, i) in topStats.topCompanies" :key="i" class="flex items-center justify-between p-3 bg-slate-50 rounded-xl">
            <div class="flex items-center gap-3">
              <span class="w-6 h-6 bg-white rounded-lg flex items-center justify-center text-xs font-black text-slate-400 shadow-sm">{{ i + 1 }}</span>
              <div>
                <p class="text-xs font-bold text-slate-800">{{ company.companyName }}</p>
                <p class="text-[10px] text-slate-400">{{ company.contractCount }} hợp đồng</p>
              </div>
            </div>
            <span class="text-xs font-black text-indigo-600">{{ formatCurrency(company.totalRevenue) }}</span>
          </div>
        </div>
      </div>

      <!-- Top Staff -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-8 h-8 bg-emerald-50 text-emerald-600 rounded-lg flex items-center justify-center">
            <Users class="w-4 h-4" />
          </div>
          <h3 class="font-black text-slate-800">Top Nhân Sự</h3>
        </div>
        <div class="space-y-3">
          <div v-for="(staff, i) in topStats.topStaff" :key="i" class="flex items-center justify-between p-3 bg-slate-50 rounded-xl">
            <div class="flex items-center gap-3">
              <span class="w-6 h-6 bg-white rounded-lg flex items-center justify-center text-xs font-black text-slate-400 shadow-sm">{{ i + 1 }}</span>
              <p class="text-xs font-bold text-slate-800">{{ staff.staffName }}</p>
            </div>
            <span class="text-xs font-black text-emerald-600">{{ staff.assignmentCount }} ca</span>
          </div>
        </div>
      </div>

      <!-- Low Stock Alert -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 bg-rose-50 text-rose-600 rounded-lg flex items-center justify-center">
              <AlertTriangle class="w-4 h-4" />
            </div>
            <h3 class="font-black text-slate-800">Cảnh Báo Kho</h3>
          </div>
          <span v-if="lowStockItems.length > 0" class="px-2 py-1 bg-rose-100 text-rose-600 rounded-full text-[10px] font-bold">{{ lowStockItems.length }} mục</span>
        </div>
        
        <div v-if="lowStockItems.length === 0" class="flex flex-col items-center justify-center py-8 text-slate-400">
          <CheckCircle class="w-10 h-10 mb-2 opacity-30" />
          <p class="text-xs font-bold">Kho hàng an toàn</p>
        </div>
        
        <div v-else class="space-y-2 max-h-[200px] overflow-y-auto">
          <div v-for="(item, i) in lowStockItems.slice(0, 5)" :key="i" class="flex items-center justify-between p-3 bg-rose-50 rounded-xl border border-rose-100">
            <div>
              <p class="text-xs font-bold text-slate-800">{{ item.supplyName }}</p>
              <p class="text-[10px] text-slate-400">{{ item.category }}</p>
            </div>
            <div class="text-right">
              <p class="text-xs font-black text-rose-600">{{ item.currentStock }} {{ item.unit }}</p>
              <p class="text-[10px] text-slate-400">Tối thiểu: {{ item.minStockLevel }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ══ TAB: THỐNG KÊ THEO KỲ (P&L Tổng hợp) ══ -->
    <div v-else-if="activeTab === 'period'" class="flex flex-col gap-4" id="tab-period-content">
      <!-- KPI tóm tắt kỳ -->
      <div v-if="periodSummary" class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="bg-gradient-to-br from-indigo-500 to-indigo-700 rounded-2xl p-5 text-white relative overflow-hidden shadow-lg shadow-indigo-200">
          <div class="absolute -right-4 -bottom-4 w-20 h-20 bg-white/10 rounded-full blur-xl"></div>
          <div class="w-9 h-9 bg-white/20 rounded-xl flex items-center justify-center mb-3"><Stethoscope class="w-5 h-5" /></div>
          <p class="text-[10px] font-bold uppercase tracking-wider text-indigo-200 mb-1">Số Đoàn Khám</p>
          <h3 class="text-2xl font-black">{{ periodSummary.totalGroups }}</h3>
          <p class="text-[10px] text-indigo-200 mt-1">{{ periodSummary.totalPatients }} bệnh nhân</p>
        </div>
        <div class="bg-gradient-to-br from-emerald-500 to-emerald-700 rounded-2xl p-5 text-white relative overflow-hidden shadow-lg shadow-emerald-200">
          <div class="absolute -right-4 -bottom-4 w-20 h-20 bg-white/10 rounded-full blur-xl"></div>
          <div class="w-9 h-9 bg-white/20 rounded-xl flex items-center justify-center mb-3"><DollarSign class="w-5 h-5" /></div>
          <p class="text-[10px] font-bold uppercase tracking-wider text-emerald-200 mb-1">Doanh Thu</p>
          <h3 class="text-xl font-black">{{ formatCurrency(periodSummary.totalRevenue) }}</h3>
        </div>
        <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
          <div class="w-9 h-9 bg-rose-50 text-rose-600 rounded-xl flex items-center justify-center mb-3"><TrendingDown class="w-5 h-5" /></div>
          <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1">Tổng Chi Phí</p>
          <h3 class="text-xl font-black text-slate-800">{{ formatCurrency(periodSummary.totalCost) }}</h3>
          <div class="flex flex-col gap-0.5 mt-2 text-[10px] text-slate-500">
            <span>NS: {{ formatCurrency(periodSummary.totalLaborCost) }}</span>
            <span>VT: {{ formatCurrency(periodSummary.totalMaterialCost) }}</span>
          </div>
        </div>
        <div class="rounded-2xl p-5 border shadow-sm" :class="periodSummary.profit >= 0 ? 'bg-emerald-50 border-emerald-200' : 'bg-rose-50 border-rose-200'">
          <div class="w-9 h-9 rounded-xl flex items-center justify-center mb-3" :class="periodSummary.profit >= 0 ? 'bg-emerald-100 text-emerald-600' : 'bg-rose-100 text-rose-600'">
            <TrendingUp class="w-5 h-5" />
          </div>
          <p class="text-[10px] font-bold uppercase tracking-wider mb-1" :class="periodSummary.profit >= 0 ? 'text-emerald-400' : 'text-rose-400'">Lợi Nhuận</p>
          <h3 class="text-xl font-black" :class="periodSummary.profit >= 0 ? 'text-emerald-700' : 'text-rose-700'">{{ formatCurrency(periodSummary.profit) }}</h3>
          <p class="text-[10px] mt-1 font-bold" :class="periodSummary.profit >= 0 ? 'text-emerald-500' : 'text-rose-500'">Biên: {{ periodSummary.profitMargin.toFixed(1) }}%</p>
        </div>
      </div>

      <!-- Bảng chi tiết theo kỳ -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
        <div class="p-5 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h3 class="font-black text-slate-800">Chi Tiết P&L Theo Kỳ</h3>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mt-0.5">
              {{ periodSummary?.from }} → {{ periodSummary?.to }}
            </p>
          </div>
        </div>
        <div v-if="!periodSummary" class="flex items-center justify-center py-16 text-slate-400">
          <div class="text-center"><BarChart3 class="w-12 h-12 mx-auto mb-3 opacity-20" /><p class="text-sm font-bold">Chọn khoảng thời gian và nhấn Làm mới</p></div>
        </div>
        <div v-else class="p-5">
          <div class="space-y-2">
            <div v-for="row in periodBreakdown" :key="row.label"
              class="flex items-center justify-between p-4 rounded-xl"
              :class="row.color">
              <div class="flex items-center gap-3">
                <span class="text-lg">{{ row.icon }}</span>
                <span class="text-sm font-bold text-slate-700">{{ row.label }}</span>
              </div>
              <span class="text-sm font-black" :class="row.textColor">{{ formatCurrency(row.value) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ══ TAB: P&L THEO ĐOÀN ══ -->
    <div v-else-if="activeTab === 'pnl'" class="flex flex-col gap-4" id="tab-pnl-content">
      <!-- Chọn đoàn khám -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-wrap items-center gap-4">
        <div class="flex items-center gap-2">
          <Stethoscope class="w-5 h-5 text-indigo-500" />
          <span class="text-sm font-bold text-slate-700">Chọn đoàn khám:</span>
        </div>
        <select v-model="selectedGroupId" id="pnl-group-select"
          class="flex-1 min-w-[250px] bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm font-bold text-slate-700 outline-none focus:ring-2 focus:ring-indigo-300">
          <option value="">-- Chọn đoàn --</option>
          <option v-for="g in groupList" :key="g.groupId" :value="g.groupId">
            {{ g.examDate }} | {{ g.groupName }} ({{ g.companyName }})
          </option>
        </select>
        <button @click="loadGroupPnl" id="btn-load-pnl"
          :disabled="!selectedGroupId || pnlLoading"
          class="px-5 py-2.5 bg-indigo-600 text-white rounded-xl text-sm font-bold hover:bg-indigo-700 disabled:opacity-40 transition-all flex items-center gap-2">
          <RefreshCw class="w-4 h-4" :class="{'animate-spin': pnlLoading}" /> Xem P&L
        </button>
        <button v-if="groupPnl" @click="exportGroupPnl" id="btn-export-pnl"
          class="px-5 py-2.5 bg-emerald-600 text-white rounded-xl text-sm font-bold hover:bg-emerald-700 transition-all flex items-center gap-2">
          <Download class="w-4 h-4" /> Xuất Excel
        </button>
      </div>

      <!-- P&L Card -->
      <div v-if="groupPnl" class="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <!-- KPI summary -->
        <div class="flex flex-col gap-3">
          <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
            <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-1">Đoàn khám</p>
            <p class="text-base font-black text-slate-800">{{ groupPnl.groupName }}</p>
            <p class="text-xs text-slate-500 mt-1">{{ groupPnl.companyName }}</p>
            <div class="flex gap-3 mt-3 text-[10px]">
              <span class="px-2 py-1 bg-indigo-50 text-indigo-600 rounded-full font-bold">📅 {{ groupPnl.examDate }}</span>
              <span class="px-2 py-1 bg-slate-50 text-slate-600 rounded-full font-bold">👥 {{ groupPnl.patientCount }} BN</span>
            </div>
          </div>

          <div class="bg-gradient-to-br from-indigo-500 to-indigo-700 rounded-2xl p-5 text-white">
            <p class="text-[10px] font-bold uppercase tracking-wider text-indigo-200 mb-1">Doanh Thu HĐ</p>
            <h3 class="text-xl font-black">{{ formatCurrency(groupPnl.contractValue) }}</h3>
          </div>

          <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm">
            <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-2">Chi Phí</p>
            <div class="space-y-2">
              <div class="flex justify-between text-xs">
                <span class="text-slate-600">👥 Nhân sự</span>
                <span class="font-bold text-rose-600">{{ formatCurrency(groupPnl.laborCost) }}</span>
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-600">📦 Vật tư</span>
                <span class="font-bold text-rose-600">{{ formatCurrency(groupPnl.materialCost) }}</span>
              </div>
              <div class="flex justify-between text-xs">
                <span class="text-slate-600">🔧 Khác</span>
                <span class="font-bold text-rose-600">{{ formatCurrency(groupPnl.otherCost) }}</span>
              </div>
              <div class="flex justify-between text-xs pt-2 border-t border-slate-100">
                <span class="font-bold text-slate-700">Tổng chi phí</span>
                <span class="font-black text-rose-700">{{ formatCurrency(groupPnl.totalCost) }}</span>
              </div>
            </div>
          </div>

          <!-- Lợi nhuận -->
          <div class="rounded-2xl p-5 border shadow-sm"
            :class="groupPnl.profit >= 0 ? 'bg-emerald-50 border-emerald-200' : 'bg-rose-50 border-rose-200'">
            <p class="text-[10px] font-bold uppercase tracking-wider mb-1"
              :class="groupPnl.profit >= 0 ? 'text-emerald-400' : 'text-rose-400'">Lợi Nhuận Ròng</p>
            <h3 class="text-2xl font-black"
              :class="groupPnl.profit >= 0 ? 'text-emerald-700' : 'text-rose-700'">
              {{ formatCurrency(groupPnl.profit) }}
            </h3>
            <div class="mt-3">
              <div class="w-full bg-white rounded-full h-2 overflow-hidden">
                <div class="h-full rounded-full transition-all duration-700"
                  :class="groupPnl.profit >= 0 ? 'bg-emerald-500' : 'bg-rose-500'"
                  :style="{ width: Math.min(Math.abs(groupPnl.profitMargin), 100) + '%' }"></div>
              </div>
              <p class="text-xs font-black mt-1.5" :class="groupPnl.profit >= 0 ? 'text-emerald-600' : 'text-rose-600'">
                Biên lợi nhuận: {{ groupPnl.profitMargin.toFixed(1) }}%
              </p>
            </div>
          </div>
        </div>

        <!-- Chi tiết nhân sự + vật tư -->
        <div class="lg:col-span-2 flex flex-col gap-4">
          <!-- Nhân sự -->
          <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
            <div class="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 class="font-black text-slate-800 flex items-center gap-2">
                <span class="w-7 h-7 bg-indigo-50 text-indigo-600 rounded-lg flex items-center justify-center text-sm">👥</span>
                Chi Tiết Nhân Sự
              </h3>
              <span class="text-xs font-bold text-slate-400">{{ groupPnl.staffLines.length }} người</span>
            </div>
            <div class="overflow-x-auto">
              <table class="w-full text-xs">
                <thead>
                  <tr class="bg-slate-50">
                    <th class="px-4 py-3 text-left font-bold text-slate-500">Nhân viên</th>
                    <th class="px-4 py-3 text-center font-bold text-slate-500">Ca làm</th>
                    <th class="px-4 py-3 text-right font-bold text-slate-500">Đơn giá/ngày</th>
                    <th class="px-4 py-3 text-right font-bold text-slate-500">Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="!groupPnl.staffLines.length">
                    <td colspan="4" class="px-4 py-8 text-center text-slate-400 italic">Chưa có nhân sự được gán</td>
                  </tr>
                  <tr v-for="(staff, i) in groupPnl.staffLines" :key="i"
                    class="border-t border-slate-50 hover:bg-slate-50 transition-colors">
                    <td class="px-4 py-3 font-bold text-slate-700">{{ staff.staffName }}</td>
                    <td class="px-4 py-3 text-center">
                      <span class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                        :class="staff.shiftType === 1 ? 'bg-indigo-50 text-indigo-600' : 'bg-amber-50 text-amber-600'">
                        {{ staff.shiftType === 1 ? 'Cả ngày' : 'Nửa ngày' }}
                      </span>
                    </td>
                    <td class="px-4 py-3 text-right text-slate-600">{{ formatCurrency(staff.dailyRate) }}</td>
                    <td class="px-4 py-3 text-right font-bold text-slate-800">{{ formatCurrency(staff.salary) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="border-t-2 border-slate-200 bg-rose-50">
                    <td colspan="3" class="px-4 py-3 font-black text-slate-700">Tổng chi phí nhân sự</td>
                    <td class="px-4 py-3 text-right font-black text-rose-700">{{ formatCurrency(groupPnl.laborCost) }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>

          <!-- Vật tư -->
          <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
            <div class="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 class="font-black text-slate-800 flex items-center gap-2">
                <span class="w-7 h-7 bg-violet-50 text-violet-600 rounded-lg flex items-center justify-center text-sm">📦</span>
                Chi Tiết Vật Tư
              </h3>
              <span class="text-xs font-bold text-slate-400">{{ groupPnl.materialLines.length }} mặt hàng</span>
            </div>
            <div class="overflow-x-auto">
              <table class="w-full text-xs">
                <thead>
                  <tr class="bg-slate-50">
                    <th class="px-4 py-3 text-left font-bold text-slate-500">Tên vật tư</th>
                    <th class="px-4 py-3 text-center font-bold text-slate-500">ĐVT</th>
                    <th class="px-4 py-3 text-center font-bold text-slate-500">Số lượng</th>
                    <th class="px-4 py-3 text-right font-bold text-slate-500">Đơn giá</th>
                    <th class="px-4 py-3 text-right font-bold text-slate-500">Thành tiền</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="!groupPnl.materialLines.length">
                    <td colspan="5" class="px-4 py-8 text-center text-slate-400 italic">Chưa có vật tư xuất kho</td>
                  </tr>
                  <tr v-for="(mat, i) in groupPnl.materialLines" :key="i"
                    class="border-t border-slate-50 hover:bg-slate-50 transition-colors">
                    <td class="px-4 py-3 font-bold text-slate-700">{{ mat.itemName }}</td>
                    <td class="px-4 py-3 text-center text-slate-500">{{ mat.unit }}</td>
                    <td class="px-4 py-3 text-center font-bold text-slate-700">{{ mat.quantity }}</td>
                    <td class="px-4 py-3 text-right text-slate-600">{{ formatCurrency(mat.unitPrice) }}</td>
                    <td class="px-4 py-3 text-right font-bold text-slate-800">{{ formatCurrency(mat.totalValue) }}</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="border-t-2 border-slate-200 bg-rose-50">
                    <td colspan="4" class="px-4 py-3 font-black text-slate-700">Tổng chi phí vật tư</td>
                    <td class="px-4 py-3 text-right font-black text-rose-700">{{ formatCurrency(groupPnl.materialCost) }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-else class="flex flex-col items-center justify-center py-20 text-slate-400">
        <TrendingUp class="w-16 h-16 mb-4 opacity-20" />
        <p class="text-base font-black">Chọn đoàn khám để xem P&L chi tiết</p>
        <p class="text-sm mt-1">Chi phí nhân sự, vật tư và lợi nhuận từng đoàn</p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'
import { parseApiError } from '../services/errorHelper'
import {
  BarChart3, RefreshCw, FileDown, Download, DollarSign, TrendingUp, TrendingDown,
  FileCheck, Stethoscope, Building2, Users, AlertTriangle, CheckCircle
} from 'lucide-vue-next'

const toast = useToast()
const loading = ref(false)

// ── Tab navigation ──────────────────────────────────────────────
const activeTab = ref('dashboard')
const tabs = [
  { id: 'dashboard', label: 'Tổng quan', icon: BarChart3 },
  { id: 'period',    label: 'Thống kê theo kỳ', icon: TrendingUp },
  { id: 'pnl',       label: 'P&L Đoàn khám', icon: DollarSign },
]

// Filters
const filters = ref({
  from: new Date(new Date().getFullYear(), new Date().getMonth() - 5, 1).toISOString().split('T')[0],
  to: new Date().toISOString().split('T')[0]
})

// Data
const dashboard = ref(null)
const revenueChart = ref({ data: [] })
const contractChart = ref({ byStatus: [], byMonth: [] })
const topStats = ref({ topCompanies: [], topStaff: [], topMedicalGroups: [] })
const lowStockItems = ref([])

// ── Period Summary (tab Thống kê theo kỳ) ──────────────────────
const periodSummary = ref(null)

const periodBreakdown = computed(() => {
  if (!periodSummary.value) return []
  const p = periodSummary.value
  return [
    { icon: '💰', label: 'Doanh thu hợp đồng',  value: p.totalRevenue,      color: 'bg-indigo-50',  textColor: 'text-indigo-700' },
    { icon: '👥', label: 'Chi phí nhân sự',      value: p.totalLaborCost,    color: 'bg-rose-50',    textColor: 'text-rose-600' },
    { icon: '📦', label: 'Chi phí vật tư',       value: p.totalMaterialCost, color: 'bg-rose-50',    textColor: 'text-rose-600' },
    { icon: '🔧', label: 'Chi phí khác',          value: p.totalOtherCost,    color: 'bg-rose-50',    textColor: 'text-rose-600' },
    { icon: '📊', label: 'Tổng chi phí',          value: p.totalCost,         color: 'bg-orange-50',  textColor: 'text-orange-700' },
    { icon: p.profit >= 0 ? '✅' : '❌',
      label: p.profit >= 0 ? 'Lợi nhuận ròng' : 'Lỗ ròng',
      value: p.profit,
      color: p.profit >= 0 ? 'bg-emerald-50' : 'bg-red-50',
      textColor: p.profit >= 0 ? 'text-emerald-700' : 'text-red-700' },
  ]
})

const fetchPeriodSummary = async () => {
  try {
    const params = { from: filters.value.from, to: filters.value.to }
    const res = await apiClient.get('/api/Analytics/period-summary', { params })
    periodSummary.value = res.data
  } catch (error) {
    toast.error(parseApiError(error))
  }
}

// ── Group PnL (tab P&L Đoàn khám) ─────────────────────────────
const groupList   = ref([])
const selectedGroupId = ref('')
const groupPnl    = ref(null)
const pnlLoading  = ref(false)

const fetchGroupList = async () => {
  try {
    const res = await apiClient.get('/api/Analytics/groups')
    groupList.value = res.data
  } catch {/* bỏ qua nếu lỗi */}
}

const loadGroupPnl = async () => {
  if (!selectedGroupId.value) return
  pnlLoading.value = true
  try {
    const res = await apiClient.get(`/api/Analytics/group-pnl/${selectedGroupId.value}`)
    groupPnl.value = res.data
  } catch (error) {
    toast.error(parseApiError(error))
  } finally {
    pnlLoading.value = false
  }
}

const exportGroupPnl = async () => {
  if (!selectedGroupId.value) return
  try {
    const res = await apiClient.get(
      `/api/MedicalGroups/${selectedGroupId.value}/export-pnl`,
      { responseType: 'blob' }
    )
    const blob = new Blob([res.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    })
    const url  = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href  = url
    link.download = `PNL_Doan${selectedGroupId.value}_${new Date().toISOString().split('T')[0]}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    toast.success('Đã xuất Excel P&L')
  } catch (error) {
    toast.error(parseApiError(error))
  }
}

// Computed
const maxRevenue = computed(() => {
  if (!revenueChart.value.data.length) return 1
  return Math.max(...revenueChart.value.data.map(d => Math.max(d.revenue, d.cost)))
})

// Methods
const fetchDashboard = async () => {
  const params = { from: filters.value.from, to: filters.value.to }
  const res = await apiClient.get('/api/UnifiedReports/dashboard', { params })
  dashboard.value = res.data
}

const fetchRevenueChart = async () => {
  const from = new Date()
  from.setMonth(from.getMonth() - 5)
  const to = new Date()
  
  const params = { 
    from: from.toISOString().split('T')[0], 
    to: to.toISOString().split('T')[0] 
  }
  const res = await apiClient.get('/api/UnifiedReports/charts/revenue', { params })
  revenueChart.value = res.data
}

const fetchContractChart = async () => {
  const from = new Date(filters.value.from)
  const to = new Date(filters.value.to)
  
  const params = { 
    from: from.toISOString().split('T')[0], 
    to: to.toISOString().split('T')[0] 
  }
  const res = await apiClient.get('/api/UnifiedReports/charts/contracts', { params })
  contractChart.value = res.data
}

const fetchTopStats = async () => {
  const params = { from: filters.value.from, to: filters.value.to }
  const res = await apiClient.get('/api/UnifiedReports/top-stats', { params })
  topStats.value = res.data
}

const fetchLowStock = async () => {
  const res = await apiClient.get('/api/UnifiedReports/low-stock')
  lowStockItems.value = res.data
}

const refreshData = async () => {
  loading.value = true
  try {
    const dashboardTasks = [
      fetchDashboard(),
      fetchRevenueChart(),
      fetchContractChart(),
      fetchTopStats(),
      fetchLowStock()
    ]
    // Load period summary nếu đang ở tab period
    if (activeTab.value === 'period') dashboardTasks.push(fetchPeriodSummary())
    await Promise.all(dashboardTasks)
    toast.success('Đã cập nhật dữ liệu thống kê')
  } catch (error) {
    toast.error(parseApiError(error))
  } finally {
    loading.value = false
  }
}

const exportData = async (type) => {
  try {
    const params = { from: filters.value.from, to: filters.value.to }
    const endpoint = type === 'pdf' ? '/api/UnifiedReports/export/pdf' : '/api/UnifiedReports/export/excel'
    const res = await apiClient.get(endpoint, { params, responseType: 'blob' })
    
    const blob = new Blob([res.data], { type: type === 'pdf' ? 'application/pdf' : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `BaoCao_ThongKe_${new Date().toISOString().split('T')[0]}.${type === 'pdf' ? 'pdf' : 'xlsx'}`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
    toast.success(`Đã xuất ${type.toUpperCase()}`)
  } catch (error) {
    toast.error(parseApiError(error))
  }
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(value)
}

const formatPercent = (value) => {
  return new Intl.NumberFormat('vi-VN', { style: 'percent', minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(value / 100)
}

const getStatusLabel = (status) => {
  const map = {
    'Active': 'Đang hoạt động',
    'Finished': 'Đã hoàn thành',
    'Pending': 'Chờ xử lý',
    'Draft': 'Bản nháp',
    'Cancelled': 'Đã hủy'
  }
  return map[status] || status
}

const getStatusColor = (status) => {
  const map = {
    'Active': 'text-emerald-600',
    'Finished': 'text-indigo-600',
    'Pending': 'text-amber-600',
    'Draft': 'text-slate-600',
    'Cancelled': 'text-rose-600'
  }
  return map[status] || 'text-slate-600'
}

const getStatusBgColor = (status) => {
  const map = {
    'Active': 'bg-emerald-500',
    'Finished': 'bg-indigo-500',
    'Pending': 'bg-amber-500',
    'Draft': 'bg-slate-500',
    'Cancelled': 'bg-rose-500'
  }
  return map[status] || 'bg-slate-500'
}

onMounted(async () => {
  await Promise.all([
    refreshData(),
    fetchGroupList(),
    fetchPeriodSummary()
  ])
})
</script>

<style scoped>
.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.animate-fade-in-up {
  animation: fadeInUp 0.5s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
