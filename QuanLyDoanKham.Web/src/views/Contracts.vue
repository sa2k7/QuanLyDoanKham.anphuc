<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans p-6">
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg">
            <FileText class="w-6 h-6" />
          </div>
          {{ i18n.t('contracts.title') }}
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[10px] mt-2">{{ i18n.t('contracts.subtitle') }}</p>
      </div>
      <button v-if="can('HopDong.Create')" 
              @click="showForm = !showForm" 
              class="btn-premium primary">
        <Plus class="w-5 h-5" />
        <span>{{ showForm ? i18n.t('common.cancel') : i18n.t('contracts.addBtn') }}</span>
      </button>
    </div>



    <!-- Creation Area -->
    <div v-if="showForm" class="premium-card p-10 mb-12 overflow-hidden animate-slide-up relative">
        <div class="flex items-center gap-4 mb-10">
            <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center">
                <PlusCircle class="w-7 h-7" />
            </div>
            <div>
                <h3 class="text-2xl font-black text-slate-800 ">{{ i18n.t('contracts.formTitle') }}</h3>
                <p class="text-xs font-black text-slate-400 uppercase tracking-[0.3em] mt-1">{{ i18n.t('contracts.formSubtitle') }}</p>
            </div>
        </div>
        <form @submit.prevent="addContract" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                            <Building2 class="w-3 h-3" /> Công ty doanh nghiệp
                        </label>
                        <select v-model="newContract.companyId" required class="input-premium w-full">
                            <option disabled :value="null">-- Tuyển chọn Công ty --</option>
                            <option v-for="c in companies" :key="c.companyId" :value="c.companyId">{{ c.shortName || c.companyName }}</option>
                        </select>
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                            <Tag class="w-3 h-3" /> Tên hợp đồng / Ghi chú nhanh
                        </label>
                        <input v-model="newContract.contractName" placeholder="VD: Khám sức khỏe 2024" class="input-premium w-full" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                            <Hash class="w-3 h-3" /> Mã hợp đồng (Tự sinh nếu trống)
                        </label>
                        <input v-model="newContract.contractCode" placeholder="VD: HD-001" class="input-premium w-full" />
                    </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                    <CreditCard class="w-3 h-3" /> Đơn giá (VNĐ/Người)
                </label>
                <CurrencyInput v-model="newContract.unitPrice" :required="true" placeholder="0" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                    <Users class="w-3.5 h-3.5" /> Số lượng dự kiến
                </label>
                <CurrencyInput v-model="newContract.expectedQuantity" :required="true" placeholder="0" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                    <Calendar class="w-3 h-3" /> Ngày ký
                </label>
                <input v-model="newContract.signingDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                    <Calendar class="w-3 h-3" /> Ngày bắt đầu
                </label>
                <input v-model="newContract.startDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                    <Calendar class="w-3 h-3" /> Ngày kết thúc
                </label>
                <input v-model="newContract.endDate" type="date" required class="input-premium w-full" />
            </div>
            <div class="lg:col-span-3 flex justify-between items-center bg-slate-50 p-6 rounded-2xl border border-slate-100 shadow-inner mt-4">
                <div>
                   <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng giá trị dự kiến</p>
                   <p class="text-2xl font-black text-primary">{{ formatPrice(newContract.unitPrice * newContract.expectedQuantity) }}</p>
                </div>
                <div class="flex items-center gap-3">
                    <button type="button" @click="showForm = false" 
                            class="bg-slate-100 text-slate-500 px-8 py-3 rounded-2xl font-black hover:bg-slate-200 transition-all border border-slate-200 uppercase tracking-widest text-xs">
                        HỦY
                    </button>
                    <button type="submit" class="btn-premium primary !bg-emerald-500 !shadow-emerald-100 !rounded-2xl">XÁC NHẬN TẠO</button>
                </div>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-8 flex-wrap">
        <button @click="activeTab = 'draft'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'draft' ? 'bg-gradient-to-r from-slate-500 to-slate-600 text-white shadow-slate-500/30' : 'bg-slate-50 border border-slate-200 text-slate-500 hover:bg-slate-100']">
            <Edit3 class="w-4 h-4" />
            <span>Nháp ({{ String(filteredList.draft.length).padStart(3, '0') }})</span>
        </button>
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
        <button @click="activeTab = 'rejected'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm flex items-center gap-3', 
                         activeTab === 'rejected' ? 'bg-gradient-to-r from-rose-500 to-red-500 text-white shadow-rose-500/30' : 'bg-rose-50/30 text-rose-600 border border-rose-100/50 hover:bg-rose-50']">
            <XCircle class="w-4 h-4" />
            <span>Từ chối ({{ String(filteredList.rejected.length).padStart(3, '0') }})</span>
        </button>
    </div>

    <!-- Contract Table With Color Indicator -->
    <div :class="['premium-card p-0 overflow-hidden mt-6 transition-all duration-500', 
                 activeTab === 'draft' ? 'shadow-slate-500/5' : 
                 activeTab === 'pending' ? 'shadow-amber-500/5' : 
                 activeTab === 'approved' ? 'shadow-blue-500/5' :
                 activeTab === 'active' ? 'shadow-violet-500/5' :
                 activeTab === 'rejected' ? 'shadow-rose-500/5' :
                 'shadow-emerald-500/5']">
        <!-- Tab accent line -->
        <div :class="['h-2 w-full transition-all duration-500', 
                      activeTab === 'draft' ? 'bg-slate-500' : 
                      activeTab === 'pending' ? 'bg-amber-400' : 
                      activeTab === 'approved' ? 'bg-blue-500' : 
                      activeTab === 'active' ? 'bg-violet-500' : 
                      activeTab === 'rejected' ? 'bg-rose-500' : 'bg-emerald-500']"></div>
        <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
                <thead class="sticky top-0 z-30 bg-white/95 backdrop-blur-md border-b border-slate-100/50 text-[10px] font-black uppercase tracking-[0.2em] text-slate-400">
                    <tr>
                        <th class="px-8 py-6 text-center w-20"># STT</th>
                        <th class="px-6 py-6">
                            <div class="flex items-center gap-2">
                                <Building2 class="w-3.5 h-3.5 text-indigo-400" />
                                {{ i18n.t('contracts.table.client') }}
                            </div>
                        </th>
                        <th class="px-6 py-6 text-right w-64">
                            <div class="flex items-center justify-end gap-2">
                                <DollarSign class="w-3.5 h-3.5 text-emerald-500" />
                                THÀNH TIỀN & QUY MÔ
                            </div>
                        </th>
                        <th class="px-6 py-6 text-center w-56">
                            <div class="flex items-center justify-center gap-2">
                                <Calendar class="w-3.5 h-3.5 text-amber-500" />
                                THỜI HẠN TRIỂN KHAI
                            </div>
                        </th>
                        <th class="px-4 py-6 text-center w-36">
                            <div class="flex items-center justify-center gap-2">
                                <ShieldCheck class="w-3.5 h-3.5 text-indigo-500" />
                                TRẠNG THÁI
                            </div>
                        </th>
                        <th class="px-6 py-6 text-center w-40">THAO TÁC</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="contract in filteredList[activeTab]" :key="contract.healthContractId" 
                        class="group hover:bg-slate-50/50 transition-all duration-300 text-[11px] border-b border-slate-50 last:border-0">
                        <td class="px-8 py-5 text-center">
                            <span class="font-black text-slate-700 tabular-nums text-xs">#{{ String(contract.healthContractId).padStart(3, '0') }}</span>
                        </td>
                        <td class="px-6 py-5">
                            <div class="flex items-center gap-4">
                                <div :class="['w-10 h-10 rounded-2xl flex items-center justify-center border shadow-sm transition-all duration-500 group-hover:scale-110',
                                             activeTab === 'draft' ? 'bg-slate-50 border-slate-100 text-slate-500' : 
                                             activeTab === 'pending' ? 'bg-amber-50 border-amber-100 text-amber-500' : 
                                             activeTab === 'approved' ? 'bg-blue-50 border-blue-100 text-blue-500' : 
                                             activeTab === 'active' ? 'bg-violet-50 border-violet-100 text-violet-500' : 
                                             activeTab === 'rejected' ? 'bg-rose-50 border-rose-100 text-rose-500' :
                                             'bg-emerald-50 border-emerald-100 text-emerald-500']">
                                    <FileText class="w-5 h-5" />
                                </div>
                                <div class="min-w-0">
                                    <p class="font-black text-slate-800 line-clamp-1 truncate text-xs uppercase tracking-tight">{{ contract.contractName || 'Chưa đặt tên' }}</p>
                                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-[0.1em] mt-1 line-clamp-1">HĐ: {{ contract.contractCode || '---' }} • {{ contract.companyName }}</p>
                                </div>
                            </div>
                        </td>
                        <td class="px-6 py-5 text-right">
                            <div class="flex flex-col items-end">
                                <span class="font-black text-indigo-600 text-sm tabular-nums">{{ formatPrice(contract.unitPrice * contract.expectedQuantity) }}</span>
                                <div class="flex items-center gap-2 mt-1">
                                    <span class="text-[9px] font-black text-slate-400 uppercase tracking-tighter tabular-nums bg-slate-50 px-2 py-0.5 rounded border border-slate-100">{{ contract.expectedQuantity }} NGƯỜI</span>
                                    <span class="text-[9px] font-black text-slate-300">•</span>
                                    <span class="text-[9px] font-black text-slate-400 uppercase tracking-tighter tabular-nums">{{ formatPrice(contract.unitPrice) }}/ng</span>
                                </div>
                            </div>
                        </td>
                        <td class="px-6 py-5 text-center">
                            <div class="flex flex-col items-center gap-1.5">
                                <div class="flex items-center gap-2 px-3 py-1 bg-slate-50 rounded-xl border border-slate-100/50 shadow-sm">
                                    <span class="font-black text-slate-600 tabular-nums text-[10px]">
                                        {{ formatDate(contract.startDate) }}
                                    </span>
                                </div>
                                <ArrowDown class="w-3.5 h-3.5 text-amber-400 animate-bounce-slow" />
                                <div class="flex items-center gap-2 px-3 py-1 bg-white rounded-xl border border-slate-200 shadow-sm">
                                    <span class="font-black text-slate-600 tabular-nums text-[10px]">
                                        {{ formatDate(contract.endDate) }}
                                    </span>
                                </div>
                            </div>
                        </td>
                        <td class="px-4 py-5 text-center">
                            <div :class="['inline-flex items-center gap-2 px-3 py-1.5 rounded-xl border text-[9px] font-black uppercase tracking-widest shadow-sm', getStatusClass(contract.status)]">
                                <div :class="['w-2 h-2 rounded-full shadow-inner animate-pulse', 
                                             contract.status === 'Draft' ? 'bg-slate-400' : 
                                             contract.status === 'PendingApproval' ? 'bg-amber-500' : 
                                             contract.status === 'Approved' ? 'bg-blue-500' : 
                                             contract.status === 'Active' ? 'bg-violet-500' : 
                                             contract.status === 'Rejected' ? 'bg-rose-500' : 'bg-emerald-500']"></div>
                                {{ getStatusLabel(contract.status) }}
                            </div>
                        </td>
                        <td class="px-6 py-5 text-center">
                            <div class="flex items-center justify-center gap-2">
                                <button @click="openDetails(contract)" 
                                        class="p-3 bg-blue-50 text-blue-500 hover:bg-blue-600 hover:text-white rounded-2xl transition-all border border-blue-100 shadow-sm group/btn active:scale-95"
                                        title="Xem chi tiết">
                                    <Eye class="w-5 h-5 group-hover/btn:rotate-6 transition-transform" />
                                </button>
                                <button v-if="['Draft', 'Rejected'].includes(contract.status) && canEdit"
                                        @click="openModal(contract)" 
                                        class="p-3 bg-indigo-50 text-indigo-500 hover:bg-indigo-600 hover:text-white rounded-2xl transition-all border border-indigo-100 shadow-sm group/btn active:scale-95"
                                        title="Chỉnh sửa">
                                    <Edit3 class="w-5 h-5 group-hover/btn:rotate-6 transition-transform" />
                                </button>
                                <button v-if="['Draft', 'Rejected'].includes(contract.status) && canEdit"
                                        @click="handleDeleteContract(contract.healthContractId)" 
                                        class="p-3 bg-rose-50 text-rose-500 hover:bg-rose-600 hover:text-white rounded-2xl transition-all border border-rose-100 shadow-sm group/btn active:scale-95"
                                        title="Xóa">
                                    <Trash2 class="w-5 h-5 group-hover/btn:rotate-6 transition-transform" />
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr v-if="filteredList[activeTab].length === 0">
                        <td colspan="6" class="py-20 text-center">
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
                             detailsModal.data.status === 'PendingApproval' ? 'from-amber-400 to-amber-600' : 
                             detailsModal.data.status === 'Approved' ? 'from-blue-500 to-indigo-600' :
                             detailsModal.data.status === 'Active' ? 'from-violet-500 to-purple-600' :
                             detailsModal.data.status === 'Rejected' ? 'from-rose-500 to-rose-700' :
                             'from-emerald-500 to-teal-600']"></div>
                
                <!-- Close Button -->
                <div class="absolute top-4 right-6 z-[60] flex items-center gap-3">
                    <button @click="detailsModal.show = false" class="bg-slate-50 p-2 rounded-xl hover:bg-slate-200 transition-all text-slate-400 flex items-center justify-center border border-slate-100 shadow-sm" title="Đóng">
                        <X class="w-4 h-4" />
                    </button>
                </div>

                <!-- Main Content Body -->
                <div class="relative z-10 pt-4">
                    <div class="px-6 pt-2 pb-1">
                        <!-- Modal Header Info -->
                        <div class="flex flex-wrap items-center justify-between mb-4 gap-4">
                            <div class="flex items-center gap-4">
                                <div :class="['w-10 h-10 rounded-xl flex items-center justify-center shadow-inner border shrink-0 transition-all duration-500', 
                                             detailsModal.data.status === 'PendingApproval' ? 'bg-amber-50 text-amber-600 border-amber-100' : 
                                             detailsModal.data.status === 'Approved' ? 'bg-blue-50 text-blue-600 border-blue-100' :
                                             detailsModal.data.status === 'Active' ? 'bg-violet-50 text-violet-600 border-violet-100' :
                                             detailsModal.data.status === 'Rejected' ? 'bg-rose-50 text-rose-600 border-rose-100' :
                                             'bg-emerald-50 text-emerald-600 border-emerald-100']">
                                    <FileText class="w-5 h-5" />
                                </div>
                                <div>
                                    <div class="flex items-center gap-3">
                                        <h3 class="text-xl font-black text-slate-800">{{ detailsModal.data.contractName || 'Chi tiết hợp đồng' }}</h3>
                                        <span :class="['px-1.5 py-0.5 rounded-md text-[8px] font-black uppercase tracking-tighter border shrink-0 flex items-center h-4', getStatusClass(detailsModal.data.status)]">
                                            {{ getStatusLabel(detailsModal.data.status) }}
                                        </span>
                                    </div>
                                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">HĐ: {{ detailsModal.data.contractCode }} — {{ detailsModal.data.companyName }}</p>
                                </div>
                            </div>
                        </div>

                        <!-- Data Fields Grid -->
                        <div v-if="!isEditing" class="grid grid-cols-1 md:grid-cols-3 gap-x-4 gap-y-3 mb-4 pb-4 border-b border-slate-100">
                            <div class="flex flex-col gap-1 col-span-3">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên hợp đồng / Ghi chú</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-slate-700 min-h-[38px] flex items-center px-4 font-black py-2 text-xs">
                                    {{ detailsModal.data.contractName || '---' }}
                                </div>
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã hợp đồng</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-slate-700 min-h-[38px] flex items-center px-3 font-black py-2 text-xs">
                                    {{ detailsModal.data.contractCode || '---' }}
                                </div>
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá niêm yết</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-blue-600 min-h-[38px] flex items-center px-3 font-black py-2 text-xs">
                                    {{ formatPrice(detailsModal.data.unitPrice) }} / ng
                                </div>
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng / Quy mô</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-slate-700 min-h-[38px] flex items-center px-3 font-black py-2 text-xs">
                                    {{ detailsModal.data.expectedQuantity }} người
                                </div>
                            </div>
                            <div class="flex flex-col gap-1 md:col-span-1.5 md:col-start-1 md:col-end-2">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-slate-600 min-h-[38px] flex items-center px-3 font-black py-2 text-xs">
                                    {{ formatDate(detailsModal.data.startDate) }}
                                </div>
                            </div>
                            <div class="flex flex-col gap-1 md:col-span-1.5">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                                <div class="input-premium bg-slate-50/50 border-slate-100 text-slate-600 min-h-[38px] flex items-center px-3 font-black py-2 text-xs">
                                    {{ formatDate(detailsModal.data.endDate) }}
                                </div>
                            </div>
                        </div>

                        <!-- Edit Form Grid -->
                        <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-x-4 gap-y-3 mb-4">
                            <div class="flex flex-col gap-1 col-span-3">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên hợp đồng / Ghi chú</label>
                                <input v-model="detailsModal.data.contractName" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã hợp đồng</label>
                                <input v-model="detailsModal.data.contractCode" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá niêm *</label>
                                <CurrencyInput v-model="detailsModal.data.unitPrice" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng / Quy mô *</label>
                                <CurrencyInput v-model="detailsModal.data.expectedQuantity" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                                <input type="date" v-model="detailsModal.data.startDate" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div class="flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                                <input type="date" v-model="detailsModal.data.endDate" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full py-2 min-h-[38px] text-xs" />
                            </div>
                            <div v-if="!['Draft', 'Rejected'].includes(detailsModal.data.status)" class="md:col-span-3 flex flex-col gap-1">
                                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1 flex items-center gap-2">
                                    <ShieldCheck class="w-3 h-3" /> Trạng thái vận hành
                                </label>
                                <select v-model="detailsModal.data.status" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full cursor-pointer font-black text-xs uppercase tracking-widest py-2 min-h-[38px]">
                                    <option value="PendingApproval">{{ getStatusLabel('PendingApproval') }}</option>
                                    <option value="Approved">{{ getStatusLabel('Approved') }}</option>
                                    <option value="Active">{{ getStatusLabel('Active') }}</option>
                                    <option value="Finished">{{ getStatusLabel('Finished') }}</option>
                                    <option value="Locked">{{ getStatusLabel('Locked') }}</option>
                                </select>
                            </div>
                        </div>

                        <!-- Attachments -->
                        <div v-if="detailsModal.data.filePath" class="flex items-center gap-3 p-4 bg-indigo-50 rounded-2xl border border-indigo-100 mb-4">
                            <FileText class="w-6 h-6 text-indigo-600" />
                            <div class="flex-1">
                                <p class="text-[10px] font-black uppercase tracking-widest text-indigo-400">File đính kèm</p>
                                <p class="text-xs font-black text-slate-700 truncate">{{ detailsModal.data.filePath.split('\\').pop() }}</p>
                            </div>
                            <a :href="'/' + detailsModal.data.filePath" target="_blank" class="px-4 py-2 bg-white text-indigo-600 rounded-xl text-[10px] font-black shadow-sm">XEM FILE</a>
                        </div>

                        <!-- Status History -->
                        <div v-if="detailsModal.data.statusHistories?.length > 0" class="mt-4 pt-4 border-t border-slate-100">
                            <h4 class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2">
                                <History class="w-3 h-3" /> Lịch sử thay đổi
                            </h4>
                            <div class="space-y-2">
                                <div v-for="h in detailsModal.data.statusHistories" :key="h.id" class="flex gap-3 items-start bg-slate-50/50 p-3 rounded-xl border border-slate-200">
                                    <div class="w-1.5 h-1.5 rounded-full bg-indigo-400 mt-1.5 shrink-0"></div>
                                    <div class="flex-1 min-w-0 text-[10px]">
                                        <div class="flex justify-between items-center mb-0.5">
                                            <span class="font-black text-slate-800 uppercase tracking-tighter">
                                                {{ getStatusLabel(h.oldStatus) }} → {{ getStatusLabel(h.newStatus) }}
                                            </span>
                                            <span class="text-[8px] font-black text-slate-600">{{ formatDate(h.changedAt) }}</span>
                                        </div>
                                        <p v-if="h.note" class="text-slate-500 italic">"{{ h.note }}"</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div> <!-- End Padding Wrapper -->
                </div> <!-- End Main Content Body -->

                <!-- Approval Panel (Sibling to Body) -->
                <div v-if="!isEditing && (detailsModal.data.status === 'PendingApproval' || detailsModal.data.statusHistories?.length > 0)" class="border-t border-slate-100 bg-white">
                    <ContractApprovalPanel 
                        :contract="detailsModal.data"
                        :totalSteps="3"
                        @submit="handleSubmitForApproval"
                        @approved="handleSaveApproved"
                        @rejected="handleSaveApproved"
                        @reset="fetchList"
                    />
                </div>

                <!-- Footer Actions -->
                <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-between gap-3">
                    <div class="flex items-center gap-2">
                        <button v-if="!isEditing && ['Draft', 'Rejected'].includes(detailsModal.data.status) && canEdit" 
                                @click="handleDeleteContract(detailsModal.data.healthContractId)" 
                                class="w-10 h-10 rounded-xl bg-rose-50 text-rose-500 flex items-center justify-center hover:bg-rose-500 hover:text-white transition-all border border-rose-100 shadow-sm">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </div>

                    <div class="flex items-center gap-3">
                        <template v-if="!isEditing">
                            <button v-if="detailsModal.data.status === 'Draft' && canEdit" 
                                    @click="handleSubmitForApproval(detailsModal.data.healthContractId)"
                                    class="bg-indigo-600 text-white px-5 py-2.5 rounded-xl font-black transition-all flex items-center justify-center gap-2 shadow-lg text-[10px] uppercase tracking-widest">
                                <Send class="w-4 h-4" /> <span>GỬI PHÊ DUYỆT</span>
                            </button>
                            <button v-if="detailsModal.data.status === 'Approved' && canActivate" 
                                    @click="handleActivateContract(detailsModal.data.healthContractId)"
                                    class="bg-blue-600 text-white px-5 py-2.5 rounded-xl font-black shadow-lg text-[10px] uppercase tracking-widest flex items-center justify-center gap-2">
                                <Clock class="w-4 h-4" /> <span>KÍCH HOẠT</span>
                            </button>
                            <button v-if="detailsModal.data.status === 'Active' && canEdit" 
                                    @click="handleFinishContract(detailsModal.data.healthContractId)"
                                    class="bg-slate-800 text-white px-5 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest flex items-center justify-center gap-2">
                                <Lock class="w-4 h-4" /> <span>KẾT THÚC</span>
                            </button>
                            <button v-if="detailsModal.data.status === 'Finished' && canEdit" 
                                    @click="goToSettlement(detailsModal.data.healthContractId)"
                                    class="bg-emerald-600 text-white px-5 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest flex items-center justify-center gap-2">
                                <Calculator class="w-4 h-4" /> <span>QUYẾT TOÁN</span>
                            </button>
                            <button v-if="['Draft', 'Rejected'].includes(detailsModal.data.status) && canEdit" 
                                    @click="isEditing = true" 
                                    class="bg-white text-indigo-600 px-5 py-2.5 rounded-xl font-black border border-indigo-100 text-[10px] uppercase tracking-widest flex items-center justify-center gap-2">
                                CHỈNH SỬA
                            </button>
                            <button @click="detailsModal.show = false" 
                                    class="bg-white border border-slate-200 text-slate-500 px-5 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 hover:bg-slate-50 hover:border-slate-300 transition-all">
                                ĐÓNG
                            </button>
                        </template>
                        <template v-else>
                            <button @click="handleUpdateContract" 
                                    class="bg-emerald-600 text-white px-6 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 hover:bg-emerald-700 transition-all">
                                LƯU THAY ĐỔI
                            </button>
                            <button @click="isEditing = false" 
                                    class="bg-white text-slate-500 px-6 py-2.5 rounded-xl font-black border border-slate-200 text-[10px] uppercase tracking-widest flex items-center justify-center gap-2 hover:bg-slate-50 hover:border-slate-300 transition-all">
                                HỦY
                            </button>
                        </template>
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
import { useRouter } from 'vue-router'
import apiClient from '../services/apiClient'
import { parseApiError } from '../services/errorHelper'
import { 
    Plus, FileText, Search, PlusCircle, Edit3, Trash2, 
    Clock, CheckCircle, Activity, FileCheck, XCircle, X,
    ArrowRight, ArrowDown, History, Save, Lock, Unlock, Calculator,
    Building2, DollarSign, Users, ShieldCheck, Calendar, Settings2, Eye, Tag, Hash, CreditCard, Info,
    Send, Award, Undo, Quote, Ban
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

import ContractApprovalPanel from '../components/ContractApprovalPanel.vue'

import CurrencyInput from '../components/CurrencyInput.vue'
import { usePermission } from '../composables/usePermission'
import { useI18nStore } from '../stores/i18n'

const i18n = useI18nStore()
const { can } = usePermission()
const router = useRouter()
const canEdit = computed(() => can('HopDong.Edit'))
const canActivate = computed(() => can('HopDong.Edit'))

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
    startDate: null,
    endDate: null,
    unitPrice: 0,
    expectedQuantity: 0,
    unitName: 'Người',
    status: 'Draft'
})

