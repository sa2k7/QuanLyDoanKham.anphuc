<template>
  <div class="space-y-6 animate-fade-in">
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-blue-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <FileText class="w-6 h-6" />
          </div>
          Hệ thống Hợp đồng
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-blue-600 font-black tabular-nums">{{ String(list?.length || 0).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[10px] mt-2">Quản lý pháp lý và giá trị hợp đồng</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
              @click="showForm = !showForm" 
              class="btn-premium bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-8 py-3 shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:-translate-y-0.5 transition-all">
        <Plus class="w-5 h-5" />
        <span>{{ showForm ? 'HỦY BỎ' : 'TẠO HỢP ĐỒNG' }}</span>
      </button>
    </div>

    <!-- Stats Summary Section -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
        <StatCard 
            title="Tổng giá trị HĐ (Dự kiến)"
            :value="formatPrice(list?.reduce((sum, c) => sum + (c.totalAmount || 0), 0) || 0)"
            :icon="DollarSign"
            variant="default"
        />
        <StatCard 
            title="Giá trị nghiệm thu"
            :value="formatPrice(list?.filter(c => ['Finished', 'Locked'].includes(c.status)).reduce((sum, c) => sum + (c.totalAmount || 0), 0) || 0)"
            :icon="Sparkles"
            variant="emerald"
        />
        <StatCard 
            title="HĐ Đang thực hiện"
            :value="String(list?.filter(c => ['Active', 'Pending'].includes(c.status)).length || 0).padStart(3, '0')"
            :icon="Clock"
            variant="indigo"
            subtext="Dự án đang vận hành"
        />
        <StatCard 
            title="Tổng quy mô khám"
            :value="String(list?.reduce((sum, c) => sum + (c.expectedQuantity || 0), 0) || 0).padStart(3, '0')"
            :icon="Users"
            variant="sky"
            subtext="Số lượng người (Dự kiến)"
        />
    </div>

    <!-- Creation Area -->
    <div v-if="showForm" class="premium-card p-10 bg-white/95 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 mb-12 animate-slide-up relative overflow-hidden">
        <div class="flex items-center gap-4 mb-10">
            <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center">
                <PlusCircle class="w-7 h-7" />
            </div>
            <div>
                <h3 class="text-2xl font-black text-slate-800 ">Ký kết Hợp đồng mới</h3>
                <p class="text-xs font-black text-slate-400 uppercase tracking-[0.3em] mt-1">Soạn thảo hồ sơ pháp lý đối tác</p>
            </div>
        </div>
        <form @submit.prevent="addContract" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đối tác doanh nghiệp</label>
                        <select v-model="newContract.companyId" required class="input-premium w-full">
                            <option disabled :value="null">-- Tuyển chọn đối tác --</option>
                            <option v-for="c in companies" :key="c.companyId" :value="c.companyId">{{ c.shortName || c.companyName }}</option>
                        </select>
                    </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá (VNĐ/Người)</label>
                <CurrencyInput v-model="newContract.unitPrice" :required="true" placeholder="0" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng dự kiến</label>
                <CurrencyInput v-model="newContract.expectedQuantity" :required="true" placeholder="0" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày ký</label>
                <input v-model="newContract.signingDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                <input v-model="newContract.startDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                <input v-model="newContract.endDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="lg:col-span-3 flex justify-between items-center bg-slate-50 p-6 rounded-2xl border border-slate-100 shadow-inner mt-4">
                <div>
                   <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng giá trị dự kiến</p>
                   <p class="text-2xl font-black text-primary">{{ formatPrice(newContract.unitPrice * newContract.expectedQuantity) }}</p>
                </div>
                <button type="submit" class="btn-premium bg-gradient-to-r from-teal-500 to-emerald-500 text-white px-10 py-3 shadow-lg shadow-teal-500/30 hover:shadow-teal-500/50 hover:-translate-y-1 transition-all">XÁC NHẬN KÝ KẾT</button>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-8">
        <button @click="activeTab = 'pending'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'pending' ? 'bg-gradient-to-r from-amber-400 to-amber-500 text-white shadow-amber-500/30' : 'bg-amber-50/30 text-amber-600 border border-amber-100/50 hover:bg-amber-50']">
            <Clock class="w-4 h-4" />
            <span>Chờ duyệt ({{ String(filteredList.pending.length).padStart(3, '0') }})</span>
        </button>
        <button @click="activeTab = 'approved'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'approved' ? 'bg-gradient-to-r from-blue-500 to-indigo-500 text-white shadow-blue-500/30' : 'bg-blue-50/30 text-blue-600 border border-blue-100/50 hover:bg-blue-50']">
            <CheckCircle class="w-4 h-4" />
            <span>Đã phê duyệt ({{ String(filteredList.approved.length).padStart(3, '0') }})</span>
        </button>
        <button @click="activeTab = 'active'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'active' ? 'bg-gradient-to-r from-violet-500 to-purple-500 text-white shadow-violet-500/30' : 'bg-violet-50/30 text-violet-600 border border-violet-100/50 hover:bg-violet-50']">
            <Activity class="w-4 h-4" />
            <span>Đang thực hiện ({{ String(filteredList.active.length).padStart(3, '0') }})</span>
        </button>
        <button @click="activeTab = 'finished'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'finished' ? 'bg-gradient-to-r from-emerald-500 to-teal-500 text-white shadow-emerald-500/30' : 'bg-emerald-50/30 text-emerald-600 border border-emerald-100/50 hover:bg-emerald-50']">
            <FileCheck class="w-4 h-4" />
            <span>Đã kết thúc ({{ String(filteredList.finished.length).padStart(3, '0') }})</span>
        </button>
    </div>

    <!-- Contract Table With Color Indicator -->
    <div :class="['premium-card bg-white/95 backdrop-blur-xl rounded-[2.5rem] shadow-2xl shadow-slate-200/50 border overflow-hidden mt-6 transition-all duration-500', 
                 activeTab === 'pending' ? 'border-amber-200/50 shadow-amber-500/5' : 
                 activeTab === 'approved' ? 'border-blue-200/50 shadow-blue-500/5' :
                 activeTab === 'active' ? 'border-violet-200/50 shadow-violet-500/5' :
                 'border-emerald-200/50 shadow-emerald-500/5']">
        <!-- Tab accent line -->
        <div :class="['h-2 w-full transition-all duration-500', 
                      activeTab === 'pending' ? 'bg-amber-400' : 
                      activeTab === 'approved' ? 'bg-blue-500' : 
                      activeTab === 'active' ? 'bg-violet-500' : 'bg-emerald-500']"></div>
        <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
                <thead class="sticky top-0 z-30 bg-white/90 backdrop-blur-md border-b border-slate-100/50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="px-8 py-5 text-center">STT</th>
                        <th class="px-4 py-5">Hợp đồng / Đối tác</th>
                        <th class="px-4 py-5">Giá trị quyết toán</th>
                        <th class="px-4 py-5 text-center">Quy mô</th>
                        <th class="px-4 py-5 text-center">Trạng thái</th>
                        <th class="px-4 py-5 text-center">Hạn hiệu lực</th>
                        <th class="px-8 py-5 text-right">Thao tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(contract, index) in filteredList[activeTab]" :key="contract.healthContractId" 
                        class="group hover:bg-slate-50/50 transition-all duration-300">
                        <td class="px-8 py-6 text-center">
                            <span :class="['text-xs font-black tabular-nums transition-colors duration-500', 
                                         activeTab === 'pending' ? 'text-amber-500' : 
                                         activeTab === 'approved' ? 'text-blue-500' : 
                                         activeTab === 'active' ? 'text-violet-500' : 'text-emerald-500']">
                                {{ String(index + 1).padStart(3, '0') }}
                            </span>
                        </td>
                        <td class="px-4 py-6">
                            <div class="flex items-center gap-4">
                                <div :class="['w-10 h-10 rounded-xl flex items-center justify-center border shadow-sm transition-all duration-500',
                                             activeTab === 'pending' ? 'bg-amber-50 border-amber-100 text-amber-500' : 
                                             activeTab === 'approved' ? 'bg-blue-50 border-blue-100 text-blue-500' : 
                                             activeTab === 'active' ? 'bg-violet-50 border-violet-100 text-violet-500' : 
                                             'bg-emerald-50 border-emerald-100 text-emerald-500']">
                                    <FileText class="w-5 h-5" />
                                </div>
                                <div class="flex flex-col">
                                    <span class="text-sm font-black text-slate-800 uppercase tracking-tight group-hover:text-blue-600 transition-colors leading-tight mb-1">
                                        {{ contract.companyName }}
                                    </span>
                                    <div class="flex items-center gap-2">
                                        <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest bg-slate-100 px-1.5 py-0.5 rounded">
                                            Mã HĐ: {{ contract.healthContractId }}
                                        </span>
                                        <span class="text-[10px] text-slate-400 flex items-center gap-1 font-medium">
                                            <Calendar class="w-3 h-3" />
                                            {{ new Date(contract.startDate).toLocaleDateString('vi-VN') }}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="px-4 py-6">
                            <div class="flex flex-col">
                                <span class="text-sm font-bold text-slate-700 tabular-nums">
                                    {{ Number(contract.finalSettlementValue || 0).toLocaleString('vi-VN') }}
                                    <span class="text-[10px] font-black text-slate-400 ml-0.5 uppercase">đ</span>
                                </span>
                                <span class="text-[10px] font-black text-slate-300 uppercase tracking-widest mt-0.5">Giá trị hợp đồng</span>
                            </div>
                        </td>
                        <td class="px-4 py-6 text-center">
                            <div class="inline-flex flex-col items-center">
                                <span class="text-sm font-bold text-slate-700 tabular-nums">{{ contract.expectedQuantity }}</span>
                                <span class="text-[10px] font-black text-slate-300 uppercase tracking-widest">Người</span>
                            </div>
                        </td>
                        <td class="px-4 py-6 text-center">
                            <div :class="[getStatusClass(contract.status), 'mx-auto']">
                                <div :class="['w-2 h-2 rounded-full mr-2 animate-pulse', 
                                            contract.status === 'Pending' ? 'bg-amber-500' : 
                                            contract.status === 'Approved' ? 'bg-blue-500' : 
                                            contract.status === 'Active' ? 'bg-violet-500' : 'bg-emerald-500']"></div>
                                {{ getStatusLabel(contract.status) }}
                            </div>
                        </td>
                        <td class="px-4 py-6 text-center">
                            <span class="text-[11px] font-bold text-slate-500 tabular-nums bg-slate-100/50 px-2 py-1 rounded-lg border border-slate-100">
                                {{ new Date(contract.endDate).toLocaleDateString('vi-VN') }}
                            </span>
                        </td>
                        <td class="px-8 py-6 text-right">
                            <div class="flex items-center justify-end gap-2">
                                <button @click="openDetails(contract)" 
                                        class="p-2.5 bg-slate-50 text-slate-400 hover:bg-blue-50 hover:text-blue-600 rounded-xl transition-all border border-slate-100 hover:border-blue-100 shadow-sm group/btn">
                                    <Eye class="w-4.5 h-4.5 group-hover/btn:scale-110 transition-transform" />
                                </button>
                                <button @click="openModal(contract)" 
                                        class="p-2.5 bg-slate-50 text-slate-400 hover:bg-indigo-50 hover:text-indigo-600 rounded-xl transition-all border border-slate-100 hover:border-indigo-100 shadow-sm group/btn">
                                    <Edit3 class="w-4.5 h-4.5 group-hover/btn:scale-110 transition-transform" />
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr v-if="filteredList[activeTab].length === 0">
                        <td colspan="7" class="py-20 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <FileText class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không tìm thấy hợp đồng nào</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Hidden File Input -->
    <input type="file" ref="fileInput" class="hidden" @change="handleFileUpload" accept=".pdf,.doc,.docx,.xlsx" />

    <!-- Contract Detail Modal -->
    <Teleport to="body">
      <div v-if="detailsModal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-md p-4 overflow-y-auto">
          <div class="bg-white/95 backdrop-blur-3xl w-full max-w-2xl rounded-[2.5rem] border border-slate-100 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
            
            <!-- Header Accent Line -->
            <div :class="['absolute top-0 left-0 right-0 h-4 bg-gradient-to-r transition-all duration-500 z-0', 
                         detailsModal.data.status === 'Pending' ? 'from-amber-400 to-amber-600' : 
                         detailsModal.data.status === 'Approved' ? 'from-blue-500 to-indigo-600' :
                         detailsModal.data.status === 'Active' ? 'from-violet-500 to-purple-600' :
                         'from-emerald-500 to-teal-600']"></div>
            
            <!-- Close Button -->
            <button @click="detailsModal.show = false" class="absolute top-8 right-8 bg-slate-100 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-500 z-[60] flex items-center justify-center border border-slate-100 shadow-sm">
                <X class="w-5 h-5" />
            </button>

            <div class="relative z-10 pt-10">
                <div class="p-10 pb-4">
                    <div class="flex justify-between items-start mb-8">
                        <div class="flex items-center gap-4">
                        <div :class="['w-14 h-14 rounded-3xl flex items-center justify-center shadow-inner border shrink-0 transition-all duration-500', 
                                     detailsModal.data.status === 'Pending' ? 'bg-amber-50 text-amber-600 border-amber-100' : 
                                     detailsModal.data.status === 'Approved' ? 'bg-blue-50 text-blue-600 border-blue-100' :
                                     detailsModal.data.status === 'Active' ? 'bg-violet-50 text-violet-600 border-violet-100' :
                                     'bg-emerald-50 text-emerald-600 border-emerald-100']">
                                <FileText class="w-7 h-7" />
                            </div>
                            <div>
                                <h3 class="text-2xl font-black text-slate-800">[HĐ-{{ detailsModal.data.healthContractId }}] {{ detailsModal.data.shortName || detailsModal.data.companyName }}</h3>
                                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Hồ sơ pháp lý đối tác</p>
                            </div>
                        </div>
                        <span :class="['px-4 py-2 text-center rounded-xl text-[10px] font-black uppercase tracking-widest border-2 w-32 shrink-0', getStatusClass(detailsModal.data.status)]">
                            {{ getStatusLabel(detailsModal.data.status) }}
                        </span>
                    </div>

                <div v-if="!isEditing" class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 mb-8 pb-8 border-b border-slate-100">
                    <div class="flex flex-col gap-1">
                        <p class="text-[10px] font-black tracking-widest text-slate-400 uppercase">TỔNG GIÁ TRỊ HỢP ĐỒNG</p>
                        <p class="text-3xl font-black text-blue-600 tracking-tight">{{ formatPrice(detailsModal.data.totalAmount) }}</p>
                    </div>
                    <div class="flex flex-col gap-1">
                        <p class="text-[10px] font-black tracking-widest text-slate-400 uppercase">QUY MÔ DỰ KIẾN</p>
                        <p class="text-3xl font-black text-slate-700 tracking-tight">{{ detailsModal.data.expectedQuantity }} <span class="text-base text-slate-400 font-medium lowercase">{{ detailsModal.data.unitName }}</span></p>
                    </div>
                    <div class="flex flex-col gap-1">
                        <p class="text-[10px] font-black tracking-widest text-slate-400 uppercase">NGÀY BẮT ĐẦU</p>
                        <p class="text-lg font-black text-slate-600">{{ formatDate(detailsModal.data.startDate) }}</p>
                    </div>
                    <div class="flex flex-col gap-1">
                        <p class="text-[10px] font-black tracking-widest text-slate-400 uppercase">NGÀY KẾT THÚC</p>
                        <p class="text-lg font-black text-slate-600">{{ formatDate(detailsModal.data.endDate) }}</p>
                    </div>
                </div>

                <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-5 mb-8">
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá niêm *</label>
                        <CurrencyInput v-model="detailsModal.data.unitPrice" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng / Quy mô *</label>
                        <CurrencyInput v-model="detailsModal.data.expectedQuantity" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                        <input type="date" v-model="detailsModal.data.startDate" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                        <input type="date" v-model="detailsModal.data.endDate" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
                    </div>
                    <div class="md:col-span-2 flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Trạng thái vận hành</label>
                        <select v-model="detailsModal.data.status" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full cursor-pointer font-black text-xs uppercase tracking-widest">
                            <option value="Pending">{{ getStatusLabel('Pending') }}</option>
                            <option value="Approved">{{ getStatusLabel('Approved') }}</option>
                            <option value="Active">{{ getStatusLabel('Active') }}</option>
                            <option value="Finished">{{ getStatusLabel('Finished') }}</option>
                            <option value="Locked">{{ getStatusLabel('Locked') }}</option>
                        </select>
                    </div>
                </div>

                <div class="space-y-4">
                    <div v-if="detailsModal.data.filePath" class="flex items-center gap-3 p-4 bg-indigo-50 rounded-2xl border border-indigo-100">
                        <FileText class="w-6 h-6 text-indigo-600" />
                        <div class="flex-1">
                            <p class="text-[10px] font-black uppercase tracking-widest text-indigo-400">File đính kèm</p>
                            <p class="text-xs font-black text-slate-700 truncate">{{ detailsModal.data.filePath.split('\\').pop() }}</p>
                        </div>
                        <a :href="'/' + detailsModal.data.filePath" target="_blank" class="px-4 py-2 bg-white text-indigo-600 rounded-xl text-[10px] font-black shadow-sm">XEM FILE</a>
                    </div>
                </div>

                <!-- Status History Section -->
                <div v-if="detailsModal.data.statusHistories?.length > 0" class="mt-8 pt-8 border-t border-slate-100">
                    <h4 class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-4 flex items-center gap-2">
                        <History class="w-3 h-3" /> Lịch sử thay đổi trạng thái
                    </h4>
                    <div class="space-y-3">
                        <div v-for="h in detailsModal.data.statusHistories" :key="h.id" class="flex gap-4 items-start bg-slate-50 p-3 rounded-xl border border-slate-100">
                            <div class="w-1.5 h-1.5 rounded-full bg-indigo-400 mt-1.5 shrink-0"></div>
                            <div class="flex-1 min-w-0">
                                <div class="flex justify-between items-center mb-1">
                                    <span class="text-[9px] font-black text-slate-800 uppercase tracking-widest">
                                        {{ getStatusLabel(h.oldStatus) }} <ArrowRight class="inline w-2 h-2 mx-1" /> {{ getStatusLabel(h.newStatus) }}
                                    </span>
                                    <span class="text-[8px] font-black text-slate-400 uppercase">{{ formatDate(h.changedAt) }}</span>
                                </div>
                                <p v-if="h.note" class="text-[10px] text-slate-500 italic">"{{ h.note }}"</p>
                                <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-1">Người thực hiện: {{ h.changedBy }}</p>
                            </div>
                        </div>
                    </div>
                    </div>
                </div>

                <div class="p-10 pt-6 bg-white border-t border-slate-50 relative z-20 flex flex-wrap items-center justify-end gap-3">
                    <template v-if="!isEditing">
                        <button v-if="detailsModal.data.status === 'Pending' && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleApproveContract(detailsModal.data.healthContractId)"
                                 class="flex-1 bg-gradient-to-r from-emerald-500 to-teal-500 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-emerald-500/30 hover:shadow-emerald-500/50 hover:-translate-y-1">
                            <Sparkles class="w-5 h-5" />
                            <span>PHÊ DUYỆT HĐ</span>
                        </button>
                        <button v-if="detailsModal.data.status === 'Approved' && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleActivateContract(detailsModal.data.healthContractId)"
                                 class="flex-1 bg-gradient-to-r from-blue-500 to-indigo-500 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:-translate-y-1">
                            <Clock class="w-5 h-5" />
                            <span>KÍCH HOẠT HĐ</span>
                        </button>
                        <button v-if="detailsModal.data.status === 'Active' && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleFinishContract(detailsModal.data.healthContractId)"
                                class="flex-1 bg-gradient-to-r from-slate-700 to-slate-900 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-slate-900/20 hover:shadow-slate-900/40 hover:-translate-y-1">
                            <Lock class="w-5 h-5" />
                            <span>KẾT THÚC HĐ</span>
                        </button>
                        <button v-if="detailsModal.data.status === 'Finished' && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleLockContract(detailsModal.data.healthContractId)" 
                                class="flex-1 bg-gradient-to-r from-slate-700 to-slate-900 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-slate-900/20 hover:shadow-slate-900/40 hover:-translate-y-1">
                            <Lock class="w-5 h-5" />
                            <span>KHÓA TÀI CHÍNH</span>
                        </button>
                        <button v-if="detailsModal.data.status === 'Locked' && authStore.role === 'Admin'" 
                                @click="handleUnlockContract(detailsModal.data.healthContractId)" 
                                class="flex-1 bg-gradient-to-r from-amber-400 to-amber-600 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-amber-500/30 hover:shadow-amber-500/50 hover:-translate-y-1">
                            <Unlock class="w-5 h-5" />
                            <span>MỞ KHÓA</span>
                        </button>
                        <button v-if="detailsModal.data.status === 'Pending' && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="isEditing = true" 
                                 class="bg-white text-indigo-600 px-8 py-4 rounded-2xl font-black hover:bg-indigo-50 transition-all flex items-center justify-center border border-indigo-100 shadow-sm hover:shadow-md hover:-translate-y-1">
                            <Edit3 class="w-5 h-5 mr-2" />
                            CHỈNH SỬA
                        </button>
                        <button v-if="authStore.role === 'Admin'" 
                                @click="handleDeleteContract(detailsModal.data.healthContractId)" 
                                class="w-12 h-12 rounded-2xl bg-rose-50 text-rose-500 flex items-center justify-center hover:bg-rose-500 hover:text-white transition-all border border-rose-100 hover:border-rose-500 hover:shadow-lg shadow-sm">
                            <Trash2 class="w-5 h-5" />
                        </button>
                    </template>
                    <template v-else>
                        <button @click="handleUpdateContract" 
                                :class="['flex-1 text-white px-8 py-4 rounded-2xl font-black transition-all shadow-lg hover:-translate-y-1 active:scale-95 flex items-center justify-center bg-gradient-to-r duration-500',
                                         detailsModal.data.status === 'Pending' ? 'from-amber-400 to-amber-600 shadow-amber-500/30 hover:shadow-amber-500/50' : 
                                         detailsModal.data.status === 'Approved' ? 'from-blue-500 to-indigo-600 shadow-blue-500/30 hover:shadow-blue-500/50' :
                                         detailsModal.data.status === 'Active' ? 'from-violet-500 to-purple-600 shadow-violet-500/30 hover:shadow-violet-500/50' :
                                         'from-emerald-500 to-teal-600 shadow-emerald-500/30 hover:shadow-emerald-500/50']">
                            <Save class="w-5 h-5 mr-2" />
                            LƯU THAY ĐỔI
                        </button>
                        <button @click="detailsModal.show = false" 
                                 class="bg-slate-50 text-slate-500 px-8 py-4 rounded-2xl font-black hover:bg-slate-100 transition-all border border-slate-200 shadow-sm uppercase tracking-widest text-xs">
                             HỦY
                         </button>
                    </template>
                    <button v-if="!isEditing" @click="detailsModal.show = false" 
                            class="bg-slate-50 border border-slate-200 text-slate-500 px-8 py-4 rounded-2xl font-black hover:bg-slate-100 transition-all shadow-sm">
                        ĐÓNG
                    </button>
                </div>
            </div>
        </div>
      </div>
    </Teleport>

    
    <!-- Confirm Dialog -->
    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Plus, FileText, Calendar, ArrowRight, Trash2, Save, PlusCircle, History, Sparkles, Clock, Lock, Upload, X, DollarSign, Users, Eye, Edit3, Unlock, CheckCircle, Activity, FileCheck } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import StatCard from '../components/StatCard.vue'

