<template>
  <div class="space-y-8 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20">
            <Stethoscope class="w-6 h-6" />
          </div>
          {{ i18n.t('groups.title') }}
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-primary font-black tabular-nums">{{ String(groups.length).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[9px] mt-2">{{ i18n.t('groups.subtitle') }}</p>
      </div>

      <div class="flex items-center gap-4">
        <button v-if="can('DoanKham.Create')" 
                @click="exportGroups"                 class="btn-premium bg-white border-2 border-slate-900 text-slate-800 px-6 py-3 rounded-xl shadow-[4px_4px_0px_#0f172a] hover:bg-slate-50 transition-all font-black">
            <Download class="w-4 h-4 mr-2" />
            <span class="text-[10px] font-black uppercase tracking-widest">Xuất DS Đoàn</span>
        </button>
        <button v-if="can('DoanKham.Create')" 
                @click="showForm = !showForm"                 class="btn-premium bg-primary text-white px-8 py-3 rounded-xl border-2 border-primary shadow-[4px_4px_0px_#152a41] hover:bg-primary/90 transition-all font-black">
            <Plus class="w-5 h-5 mr-2" />
            <span class="text-[10px] font-black uppercase tracking-widest ">{{ i18n.t('groups.addBtn') }}</span>
        </button>
      </div>
    </div>



    <!-- Smart Create Form -->
    <div v-if="showForm" class="premium-card p-10 bg-white rounded-[2rem] shadow-[4px_4px_0px_#0f172a] border-2 border-slate-900 mb-10 animate-slide-up relative overflow-hidden">
        <div class="absolute top-0 right-0 p-4">
            <button @click="showForm = false" class="p-2 hover:bg-slate-100 rounded-full transition-all">
                <X class="w-5 h-5 text-slate-400" />
            </button>
        </div>

        <!-- Tabs for Creation Mode -->
        <div class="flex gap-4 mb-8 p-1 bg-slate-50 rounded-2xl border-2 border-slate-100">
            <button @click="createMode = 'manual'" :class="['flex-1 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all', createMode === 'manual' ? 'bg-white text-slate-900 shadow-sm border border-slate-200' : 'text-slate-400 opacity-50']">
                Khai báo thủ công
            </button>
            <button @click="createMode = 'smart'" :class="['flex-1 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all', createMode === 'smart' ? 'bg-white text-slate-900 shadow-sm border border-slate-200' : 'text-slate-400 opacity-50']">
                Clone từ đoàn cũ
            </button>
            <button @click="createMode = 'auto'" :class="['flex-1 py-3 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all', createMode === 'auto' ? 'bg-indigo-600 text-white shadow-lg' : 'text-slate-400 opacity-50']">
                 PHÂN BỔ TỰ ĐỘNG (BETA)
            </button>
        </div>

        <!-- Mode: Manual -->
        <form v-if="createMode === 'manual'" @submit.prevent="addGroup" class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Hợp đồng mục tiêu</label>
                <select v-model="newGroup.healthContractId" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                    <option :value="null" disabled>-- Chọn hợp đồng --</option>
                    <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                        [HĐ-{{ c.healthContractId }}] {{ c.shortName || c.companyName }}
                    </option>
                </select>
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Tên đoàn khám</label>
                <input v-model="newGroup.groupName" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all placeholder:text-slate-300" placeholder="VD: Khám sức khỏe CN 2026" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Ngày triển khai</label>
                <input v-model="newGroup.examDate" type="date" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all" />
            </div>
            <div class="md:col-span-3 flex justify-end pt-4">
                 <button type="submit" class="btn-premium bg-slate-900 text-white px-12 py-4 rounded-2xl border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] hover:bg-slate-800 transition-all font-black uppercase tracking-widest text-[11px] ">KÍCH HOẠT ĐOÀN MỚI</button>
            </div>
        </form>

        <!-- Mode: Smart/Clone -->
        <div v-if="createMode === 'smart'" class="grid grid-cols-1 md:grid-cols-2 gap-8">
             <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Chọn Hợp đồng nguồn</label>
                <select v-model="selectedContractForAuto" @change="onSmartContractSelect" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                    <option :value="null" disabled>-- Chọn hợp đồng --</option>
                    <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                        [HĐ-{{ c.healthContractId }}] {{ c.shortName || c.companyName }}
                    </option>
                </select>
            </div>
            <div v-if="smartPreview" class="bg-indigo-50 rounded-[1.5rem] p-6 border-2 border-indigo-100 space-y-4">
                <div class="space-y-3">
                    <div class="flex items-center gap-2">
                        <span class="text-[9px] font-black text-slate-500 uppercase tracking-widest w-24">Tên đoàn:</span>
                        <input v-model="smartPreview.groupName" class="flex-1 bg-white rounded-lg px-3 py-2 text-xs font-black text-slate-800 border border-indigo-100 outline-none" />
                    </div>
                    <div class="flex items-center gap-2">
                        <span class="text-[9px] font-black text-slate-500 uppercase tracking-widest w-24">Ngày:</span>
                        <input v-model="smartPreview.examDate" type="date" class="flex-1 bg-white rounded-lg px-3 py-2 text-xs font-black text-slate-800 border border-indigo-100 outline-none" />
                    </div>
                </div>
                <div v-if="lastGroupOfContract" class="flex items-center gap-3 p-3 bg-white rounded-xl border border-indigo-100">
                    <input type="checkbox" id="cloneStaff" v-model="doCloneStaff" class="w-4 h-4 accent-indigo-600" />
                    <label for="cloneStaff" class="text-[10px] font-black text-slate-700 cursor-pointer">Sao chép đội ngũ từ đoàn cũ</label>
                </div>
                <button @click="confirmSmartCreate" class="w-full bg-indigo-600 text-white py-3 rounded-xl font-black text-[10px] uppercase tracking-widest border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] hover:bg-indigo-700 transition-all">
                    XÁC NHẬN TẠO NHANH
                </button>
            </div>
            <div v-else class="flex items-center justify-center border-2 border-dashed border-slate-100 rounded-3xl p-10 text-slate-300 font-black text-[10px] uppercase tracking-widest text-center">
                Vui lòng chọn hợp đồng để xem trước
            </div>
        </div>

        <!-- Mode: Auto-Assignment -->
        <div v-if="createMode === 'auto'" class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div class="space-y-6">
                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Hợp đồng & Thông tin cơ bản</label>
                    <select v-model="autoAssignData.healthContractId" @change="onAutoContractSelect" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                        <option :value="null" disabled>-- Chọn hợp đồng --</option>
                        <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                            [HĐ-{{ c.healthContractId }}] {{ c.shortName || c.companyName }}
                        </option>
                    </select>
                    <input v-model="autoAssignData.groupName" placeholder="Tên đoàn khám" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all mt-3" />
                    <input v-model="autoAssignData.examDate" type="date" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all mt-3" />
                </div>
            </div>

            <div class="bg-indigo-600/5 rounded-3xl p-8 border-2 border-indigo-600/10 space-y-6">
                <h5 class="text-[10px] font-black uppercase tracking-widest text-indigo-600 flex items-center gap-2">
                    <Sparkles class="w-4 h-4" /> Cấu hình mô hình phân bổ
                </h5>
                <div class="grid grid-cols-2 gap-4">
                    <div class="space-y-2">
                        <label class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Định mức (Người/NV)</label>
                        <input v-model.number="autoAssignData.targetRatio" type="number" class="w-full px-4 py-3 rounded-xl bg-white border border-slate-200 outline-none font-black text-slate-700" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Bác sĩ tối thiểu</label>
                        <input v-model.number="autoAssignData.minimumDoctors" type="number" class="w-full px-4 py-3 rounded-xl bg-white border border-slate-200 outline-none font-black text-slate-700" />
                    </div>
                </div>
                <div class="space-y-2">
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Chế độ nghiêm ngặt</label>
                    <select v-model="autoAssignData.assignmentMode" class="w-full px-4 py-3 rounded-xl bg-white border border-slate-200 outline-none font-black text-slate-700">
                        <option value="Partial">Lướt qua (Điền tối đa nhân sự hiện có)</option>
                        <option value="Strict">Nghiêm ngặt (Rollback nếu thiếu nhân sự P0)</option>
                    </select>
                </div>
                <button @click="handleAutoAssign" :disabled="isAutoAssignLoading" class="w-full bg-slate-900 text-white py-4 rounded-2xl font-black text-xs border-2 border-slate-900 shadow-[4px_4px_0px_#1e293b] active:scale-95 transition-all flex items-center justify-center gap-3">
                    <RefreshCw v-if="isAutoAssignLoading" class="w-4 h-4 animate-spin" />
                    <Zap v-else class="w-4 h-4" />
                    {{ isAutoAssignLoading ? 'HỆ THỐNG ĐANG PHÂN BỔ...' : 'PHÂN BỔ NHÂN SỰ & TẠO ĐOÀN' }}
                </button>
            </div>
        </div>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-6">
        <button @click="activeTab = 'Open'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Open' ? 'bg-primary text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            {{ i18n.t('groups.tabOpen') }} ({{ openGroups.length }})
        </button>
        <button @click="activeTab = 'Finished'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Finished' ? 'bg-slate-800 text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            {{ i18n.t('groups.tabFinished') }} ({{ closedGroups.length }})
        </button>
    </div>

    <!-- Danh sách Đoàn -->
    <div class="space-y-6">
        <div v-if="filteredGroups.length === 0" class="premium-card bg-white rounded-[2rem] shadow-xl border border-slate-100 p-16 text-center flex flex-col items-center justify-center">
            <div class="w-24 h-24 bg-slate-50 text-slate-300 rounded-[2rem] flex items-center justify-center mb-6 border border-slate-100 shadow-inner">
                <Stethoscope class="w-12 h-12" />
            </div>
            <h4 class="text-xl font-black text-slate-800 uppercase tracking-widest">{{ i18n.t('common.noData') }}</h4>
            <p class="text-xs font-black text-slate-400 mt-2 uppercase tracking-widest">Không có dữ liệu hiển thị trong mục này</p>
        </div>

        <div v-for="group in filteredGroups" :key="group.groupId" 
             class="premium-card bg-white rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden group/card">
            
            <div class="p-5 bg-primary text-white flex justify-between items-center transition-all">
                <div class="flex items-center gap-5">
                    <div class="w-14 h-14 bg-white/20 backdrop-blur-md rounded-2xl flex items-center justify-center text-white shadow-sm border border-white/10">
                        <Stethoscope class="w-7 h-7" />
                    </div>
                    <div>
                        <h4 class="text-xl font-black ">{{ group.groupName }}</h4>
                        <p class="text-[11px] font-black text-white/80 uppercase tracking-widest mt-1">
                            [HĐ-{{ group.healthContractId }}] • {{ group.shortName || group.companyName }} • {{ formatDate(group.examDate) }}
                        </p>
                    </div>
                </div>
                <div class="flex items-center gap-3">
                   <span :class="['px-4 py-1.5 rounded-full text-[9px] font-black uppercase tracking-widest border border-white/20 shadow-sm', getStatusClass(group.status)]">{{ getStatusLabel(group.status) }}</span>
                   <button v-if="group.status === 'Open' && can('DoanKham.Edit')" @click="updateStatus(group.groupId, 'Finished')" class="text-[9px] font-black bg-white text-primary hover:bg-slate-50 px-4 py-2 rounded-xl transition-all shadow-sm">{{ i18n.t('groups.btnFinish') }}</button>
                </div>
            </div>

            <div class="p-8">
                <!-- Tabs for Card -->
                <div class="flex gap-4 mb-6 border-b border-slate-100 pb-2 mt-2">
                    <button @click="cardTab[group.groupId] = 'staffs'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', (cardTab[group.groupId] || 'staffs') === 'staffs' ? 'bg-primary text-white' : 'bg-white text-slate-400 hover:bg-slate-100']">{{ i18n.t('groups.tabStaff') }}</button>
                    <button @click="cardTab[group.groupId] = 'positions'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', cardTab[group.groupId] === 'positions' ? 'bg-primary text-white' : 'bg-white text-slate-400 hover:bg-slate-100']">{{ i18n.t('groups.tabPositions') }}</button>
                </div>

                <!-- Nhân sự List Table -->
                <div v-show="(cardTab[group.groupId] || 'staffs') === 'staffs'" class="space-y-6 animate-fade-in">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400 ">
                            <UsersIcon class="w-4 h-4" /> Đội ngũ đi khám
                        </h5>
                        <div class="flex items-center gap-3">
                            <button v-if="group.status === 'Open' && can('DoanKham.StaffAssign')" @click="openStaffModal(group.groupId)" class="text-[9px] font-black text-primary uppercase tracking-widest hover:underline">{{ i18n.t('groups.btnAssign') }}</button>
                            <div v-if="can('DoanKham.StaffAssign')" class="w-px h-3 bg-slate-200"></div>
                            <button v-if="group.status === 'Open' && can('DoanKham.StaffAssign')" @click="handleAiSuggest(group.groupId)" :disabled="isAiLoading" class="text-[9px] font-black text-primary uppercase tracking-widest hover:underline flex items-center gap-1">
                                <Sparkles class="w-3 h-3" v-if="!isAiLoading" />
                                <RefreshCw class="w-3 h-3 animate-spin" v-else />
                                {{ isAiLoading ? 'AI ĐANG TÍNH TOÁN...' : '✨ ' + i18n.t('groups.btnAICopilot') }}
                            </button>
                            <div class="w-px h-3 bg-slate-200"></div>
                            <button @click="exportGroupStaff(group.groupId, group.groupName)" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest hover:underline">XUẤT DS ĐI ĐOÀN (EXCEL)</button>
                            <div class="w-px h-3 bg-slate-200"></div>
                            <button v-if="group.status === 'Open' && can('ChamCong.QR')" @click="openQrModal(group.groupId)" class="text-[9px] font-black text-purple-600 uppercase tracking-widest hover:underline flex items-center gap-1">
                                <QrCode class="w-3 h-3" />
                                MÃ QR CHẤM CÔNG
                            </button>
                        </div>
                    </div>
                    
                    <div class="overflow-x-auto border border-slate-100 rounded-2xl">
                        <table class="w-full text-left bg-white">
                            <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                                <tr>
                                    <th class="p-4 text-center w-16">{{ i18n.t('common.stt') }}</th>
                                    <th class="p-4">{{ i18n.t('groups.table.staff') }}</th>
                                    <th class="p-4">{{ i18n.t('groups.table.position') }}</th>
                                    <th class="p-4 text-center">{{ i18n.t('groups.table.shift') }}</th>
                                    <th class="p-4 text-center">Trạng thái</th>
                                    <th class="p-4 text-center">{{ i18n.t('common.actions') }}</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-slate-50 text-xs">
                                <tr v-if="!staffDetails[group.groupId] || staffDetails[group.groupId].length === 0">
                                    <td colspan="6" class="p-8 text-center bg-slate-50/50">
                                        <div class="flex flex-col items-center justify-center text-slate-400">
                                            <UsersIcon class="w-8 h-8 mb-3 opacity-20" />
                                            <span class="text-[10px] font-black uppercase tracking-widest">Chưa gán nhân sự nào</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr v-for="(s, index) in staffDetails[group.groupId]" :key="s.id" class="hover:bg-slate-50/50 transition-all">
                                    <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                                    <td class="p-4">
                                        <div class="font-black text-slate-800 uppercase tracking-widest">{{ s.fullName }}</div>
                                        <div class="text-[9px] text-slate-400 uppercase tracking-widest font-black mt-1">{{ s.jobTitle }}</div>
                                    </td>
                                    <td class="p-4">
                                        <span class="px-3 py-1 bg-indigo-50 text-primary rounded-lg font-black uppercase tracking-widest text-[9px]">
                                            {{ s.workPosition || 'Chưa gán' }}
                                        </span>
                                    </td>
                                    <td class="p-4 text-center font-black">{{ s.shiftType === 1 ? 'Cả ngày' : 'Nửa ngày' }}</td>
                                    <td class="p-4 text-center">
                                        <div class="flex flex-col items-center gap-1">
                                            <span :class="['px-3 py-1 rounded-lg font-black text-[9px] uppercase tracking-widest ', 
                                                        s.workStatus === 'Đã tham gia' ? 'bg-emerald-100 text-emerald-600' : 
                                                        s.workStatus === 'Vắng mặt' ? 'bg-rose-100 text-rose-600' : 'bg-slate-100 text-slate-400']">
                                                {{ s.workStatus || 'Đang chờ' }}
                                            </span>
                                            <span v-if="s.checkInTime" class="text-[8px] font-black text-slate-400">Vào: {{ new Date(s.checkInTime).toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'}) }}</span>
                                        </div>
                                    </td>
                                    <td class="p-4">
                                        <div class="flex items-center justify-center gap-2">
                                            <button v-if="canCheckIn(s, group)" 
                                                    @click="checkIn(s.id, group.groupId)"
                                                    class="btn-action-premium variant-indigo text-slate-400" title="Vào đoàn">
                                                <LogIn class="w-5 h-5" />
                                            </button>
                                            <button v-if="canCheckOut(s, group)" 
                                                    @click="checkOut(s.id, group.groupId)"
                                                    class="btn-action-premium variant-rose text-slate-400" title="Rời đoàn">
                                                <LogOut class="w-5 h-5" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                                <tr v-if="!staffDetails[group.groupId]?.length">
                                    <td colspan="6" class="p-10 text-center text-slate-300 font-black uppercase tracking-widest text-[10px] ">Chưa phân bổ nhân sự</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>

                <!-- Vị trí List Table -->
                <div v-show="cardTab[group.groupId] === 'positions'" class="space-y-6 animate-fade-in">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400 ">
                            <ShieldCheck class="w-4 h-4" /> Cơ cấu vị trí trực tại đoàn
                        </h5>
                        <div class="flex items-center gap-3">
                            <button v-if="group.status === 'Open' && can('DoanKham.SetPosition')" @click="openPositionModal(group.groupId)" class="text-[9px] font-black text-primary uppercase tracking-widest hover:underline">{{ i18n.t('groups.btnAddPosition') }}</button>
                        </div>
                    </div>
                    
                    <div class="overflow-x-auto border border-slate-100 rounded-2xl">
                        <table class="w-full text-left bg-white">
                            <thead class="bg-indigo-50/50 text-[10px] font-black uppercase tracking-widest text-primary">
                                <tr>
                                    <th class="p-4 w-16 text-center">STT</th>
                                    <th class="p-4">Tên Vị trí</th>
                                    <th class="p-4 text-center">Số lượng cần</th>
                                    <th class="p-4 text-center">Đã phân công</th>
                                    <th class="p-4 text-center">Tác vụ</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-slate-50 text-xs">
                                <tr v-for="(p, index) in groupPositions[group.groupId]" :key="p.positionId" class="hover:bg-slate-50/50 transition-all">
                                    <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                                    <td class="p-4 font-black text-slate-800 uppercase tracking-widest">{{ p.positionName }}</td>
                                    <td class="p-4 text-center font-black">{{ p.requiredCount }}</td>
                                    <td class="p-4 text-center">
                                       <span :class="['px-3 py-1 text-primary rounded-lg font-black text-[9px]', p.assignedCount >= p.requiredCount ? 'bg-emerald-50 text-emerald-600' : 'bg-indigo-50']">
                                          {{ p.assignedCount }} / {{ p.requiredCount }}
                                       </span>
                                    </td>
                                    <td class="p-4 text-center">
                                        <button v-if="group.status === 'Open' && can('DoanKham.SetPosition')" 
                                                @click="removePosition(p.positionId, group.groupId)"
                                                class="text-rose-400 hover:text-rose-600 p-2" title="Xóa vị trí">
                                            <Trash2 class="w-4 h-4" />
                                        </button>
                                    </td>
                                </tr>
                                <tr v-if="!groupPositions[group.groupId]?.length">
                                    <td colspan="5" class="p-10 text-center text-slate-300 font-black uppercase tracking-widest text-[10px] ">Chưa có cấu hình vị trí cho đoàn này.</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>

                <!-- Tài liệu đính kèm -->
                <div class="mt-10 pt-8 border-t border-slate-50 flex flex-col md:flex-row justify-between items-center gap-6">
                    <div class="flex items-center gap-4">
                        <FileText class="w-5 h-5 text-slate-400" />
                        <span class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Dữ liệu kết quả & Tài liệu đoàn</span>
                    </div>
                    <div class="flex items-center gap-3">
                        <div v-if="group.importFilePath" class="flex items-center gap-3 p-3 bg-emerald-50 rounded-xl border border-emerald-100">
                             <FileIcon class="w-4 h-4 text-emerald-500" />
                             <span class="text-[10px] font-black text-slate-600 truncate max-w-[150px]">{{ group.importFilePath.split('/').pop() }}</span>
                             <a :href="'/' + group.importFilePath" target="_blank" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest underline ml-2">Tải về</a>
                        </div>
                        <button v-if="group.status === 'Open'" @click="triggerImport(group.groupId)" class="btn-premium bg-white border border-slate-200 text-slate-600 text-[10px] px-6">
                           IMPORT KẾT QUẢ
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Staff Selection Modal -->
    <Teleport to="body">
      <div v-if="modals.staff.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4 overflow-y-auto">
          <div class="bg-white w-full max-w-xl rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Accent Line -->
              <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

              <button @click="modals.staff.show = false" class="absolute top-8 right-8 bg-white p-2 rounded-full hover:bg-slate-100 transition-all text-slate-400 z-[60] flex items-center justify-center border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a]">
                  <X class="w-5 h-5" />
              </button>

              <div class="relative z-10 pt-12">
                  <div class="p-10 pb-6">
                      <div class="flex items-center gap-4 mb-8">
                          <div class="w-14 h-14 bg-teal-50 text-teal-600 rounded-3xl flex items-center justify-center shadow-inner border border-teal-100">
                              <UsersIcon class="w-7 h-7" />
                          </div>
                          <div>
                              <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest ">Điều động nhân sự</h3>
                              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Phân bổ vị trí & Ca làm tại đoàn khám</p>
                          </div>
                      </div>

                      <form id="staffForm" @submit.prevent="addStaff" class="space-y-6">
                          <div class="space-y-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                                  <CheckCircle2 class="w-3 h-3 text-indigo-400" /> Chọn nhân viên (Đảm bảo không trùng lịch)
                              </label>
                              <select v-model="modals.staff.data.staffId" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all appearance-none">
                                  <option :value="null" disabled>-- Chọn nhân sự từ danh sách --</option>
                                  <option v-for="s in staffList" :key="s.staffId" :value="s.staffId">{{ s.fullName }} — {{ s.jobTitle }}</option>
                              </select>
                          </div>
                          
                          <div class="grid grid-cols-2 gap-4">
                              <div class="space-y-2">
                                  <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                                      <Clock class="w-3 h-3 text-indigo-400" /> Loại ca làm
                                  </label>
                                  <select v-model="modals.staff.data.shiftType" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                                      <option :value="1.0">Cả ngày (1.0)</option>
                                      <option :value="0.5">Nửa ngày (0.5)</option>
                                  </select>
                              </div>
                              <div class="space-y-2">
                                  <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                                      <RefreshCw class="w-3 h-3 text-indigo-400" /> Trạng thái
                                  </label>
                                  <select v-model="modals.staff.data.workStatus" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                                      <option value="Đang chờ">Đang chờ</option>
                                      <option value="Đã tham gia">Đã tham gia</option>
                                  </select>
                              </div>
                          </div>

                          <div class="space-y-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                                  <Stethoscope class="w-3 h-3 text-indigo-400" /> Vị trí làm việc
                              </label>
                              <div v-if="selectedStaffType === 'BacSi'" class="flex items-center gap-2 px-3 py-2 bg-amber-50 rounded-xl border border-amber-100 mb-2">
                                  <span class="text-[9px] font-black text-amber-600 uppercase tracking-widest">⚠️ Bác sĩ chỉ được phân vào vị trí khám bệnh</span>
                              </div>
                              <select v-model="modals.staff.data.positionId" required @change="onStaffSelect" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                                  <option :value="null" disabled>-- Chọn vị trí trống --</option>
                                  <option v-for="p in groupPositions[modals.staff.groupId]" :key="p.positionId" :value="p.positionId" :disabled="selectedStaffType === 'BacSi' && !isDoctorPosition(p.positionName)">
                                      {{ p.positionName }} (Đã xếp: {{ p.assignedCount }}/{{ p.requiredCount }})
                                  </option>
                              </select>
                          </div>
                      </form>
                  </div>

                  <div class="px-10 pb-10 pt-2 bg-white relative z-20">
                      <div class="flex gap-4 pt-4">
                          <button type="button" @click="modals.staff.show = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest hover:text-slate-600 transition-all underline decoration-2 underline-offset-8">Hủy bỏ</button>
                           <button form="staffForm" type="submit" class="flex-[2] bg-slate-900 text-white py-4 rounded-2xl font-black text-xs uppercase tracking-[0.2em] border-2 border-slate-900 shadow-[4px_4px_0px_#1e293b] transition-all active:scale-95 hover:bg-teal-600">XÁC NHẬN ĐIỀU ĐỘNG</button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- Position Creation Modal -->
    <Teleport to="body">
      <div v-if="modals.position.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4 overflow-y-auto">
          <div class="bg-white w-full max-w-lg rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-indigo-400 to-indigo-600 z-0"></div>

              <button @click="modals.position.show = false" class="absolute top-8 right-8 bg-white p-2 rounded-full hover:bg-slate-100 transition-all text-slate-400 z-[60] flex items-center justify-center border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a]">
                  <X class="w-5 h-5" />
              </button>

              <div class="relative z-10 pt-12">
                  <div class="p-10 pb-6">
                      <div class="flex items-center gap-4 mb-8">
                          <div class="w-14 h-14 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center shadow-inner border border-indigo-100">
                              <ShieldCheck class="w-7 h-7" />
                          </div>
                          <div>
                              <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest ">Thêm vị trí trực</h3>
                              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Thiết lập cơ cấu nhân sự</p>
                          </div>
                      </div>

                      <form id="positionForm" @submit.prevent="addPosition" class="space-y-6">
                          <div class="space-y-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">Tên vị trí (Trạm)</label>
                              <input v-model="modals.position.data.positionName" required placeholder="VD: Khám nội, Siêu âm..." class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all" />
                          </div>
                          
                          <div class="space-y-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">Số lượng cần thiết</label>
                              <input v-model.number="modals.position.data.requiredCount" type="number" min="1" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all" />
                          </div>
                      </form>
                  </div>

                  <div class="px-10 pb-10 pt-2 bg-white relative z-20">
                      <div class="flex gap-4 pt-4">
                          <button type="button" @click="modals.position.show = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest hover:text-slate-600 transition-all underline decoration-2 underline-offset-8">Hủy bỏ</button>
                           <button form="positionForm" type="submit" class="flex-[2] bg-slate-900 text-white py-4 rounded-2xl font-black text-xs uppercase tracking-[0.2em] border-2 border-slate-900 shadow-[4px_4px_0px_#1e293b] transition-all active:scale-95 hover:bg-indigo-600">LƯU VỊ TRÍ</button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- QR Attendance Modal -->
    <Teleport to="body">
      <div v-if="modals.qr.show" class="fixed inset-0 z-[110] flex items-center justify-center bg-slate-900/90 backdrop-blur-xl p-4 overflow-y-auto">
          <div class="bg-white w-full max-w-sm rounded-[3rem] border-4 border-slate-900 shadow-[0_32px_64px_-12px_rgba(0,0,0,0.5)] animate-fade-in-up relative overflow-hidden">
              <div class="absolute top-0 left-0 right-0 h-3 bg-gradient-to-r from-purple-500 via-indigo-500 to-blue-500"></div>
              
              <button @click="modals.qr.show = false" class="absolute top-8 right-8 bg-slate-50 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-400 z-[60] border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a]">
                  <X class="w-5 h-5" />
              </button>

              <div class="p-10 text-center">
                  <div class="mb-6 mt-4">
                      <div class="w-20 h-20 bg-indigo-50 text-indigo-600 rounded-[2rem] flex items-center justify-center mx-auto mb-4 border-2 border-indigo-100 shadow-inner">
                          <QrCode class="w-10 h-10" />
                      </div>
                      <h3 class="text-xl font-black text-slate-800 uppercase tracking-tighter">QR CHẤM CÔNG</h3>
                      <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ qrData?.groupName || 'Đoàn khám' }}</p>
                  </div>

                  <div v-if="qrData" class="space-y-6">
                      <div class="bg-white p-6 rounded-[2.5rem] border-4 border-slate-900 shadow-[8px_8px_0px_#0f172a] inline-block mb-4">
                          <img :src="qrData.pngBase64" class="w-48 h-48 mx-auto rendering-pixelated" alt="Mã QR Chấm công" />
                      </div>

                      <div class="space-y-2">
                        <div class="flex items-center justify-center gap-2 px-4 py-2 bg-amber-50 text-amber-700 rounded-xl text-[10px] font-black uppercase border border-amber-200">
                            <Clock class="w-3 h-3" /> Hết hạn: {{ formatDateTime(qrData.expiresAt) }}
                        </div>
                        <p class="text-[10px] text-slate-400 font-medium px-6 leading-relaxed">Nhân sự dùng điện thoại quét mã này để tự động ghi nhận giờ vào/ra đoàn.</p>
                      </div>

                      <div class="pt-4 flex flex-col gap-3">
                          <button @click="copyQrUrl" class="w-full py-3 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-2xl font-black text-[10px] uppercase tracking-widest transition-all">Sao chép Link</button>
                          <button @click="modals.qr.show = false" class="w-full py-4 bg-slate-900 text-white rounded-2xl font-black text-xs uppercase tracking-[0.2em] shadow-[4px_4px_0px_#1e293b] active:scale-95 transition-all">XÁC NHẬN</button>
                      </div>
                  </div>
                  <div v-else class="py-20 flex flex-col items-center gap-4">
                      <RefreshCw class="w-8 h-8 animate-spin text-slate-200" />
                      <span class="text-[10px] font-black text-slate-400 uppercase">Đang tạo mã token...</span>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- Hidden Input for Import -->
    <input type="file" ref="importInput" class="hidden" @change="handleImportFile" />

    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />

    <!-- Auto Assignment Result Modal -->
    <Teleport to="body">
         <div v-if="showAutoAssignModal" class="fixed inset-0 z-[200] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4">
            <div class="bg-white w-full max-w-2xl rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up overflow-hidden relative">
                <div class="p-10">
                    <div class="flex items-center gap-4 mb-8">
                        <div class="w-16 h-16 bg-emerald-50 text-emerald-600 rounded-3xl flex items-center justify-center">
                            <ShieldCheck class="w-8 h-8" />
                        </div>
                        <div>
                            <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest">Kết quả Phân bổ</h3>
                            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Hệ thống đã tự động gán nhân sự cho đoàn</p>
                        </div>
                    </div>

                    <div v-if="autoAssignResult" class="space-y-6">
                        <div class="grid grid-cols-3 gap-4">
                            <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100 text-center">
                                <div class="text-[9px] font-black text-slate-400 uppercase mb-1">Yêu cầu</div>
                                <div class="text-xl font-black text-slate-800">{{ autoAssignResult.summary.requiredHeadcount }}</div>
                            </div>
                            <div class="bg-emerald-50 p-4 rounded-2xl border border-emerald-100 text-center">
                                <div class="text-[9px] font-black text-emerald-400 uppercase mb-1">Đã gán</div>
                                <div class="text-xl font-black text-emerald-600">{{ autoAssignResult.summary.assignedCount }}</div>
                            </div>
                            <div class="bg-rose-50 p-4 rounded-2xl border border-rose-100 text-center">
                                <div class="text-[9px] font-black text-rose-400 uppercase mb-1">Thiếu</div>
                                <div class="text-xl font-black text-rose-600">{{ autoAssignResult.summary.missingCount }}</div>
                            </div>
                        </div>

                        <div v-if="autoAssignResult.warnings.length > 0" class="p-4 bg-amber-50 rounded-2xl border border-amber-100 space-y-2">
                             <p class="text-[9px] font-black text-amber-600 uppercase tracking-widest">Cảnh báo từ hệ thống:</p>
                             <ul class="text-[10px] text-amber-500 font-medium list-disc list-inside">
                                <li v-for="(w, i) in autoAssignResult.warnings" :key="i">{{ w }}</li>
                             </ul>
                        </div>

                        <div class="max-h-60 overflow-y-auto border border-slate-100 rounded-2xl">
                             <table class="w-full text-left text-xs">
                                <thead class="bg-slate-50 text-[9px] font-black uppercase text-slate-400">
                                    <tr>
                                        <th class="p-3">Nhân sự</th>
                                        <th class="p-3 text-center">Vị trí</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-slate-50">
                                    <tr v-for="s in autoAssignResult.assignedStaff" :key="s.staffId">
                                        <td class="p-3 font-semibold">{{ s.staffName }}</td>
                                        <td class="p-3 text-center">
                                            <span class="px-2 py-0.5 bg-indigo-50 text-indigo-600 rounded-md font-black text-[9px] uppercase">{{ s.workPosition }}</span>
                                        </td>
                                    </tr>
                                </tbody>
                             </table>
                        </div>
                    </div>

                    <div class="flex gap-4 mt-8">
                        <button @click="showAutoAssignModal = false; showForm = false" class="flex-1 bg-slate-900 text-white py-4 rounded-2xl font-black text-xs uppercase tracking-widest border-2 border-slate-900 shadow-[4px_4px_0px_#1e293b]">ĐÓNG VÀ XEM ĐOÀN</button>
                    </div>
                </div>
            </div>
         </div>
    </Teleport>

    <!-- AI Suggestion Modal -->
    <Teleport to="body">
         <div v-if="showAiModal" class="fixed inset-0 z-[200] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4">
            <div class="bg-white w-full max-w-4xl rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up overflow-hidden relative">
                <!-- Header Accent Line -->
                <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-indigo-500 to-purple-600 z-0"></div>
                
                <button @click="showAiModal = false" class="absolute top-8 right-8 bg-white p-2 rounded-full hover:bg-slate-100 transition-all text-slate-400 z-[60] flex items-center justify-center border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a]">
                    <X class="w-5 h-5" />
                </button>

                <div class="relative z-10 p-10 pt-12">
                    <div class="flex items-center gap-4 mb-10">
                        <div class="w-16 h-16 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center shadow-inner border border-indigo-100">
                            <Brain class="w-8 h-8" />
                        </div>
                        <div>
                            <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest">AI Suggestion Result</h3>
                            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mt-1">Phân bổ nhân sự tối ưu bởi Gemini AI</p>
                        </div>
                    </div>

                    <div class="bg-slate-50 rounded-[2rem] border border-slate-100 p-8 mb-8">
                        <div class="overflow-x-auto">
                            <table class="w-full text-left">
                                <thead class="text-[10px] font-black uppercase tracking-widest text-slate-400 border-b border-slate-100">
                                    <tr>
                                        <th class="pb-4">Nhân sự</th>
                                        <th class="pb-4">Vị trí gợi ý</th>
                                        <th class="pb-4">Lý do điều động</th>
                                        <th class="pb-4 text-center">Ca làm</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-slate-100">
                                    <tr v-for="s in aiSuggestions" :key="s.staffId" class="text-xs">
                                        <td class="py-4 font-black text-slate-700">
                                            {{ staffList.find(st => st.staffId === s.staffId)?.fullName || 'Không xác định' }}
                                        </td>
                                        <td class="py-4">
                                            <span class="px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg font-black uppercase tracking-widest text-[9px]">
                                                {{ s.workPosition }}
                                            </span>
                                        </td>
                                        <td class="py-4 text-slate-500 italic">"{{ s.reason }}"</td>
                                        <td class="py-4 text-center font-black">{{ s.shiftType === 1 ? 'Cả ngày' : 'Nửa ngày' }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div class="flex gap-4">
                         <button @click="applyAiSuggestions" class="flex-1 bg-indigo-600 text-white py-4 rounded-2xl font-black hover:bg-indigo-700 transition-all border-2 border-slate-900 shadow-[4px_4px_0px_#0f172a] flex items-center justify-center gap-2">
                            <Sparkles class="w-5 h-5" />
                            ÁP DỤNG TOÀN BỘ GỢI Ý
                        </button>
                        <button @click="showAiModal = false" class="px-8 py-4 bg-white border-2 border-slate-100 rounded-2xl font-black text-slate-400 hover:bg-slate-50 transition-all">
                            HỦY BỎ
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import apiClient from '../services/apiClient'
import { 
    Stethoscope, Plus, Building2, Calendar, Users as UsersIcon, FileText, Trash2, Sparkles, Brain, 
    FileIcon, X, Download, Upload as UploadIcon, LogIn, LogOut, CheckCircle2, Clock, Zap, RefreshCw,
    ShieldCheck, Scale, ListTodo, QrCode
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { parseApiError } from '../services/errorHelper'
import { usePermission } from '@/composables/usePermission'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'


const authStore = useAuthStore()
const { can } = usePermission()
const toast = useToast()
const groups = ref([])
const contracts = ref([])
const staffList = ref([])
const staffDetails = ref({})
const showForm = ref(false)
const activeTab = ref('Open')
const importInput = ref(null)
const currentGroupId = ref(null)
const selectedContractForAuto = ref(null)
const createMode = ref('manual') // manual, smart, auto

const autoAssignData = ref({
    healthContractId: null,
    groupName: '',
    examDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    targetRatio: 20,
    minimumDoctors: 2,
    assignmentMode: 'Partial'
})
const isAutoAssignLoading = ref(false)
const showAutoAssignModal = ref(false)
const autoAssignResult = ref(null)

const onAutoContractSelect = () => {
    if (!autoAssignData.value.healthContractId) return
    const contract = contracts.value.find(c => c.healthContractId === autoAssignData.value.healthContractId)
    if (!contract) return
    
    const contractGroups = groups.value.filter(g => g.healthContractId === autoAssignData.value.healthContractId)
    const nextSequence = contractGroups.length + 1
    const displayName = contract.shortName || contract.companyName
    autoAssignData.value.groupName = `Đoàn ${String(nextSequence).padStart(2, '0')} - ${displayName} - TĐPB`
}

const handleAutoAssign = async () => {
    if (!autoAssignData.value.healthContractId || !autoAssignData.value.groupName) {
        toast.error("Vui lòng điền đầy đủ thông tin")
        return
    }
    
    // Tạo IdempotencyKey để chống user click đúp tạo 2 đoàn giống hệt nhau
    const payload = {
        ...autoAssignData.value,
        idempotencyKey: crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2)
    }

    try {
        isAutoAssignLoading.value = true
        const res = await apiClient.post('/api/MedicalGroups/auto-create-with-staff', payload)
        autoAssignResult.value = res.data
        showAutoAssignModal.value = true
        fetchData()
        toast.success("Hệ thống đã thực hiện phân bổ nhân sự!")
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        isAutoAssignLoading.value = false
    }
}

// Track the selected staff's type to enforce doctor restrictions
const selectedStaffType = computed(() => {
    const sid = modals.value.staff.data.staffId
    if (!sid) return null
    return staffList.value.find(s => s.staffId === sid)?.staffType || null
})

// Auto-adjust workPosition when staff changes: if doctor, default to 'Khám nội'
const onStaffSelect = () => {
    // Left empty since we enforce this directly in the UI options
}

const newGroup = ref({ healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0] })
const cardTab = ref({})
const groupPositions = ref({})

