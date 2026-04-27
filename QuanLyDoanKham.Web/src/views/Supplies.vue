<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans p-6">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-10">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20">
            <Package class="w-6 h-6" />
          </div>
          {{ i18n.t('supplies.title') || 'Kho Vật Tư Y Tế' }}
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[9px] mt-2">Quản lý nhập xuất & theo dõi tồn kho tiêu hao</p>
      </div>

      <div class="flex items-center gap-4">
        <!-- Tab Switcher -->
        <div class="flex bg-slate-100 p-1.5 rounded-2xl border border-slate-200 shadow-inner">
            <button @click="activeTab = 'inventory'" :class="['px-6 py-2.5 rounded-xl text-[11px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'inventory' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <Box class="w-4 h-4" />
                Tồn Kho
            </button>
            <button @click="activeTab = 'history'" :class="['px-6 py-2.5 rounded-xl text-[11px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'history' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <History class="w-4 h-4" />
                Lịch Sử
            </button>
            <button @click="activeTab = 'report'" :class="['px-6 py-2.5 rounded-xl text-[11px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'report' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <FileText class="w-4 h-4" />
                Báo Cáo
            </button>
        </div>

        <button v-if="can('Kho.Edit') && activeTab === 'inventory'" 
                @click="openAddModal" class="btn-premium primary">
            <Plus class="w-5 h-5" />
            <span>Thêm Vật Tư</span>
        </button>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-8 mb-10">
        <div class="premium-card p-8 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-6">
            <div class="w-16 h-16 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center shadow-inner">
                <Box class="w-8 h-8" />
            </div>
            <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng loại vật tư</p>
                <h3 class="text-3xl font-black text-slate-800 tabular-nums">{{ supplies.length }}</h3>
            </div>
        </div>
        <div class="premium-card p-8 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-6">
            <div class="w-16 h-16 bg-rose-50 text-rose-600 rounded-3xl flex items-center justify-center shadow-inner">
                <AlertTriangle class="w-8 h-8" />
            </div>
            <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Sắp hết hàng</p>
                <h3 class="text-3xl font-black text-rose-600 tabular-nums">{{ lowStockItems.length }}</h3>
            </div>
        </div>
        <div class="premium-card p-8 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-6">
            <div class="w-16 h-16 bg-emerald-50 text-emerald-600 rounded-3xl flex items-center justify-center shadow-inner">
                <TrendingUp class="w-8 h-8" />
            </div>
            <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Giá trị kho ước tính</p>
                <h3 class="text-3xl font-black text-emerald-600 tabular-nums">{{ formatCurrency(totalValue) }}</h3>
            </div>
        </div>
    </div>

    <!-- Main Content Section -->
    <div v-if="activeTab === 'inventory'" class="premium-card bg-white border border-slate-100 overflow-hidden">
        <div class="p-6 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
            <h4 class="text-xs font-black text-slate-800 uppercase tracking-widest">Danh mục tồn kho thực tế</h4>
            <div class="flex items-center gap-3">
                <div class="relative">
                    <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                    <input type="text" v-model="searchQuery" placeholder="Tìm kiếm vật tư..." class="pl-10 pr-4 py-2 bg-white border border-slate-200 rounded-xl text-xs outline-none focus:border-primary transition-all w-64" />
                </div>
            </div>
        </div>
        
        <div class="overflow-x-auto scrollbar-premium">
            <table class="w-full text-left">
                <thead>
                    <tr class="bg-slate-50/50 text-[10px] font-black text-slate-400 uppercase tracking-widest">
                        <th class="p-6">Vật Tư & Phân Loại</th>
                        <th class="p-6 text-center">Đơn Vị</th>
                        <th class="p-6 text-center">Tồn Kho</th>
                        <th class="p-6 text-center">Trạng Thái</th>
                        <th class="p-6 text-right">Giá Tham Khảo</th>
                        <th class="p-6 text-center">Thao Tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="s in filteredSupplies" :key="s.supplyId" class="hover:bg-slate-50/50 transition-all group">
                        <td class="p-6">
                            <div class="flex items-center gap-4">
                                <div :class="['w-10 h-10 rounded-xl flex items-center justify-center shadow-sm border border-slate-100', getCategoryBg(s.category)]">
                                    <component :is="getCategoryIcon(s.category)" class="w-5 h-5" />
                                </div>
                                <div>
                                    <p class="font-black text-slate-800 uppercase tracking-widest text-sm">{{ s.itemName }}</p>
                                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ s.category }}</p>
                                </div>
                            </div>
                        </td>
                        <td class="p-6 text-center">
                            <span class="text-[10px] font-black text-slate-500 uppercase">{{ s.unit }}</span>
                        </td>
                        <td class="p-6 text-center">
                            <div class="flex flex-col items-center gap-1">
                                <span :class="['text-lg font-black tabular-nums', s.currentStock <= s.minStockLevel ? 'text-rose-600' : 'text-slate-800']">
                                    {{ s.currentStock }}
                                </span>
                                <span class="text-[8px] font-black text-slate-300 uppercase tracking-widest">Ngưỡng: {{ s.minStockLevel }}</span>
                            </div>
                        </td>
                        <td class="p-6">
                            <div class="flex flex-col gap-2 w-32 mx-auto">
                                <div class="h-1.5 bg-slate-100 rounded-full overflow-hidden">
                                    <div :class="['h-full rounded-full transition-all duration-1000', getStockColor(s)]" 
                                         :style="{ width: Math.min((s.currentStock / 100) * 100, 100) + '%' }"></div>
                                </div>
                                <p :class="['text-[8px] font-black uppercase tracking-widest text-center italic', getStockTextClass(s)]">
                                    {{ getStockLabel(s) }}
                                </p>
                            </div>
                        </td>
                        <td class="p-6 text-right">
                            <p class="font-black text-slate-700 tabular-nums">{{ formatCurrency(s.typicalUnitPrice) }}</p>
                        </td>
                        <td class="p-6">
                            <div class="flex items-center justify-center gap-2 transition-all">
                                <button @click="openHistory(s)" class="p-2.5 rounded-xl hover:bg-indigo-50 text-slate-400 hover:text-indigo-600 transition-all hover:scale-110 group/btn" title="Lịch sử">
                                    <History class="w-4 h-4" />
                                </button>
                                <button v-if="can('Kho.Edit')" @click="openImportModal(s)" class="p-2.5 rounded-xl bg-emerald-50 text-emerald-600 hover:bg-emerald-100 transition-all hover:scale-110 shadow-sm border border-emerald-100" title="Nhập bổ sung">
                                    <ArrowDownLeft class="w-4 h-4" />
                                </button>
                                <button v-if="can('Kho.Edit')" @click="openExportModal(s)" class="p-2.5 rounded-xl bg-rose-50 text-rose-600 hover:bg-rose-100 transition-all hover:scale-110 shadow-sm border border-rose-100" title="Xuất cho đoàn">
                                    <ArrowUpRight class="w-4 h-4" />
                                </button>
                                <button v-if="can('Kho.Edit')" @click="confirmDelete(s)" class="p-2.5 rounded-xl hover:bg-rose-50 text-slate-400 hover:text-rose-600 transition-all hover:scale-110" title="Xóa">
                                    <Trash2 class="w-4 h-4" />
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr v-if="filteredSupplies.length === 0">
                        <td colspan="6" class="p-20 text-center">
                            <div class="flex flex-col items-center justify-center text-slate-300">
                                <Inbox class="w-16 h-16 mb-4 opacity-10" />
                                <p class="text-xs font-black uppercase tracking-[0.2em]">{{ searchQuery ? 'Không tìm thấy vật tư phù hợp' : 'Chưa có dữ liệu vật tư' }}</p>
                                <button v-if="!searchQuery && can('Kho.Edit')" @click="openAddModal" class="mt-6 btn-premium secondary">Thêm vật tư đầu tiên</button>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Global History Section -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-8 animate-fade-in-up">
        <!-- Import History (Left) -->
        <div class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col h-[700px]">
            <div class="p-6 border-b border-emerald-50 bg-emerald-50/20 flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <div class="w-8 h-8 bg-emerald-100 text-emerald-600 rounded-lg flex items-center justify-center shadow-sm">
                        <ArrowDownLeft class="w-5 h-5" />
                    </div>
                    <h4 class="text-xs font-black text-emerald-800 uppercase tracking-widest">Lịch sử Nhập kho bổ sung</h4>
                </div>
                <span class="px-3 py-1 bg-emerald-50 text-emerald-600 rounded-full text-[10px] font-black tabular-nums">{{ importHistory.length }} bản ghi</span>
            </div>
            <div class="flex-1 overflow-y-auto scrollbar-premium p-4 space-y-3">
                <div v-for="m in importHistory" :key="m.movementId" class="p-4 bg-slate-50/50 rounded-2xl border border-slate-100 hover:border-emerald-200 transition-all group">
                    <div class="flex justify-between items-start mb-2">
                        <div>
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[11px]">{{ m.itemName }}</p>
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ formatDate(m.movementDate, true) }}</p>
                        </div>
                        <div class="text-right text-emerald-600">
                            <p class="text-sm font-black tabular-nums">+{{ m.quantity }}</p>
                            <p class="text-[8px] font-black uppercase tracking-widest opacity-60">{{ m.unit }}</p>
                        </div>
                    </div>
                    <div class="flex justify-between items-center mt-3 pt-3 border-t border-slate-100/50 text-[9px] font-black uppercase tracking-widest text-slate-400">
                        <div class="flex items-center gap-2">
                            <span class="text-slate-300">Ghi chú:</span>
                            <span class="text-slate-500 italic">"{{ m.note || 'Không có' }}"</span>
                        </div>
                        <span class="text-slate-300">Bởi: {{ m.recordedByUser?.username || 'Hệ thống' }}</span>
                    </div>
                </div>
                <div v-if="importHistory.length === 0" class="h-full flex flex-col items-center justify-center opacity-20 py-20">
                    <Inbox class="w-12 h-12 mb-4" />
                    <p class="text-[10px] font-black uppercase tracking-widest">Chưa có dữ liệu nhập</p>
                </div>
            </div>
        </div>

        <!-- Export History (Right) -->
        <div class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col h-[700px]">
            <div class="p-6 border-b border-rose-50 bg-rose-50/20 flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <div class="w-8 h-8 bg-rose-100 text-rose-600 rounded-lg flex items-center justify-center shadow-sm">
                        <ArrowUpRight class="w-5 h-5" />
                    </div>
                    <h4 class="text-xs font-black text-rose-800 uppercase tracking-widest">Lịch sử Xuất kho cho đoàn</h4>
                </div>
                <span class="px-3 py-1 bg-rose-50 text-rose-600 rounded-full text-[10px] font-black tabular-nums">{{ exportHistory.length }} bản ghi</span>
            </div>
            <div class="flex-1 overflow-y-auto scrollbar-premium p-4 space-y-3">
                <div v-for="m in exportHistory" :key="m.movementId" class="p-4 bg-slate-50/50 rounded-2xl border border-slate-100 hover:border-rose-200 transition-all group">
                    <div class="flex justify-between items-start mb-2">
                        <div>
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[11px]">{{ m.itemName }}</p>
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ formatDate(m.movementDate, true) }}</p>
                        </div>
                        <div class="text-right text-rose-600">
                            <p class="text-sm font-black tabular-nums">-{{ m.quantity }}</p>
                            <p class="text-[8px] font-black uppercase tracking-widest opacity-60">{{ m.unit }}</p>
                        </div>
                    </div>
                    <div class="flex flex-col gap-2 mt-3 pt-3 border-t border-slate-100/50">
                        <div v-if="m.medicalGroup" class="flex items-center gap-2">
                            <span class="text-[9px] font-black text-slate-300 uppercase tracking-widest">Cấp cho:</span>
                            <span class="text-[10px] font-black text-rose-500 uppercase tracking-widest bg-rose-50 px-2 py-0.5 rounded-md border border-rose-100">{{ m.medicalGroup.groupName }}</span>
                        </div>
                        <div class="flex justify-between items-center text-[9px] font-black uppercase tracking-widest text-slate-400">
                            <div class="flex items-center gap-2">
                                <span class="text-slate-300">Ghi chú:</span>
                                <span class="text-slate-500 italic">"{{ m.note || 'Không có' }}"</span>
                            </div>
                            <span class="text-slate-300">Bởi: {{ m.recordedByUser?.username || 'Hệ thống' }}</span>
                        </div>
                    </div>
                </div>
                <div v-if="exportHistory.length === 0" class="h-full flex flex-col items-center justify-center opacity-20 py-20">
                    <Inbox class="w-12 h-12 mb-4" />
                    <p class="text-[10px] font-black uppercase tracking-widest">Chưa có dữ liệu xuất</p>
                </div>
            </div>
        </div>
    </div>

    <!-- Report Content Section -->
    <div v-if="activeTab === 'report'" class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col min-h-[500px]">
        <div class="p-6 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
            <h4 class="text-xs font-black text-slate-800 uppercase tracking-widest">Báo Cáo Tồn Kho Định Kỳ</h4>
            <div class="flex items-center gap-3">
                <div class="flex items-center gap-2 bg-white px-3 py-1.5 rounded-xl border border-slate-200">
                    <input type="date" v-model="reportFilters.from" class="bg-transparent border-none text-[10px] font-black text-slate-600 outline-none" />
                    <span class="text-slate-300">→</span>
                    <input type="date" v-model="reportFilters.to" class="bg-transparent border-none text-[10px] font-black text-slate-600 outline-none" />
                </div>
                <button @click="fetchPeriodicReport" class="p-2.5 bg-primary/10 text-primary rounded-xl hover:bg-primary/20 transition-all">
                    <RefreshCw class="w-4 h-4" :class="{'animate-spin': loadingReport}" />
                </button>
            </div>
        </div>

        <div v-if="loadingReport" class="flex-1 flex justify-center items-center py-20">
            <Loader2 class="w-8 h-8 text-slate-300 animate-spin" />
        </div>

        <div v-else-if="periodicReport.length > 0" class="overflow-x-auto scrollbar-premium">
            <table class="w-full text-left">
                <thead>
                    <tr class="bg-slate-50/50 text-[10px] font-black text-slate-400 uppercase tracking-widest">
                        <th class="p-4 pl-6">Vật Tư</th>
                        <th class="p-4 text-center">ĐVT</th>
                        <th class="p-4 text-center text-slate-600">Tồn Đầu Kỳ</th>
                        <th class="p-4 text-center text-emerald-600">Nhập Trong Kỳ</th>
                        <th class="p-4 text-center text-rose-600">Xuất Trong Kỳ</th>
                        <th class="p-4 text-center text-primary">Tồn Cuối Kỳ</th>
                        <th class="p-4 pr-6 text-right">Tổng Giá Trị</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="r in periodicReport" :key="r.supplyId" class="hover:bg-slate-50/50 transition-all">
                        <td class="p-4 pl-6">
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[11px]">{{ r.itemName }}</p>
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ r.category }}</p>
                        </td>
                        <td class="p-4 text-center text-[10px] font-black text-slate-500 uppercase">{{ r.unit }}</td>
                        <td class="p-4 text-center font-black text-slate-600">{{ r.tonDauKy }}</td>
                        <td class="p-4 text-center font-black text-emerald-600">+{{ r.nhapTrongKy }}</td>
                        <td class="p-4 text-center font-black text-rose-600">-{{ r.xuatTrongKy }}</td>
                        <td class="p-4 text-center font-black text-primary text-lg">{{ r.tonCuoiKy }}</td>
                        <td class="p-4 pr-6 text-right font-black text-slate-700">{{ formatCurrency(r.totalValue) }}</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-else class="flex-1 flex flex-col items-center justify-center opacity-30 py-20">
            <Box class="w-16 h-16 mb-4 text-slate-400" />
            <p class="text-[10px] font-black uppercase tracking-widest text-slate-500">Chưa có dữ liệu báo cáo</p>
        </div>
    </div>

    <!-- Modals -->
    <Teleport to="body">
        <!-- Add New Modal -->
        <div v-if="modals.add.show" class="modal-overlay" @click.self="modals.add.show = false">
            <div class="modal-box max-w-lg">
                <div class="p-10 relative">
                    <button @click="modals.add.show = false" class="absolute top-8 right-8 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-6 h-6" />
                    </button>
                    <div class="mb-8">
                        <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest mb-1">Thêm Vật Tư Mới</h3>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">Định nghĩa danh mục vật tư tiêu hao chuẩn hệ thống</p>
                    </div>
                    
                    <form @submit.prevent="saveNewSupply" class="space-y-6">
                        <div class="form-group">
                            <label>Tên vật tư <span class="text-rose-500">*</span></label>
                            <input v-model="modals.add.data.itemName" required placeholder="VD: Kim tiêm G23, Cồn 70 độ..." class="form-input" />
                        </div>
                        <div class="form-grid">
                            <div class="form-group">
                                <label>Phân loại</label>
                                <select v-model="modals.add.data.category" class="form-input">
                                    <option value="Thuốc">Thuốc</option>
                                    <option value="Vật tư y tế">Vật tư y tế</option>
                                    <option value="Thiết bị">Thiết bị</option>
                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label>Đơn vị <span class="text-rose-500">*</span></label>
                                <input v-model="modals.add.data.unit" placeholder="Cái, Hộp, Lọ..." required class="form-input" />
                            </div>
                        </div>
                        <div class="form-grid">
                            <div class="form-group">
                                <label>Ngưỡng báo động</label>
                                <input type="number" v-model="modals.add.data.minStockLevel" class="form-input" />
                            </div>
                            <div class="form-group">
                                <label>Giá nhập (VND)</label>
                                <input type="number" v-model="modals.add.data.typicalUnitPrice" class="form-input" />
                            </div>
                        </div>
                        
                        <div class="flex justify-end gap-3 mt-8 border-t pt-6">
                            <button type="button" @click="modals.add.show = false" class="btn-premium secondary">Hủy bỏ</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary">
                                <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
                                <Save v-else class="w-4 h-4" />
                                <span>Xác Nhận Lưu</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Import Modal -->
        <div v-if="modals.import.show" class="modal-overlay" @click.self="modals.import.show = false">
            <div class="modal-box max-w-md">
                <div class="p-10 relative">
                    <button @click="modals.import.show = false" class="absolute top-8 right-8 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-6 h-6" />
                    </button>
                    <div class="mb-8">
                        <h3 class="text-2xl font-black text-emerald-600 uppercase tracking-widest mb-1 flex items-center gap-3">
                            <ArrowDownLeft class="w-6 h-6" /> Nhập Kho
                        </h3>
                        <p class="text-[11px] font-black text-slate-800 mt-1 italic uppercase tracking-widest">{{ modals.import.supply?.itemName }}</p>
                    </div>
                    
                    <form @submit.prevent="processImport" class="space-y-6">
                        <div class="form-group">
                            <label>Số lượng nhập</label>
                            <input type="number" v-model="modals.import.data.quantity" required class="form-input !border-emerald-100 focus:!border-emerald-500" />
                        </div>
                        <div class="form-group">
                            <label>Giá nhập thực tế / Đơn vị</label>
                            <input type="number" v-model="modals.import.data.unitPrice" required class="form-input !border-emerald-100 focus:!border-emerald-500" />
                        </div>
                        <div class="form-group">
                            <label>Ghi chú</label>
                            <textarea v-model="modals.import.data.note" class="form-input min-h-[100px] !border-emerald-100 focus:!border-emerald-500" placeholder="Nhập từ NCC, Lô số..."></textarea>
                        </div>
                        <div class="flex justify-end gap-3 mt-8">
                            <button type="button" @click="modals.import.show = false" class="btn-premium secondary">Hủy</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !bg-emerald-600 !shadow-emerald-100">
                                <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
                                <CheckCircle v-else class="w-4 h-4" />
                                <span>Xác Nhận Nhập</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Export Modal -->
        <div v-if="modals.export.show" class="modal-overlay" @click.self="modals.export.show = false">
            <div class="modal-box max-w-md">
                <div class="p-10 relative">
                    <button @click="modals.export.show = false" class="absolute top-8 right-8 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-6 h-6" />
                    </button>
                    <div class="mb-8">
                        <h3 class="text-2xl font-black text-rose-600 uppercase tracking-widest mb-1 flex items-center gap-3">
                            <ArrowUpRight class="w-6 h-6" /> Xuất Kho Đoàn
                        </h3>
                        <p class="text-[11px] font-black text-slate-800 mt-1 italic uppercase tracking-widest">{{ modals.export.supply?.itemName }}</p>
                    </div>
                    
                    <form @submit.prevent="processExport" class="space-y-6">
                        <div class="form-group">
                            <label>Đoàn khám thụ hưởng</label>
                            <select v-model="modals.export.data.medicalGroupId" required class="form-input !border-rose-100 focus:!border-rose-500">
                                <option v-for="g in activeGroups" :key="g.groupId" :value="g.groupId">{{ g.groupName }} ({{ g.companyName }})</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <label>Số lượng xuất</label>
                            <input type="number" v-model="modals.export.data.quantity" :max="modals.export.supply?.currentStock" required class="form-input !border-rose-100 focus:!border-rose-500" />
                            <p class="text-[9px] font-black text-rose-400 uppercase mt-1">Tồn hàng tối đa: {{ modals.export.supply?.currentStock }}</p>
                        </div>
                        <div class="form-group">
                            <label>Ghi chú</label>
                            <textarea v-model="modals.export.data.note" class="form-input min-h-[100px] !border-rose-100 focus:!border-rose-500" placeholder="Cấp cho đoàn..."></textarea>
                        </div>
                        <div class="flex justify-end gap-3 mt-8">
                            <button type="button" @click="modals.export.show = false" class="btn-premium secondary">Hủy</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !bg-rose-600 !shadow-rose-100">
                                <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
                                <CheckCircle v-else class="w-4 h-4" />
                                <span>Xác Nhận Xuất</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- History Modal -->
        <div v-if="modals.history.show" class="modal-overlay" @click.self="modals.history.show = false">
            <div class="modal-box max-w-2xl h-[85vh] flex flex-col">
                <div class="p-10 relative flex-1 flex flex-col min-h-0">
                    <button @click="modals.history.show = false" class="absolute top-8 right-8 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-6 h-6" />
                    </button>
                    <div class="mb-8">
                        <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest mb-1">{{ modals.history.supply?.itemName }}</h3>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">Nhật ký biến động kho chi tiết</p>
                    </div>

                    <div class="flex-1 overflow-y-auto scrollbar-premium pr-2 space-y-4">
                        <div v-for="m in modals.history.list" :key="m.movementId" class="p-6 bg-slate-50 rounded-3xl border border-slate-100 flex items-center gap-6 group hover:border-primary/20 transition-all">
                            <div :class="['w-12 h-12 rounded-2xl flex items-center justify-center flex-shrink-0 shadow-sm', m.movementType === 'IN' ? 'bg-emerald-100 text-emerald-600' : 'bg-rose-100 text-rose-600']">
                                <ArrowDownLeft v-if="m.movementType === 'IN'" class="w-6 h-6" />
                                <ArrowUpRight v-else class="w-6 h-6" />
                            </div>
                            <div class="flex-1">
                                <div class="flex justify-between items-start">
                                    <div>
                                        <p class="font-black text-slate-800 uppercase tracking-widest text-xs">{{ m.movementType === 'IN' ? 'Nhập kho bổ sung' : 'Xuất kho sử dụng' }}</p>
                                        <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ formatDate(m.movementDate, true) }}</p>
                                    </div>
                                    <div class="text-right">
                                        <p :class="['text-xl font-black tabular-nums', m.movementType === 'IN' ? 'text-emerald-600' : 'text-rose-600']">
                                            {{ m.movementType === 'IN' ? '+' : '-' }}{{ m.quantity }}
                                        </p>
                                        <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ formatCurrency(m.unitPrice) }} / đơn vị</p>
                                    </div>
                                </div>
                                <div class="mt-4 pt-4 border-t border-slate-100 flex flex-wrap gap-4">
                                    <span v-if="m.medicalGroup" class="text-[9px] font-black text-primary uppercase tracking-widest bg-blue-50 px-2 py-1 rounded-md">Đoàn: {{ m.medicalGroup.groupName }}</span>
                                    <span class="text-[9px] font-black text-slate-400 uppercase tracking-widest italic" v-if="m.note">"{{ m.note }}"</span>
                                    <p class="text-[8px] font-black text-slate-300 uppercase tracking-widest ml-auto">Thực hiện: {{ m.recordedByUser?.username || 'Hệ thống' }}</p>
                                </div>
                            </div>
                        </div>
                        <div v-if="modals.history.list.length === 0" class="h-full flex flex-col items-center justify-center opacity-30 py-20">
                            <Inbox class="w-16 h-16 mb-4" />
                            <p class="font-black uppercase tracking-widest text-xs">Chưa có dữ liệu biến động</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
    Package, Box, AlertTriangle, TrendingUp, Search, Plus, 
    X, History, ArrowDownLeft, ArrowUpRight, Stethoscope,
    FlaskConical, Pill, Syringe, ClipboardList, RefreshCw,
    Save, CheckCircle, Inbox, Trash2, Loader2, AlertCircle, FileText
} from 'lucide-vue-next'
import { useI18nStore } from '../stores/i18n'
import { usePermission } from '../composables/usePermission'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'
import { parseApiError } from '../services/errorHelper'