import CurrencyInput from '../components/CurrencyInput.vue'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const companies = ref([])
const showForm = ref(false)
const fileInput = ref(null)
const currentUploadId = ref(null)

const newContract = ref({
    companyId: null,
    signingDate: new Date().toISOString().split('T')[0],
    startDate: '',
    endDate: '',
    unitPrice: 0,
    expectedQuantity: 0,
    unitName: 'Người',
    status: 'Pending'
})

const detailsModal = ref({ show: false, data: {} })
const isEditing = ref(false)
const activeTab = ref('pending')
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const filteredList = computed(() => {
    return {
        pending: list.value.filter(c => c.status === 'Pending'),
        approved: list.value.filter(c => c.status === 'Approved'),
        active: list.value.filter(c => c.status === 'Active' || c.status === 'InProgress'),
        finished: list.value.filter(c => ['Finished', 'Locked'].includes(c.status))
    }
})

const getStatusClass = (status) => {
    switch(status) {
        case 'Approved': return 'bg-blue-50 text-blue-600 border-blue-100'
        case 'Active': return 'bg-violet-50 text-violet-600 border-violet-100'
        case 'Finished': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'Locked': return 'bg-slate-50 text-slate-500 border-slate-200'
        default: return 'bg-amber-50 text-amber-600 border-amber-100'
    }
}