const modals = ref({
    staff: { show: false, groupId: null, data: { staffId: null, shiftType: 1.0, positionId: null, workPosition: '', workStatus: 'Đã tham gia' } },
    position: { show: false, groupId: null, data: { positionName: '', requiredCount: 1 } },
    qr: { show: false }
})
const qrData = ref(null)

const openQrModal = async (groupId) => {
    modals.value.qr.show = true
    qrData.value = null
    try {
        const res = await apiClient.get(`/api/Attendance/qr/${groupId}`)
        qrData.value = res.data
    } catch (e) {
        toast.error(parseApiError(e))
        modals.value.qr.show = false
    }
}

const copyQrUrl = () => {
    if (!qrData.value?.qrUrl) return
    navigator.clipboard.writeText(qrData.value.qrUrl)
    toast.success("Đã sao chép link chấm công vào clipboard!")
}

const formatDateTime = (d) => d ? new Date(d).toLocaleString('vi-VN') : ''
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const isAiLoading = ref(false)
const aiSuggestions = ref([])
const showAiModal = ref(false)
const currentAiGroupId = ref(null)

const handleAiSuggest = async (groupId) => {
    try {
        isAiLoading.value = true
        currentAiGroupId.value = groupId
        const res = await apiClient.post(`/api/MedicalGroups/${groupId}/ai-suggest-staff`)
        aiSuggestions.value = typeof res.data === 'string' ? JSON.parse(res.data) : res.data
        showAiModal.value = true
    } catch (e) {
        const errorMsg = e.response?.data?.message || e.response?.data || e.message;
        toast.error("Lỗi khi phân tích Dữ liệu AI: " + errorMsg)
        console.error("AI Parse Error details: ", e);
    } finally {
        isAiLoading.value = false
    }
}