const i18n = useI18nStore()
const { can } = usePermission()
const toast = useToast()

const supplies = ref([])
const activeGroups = ref([])
const searchQuery = ref('')
const isLoading = ref(false)
const saving = ref(false)

const activeTab = ref('inventory')
const historyList = ref([])
const loadingReport = ref(false)
const periodicReport = ref([])
const reportFilters = ref({
    from: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
    to: new Date().toISOString().split('T')[0]
})

const modals = ref({
    add: { show: false, data: { itemName: '', unit: '', category: 'Vật tư y tế', minStockLevel: 10, typicalUnitPrice: 0 } },
    import: { show: false, supply: null, data: { quantity: 0, unitPrice: 0, note: '' } },
    export: { show: false, supply: null, data: { medicalGroupId: null, quantity: 0, unitPrice: 0, note: '' } },
    history: { show: false, supply: null, list: [] }
})

const fetchGlobalHistory = async () => {
    isLoading.value = true
    try {
        const res = await apiClient.get('/api/Supplies/movements')
        historyList.value = res.data
    } catch (err) {
        toast.error("Không thể tải lịch sử biến động")
    } finally {
        isLoading.value = false
    }
}

// Watch tab change to fetch history
import { watch } from 'vue'
watch(activeTab, (newTab) => {
    if (newTab === 'history') {
        fetchGlobalHistory()
    }
})

