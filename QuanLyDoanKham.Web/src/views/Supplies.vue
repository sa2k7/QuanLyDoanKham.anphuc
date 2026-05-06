<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-8 pr-4 scrollbar-premium overflow-y-auto font-sans p-4">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-5">
      <div>
        <h2 class="text-xl font-bold text-slate-800 flex items-center gap-2.5">
          <div class="w-10 h-10 bg-primary text-white rounded-xl flex items-center justify-center shadow-lg">
            <Package class="w-5 h-5" />
          </div>
          {{ i18n.t('supplies.title') || 'Kho Vật Tư Y Tế' }}
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[9px] mt-1">Quản lý nhập xuất & theo dõi tồn kho tiêu hao</p>
      </div>

      <div class="flex items-center gap-3">
        <!-- Tab Switcher -->
        <div class="flex bg-slate-100 p-1 rounded-xl border border-slate-200 shadow-inner">
            <button @click="activeTab = 'inventory'" :class="['px-4 py-2 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'inventory' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <Box class="w-3.5 h-3.5" />
                Tồn Kho
            </button>
            <button @click="activeTab = 'history'" :class="['px-4 py-2 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'history' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <History class="w-3.5 h-3.5" />
                Lịch Sử
            </button>
            <button @click="activeTab = 'report'" :class="['px-4 py-2 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all duration-300 flex items-center gap-2', activeTab === 'report' ? 'bg-white text-primary shadow-sm scale-105' : 'text-slate-400 hover:text-slate-600']">
                <FileText class="w-3.5 h-3.5" />
                Báo Cáo
            </button>
        </div>

        <button v-if="(can('Kho.Edit') || can('Kho.Import')) && activeTab === 'inventory'" 
                @click="openAddModal" class="btn-premium primary !px-5 !py-2 !text-[11px]">
            <Plus class="w-4 h-4" />
            <span>Thêm Vật Tư</span>
        </button>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div class="premium-card p-4 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-4">
            <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center shadow-inner">
                <Box class="w-6 h-6" />
            </div>
            <div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Tổng loại vật tư</p>
                <h3 class="text-xl font-black text-slate-800 tabular-nums">{{ supplies.length }}</h3>
            </div>
        </div>
        <div class="premium-card p-4 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-4">
            <div class="w-12 h-12 bg-rose-50 text-rose-600 rounded-2xl flex items-center justify-center shadow-inner">
                <AlertTriangle class="w-6 h-6" />
            </div>
            <div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Sắp hết hàng</p>
                <h3 class="text-xl font-black text-rose-600 tabular-nums">{{ lowStockItems.length }}</h3>
            </div>
        </div>
        <div class="premium-card p-4 bg-white/80 backdrop-blur-md border border-slate-100 flex items-center gap-4">
            <div class="w-12 h-12 bg-emerald-50 text-emerald-600 rounded-2xl flex items-center justify-center shadow-inner">
                <TrendingUp class="w-6 h-6" />
            </div>
            <div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Giá trị kho ước tính</p>
                <h3 class="text-xl font-black text-emerald-600 tabular-nums">{{ formatCurrency(totalValue) }}</h3>
            </div>
        </div>
    </div>

    <!-- Main Content Section -->
    <div v-if="activeTab === 'inventory'" class="premium-card bg-white border border-slate-100 overflow-hidden">
        <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
            <h4 class="text-[11px] font-black text-slate-800 uppercase tracking-widest">Danh mục tồn kho thực tế</h4>
            <div class="flex items-center gap-3">
                <div class="relative">
                    <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-300" />
                    <input type="text" v-model="searchQuery" placeholder="Tìm kiếm vật tư..." class="pl-9 pr-4 py-1.5 bg-white border border-slate-200 rounded-xl text-[11px] outline-none focus:border-primary transition-all w-64" />
                </div>
            </div>
        </div>
        
        <div class="overflow-x-auto scrollbar-premium">
            <div v-if="isLoading" class="p-4 space-y-3">
              <div v-for="i in 3" :key="i"
                   class="h-16 bg-slate-100 rounded-lg animate-pulse" />
            </div>
            <table v-else class="w-full text-left">
                <thead>
                    <tr class="bg-slate-50/50 text-[9px] font-black text-slate-400 uppercase tracking-widest">
                        <th class="p-3 pl-5">Vật Tư & Phân Loại</th>
                        <th class="p-3 text-center">Đơn Vị</th>
                        <th class="p-3 text-center">Tồn Kho</th>
                        <th class="p-3 text-center">Trạng Thái</th>
                        <th class="p-3 text-right">Giá Tham Khảo</th>
                        <th class="p-3 pr-5 text-center">Thao Tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="s in filteredSupplies" :key="s.supplyId" class="hover:bg-slate-50/50 transition-all group">
                        <td class="p-3 pl-5">
                            <div class="flex items-center gap-3">
                                <div :class="['w-9 h-9 rounded-xl flex items-center justify-center shadow-sm border border-slate-100', getCategoryBg(s.category)]">
                                    <component :is="getCategoryIcon(s.category)" class="w-4 h-4" />
                                </div>
                                <div>
                                    <p class="font-black text-slate-800 uppercase tracking-widest text-[12px]">{{ s.itemName }}</p>
                                    <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ s.category }}</p>
                                </div>
                            </div>
                        </td>
                        <td class="p-3 text-center">
                            <span class="text-[9px] font-black text-slate-500 uppercase">{{ s.unit }}</span>
                        </td>
                        <td class="p-3 text-center">
                            <div class="flex flex-col items-center gap-0.5">
                                <span :class="['text-sm font-black tabular-nums', s.currentStock <= s.minStockLevel ? 'text-rose-600' : 'text-slate-800']">
                                    {{ s.currentStock }}
                                </span>
                                <span class="text-[7px] font-black text-slate-300 uppercase tracking-widest">Ngưỡng: {{ s.minStockLevel }}</span>
                            </div>
                        </td>
                        <td class="p-3">
                            <div class="flex flex-col gap-1.5 w-24 mx-auto">
                                <div class="h-1 bg-slate-100 rounded-full overflow-hidden">
                                    <div :class="['h-full rounded-full transition-all duration-1000', getStockColor(s)]" 
                                         :style="{ width: Math.min((s.currentStock / 100) * 100, 100) + '%' }"></div>
                                </div>
                                <p :class="['text-[7px] font-black uppercase tracking-widest text-center italic', getStockTextClass(s)]">
                                    {{ getStockLabel(s) }}
                                </p>
                            </div>
                        </td>
                        <td class="p-3 text-right">
                            <p class="font-black text-slate-700 tabular-nums text-[12px]">{{ formatCurrency(s.typicalUnitPrice) }}</p>
                        </td>
                        <td class="p-3 pr-5">
                            <div class="flex items-center justify-center gap-1 transition-all">
                                <button @click="openHistory(s)" class="p-2 rounded-lg hover:bg-indigo-50 text-slate-400 hover:text-indigo-600 transition-all group/btn" title="Lịch sử">
                                    <History class="w-3.5 h-3.5" />
                                </button>
                                <button v-if="can('Kho.View')" @click="openEditModal(s)" class="p-2 rounded-lg hover:bg-sky-50 text-slate-400 hover:text-sky-600 transition-all group/btn" title="Sửa/Cập nhật">
                                    <Pencil class="w-3.5 h-3.5" />
                                </button>
                                <button v-if="can('Kho.Import')" @click="openImportModal(s)" class="p-2 rounded-lg bg-emerald-50 text-emerald-600 hover:bg-emerald-100 transition-all shadow-sm border border-emerald-100" title="Nhập bổ sung">
                                    <ArrowDownLeft class="w-3.5 h-3.5" />
                                </button>
                                <button v-if="can('Kho.Export')" @click="openExportModal(s)" class="p-2 rounded-lg bg-rose-50 text-rose-600 hover:bg-rose-100 transition-all shadow-sm border border-rose-100" title="Xuất cho đoàn">
                                    <ArrowUpRight class="w-3.5 h-3.5" />
                                </button>
                                <button v-if="can('Kho.Edit')" @click="confirmDelete(s)" class="p-2 rounded-lg hover:bg-rose-50 text-slate-400 hover:text-rose-600 transition-all" title="Xóa">
                                    <Trash2 class="w-3.5 h-3.5" />
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr v-if="filteredSupplies.length === 0">
                        <td colspan="6" class="p-10 text-center">
                            <div v-if="!isLoading && filteredSupplies.length === 0"
                                 class="flex flex-col items-center justify-center py-16 text-slate-400">
                                <component :is="Inbox" class="w-12 h-12 mb-3 opacity-40" />
                                <p class="font-bold text-sm">Chưa có dữ liệu</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Global History Section -->
    <div v-if="activeTab === 'history'" class="grid grid-cols-1 lg:grid-cols-2 gap-4 animate-fade-in-up">
        <!-- Import History (Left) -->
        <div class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col h-[600px]">
            <div class="p-4 border-b border-emerald-50 bg-emerald-50/20 flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <div class="w-7 h-7 bg-emerald-100 text-emerald-600 rounded-lg flex items-center justify-center shadow-sm">
                        <ArrowDownLeft class="w-4 h-4" />
                    </div>
                    <h4 class="text-[11px] font-black text-emerald-800 uppercase tracking-widest">Lịch sử Nhập kho bổ sung</h4>
                </div>
                <span class="px-2 py-0.5 bg-emerald-50 text-emerald-600 rounded-full text-[9px] font-black tabular-nums">{{ importHistory.length }} bản ghi</span>
            </div>
            <div class="flex-1 overflow-y-auto scrollbar-premium p-3 space-y-2">
                <div v-for="m in importHistory" :key="m.movementId" class="p-3 bg-slate-50/50 rounded-xl border border-slate-100 hover:border-emerald-200 transition-all group">
                    <div class="flex justify-between items-start mb-1">
                        <div>
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[10px]">{{ m.itemName }}</p>
                            <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">{{ formatDate(m.movementDate, true) }}</p>
                        </div>
                        <div class="text-right text-emerald-600">
                            <p class="text-xs font-black tabular-nums">+{{ m.quantity }}</p>
                            <p class="text-[7px] font-black uppercase tracking-widest opacity-60">{{ m.unit }}</p>
                        </div>
                    </div>
                    <div class="flex justify-between items-center mt-2 pt-2 border-t border-slate-100/50 text-[8px] font-black uppercase tracking-widest text-slate-400">
                        <div class="flex items-center gap-2">
                            <span class="text-slate-300">Ghi chú:</span>
                            <span class="text-slate-500 italic">"{{ m.note || 'Không có' }}"</span>
                        </div>
                        <span class="text-slate-300">Bởi: {{ m.recordedByUser?.username || 'Hệ thống' }}</span>
                    </div>
                </div>
                <div v-if="importHistory.length === 0" class="h-full flex flex-col items-center justify-center opacity-20 py-20">
                    <Inbox class="w-10 h-10 mb-3" />
                    <p class="text-[9px] font-black uppercase tracking-widest">Chưa có dữ liệu nhập</p>
                </div>
            </div>
        </div>

        <!-- Export History (Right) -->
        <div class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col h-[600px]">
            <div class="p-4 border-b border-rose-50 bg-rose-50/20 flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <div class="w-7 h-7 bg-rose-100 text-rose-600 rounded-lg flex items-center justify-center shadow-sm">
                        <ArrowUpRight class="w-4 h-4" />
                    </div>
                    <h4 class="text-[11px] font-black text-rose-800 uppercase tracking-widest">Lịch sử Xuất kho cho đoàn</h4>
                </div>
                <span class="px-2 py-0.5 bg-rose-50 text-rose-600 rounded-full text-[9px] font-black tabular-nums">{{ exportHistory.length }} bản ghi</span>
            </div>
            <div class="flex-1 overflow-y-auto scrollbar-premium p-3 space-y-2">
                <div v-for="m in exportHistory" :key="m.movementId" class="p-3 bg-slate-50/50 rounded-xl border border-slate-100 hover:border-rose-200 transition-all group">
                    <div class="flex justify-between items-start mb-1">
                        <div>
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[10px]">{{ m.itemName }}</p>
                            <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">{{ formatDate(m.movementDate, true) }}</p>
                        </div>
                        <div class="text-right text-rose-600">
                            <p class="text-xs font-black tabular-nums">-{{ m.quantity }}</p>
                            <p class="text-[7px] font-black uppercase tracking-widest opacity-60">{{ m.unit }}</p>
                        </div>
                    </div>
                    <div class="flex flex-col gap-1.5 mt-2 pt-2 border-t border-slate-100/50">
                        <div v-if="m.medicalGroup" class="flex items-center gap-2">
                            <span class="text-[8px] font-black text-slate-300 uppercase tracking-widest">Cấp cho:</span>
                            <span class="text-[9px] font-black text-rose-500 uppercase tracking-widest bg-rose-50 px-1.5 py-0.5 rounded-md border border-rose-100">{{ m.medicalGroup.groupName }}</span>
                        </div>
                        <div class="flex justify-between items-center text-[8px] font-black uppercase tracking-widest text-slate-400">
                            <div class="flex items-center gap-2">
                                <span class="text-slate-300">Ghi chú:</span>
                                <span class="text-slate-500 italic">"{{ m.note || 'Không có' }}"</span>
                            </div>
                            <span class="text-slate-300">Bởi: {{ m.recordedByUser?.username || 'Hệ thống' }}</span>
                        </div>
                    </div>
                </div>
                <div v-if="exportHistory.length === 0" class="h-full flex flex-col items-center justify-center opacity-20 py-20">
                    <Inbox class="w-10 h-10 mb-3" />
                    <p class="text-[9px] font-black uppercase tracking-widest">Chưa có dữ liệu xuất</p>
                </div>
            </div>
        </div>
    </div>

    <!-- Report Content Section -->
    <div v-if="activeTab === 'report'" class="premium-card bg-white border border-slate-100 overflow-hidden flex flex-col min-h-[450px]">
        <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
            <h4 class="text-[11px] font-black text-slate-800 uppercase tracking-widest">Báo Cáo Tồn Kho Định Kỳ</h4>
            <div class="flex items-center gap-2">
                <div class="flex items-center gap-1.5 bg-white px-2 py-1 rounded-lg border border-slate-200">
                    <input type="date" v-model="reportFilters.from" class="bg-transparent border-none text-[9px] font-black text-slate-600 outline-none" />
                    <span class="text-slate-300 text-[8px]">→</span>
                    <input type="date" v-model="reportFilters.to" class="bg-transparent border-none text-[9px] font-black text-slate-600 outline-none" />
                </div>
                <button @click="fetchPeriodicReport" class="p-2 bg-primary/10 text-primary rounded-lg hover:bg-primary/20 transition-all">
                    <RefreshCw class="w-3.5 h-3.5" :class="{'animate-spin': loadingReport}" />
                </button>
            </div>
        </div>

        <div v-if="loadingReport" class="flex-1 flex justify-center items-center py-16">
            <Loader2 class="w-6 h-6 text-slate-300 animate-spin" />
        </div>

        <div v-else-if="periodicReport.length > 0" class="overflow-x-auto scrollbar-premium">
            <table class="w-full text-left">
                <thead>
                    <tr class="bg-slate-50/50 text-[9px] font-black text-slate-400 uppercase tracking-widest">
                        <th class="p-3 pl-5">Vật Tư</th>
                        <th class="p-3 text-center">ĐVT</th>
                        <th class="p-3 text-center text-slate-600">Tồn Đầu</th>
                        <th class="p-3 text-center text-emerald-600">Nhập</th>
                        <th class="p-3 text-center text-rose-600">Xuất</th>
                        <th class="p-3 text-center text-primary">Tồn Cuối</th>
                        <th class="p-3 pr-5 text-right">Tổng Giá Trị</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="r in periodicReport" :key="r.supplyId" class="hover:bg-slate-50/50 transition-all">
                        <td class="p-3 pl-5">
                            <p class="font-black text-slate-800 uppercase tracking-widest text-[10px]">{{ r.itemName }}</p>
                            <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ r.category }}</p>
                        </td>
                        <td class="p-3 text-center text-[9px] font-black text-slate-500 uppercase">{{ r.unit }}</td>
                        <td class="p-3 text-center font-black text-slate-600 text-[11px] tabular-nums">{{ r.tonDauKy }}</td>
                        <td class="p-3 text-center font-black text-emerald-600 text-[11px] tabular-nums">+{{ r.nhapTrongKy }}</td>
                        <td class="p-3 text-center font-black text-rose-600 text-[11px] tabular-nums">-{{ r.xuatTrongKy }}</td>
                        <td class="p-3 text-center font-black text-primary text-base tabular-nums">{{ r.tonCuoiKy }}</td>
                        <td class="p-3 pr-5 text-right font-black text-slate-700 text-[11px] tabular-nums">{{ formatCurrency(r.totalValue) }}</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-else class="flex-1 flex flex-col items-center justify-center opacity-30 py-16">
            <Box class="w-12 h-12 mb-3 text-slate-400" />
            <p class="text-[9px] font-black uppercase tracking-widest text-slate-500">Chưa có dữ liệu báo cáo</p>
        </div>
    </div>

    <!-- Modals -->
    <Teleport to="body">
        <!-- Add New Modal -->
        <div v-if="modals.add.show" class="modal-overlay" @click.self="modals.add.show = false">
            <div class="modal-box max-w-lg animate-scale-up !rounded-2xl">
                <div class="p-5 relative">
                    <button @click="modals.add.show = false" class="absolute top-5 right-5 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-4 h-4" />
                    </button>
                    <div class="mb-4">
                        <h3 class="text-lg font-black text-slate-800 uppercase tracking-widest mb-1">Thêm Vật Tư Mới</h3>
                        <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest italic">Định nghĩa danh mục vật tư tiêu hao chuẩn hệ thống</p>
                    </div>
                    
                    <form @submit.prevent="saveNewSupply" class="space-y-3">
                        <div class="form-group">
                            <label class="text-[9px]">Tên vật tư <span class="text-rose-500">*</span></label>
                            <input v-model="modals.add.data.itemName" required placeholder="VD: Kim tiêm G23, Cồn 70 độ..." class="form-input !py-2 !text-[13px]" />
                        </div>
                        <div class="grid grid-cols-2 gap-3">
                            <div class="form-group">
                                <label class="text-[9px]">Phân loại</label>
                                <select v-model="modals.add.data.category" class="form-input !py-2 !text-[13px]">
                                    <option value="Thuốc">Thuốc</option>
                                    <option value="Vật tư y tế">Vật tư y tế</option>
                                    <option value="Thiết bị">Thiết bị</option>
                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label class="text-[9px]">Đơn vị <span class="text-rose-500">*</span></label>
                                <input v-model="modals.add.data.unit" placeholder="Cái, Hộp, Lọ..." required class="form-input !py-2 !text-[13px]" />
                            </div>
                        </div>
                        <div class="grid grid-cols-2 gap-3">
                            <div class="form-group">
                                <label class="text-[9px]">Ngưỡng báo động</label>
                                <input type="number" v-model="modals.add.data.minStockLevel" class="form-input !py-2 !text-[13px]" />
                            </div>
                            <div class="form-group">
                                <label class="text-[9px]">Giá nhập (VND)</label>
                                <input type="number" v-model="modals.add.data.typicalUnitPrice" class="form-input !py-2 !text-[13px]" />
                            </div>
                        </div>
                        
                        <div class="flex justify-end gap-2 mt-4 border-t pt-4">
                            <button type="button" @click="modals.add.show = false" class="btn-premium secondary !py-2 !px-4 !text-[11px]">Hủy bỏ</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !py-2 !px-4 !text-[11px]">
                                <RefreshCw v-if="saving" class="w-3.5 h-3.5 animate-spin" />
                                <Save v-else class="w-3.5 h-3.5" />
                                <span>Xác Nhận Lưu</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Edit Modal -->
        <div v-if="modals.edit.show" class="modal-overlay" @click.self="modals.edit.show = false">
            <div class="modal-box max-w-lg animate-scale-up !rounded-2xl">
                <div class="p-5 relative">
                    <button @click="modals.edit.show = false" class="absolute top-5 right-5 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-4 h-4" />
                    </button>
                    <div class="mb-4">
                        <h3 class="text-lg font-black text-slate-800 uppercase tracking-widest mb-1 flex items-center gap-2">
                            <Pencil class="w-5 h-5 text-sky-500" /> Sửa / Cập Nhật Vật Tư
                        </h3>
                        <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest italic">Cập nhật thông tin danh mục vật tư</p>
                    </div>
                    
                    <form @submit.prevent="saveEditSupply" class="space-y-3">
                        <div class="form-group">
                            <label class="text-[9px]">Tên vật tư <span class="text-rose-500">*</span></label>
                            <input v-model="modals.edit.data.itemName" required placeholder="VD: Kim tiêm G23, Cồn 70 độ..." class="form-input !py-2 !text-[13px]" />
                        </div>
                        <div class="grid grid-cols-2 gap-3">
                            <div class="form-group">
                                <label class="text-[9px]">Phân loại</label>
                                <select v-model="modals.edit.data.category" class="form-input !py-2 !text-[13px]">
                                    <option value="Thuốc">Thuốc</option>
                                    <option value="Vật tư y tế">Vật tư y tế</option>
                                    <option value="Thiết bị">Thiết bị</option>
                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label class="text-[9px]">Đơn vị <span class="text-rose-500">*</span></label>
                                <input v-model="modals.edit.data.unit" placeholder="Cái, Hộp, Lọ..." required class="form-input !py-2 !text-[13px]" />
                            </div>
                        </div>
                        <div class="grid grid-cols-2 gap-3">
                            <div class="form-group">
                                <label class="text-[9px]">Ngưỡng báo động</label>
                                <input type="number" v-model="modals.edit.data.minStockLevel" class="form-input !py-2 !text-[13px]" />
                            </div>
                            <div class="form-group">
                                <label class="text-[9px]">Giá nhập (VND)</label>
                                <input type="number" v-model="modals.edit.data.typicalUnitPrice" class="form-input !py-2 !text-[13px]" />
                            </div>
                        </div>
                        
                        <div class="flex justify-end gap-2 mt-4 border-t pt-4">
                            <button type="button" @click="modals.edit.show = false" class="btn-premium secondary !py-2 !px-4 !text-[11px]">Hủy bỏ</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !py-2 !px-4 !text-[11px]">
                                <RefreshCw v-if="saving" class="w-3.5 h-3.5 animate-spin" />
                                <Save v-else class="w-3.5 h-3.5" />
                                <span>Lưu Thay Đổi</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Import Modal -->
        <div v-if="modals.import.show" class="modal-overlay" @click.self="modals.import.show = false">
            <div class="modal-box max-w-md animate-scale-up !rounded-2xl">
                <div class="p-5 relative">
                    <button @click="modals.import.show = false" class="absolute top-5 right-5 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-4 h-4" />
                    </button>
                    <div class="mb-4">
                        <h3 class="text-lg font-black text-emerald-600 uppercase tracking-widest mb-1 flex items-center gap-2">
                            <ArrowDownLeft class="w-5 h-5" /> Nhập Kho
                        </h3>
                        <p class="text-[9px] font-black text-slate-800 mt-0.5 italic uppercase tracking-widest">{{ modals.import.supply?.itemName }}</p>
                    </div>
                    
                    <form @submit.prevent="processImport" class="space-y-3">
                        <div class="form-group">
                            <label class="text-[9px]">Số lượng nhập</label>
                            <input type="number" v-model="modals.import.data.quantity" required class="form-input !py-2 !text-[13px] !border-emerald-100 focus:!border-emerald-500" />
                        </div>
                        <div class="form-group">
                            <label class="text-[9px]">Giá nhập thực tế / Đơn vị</label>
                            <input type="number" v-model="modals.import.data.unitPrice" required class="form-input !py-2 !text-[13px] !border-emerald-100 focus:!border-emerald-500" />
                        </div>
                        <div class="form-group">
                            <label class="text-[9px]">Ghi chú</label>
                            <textarea v-model="modals.import.data.note" class="form-input !py-2 !text-[13px] min-h-[80px] !border-emerald-100 focus:!border-emerald-500" placeholder="Nhập từ NCC, Lô số..."></textarea>
                        </div>
                        <div class="flex justify-end gap-2 mt-4">
                            <button type="button" @click="modals.import.show = false" class="btn-premium secondary !py-2 !px-4 !text-[11px]">Hủy</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !bg-emerald-600 !shadow-emerald-100 !py-2 !px-4 !text-[11px]">
                                <RefreshCw v-if="saving" class="w-3.5 h-3.5 animate-spin" />
                                <CheckCircle v-else class="w-3.5 h-3.5" />
                                <span>Xác Nhận Nhập</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Export Modal -->
        <div v-if="modals.export.show" class="modal-overlay" @click.self="modals.export.show = false">
            <div class="modal-box max-w-md animate-scale-up !rounded-2xl">
                <div class="p-5 relative">
                    <button @click="modals.export.show = false" class="absolute top-5 right-5 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-4 h-4" />
                    </button>
                    <div class="mb-4">
                        <h3 class="text-lg font-black text-rose-600 uppercase tracking-widest mb-1 flex items-center gap-2">
                            <ArrowUpRight class="w-5 h-5" /> Xuất Kho Đoàn
                        </h3>
                        <p class="text-[9px] font-black text-slate-800 mt-0.5 italic uppercase tracking-widest">{{ modals.export.supply?.itemName }}</p>
                    </div>
                    
                    <form @submit.prevent="processExport" class="space-y-3">
                        <div class="form-group">
                            <label class="text-[9px]">Đoàn khám thụ hưởng</label>
                            <select v-if="activeGroups.length > 0" v-model="modals.export.data.medicalGroupId" required class="form-input !py-2 !text-[13px] !border-rose-100 focus:!border-rose-500">
                                <option :value="null" disabled>-- Chọn đoàn khám --</option>
                                <option v-for="g in activeGroups" :key="g.groupId" :value="g.groupId">{{ g.groupName }} ({{ g.companyName }})</option>
                            </select>
                            <div v-else class="form-input !py-2 !text-[11px] bg-amber-50 border-amber-200 text-amber-700 flex items-center gap-2">
                                <span>Nhập mã đoàn khám (GroupId):</span>
                                <input type="number" v-model.number="modals.export.data.medicalGroupId" required class="flex-1 bg-transparent border-none outline-none font-black text-[13px]" placeholder="VD: 1, 2, 3..." />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="text-[9px]">Số lượng xuất</label>
                            <input type="number" v-model="modals.export.data.quantity" :max="modals.export.supply?.currentStock" required class="form-input !py-2 !text-[13px] !border-rose-100 focus:!border-rose-500" />
                            <p class="text-[8px] font-black text-rose-400 uppercase mt-0.5">Tồn hàng tối đa: {{ modals.export.supply?.currentStock }}</p>
                        </div>
                        <div class="form-group">
                            <label class="text-[9px]">Ghi chú</label>
                            <textarea v-model="modals.export.data.note" class="form-input !py-2 !text-[13px] min-h-[80px] !border-rose-100 focus:!border-rose-500" placeholder="Cấp cho đoàn..."></textarea>
                        </div>
                        <div class="flex justify-end gap-2 mt-4">
                            <button type="button" @click="modals.export.show = false" class="btn-premium secondary !py-2 !px-4 !text-[11px]">Hủy</button>
                            <button type="submit" :disabled="saving" class="btn-premium primary !bg-rose-600 !shadow-rose-100 !py-2 !px-4 !text-[11px]">
                                <RefreshCw v-if="saving" class="w-3.5 h-3.5 animate-spin" />
                                <CheckCircle v-else class="w-3.5 h-3.5" />
                                <span>Xác Nhận Xuất</span>
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- History Modal -->
        <div v-if="modals.history.show" class="modal-overlay" @click.self="modals.history.show = false">
            <div class="modal-box max-w-2xl h-[80vh] flex flex-col animate-scale-up !rounded-2xl">
                <div class="p-5 relative flex-1 flex flex-col min-h-0">
                    <button @click="modals.history.show = false" class="absolute top-5 right-5 text-slate-400 hover:rotate-90 transition-all duration-300">
                        <X class="w-4 h-4" />
                    </button>
                    <div class="mb-4">
                        <h3 class="text-lg font-black text-slate-800 uppercase tracking-widest mb-1">{{ modals.history.supply?.itemName }}</h3>
                        <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest italic">Nhật ký biến động kho chi tiết</p>
                    </div>

                    <div class="flex-1 overflow-y-auto scrollbar-premium pr-1 space-y-3">
                        <div v-for="m in modals.history.list" :key="m.movementId" class="p-3 bg-slate-50 rounded-xl border border-slate-100 flex items-center gap-4 group hover:border-primary/20 transition-all">
                            <div :class="['w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0 shadow-sm', m.movementType === 'IN' ? 'bg-emerald-100 text-emerald-600' : 'bg-rose-100 text-rose-600']">
                                <ArrowDownLeft v-if="m.movementType === 'IN'" class="w-4 h-4" />
                                <ArrowUpRight v-else class="w-4 h-4" />
                            </div>
                            <div class="flex-1">
                                <div class="flex justify-between items-start">
                                    <div>
                                        <p class="font-black text-slate-800 uppercase tracking-widest text-[10px]">{{ m.movementType === 'IN' ? 'Nhập kho bổ sung' : 'Xuất kho sử dụng' }}</p>
                                        <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ formatDate(m.movementDate, true) }}</p>
                                    </div>
                                    <div class="text-right">
                                        <p :class="['text-sm font-black tabular-nums', m.movementType === 'IN' ? 'text-emerald-600' : 'text-rose-600']">
                                            {{ m.movementType === 'IN' ? '+' : '-' }}{{ m.quantity }}
                                        </p>
                                        <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">{{ formatCurrency(m.unitPrice) }} / đơn vị</p>
                                    </div>
                                </div>
                                <div class="mt-2 pt-2 border-t border-slate-100 flex flex-wrap gap-3">
                                    <span v-if="m.medicalGroup" class="text-[8px] font-black text-primary uppercase tracking-widest bg-blue-50 px-1.5 py-0.5 rounded-md">Đoàn: {{ m.medicalGroup.groupName }}</span>
                                    <span class="text-[8px] font-black text-slate-400 uppercase tracking-widest italic" v-if="m.note">"{{ m.note }}"</span>
                                    <p class="text-[7px] font-black text-slate-300 uppercase tracking-widest ml-auto">Bởi: {{ m.recordedByUser?.username || 'Hệ thống' }}</p>
                                </div>
                            </div>
                        </div>
                        <div v-if="modals.history.list.length === 0" class="h-full flex flex-col items-center justify-center opacity-30 py-16">
                            <Inbox class="w-10 h-10 mb-3" />
                            <p class="font-black uppercase tracking-widest text-[9px]">Chưa có dữ liệu biến động</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Delete Confirm Modal -->
        <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
            <div class="modal-box max-w-sm animate-scale-up !rounded-2xl">
                <div class="p-6 text-center">
                    <div class="w-14 h-14 bg-rose-100 text-rose-500 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-inner">
                        <Trash2 class="w-7 h-7" />
                    </div>
                    <h3 class="text-lg font-black text-slate-800 italic uppercase tracking-tight mb-1">Xác nhận xóa</h3>
                    <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-6 leading-relaxed">
                        Xóa vật tư <span class="text-rose-500 underline">{{ deleteTarget.itemName }}</span>?<br/>
                        Hành động này không thể khôi phục.
                    </p>
                    <div class="flex gap-2 justify-center">
                        <button @click="deleteTarget = null" class="btn-premium secondary !py-2 !px-5 !text-[11px]">Hủy</button>
                        <button @click="deleteSupply" :disabled="saving" class="btn-premium primary !bg-rose-500 !shadow-rose-200 !py-2 !px-6 !text-[11px]">
                            <RefreshCw v-if="saving" class="w-3.5 h-3.5 animate-spin" />
                            <Trash2 v-else class="w-3.5 h-3.5" />
                            <span>Xóa vĩnh viễn</span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { 
    Package, Box, AlertTriangle, TrendingUp, Search, Plus, 
    X, History, ArrowDownLeft, ArrowUpRight, Stethoscope,
    FlaskConical, Pill, Syringe, ClipboardList, RefreshCw,
    Save, CheckCircle, Inbox, Trash2, Loader2, AlertCircle, FileText, Pencil
} from 'lucide-vue-next'
import { useI18nStore } from '../stores/i18n'
import { usePermission } from '../composables/usePermission'
import { useAuthStore } from '../stores/auth'
import apiClient from '../services/apiClient'
import { useToast } from '../composables/useToast'
import { parseApiError } from '../services/errorHelper'