const getStatusLabel = (status) => {
    switch(status) {
        case 'Active': return 'Đang thực hiện'
        case 'Approved': return 'Đã phê duyệt'
        case 'Finished': return 'Hợp đồng kết thúc'
        case 'Locked': return 'Đã khóa tài chính'
        case 'Pending': return 'Đang chờ phê duyệt'
        default: return status
    }
}

const fetchList = async () => {
    try {
        const res = await axios.get('/api/HealthContracts')
        list.value = res.data
    } catch (e) { toast.error("Lỗi khi tải danh sách hợp đồng") }
}

const fetchCompanies = async () => {
    try {
        const res = await axios.get('/api/Companies')
        companies.value = res.data
    } catch (e) { console.error(e) }
}

const addContract = async () => {
    try {
        if (!newContract.value.companyId) return toast.warning("Chưa chọn đối tác!")
        
        // Frontend Duplicate Check (Pre-flight)
        const duplicate = list.value.find(c => 
            c.companyId === newContract.value.companyId && 
            c.signingDate.split('T')[0] === newContract.value.signingDate
        )
        if (duplicate) {
            toast.warning("Công ty này đã có hợp đồng trong ngày hôm nay!")
            return
        }

        const payload = { ...newContract.value, totalAmount: newContract.value.unitPrice * newContract.value.expectedQuantity };
        await axios.post('/api/HealthContracts', payload)
        toast.success("Tạo hợp đồng thành công!")
        fetchList()
        showForm.value = false
        resetForm()
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi khi tạo hợp đồng") 
    }
}