const fetchSupplies = async () => {
    isLoading.value = true
    try {
        const res = await apiClient.get('/api/Supplies')
        supplies.value = res.data
    } catch (err) {
        toast.error("Không thể tải danh sách vật tư")
    } finally {
        isLoading.value = false
    }
}

const fetchPeriodicReport = async () => {
    loadingReport.value = true
    try {
        const res = await apiClient.get('/api/InventoryReports/periodic-stock-report', {
            params: {
                fromDate: reportFilters.value.from,
                toDate: reportFilters.value.to
            }
        })
        periodicReport.value = res.data.items || []
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        loadingReport.value = false
    }
}

const fetchData = async () => {
    try {
        const res = await apiClient.get('/api/MedicalGroups')
        activeGroups.value = (res.data || []).filter(g => g.status === 'Open')
    } catch (err) {}
}

const filteredSupplies = computed(() => {
    if (!searchQuery.value) return supplies.value
    const q = searchQuery.value.toLowerCase()
    return supplies.value.filter(s => s.itemName.toLowerCase().includes(q) || s.category.toLowerCase().includes(q))
})

const lowStockItems = computed(() => supplies.value.filter(s => s.currentStock <= s.minStockLevel))
const totalValue = computed(() => {
    if (!supplies.value) return 0
    return supplies.value.reduce((acc, s) => acc + ((s.currentStock || 0) * (s.typicalUnitPrice || 0)), 0)
})

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)
const formatDate = (dateStr, includeTime = false) => {
    if (!dateStr) return ''
    const d = new Date(dateStr)
    return includeTime ? d.toLocaleString('vi-VN') : d.toLocaleDateString('vi-VN')
}