const i18n = useI18nStore()
const { can } = usePermission()
const authStore = useAuthStore()
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
    edit: { show: false, supply: null, data: { itemName: '', unit: '', category: 'Vật tư y tế', minStockLevel: 10, typicalUnitPrice: 0 } },
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

// Watch tab change to fetch history or reports
watch(activeTab, (newTab) => {
    if (newTab === 'history') {
        fetchGlobalHistory()
    } else if (newTab === 'report') {
        fetchPeriodicReport()
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
    // Users with DoanKham.View use the full MedicalGroups endpoint (existing behaviour).
    // Users with Kho.Export but NOT DoanKham.View (e.g. WarehouseManager) use the
    // warehouse-scoped endpoint so they never hit a 403.
    if (authStore.hasPermission('DoanKham.View')) {
        try {
            const res = await apiClient.get('/api/MedicalGroups')
            activeGroups.value = (res.data || []).filter(g => g.status === 'Open')
        } catch (err) { /* silent */ }
    } else if (authStore.hasPermission('Kho.Export')) {
        try {
            const res = await apiClient.get('/api/warehouse/campaigns')
            // Warehouse endpoint returns Active/Completed campaigns.
            // Map to the same shape the export modal expects (groupId, groupName, companyName).
            activeGroups.value = (res.data || []).map(g => ({
                groupId:     g.groupId,
                groupName:   g.groupName,
                companyName: g.companyName ?? g.company?.companyName ?? ''
            }))
        } catch (err) { /* silent */ }
    } else {
        activeGroups.value = []
    }
}

const filteredSupplies = computed(() => {
    if (!searchQuery.value) return supplies.value
    const q = searchQuery.value.toLowerCase()
    return supplies.value.filter(s => s.itemName.toLowerCase().includes(q) || s.category.toLowerCase().includes(q))
})

const importHistory = computed(() => historyList.value.filter(m => m.movementType === 'IN'))
const exportHistory = computed(() => historyList.value.filter(m => m.movementType === 'OUT'))

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
        const payload = {
            ...modals.value.add.data,
            minStockLevel: Number(modals.value.add.data.minStockLevel) || 0,
            typicalUnitPrice: Number(modals.value.add.data.typicalUnitPrice) || 0
        }
        await apiClient.post('/api/Supplies', payload)
        toast.success("Đã thêm vật tư mới")
        modals.value.add.show = false
        fetchSupplies()
    } catch (err) {
        toast.error(parseApiError(err))
    } finally {
        saving.value = false
    }
}