const resetForm = () => {
    newContract.value = { companyId: null, signingDate: new Date().toISOString().split('T')[0], startDate: '', endDate: '', unitPrice: 0, expectedQuantity: 0, unitName: 'Người', status: 'Pending' }
}

const openDetails = (contract) => {
    const data = { ...contract };
    if (data.signingDate) data.signingDate = data.signingDate.split('T')[0];
    if (data.startDate) data.startDate = data.startDate.split('T')[0];
    if (data.endDate) data.endDate = data.endDate.split('T')[0];
    detailsModal.value.data = data
    detailsModal.value.show = true
    isEditing.value = false
}

const openModal = (contract) => {
    openDetails(contract)
    isEditing.value = true
}

const handleUpdateContract = async () => {
    try {
        await axios.put(`/api/HealthContracts/${detailsModal.value.data.healthContractId}`, detailsModal.value.data)
        toast.success("Đã cập nhật hợp đồng!")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi khi cập nhật") 
    }
}

const triggerUpload = (id) => {
    currentUploadId.value = id
    fileInput.value.click()
}

const handleFileUpload = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        await axios.post(`/api/HealthContracts/${currentUploadId.value}/upload`, formData)
        toast.success("Đã tải lên văn bản hợp đồng!")
        fetchList()
    } catch (err) { 
        toast.error(err.response?.data || "Lỗi khi tải file") 
    }
}