// Category Helpers
const getCategoryIcon = (cat) => {
    if (cat?.includes('Thuốc')) return Pill
    if (cat?.includes('Thiết bị')) return FlaskConical
    if (cat?.includes('Y tế')) return Syringe
    return Box
}
const getCategoryBg = (cat) => {
    if (cat?.includes('Thuốc')) return 'bg-rose-50 text-rose-500'
    if (cat?.includes('Thiết bị')) return 'bg-sky-50 text-sky-500'
    return 'bg-indigo-50 text-indigo-500'
}

// Stock Logic
const getStockColor = (s) => {
    if (s.currentStock <= 0) return 'bg-slate-300'
    if (s.currentStock <= s.minStockLevel) return 'bg-rose-500 shadow-[0_0_10px_rgba(244,63,94,0.4)]'
    if (s.currentStock < s.minStockLevel * 2) return 'bg-amber-500'
    return 'bg-emerald-500'
}
const getStockTextClass = (s) => {
    if (s.currentStock <= 0) return 'text-slate-400'
    if (s.currentStock <= s.minStockLevel) return 'text-rose-600'
    return 'text-slate-400'
}
const getStockLabel = (s) => {
    if (s.currentStock <= 0) return 'Hết hàng'
    if (s.currentStock <= s.minStockLevel) return 'Sắp hết (Báo động)'
    return 'Duy trì ổn định'
}