const openEditModal = (s) => {
    modals.value.edit.supply = s
    modals.value.edit.data = { itemName: s.itemName, unit: s.unit, category: s.category, minStockLevel: s.minStockLevel, typicalUnitPrice: s.typicalUnitPrice }
    modals.value.edit.show = true
}

const saveEditSupply = async () => {
    saving.value = true
    try {
        const payload = {
            ...modals.value.edit.data,
            minStockLevel: Number(modals.value.edit.data.minStockLevel) || 0,
            typicalUnitPrice: Number(modals.value.edit.data.typicalUnitPrice) || 0
        }
        await apiClient.put(`/api/Supplies/${modals.value.edit.supply.supplyId}`, payload)
        toast.success("Đã cập nhật vật tư")
        modals.value.edit.show = false
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

const deleteTarget = ref(null)
const confirmDelete = (s) => { deleteTarget.value = s }
const deleteSupply = async () => {
    if (!deleteTarget.value) return
    saving.value = true
    try {
        await apiClient.delete(`/api/Supplies/${deleteTarget.value.supplyId}`)
        toast.success('Đã xóa vật tư thành công!')
        deleteTarget.value = null
        fetchSupplies()
    } catch (err) {
        toast.error(parseApiError(err))
    } finally {
        saving.value = false
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
  background: white; border-radius: 1.5rem; overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  width: 100%; animation: scaleUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; }
.form-group { display: flex; flex-direction: column; gap: 0.25rem; }
.form-group label { font-size: 9px; font-weight: 900; text-transform: uppercase; color: #94a3b8; letter-spacing: 0.1em; }
.form-input { 
  width: 100%; padding: 0.5rem 1rem; border-radius: 0.75rem; border: 1px solid #e2e8f0;
  outline: none; transition: all 0.2s; font-size: 13px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.05em;
}
.form-input:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1); }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes scaleUp { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>