const handleLockContract = (id) => {
    confirmData.value = {
        show: true,
        title: 'Khóa hợp đồng',
        message: 'Khi đã khóa, thông tin tài chính sẽ không được phép chỉnh sửa. Tiếp tục?',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await axios.put(`/api/HealthContracts/${id}/lock`)
                toast.success("Hợp đồng đã được khóa an toàn")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(e.response?.data || "Không thể khóa hợp đồng") 
            }
        }
    }
}

const handleUnlockContract = (id) => {
    confirmData.value = {
        show: true,
        title: 'Mở khóa hợp đồng',
        message: 'Bạn có chắc chắn muốn mở khóa để chỉnh sửa lại thông tin?',
        variant: 'warning',
        onConfirm: async () => {
            try {
                await axios.put(`/api/HealthContracts/${id}/unlock`)
                toast.success("Đã mở khóa hợp đồng")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(e.response?.data || "Lỗi khi mở khóa") 
            }
        }
    }
}

const handleDeleteContract = (id) => {
    confirmData.value = {
        show: true, 
        title: 'Xóa hợp đồng', 
        message: 'Hành động này sẽ xóa vĩnh viễn dữ liệu. Bạn chắc chắn chứ?', 
        variant: 'danger',
        onConfirm: async () => {
            try {
                await axios.delete(`/api/HealthContracts/${id}`)
                toast.success("Đã xóa hợp đồng thành công")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(e.response?.data || "Lỗi khi xóa") 
            }
        }
    }
}