const applyAiSuggestions = async () => {
    try {
        const groupId = currentAiGroupId.value
        for (const s of aiSuggestions.value) {
            await apiClient.post(`/api/MedicalGroups/${groupId}/staffs`, {
                staffId: s.staffId,
                workPosition: s.workPosition,
                shiftType: s.shiftType,
                workStatus: 'Đã tham gia'
            })
        }
        toast.success("Đã áp dụng toàn bộ gợi ý từ AI!")
        showAiModal.value = false
        fetchGroupStaff(groupId)
    } catch (e) {
        toast.error("Lỗi khi áp dụng gợi ý: " + e.message)
    }
}

const openGroups = computed(() => groups.value.filter(g => g.status === 'Open'))
const closedGroups = computed(() => groups.value.filter(g => g.status !== 'Open'))
const filteredGroups = computed(() => activeTab.value === 'Open' ? openGroups.value : closedGroups.value)

// Only show approved/active contracts for group creation
const approvedContracts = computed(() => contracts.value.filter(c => 
    c.status === 'Approved' || c.status === 'Active'
))

// Permission helpers: MedicalStaff can only check-in/out for themselves (backend validates by EmployeeCode)
const canCheckIn = (staffDetail, group) => {
    if (group.status !== 'Open' || staffDetail.checkInTime) return false
    if (can('ChamCong.CheckInOut')) return true
    // MedicalStaff can only check-in for themselves — backend validates identity
    if (staffDetail.employeeCode?.toLowerCase() === authStore.currentUser?.toLowerCase()) return true
    return false
}