// Modal Handlers
const openAddModal = () => {
    modals.value.add.data = { itemName: '', unit: '', category: 'Vật tư y tế', minStockLevel: 10, typicalUnitPrice: 0 }
    modals.value.add.show = true
}

const saveNewSupply = async () => {
    saving.value = true
    try {
        await apiClient.post('/api/Supplies', modals.value.add.data)
        toast.success("Đã thêm vật tư mới")
        modals.value.add.show = false
        fetchSupplies()
    } catch (err) {
        toast.error(parseApiError(err))
    } finally {
        saving.value = false
    }
}

const openImportModal = (s) => {
    modals.value.import.supply = s
    modals.value.import.data = { supplyId: s.supplyId, quantity: 0, unitPrice: s.typicalUnitPrice, note: '' }
    modals.value.import.show = true
}

const processImport = async () => {
    saving.value = true
    try {
        await apiClient.post('/api/Supplies/import', modals.value.import.data)
        toast.success("Đã nhập kho thành công")
        modals.value.import.show = false
        fetchSupplies()
    } catch (err) {
        toast.error(parseApiError(err))
    } finally {
        saving.value = false
    }
}

const openExportModal = (s) => {
    modals.value.export.supply = s
    modals.value.export.data = { supplyId: s.supplyId, medicalGroupId: activeGroups.value[0]?.groupId || null, quantity: 0, unitPrice: s.typicalUnitPrice, note: '' }
    modals.value.export.show = true
}

