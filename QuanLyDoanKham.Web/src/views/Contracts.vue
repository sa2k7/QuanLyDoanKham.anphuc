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
          <span class="text-blue-600 font-black tabular-nums">{{ String(list.length).padStart(3, '0') }}</span>
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
            :value="formatPrice(list.reduce((sum, c) => sum + c.totalAmount, 0))"
            :icon="DollarSign"
            variant="default"
        />
        <StatCard 
            title="Giá trị nghiệm thu"
            :value="formatPrice(list.filter(c => ['Finished', 'Locked'].includes(c.status)).reduce((sum, c) => sum + c.totalAmount, 0))"
            :icon="Sparkles"
            variant="emerald"
        />
        <StatCard 
            title="HĐ Đang thực hiện"
            :value="String(list.filter(c => ['Active', 'Pending'].includes(c.status)).length).padStart(3, '0')"
            :icon="Clock"
            variant="indigo"
            subtext="Dự án đang vận hành"
        />
        <StatCard 
            title="Tổng quy mô khám"
            :value="String(list.reduce((sum, c) => sum + (c.expectedQuantity || 0), 0)).padStart(3, '0')"
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
                <CurrencyInput v-model="newContract.unitPrice" required placeholder="0" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng dự kiến</label>
                <CurrencyInput v-model="newContract.expectedQuantity" required placeholder="0" />
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
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm', 
                         activeTab === 'pending' ? 'bg-gradient-to-r from-amber-400 to-amber-500 text-white shadow-amber-500/30' : 'bg-white/80 text-slate-400 border border-slate-100/50 hover:bg-slate-50']">
            Chờ duyệt ({{ String(filteredList.pending.length).padStart(3, '0') }})
        </button>
        <button @click="activeTab = 'approved'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm', 
                         activeTab === 'approved' ? 'bg-gradient-to-r from-blue-500 to-indigo-500 text-white shadow-blue-500/30' : 'bg-white/80 text-slate-400 border border-slate-100/50 hover:bg-slate-50']">
            Đã phê duyệt ({{ String(filteredList.approved.length).padStart(3, '0') }})
        </button>
        <button @click="activeTab = 'active'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm', 
                         activeTab === 'active' ? 'bg-gradient-to-r from-blue-500 to-indigo-500 text-white shadow-blue-500/30' : 'bg-white/80 text-slate-400 border border-slate-100/50 hover:bg-slate-50']">
            Đang thực hiện ({{ String(filteredList.active.length).padStart(3, '0') }})
        </button>
        <button @click="activeTab = 'finished'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-sm', 
                         activeTab === 'finished' ? 'bg-gradient-to-r from-emerald-500 to-teal-500 text-white shadow-emerald-500/30' : 'bg-white/80 text-slate-400 border border-slate-100/50 hover:bg-slate-50']">
            Đã kết thúc ({{ String(filteredList.finished.length).padStart(3, '0') }})
        </button>
    </div>

    <!-- Contract Table -->
    <div class="premium-card bg-white/95 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden mt-4">
        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-4 text-center w-16">STT</th>
                        <th class="p-4">Dự án / Đối tác</th>
                        <th class="p-4">Giá trị Hợp đồng</th>
                        <th class="p-4">Quy mô</th>
                        <th class="p-4 text-center">Trạng thái</th>
                        <th class="p-4">Hạn Hợp đồng</th>
                        <th class="p-4 text-center">Tác vụ</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(item, index) in filteredList[activeTab]" :key="item.healthContractId" 
                        class="text-xs hover:bg-slate-50/50 transition-all cursor-pointer" @click="openDetails(item)">
                        <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                        <td class="p-4">
                            <div class="flex items-center gap-3">
                                <div class="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center">
                                    <FileText class="w-4 h-4 shadow-sm" />
                                </div>
                                <div>
                                    <h4 class="font-black text-slate-800 uppercase tracking-widest group-hover:text-primary transition-colors">
                                        [HĐ-{{ item.healthContractId }}] {{ item.shortName || item.companyName }}
                                    </h4>
                                    <p class="text-[9px] font-black text-slate-400 mt-1 uppercase tracking-widest flex items-center gap-1">
                                        HĐ-{{ item.healthContractId }} <Calendar class="w-3 h-3 ml-2" /> {{ formatDate(item.signingDate) }}
                                    </p>
                                </div>
                            </div>
                        </td>
                        <td class="p-4 font-black text-indigo-600">
                            {{ formatPrice(item.totalAmount) }}
                        </td>
                        <td class="p-4 text-slate-700 font-black">
                            {{ item.expectedQuantity }} <span class="text-[9px] text-slate-400 uppercase tracking-widest ">{{ item.unitName }}</span>
                        </td>
                        <td class="p-4 text-center">
                            <span :class="['px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-widest border', getStatusClass(item.status)]">
                                {{ getStatusLabel(item.status) }}
                            </span>
                        </td>
                        <td class="p-4 text-slate-500 font-black uppercase tracking-widest text-[10px]">
                            {{ formatDate(item.endDate) }}
                        </td>
                        <td class="p-4 text-center">
                            <div class="flex items-center justify-center gap-2">
                                <button @click.stop="openDetails(item)" class="btn-action-premium variant-indigo text-slate-400" title="Chi tiết">
                                    <Eye class="w-5 h-5" />
                                </button>
                                <button v-if="(authStore.role === 'Admin' || authStore.role === 'ContractManager') && item.status === 'Pending'" 
                                        @click.stop="openModal(item)"
                                        class="btn-action-premium variant-indigo text-slate-400" title="Sửa">
                                    <Edit3 class="w-5 h-5" />
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
    <div v-if="detailsModal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-md p-4 overflow-y-auto">
        <div class="bg-white/95 backdrop-blur-3xl w-full max-w-2xl rounded-[2.5rem] border border-slate-100 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
            
            <!-- Header Accent Line -->
            <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-blue-500 to-blue-700 z-0"></div>
            
            <!-- Close Button -->
            <button @click="detailsModal.show = false" class="absolute top-8 right-8 bg-slate-100 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-500 z-[60] flex items-center justify-center border border-slate-100 shadow-sm">
                <X class="w-5 h-5" />
            </button>

            <div class="relative z-10 pt-10">
                <div class="p-10 pb-4">
                    <div class="flex justify-between items-start mb-8">
                        <div class="flex items-center gap-4">
                            <div class="w-14 h-14 bg-blue-50 text-blue-600 rounded-3xl flex items-center justify-center shadow-inner border border-blue-100 shrink-0">
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

                <div v-if="!isEditing" class="grid grid-cols-2 gap-8 mb-8">
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Tổng giá trị</p>
                        <p class="text-2xl font-black text-primary">{{ formatPrice(detailsModal.data.totalAmount) }}</p>
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Quy mô (Dự kiến)</p>
                        <p class="text-2xl font-black text-slate-700">{{ detailsModal.data.expectedQuantity }} {{ detailsModal.data.unitName }}</p>
                    </div>
                </div>

                <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                    <div class="bg-slate-50 p-6 rounded-[2rem] border border-slate-100/50 shadow-sm">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá niêm</label>
                        <CurrencyInput v-model="detailsModal.data.unitPrice" class="input-premium bg-white !p-2 mt-2 border-slate-200 focus:border-indigo-500 w-full shadow-sm" />
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem] border border-slate-100/50 shadow-sm">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng / Quy mô</label>
                        <CurrencyInput v-model="detailsModal.data.expectedQuantity" class="input-premium bg-white !p-2 mt-2 border-slate-200 focus:border-indigo-500 w-full shadow-sm" />
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem] border border-slate-100/50 shadow-sm">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                        <input type="date" v-model="detailsModal.data.startDate" class="input-premium bg-white !p-2 mt-2 border-slate-200 focus:border-indigo-500 w-full shadow-sm" />
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem] border border-slate-100/50 shadow-sm">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                        <input type="date" v-model="detailsModal.data.endDate" class="input-premium bg-white !p-2 mt-2 border-slate-200 focus:border-indigo-500 w-full shadow-sm" />
                    </div>
                    <div class="md:col-span-2 bg-slate-50 p-6 rounded-[2rem] border border-slate-100/50 shadow-sm">
                        <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Trạng thái vận hành</label>
                        <select v-model="detailsModal.data.status" class="input-premium bg-white !p-2 mt-2 border-slate-200 focus:border-indigo-500 w-full shadow-sm cursor-pointer font-black text-xs uppercase tracking-widest">
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
                                class="flex-1 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-8 py-4 rounded-2xl font-black transition-all shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:-translate-y-1 active:scale-95 flex items-center justify-center">
                            <Save class="w-5 h-5 mr-2" />
                            LƯU THAY ĐỔI
                        </button>
                        <button @click="isEditing = false" 
                                class="bg-slate-50 text-slate-500 px-8 py-4 rounded-2xl font-black hover:bg-slate-100 transition-all border border-slate-200 shadow-sm">
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
    
    <!-- Confirm Dialog -->
    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Plus, FileText, Calendar, ArrowRight, Trash2, Save, PlusCircle, History, Sparkles, Clock, Lock, Upload, X, DollarSign, Users, Eye, Edit3, Unlock } from 'lucide-vue-next'
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
        case 'Active': return 'bg-sky-50 text-sky-600 border-sky-100'
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

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'

onMounted(() => {
    fetchList()
    fetchCompanies()
})
</script>