const canCheckOut = (staffDetail, group) => {
    if (group.status !== 'Open' || !staffDetail.checkInTime || staffDetail.checkOutTime) return false
    if (can('ChamCong.CheckInOut')) return true
    // MedicalStaff can only check-out for themselves — backend validates identity
    if (staffDetail.employeeCode?.toLowerCase() === authStore.currentUser?.toLowerCase()) return true
    return false
}

const getStatusClass = (status) => {
    switch(status) {
        case 'Open': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'Finished': return 'bg-slate-50 text-slate-500 border-slate-200'
        case 'Locked': return 'bg-rose-50 text-rose-600 border-rose-100'
        default: return 'bg-slate-100 text-slate-400 border-slate-200'
    }
}

const getStatusLabel = (status) => {
    switch(status) {
        case 'Open': return 'Đang thực hiện'
        case 'Finished': return 'Đã hoàn tất'
        case 'Locked': return 'Đã khóa'
        default: return status
    }
}

const fetchData = async () => {
    try {
        if (authStore.role === 'MedicalStaff') {
            const res = await apiClient.get('/api/MedicalGroups/my-schedule')
            // Map my-schedule data to fit group list structure
            groups.value = res.data.map(item => ({
                groupId: item.groupId,
                groupName: item.groupName,
                examDate: item.examDate,
                companyName: item.companyName,
                status: item.groupStatus,
                // Additional info for staff detail inside this group
                myAssignmentId: item.id,
                myWorkPosition: item.workPosition,
                myWorkStatus: item.workStatus,
                myCheckIn: item.checkInTime,
                myCheckOut: item.checkOutTime
            }))
        } else {
            const res = await apiClient.get('/api/MedicalGroups')
            groups.value = res.data
        }
        
        const cRes = await apiClient.get('/api/HealthContracts')
        contracts.value = cRes.data

        if (['Admin', 'MedicalGroupManager', 'MedicalStaff', 'PersonnelManager'].includes(authStore.role)) {
            try {
                const sRes = await apiClient.get('/api/Staffs')
                staffList.value = sRes.data
            } catch (err) {
                console.warn("Không thể tải toàn bộ nhân sự:", err)
                staffList.value = []
            }
        } else {
            staffList.value = []
        }

        groups.value.forEach(g => {
            fetchGroupStaff(g.groupId)
            fetchGroupPositions(g.groupId)
        })
    } catch (e) { toast.error("Lỗi khi tải dữ liệu") }
}