const detailsModal = ref({ show: false, data: {} })
const isEditing = ref(false)
const activeTab = ref('pending')
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const filteredList = computed(() => {
    return {
        draft: list.value.filter(c => c.status === 'Draft'),
        pending: list.value.filter(c => c.status === 'PendingApproval'),
        approved: list.value.filter(c => c.status === 'Approved'),
        active: list.value.filter(c => c.status === 'Active' || c.status === 'InProgress'),
        finished: list.value.filter(c => ['Finished', 'Locked'].includes(c.status)),
        rejected: list.value.filter(c => c.status === 'Rejected')
    }
})

const getStatusClass = (status) => {
    switch(status) {
        case 'Draft': return 'bg-slate-50 text-slate-600 border-slate-200'
        case 'Approved': return 'bg-blue-50 text-blue-600 border-blue-100'
        case 'Active': return 'bg-violet-50 text-violet-600 border-violet-100'
        case 'Finished': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'Locked': return 'bg-slate-50 text-slate-500 border-slate-200'
        case 'Rejected': return 'bg-rose-50 text-rose-600 border-rose-100'
        default: return 'bg-amber-50 text-amber-600 border-amber-100'
    }
}

const getStatusLabel = (status) => {
    switch(status) {
        case 'Draft': return 'Bản nháp'
        case 'Active': return 'Đang thực hiện'
        case 'Approved': return 'Đã phê duyệt'
        case 'Finished': return 'Hợp đồng kết thúc'
        case 'Locked': return 'Đã khóa tài chính'
        case 'PendingApproval': return 'Đang chờ phê duyệt'
        case 'Rejected': return 'Bị từ chối'
        default: return status
    }
}