const processExport = async () => {
    saving.value = true
    try {
        await apiClient.post('/api/Supplies/export', modals.value.export.data)
        toast.success("Đã xuất kho cho đoàn")
        modals.value.export.show = false
        fetchSupplies()
    } catch (err) {
        toast.error(parseApiError(err))
    } finally {
        saving.value = false
    }
}

const openHistory = async (s) => {
    modals.value.history.supply = s
    modals.value.history.list = []
    modals.value.history.show = true
    try {
        const res = await apiClient.get(`/api/Supplies/movements/${s.supplyId}`)
        modals.value.history.list = res.data
    } catch (err) {
        toast.error("Không thể tải lịch sử biến động")
    }
}

onMounted(() => {
    fetchSupplies()
    fetchData()
    fetchPeriodicReport()
})
</script>

<style scoped>
.dashboard-gradient {
    background: radial-gradient(circle at top left, rgba(248, 250, 252, 1), rgba(241, 245, 249, 1));
}

.scrollbar-premium::-webkit-scrollbar {
    width: 6px;
}
.scrollbar-premium::-webkit-scrollbar-track {
    background: transparent;
}
.scrollbar-premium::-webkit-scrollbar-thumb {
    background: rgba(203, 213, 225, 0.5);
    border-radius: 10px;
}

/* Premium UI Patterns */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
  animation: fadeIn 0.3s ease;
  padding: 1rem;
}

.modal-box {
  background: white; border-radius: 2.5rem; overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  width: 100%; animation: scaleUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.5rem; }
.form-group { display: flex; flex-direction: column; gap: 0.5rem; }
.form-group label { font-size: 10px; font-weight: 800; text-transform: uppercase; color: #94a3b8; letter-spacing: 0.1em; }
.form-input { 
  width: 100%; padding: 0.75rem 1rem; border-radius: 1rem; border: 1px solid #e2e8f0;
  outline: none; transition: all 0.2s; font-size: 13px; font-weight: 600;
}
.form-input:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1); }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes scaleUp { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>