const fetchGroupStaff = async (id) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/staffs`)
        staffDetails.value[id] = res.data
    } catch (e) { console.error(e) }
}

const addGroup = async () => {
    try {
        await apiClient.post('/api/MedicalGroups', newGroup.value)
        toast.success("Khởi tạo đoàn thành công!")
        showForm.value = false
        // Reset form
        newGroup.value = { healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0] }
        fetchData()
    } catch (e) { toast.error("Lỗi khi tạo đoàn khám") }
}

const autoCreateGroup = async () => {
    if (!selectedContractForAuto.value) return
    try {
        await apiClient.post(`/api/MedicalGroups/auto-create/${selectedContractForAuto.value}`)
        toast.success("Đã tạo đoàn khám tự động từ hợp đồng!")
        showForm.value = false
        fetchData()
    } catch (e) { toast.error(e.response?.data || "Lỗi tạo tự động") }
}

// --- Smart Create Logic ---
const smartPreview = ref(null)
const lastGroupOfContract = ref(null)
const doCloneStaff = ref(false)
const conflictWarnings = ref([])

const onSmartContractSelect = async () => {
    if (!selectedContractForAuto.value) { smartPreview.value = null; return }
    const contract = contracts.value.find(c => c.healthContractId === selectedContractForAuto.value)
    if (!contract) return

    // Find groups for this contract to determine sequence number
    const contractGroups = groups.value.filter(g => g.healthContractId === selectedContractForAuto.value)
    const nextSequence = contractGroups.length + 1
    const sequenceStr = String(nextSequence).padStart(2, '0')

    // Auto-fill group name and date
    const now = new Date()
    const month = String(now.getMonth() + 1).padStart(2, '0')
    const year = now.getFullYear()
    
    const displayName = contract.shortName || contract.companyName
    smartPreview.value = {
        groupName: `Đoàn ${sequenceStr} - ${displayName} - ${month}/${year}`,
        examDate: contract.examDate ? contract.examDate.split('T')[0] : 
                  new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    }

    // Find last group for this contract to offer clone
    lastGroupOfContract.value = contractGroups.length > 0 ? contractGroups[contractGroups.length - 1] : null
    doCloneStaff.value = false
    conflictWarnings.value = []
}

const confirmSmartCreate = async () => {
    if (!smartPreview.value || !selectedContractForAuto.value) return
    try {
        // Create the group
        const res = await apiClient.post('/api/MedicalGroups', {
            healthContractId: selectedContractForAuto.value,
            groupName: smartPreview.value.groupName,
            examDate: smartPreview.value.examDate
        })
        const newGroupId = res.data.groupId

        // Clone staff if option is selected
        if (doCloneStaff.value && lastGroupOfContract.value) {
            const oldStaff = staffDetails.value[lastGroupOfContract.value.groupId] || []
            conflictWarnings.value = []
            for (const s of oldStaff) {
                // Check conflict: is staff already in another group on same date?
                const conflict = groups.value.some(g => 
                    g.groupId !== newGroupId &&
                    g.examDate?.split('T')[0] === smartPreview.value.examDate &&
                    g.status === 'Open' &&
                    (staffDetails.value[g.groupId] || []).some(ms => ms.staffId === s.staffId)
                )
                if (conflict) {
                    conflictWarnings.value.push({ staffId: s.staffId, fullName: s.fullName })
                } else {
                    try {
                        await apiClient.post(`/api/MedicalGroups/${newGroupId}/staffs`, {
                            staffId: s.staffId,
                            shiftType: s.shiftType,
                            workPosition: s.workPosition,
                            workStatus: 'Đang chờ'
                        })
                    } catch (_) {}
                }
            }
        }

        if (conflictWarnings.value.length > 0) {
            toast.success(`Đã tạo đoàn! (${conflictWarnings.value.length} nhân sự bị trùng lịch, bỏ qua)`)
        } else {
            toast.success('Đã tạo nhanh đoàn khám thành công!')
        }
        showForm.value = false
        smartPreview.value = null
        selectedContractForAuto.value = null // Reset selection
        fetchData()
    } catch (e) { toast.error(parseApiError(e)) }
}

const updateStatus = async (id, status) => {
    if (status === 'Finished') {
        confirmData.value = {
            show: true,
            title: 'Hoàn tất đoàn khám',
            message: 'Bạn có chắc chắn muốn kết thúc đoàn khám này? Sau khi hoàn tất, đoàn sẽ chuyển sang trạng thái lưu trữ.',
            variant: 'warning',
            onConfirm: async () => {
                try {
                    await apiClient.put(`/api/MedicalGroups/${id}/status`, { status: status })
                    toast.success(`Đã hoàn tất đoàn khám!`)
                    fetchData()
                } catch (e) { 
                    toast.error(parseApiError(e)) 
                }
            }
        }
        return
    }

    try {
        await apiClient.put(`/api/MedicalGroups/${id}/status`, { status: status })
        toast.success(`Đã cập nhật trạng thái: ${getStatusLabel(status)}`)
        fetchData()
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const isDoctorPosition = (name) => {
    if (!name) return false;
    const doctorWords = ['Khám', 'Siêu âm', 'Bác sĩ', 'Sản khoa', 'Nội', 'Ngoại'];
    return doctorWords.some(w => name.toLowerCase().includes(w.toLowerCase()));
}

const openStaffModal = (gid) => {
    modals.value.staff.groupId = gid
    modals.value.staff.show = true
    modals.value.staff.data = { staffId: null, shiftType: 1.0, positionId: null, workPosition: '', workStatus: 'Đã tham gia' }
}

const addStaff = async () => {
    try {
        const gid = modals.value.staff.groupId
        const payload = { ...modals.value.staff.data }
        // Map positionId to workPosition name
        const groupPositionList = groupPositions.value[gid] || []
        const pos = groupPositionList.find(p => p.positionId === payload.positionId)
        if (pos) {
            payload.workPosition = pos.positionName
        }

        await apiClient.post(`/api/MedicalGroups/${gid}/staffs`, payload)
        toast.success("Đã phân công nhân sự!")
        modals.value.staff.show = false
        fetchGroupStaff(gid)
        fetchGroupPositions(gid) // update count
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const openPositionModal = (groupId) => {
    modals.value.position.groupId = groupId
    modals.value.position.data = { positionName: '', requiredCount: 1 }
    modals.value.position.show = true
}

const fetchGroupPositions = async (id) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/positions`)
        groupPositions.value[id] = res.data
    } catch (e) { console.error(e) }
}