const fetchList = async () => {
    try {
        const res = await apiClient.get('/api/Contracts')
        list.value = res.data
    } catch (e) { toast.error("Lỗi khi tải danh sách hợp đồng") }
}

const fetchCompanies = async () => {
    try {
        const res = await apiClient.get('/api/Companies')
        companies.value = res.data
    } catch (e) { console.error(e) }
}

const addContract = async () => {
    try {
        if (!newContract.value.companyId) return toast.warning("Chưa chọn Công ty!")
        
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
        await apiClient.post('/api/Contracts', payload)
        toast.success("Tạo hợp đồng thành công!")
        fetchList()
        showForm.value = false
        resetForm()
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const resetForm = () => {
    newContract.value = { companyId: null, contractName: '', contractCode: '', signingDate: new Date().toISOString().split('T')[0], startDate: null, endDate: null, unitPrice: 0, expectedQuantity: 0, unitName: 'Người', status: 'Draft' }
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
        await apiClient.put(`/api/Contracts/${detailsModal.value.data.healthContractId}`, detailsModal.value.data)
        toast.success("Đã cập nhật hợp đồng!")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { 
        toast.error(parseApiError(e)) 
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
        await apiClient.post(`/api/Contracts/${currentUploadId.value}/upload`, formData)
        toast.success("Đã tải lên văn bản hợp đồng!")
        fetchList()
    } catch (err) { 
        toast.error(parseApiError(err)) 
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
                await apiClient.put(`/api/Contracts/${id}/lock`)
                toast.success("Hợp đồng đã được khóa an toàn")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(parseApiError(e)) 
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
                await apiClient.put(`/api/Contracts/${id}/unlock`)
                toast.success("Đã mở khóa hợp đồng")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(parseApiError(e)) 
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
                await apiClient.delete(`/api/Contracts/${id}`)
                toast.success("Đã xóa hợp đồng thành công")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { 
                toast.error(parseApiError(e)) 
            }
        }
    }
}

const handleSubmitForApproval = async (id) => {
    try {
        await apiClient.post(`/api/Contracts/${id}/submit`)
        toast.success("Đã gửi văn bản đi phê duyệt!")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { toast.error(parseApiError(e)) }
}

const handleSaveApproved = () => {
    toast.success("Hành động phê duyệt đã ghi nhận!")
    fetchList()
    detailsModal.value.show = false
}

const handleActivateContract = async (id) => {
    try {
        // Approved → Active (Đang thực hiện)
        await apiClient.patch(`/api/Contracts/${id}/status`, { status: 'Active', note: 'Kích hoạt hợp đồng để triển khai' })
        toast.success("Hợp đồng đã được kích hoạt! Bắt đầu triển khai.")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { toast.error(parseApiError(e)) }
}

const handleFinishContract = async (id) => {
    confirmData.value = {
        show: true,
        title: 'Kết thúc hợp đồng',
        message: 'Xác nhận hoàn tất hợp đồng này? Hệ thống sẽ kiểm tra các đoàn khám liên quan.',
        variant: 'info',
        onConfirm: async () => {
            try {
                await apiClient.patch(`/api/Contracts/${id}/status`, { status: 'Finished', note: 'Nghiệm thu kết thúc hợp đồng' })
                toast.success("Đã kết thúc hợp đồng!")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { toast.error(parseApiError(e)) }
        }
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN').format(p) + ' đ'
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'

const goToSettlement = (id) => {
  detailsModal.value.show = false
  router.push({ path: '/finance/settlement', query: { contractId: id } })
}

onMounted(() => {
    fetchList()
    fetchCompanies()
})
</script>