const handleApproveContract = async (id) => {
    try {
        // BUG FIX: Pending → Approved (not Active). Approved is a separate state before Active.
        await axios.patch(`/api/HealthContracts/${id}/status`, { status: 'Approved', note: 'Phê duyệt hợp đồng' })
        toast.success("Hợp đồng đã được phê duyệt! Bấm 'Kích hoạt' để bắt đầu thực hiện.")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { toast.error(e.response?.data || "Lỗi phê duyệt") }
}

const handleActivateContract = async (id) => {
    try {
        // Approved → Active (Đang thực hiện)
        await axios.patch(`/api/HealthContracts/${id}/status`, { status: 'Active', note: 'Kích hoạt hợp đồng để triển khai' })
        toast.success("Hợp đồng đã được kích hoạt! Bắt đầu triển khai.")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { toast.error(e.response?.data || "Lỗi kích hoạt") }
}

const handleFinishContract = async (id) => {
    confirmData.value = {
        show: true,
        title: 'Kết thúc hợp đồng',
        message: 'Xác nhận hoàn tất hợp đồng này? Hệ thống sẽ kiểm tra các đoàn khám liên quan.',
        variant: 'info',
        onConfirm: async () => {
            try {
                await axios.patch(`/api/HealthContracts/${id}/status`, { status: 'Finished', note: 'Nghiệm thu kết thúc hợp đồng' })
                toast.success("Đã kết thúc hợp đồng!")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { toast.error(e.response?.data || "Không thể kết thúc HĐ") }
        }
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN').format(p) + ' đ'
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'

onMounted(() => {
    fetchList()
    fetchCompanies()
})
</script>