const addPosition = async () => {
    try {
        await apiClient.post(`/api/MedicalGroups/${modals.value.position.groupId}/positions`, modals.value.position.data)
        toast.success("Thêm vị trí thành công!")
        modals.value.position.show = false
        fetchGroupPositions(modals.value.position.groupId)
    } catch (e) { toast.error(parseApiError(e)) }
}

const removePosition = async (positionId, groupId) => {
    confirmData.value = {
        show: true,
        title: 'Xóa vị trí',
        message: 'Bạn có chắc chắn muốn xóa vị trí này không? Không thể xóa vị trí đã có nhân tài.',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await apiClient.delete(`/api/MedicalGroups/positions/${positionId}`)
                toast.success("Đã xóa vị trí!")
                fetchGroupPositions(groupId)
            } catch (e) {
                toast.error(e.response?.data || "Không thể xóa vị trí")
            } finally {
                confirmData.value.show = false
            }
        }
    }
}

const exportGroups = async () => {
    try {
        const res = await apiClient.get('/api/MedicalGroups/export', { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'DanhSachDoanKham.xlsx')
        document.body.appendChild(link)
        link.click()
        toast.success("Đã xuất danh sách đoàn khám!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const exportGroupStaff = async (id, name) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${id}/export-staff`, { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `NhanSu_${name}.xlsx`)
        document.body.appendChild(link)
        link.click()
        toast.success(`Đã xuất danh sách nhân sự cho đoàn ${name}!`)
    } catch (e) { toast.error("Lỗi xuất file") }
}

const removeStaff = async (detailId, gid) => {
    try {
        await apiClient.delete(`/api/MedicalGroups/staffs/${detailId}`)
        toast.success("Đã gỡ nhân sự")
        fetchGroupStaff(gid)
    } catch (e) { toast.error("Lỗi khi gỡ nhân sự") }
}

const triggerImport = (id) => {
    currentGroupId.value = id
    importInput.value.click()
}

const checkIn = async (detailId, gid) => {
    try {
        await apiClient.post(`/api/MedicalGroups/staffs/${detailId}/checkin`)
        toast.success("Đã ghi nhận giờ vào đoàn!")
        fetchGroupStaff(gid)
    } catch (e) { toast.error(e.response?.data || "Lỗi Check-in") }
}

const checkOut = async (detailId, gid) => {
    try {
        const res = await apiClient.post(`/api/MedicalGroups/staffs/${detailId}/checkout`)
        toast.success(`Check-out thành công! Tổng giờ: ${res.data.totalHours}h`)
        fetchGroupStaff(gid)
    } catch (e) { toast.error(e.response?.data || "Lỗi Check-out") }
}

const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        const res = await apiClient.post(`/api/MedicalGroups/upload-data`, formData)
        await apiClient.put(`/api/MedicalGroups/${currentGroupId.value}`, { importFilePath: res.data.path })
        toast.success("Đã Import dữ liệu đoàn khám!")
        fetchData()
    } catch (e) { toast.error("Lỗi Import file") }
}

const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN').format(p) + ' đ'

onMounted(fetchData)
</script>
