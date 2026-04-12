<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 pr-4 scrollbar-premium overflow-y-auto font-sans p-6">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg shadow-primary/20">
            <Stethoscope class="w-6 h-6" />
          </div>
          {{ i18n.t('groups.title') }}
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[9px] mt-2">{{ i18n.t('groups.subtitle') }}</p>
      </div>

      <div class="flex items-center gap-4">
        <button v-if="can('DoanKham.Create')" 
                @click="exportGroups"                 class="btn-premium secondary">
            <Download class="w-4 h-4 mr-2" />
            <span class="text-[10px] uppercase">Xuất DS Đoàn</span>
        </button>
        <button v-if="can('DoanKham.Create')" 
                @click="showForm = !showForm"                 class="btn-premium primary">
            <Plus class="w-5 h-5 mr-2" />
            <span class="text-[10px] uppercase">{{ i18n.t('groups.addBtn') }}</span>
        </button>
      </div>
    </div>



    <!-- Smart Create Form -->
    <div v-if="showForm" class="premium-card p-4 mb-6 overflow-hidden border-indigo-100 bg-indigo-50/10 shadow-sm transition-all duration-300">
        <div class="flex items-center justify-between mb-4">
            <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center shadow-md shadow-indigo-100 shrink-0">
                    <Zap class="w-4 h-4 text-white" />
                </div>
                <h3 class="text-sm font-black text-slate-800 tracking-tight uppercase">Tạo đoàn tự động</h3>
            </div>
            <div class="flex items-center gap-3">
                <div class="flex p-1 bg-white rounded-xl border border-slate-100 shadow-sm">
                    <button @click="createMode = 'manual'" :class="['px-4 py-1.5 rounded-lg font-black text-[9px] uppercase tracking-tighter transition-all', createMode === 'manual' ? 'bg-slate-100 text-slate-900 shadow-inner' : 'text-slate-400']">Thủ công</button>
                    <button @click="createMode = 'auto'" :class="['px-4 py-1.5 rounded-lg font-black text-[9px] uppercase tracking-tighter transition-all', createMode === 'auto' ? 'bg-indigo-600 text-white shadow-lg' : 'text-slate-400']">Tự động</button>
                </div>
                <button @click="showForm = false" class="p-1.5 hover:bg-slate-200/50 rounded-lg transition-all text-slate-400">
                    <X class="w-4 h-4" />
                </button>
            </div>
        </div>

        <!-- Mode: Manual -->
        <form v-if="createMode === 'manual'" @submit.prevent="addGroup" class="flex items-end gap-4 min-w-full">
            <div class="flex-[3] min-w-0 flex flex-col gap-1.5 ">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Hợp đồng mục tiêu</label>
                <select v-model="newGroup.healthContractId" required class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] uppercase px-4 py-0 transition-all focus:ring-2 focus:ring-indigo-500/10">
                    <option :value="null" disabled>-- Chọn hợp đồng --</option>
                    <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                        [{{ c.contractCode || '---' }}] {{ c.contractName || c.companyName }}
                    </option>
                </select>
            </div>
            <div class="flex-[3] min-w-0 flex flex-col gap-1.5 ">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên đoàn khám</label>
                <input v-model="newGroup.groupName" required class="input-premium bg-white border-slate-200 w-full h-11 text-[11px] font-black px-4" placeholder="" />
            </div>
            <div class="flex-[2] min-w-0 flex flex-col gap-1.5 ">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày triển khai</label>
                <input v-model="newGroup.examDate" type="date" required class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] px-4" />
            </div>
            <button type="submit" class="h-11 px-8 rounded-xl bg-slate-800 text-white font-black text-[10px] uppercase tracking-widest hover:bg-slate-900 transition-all shadow-lg active:scale-95 shrink-0">KÍCH HOẠT ĐOÀN MỚI</button>
        </form>

        <!-- Mode: Auto-Create (Simplified) -->
        <div v-if="createMode === 'auto'" class="flex items-end gap-4 min-w-full animate-fade-in">
            <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Chọn hợp đồng nguồn</label>
                <select v-model="autoAssignData.healthContractId" @change="onAutoContractSelect" 
                        class="input-premium bg-white border-slate-200 w-full cursor-pointer font-black text-[11px] uppercase px-4 h-11 py-0 transition-all focus:ring-2 focus:ring-indigo-500/10">
                    <option :value="null">-- Chọn hợp đồng --</option>
                    <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                        [{{ c.contractCode || '---' }}] {{ c.contractName || c.companyName }}
                    </option>
                </select>
            </div>
            <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên đoàn (Tự động)</label>
                <input v-model="autoAssignData.groupName"  placeholder=""
                       class="input-premium bg-white border-slate-200 w-full h-11 text-[11px] font-black px-4" />
            </div>
            <div class="flex-[2] min-w-0 flex flex-col gap-1.5">
                <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày triển khai</label>
                <input type="date" v-model="autoAssignData.examDate"
                       class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] px-4 transition-all focus:ring-2 focus:ring-indigo-500/20" />
            </div>

            <button @click="handleAutoCreateOnly" :disabled="isAutoAssignLoading || !autoAssignData.healthContractId" 
                    class="h-11 px-8 rounded-xl font-black transition-all flex items-center justify-center gap-2 text-[10px] uppercase tracking-widest shrink-0"
                    :class="[!autoAssignData.healthContractId ? 'bg-slate-100 text-slate-400 border border-slate-200' : 'bg-blue-600 text-white hover:bg-blue-700 shadow-lg shadow-blue-500/20 active:scale-95 animate-pulse-subtle']">
                <Loader2 v-if="isAutoAssignLoading" class="w-4 h-4 animate-spin" />
                <Zap v-else class="w-4 h-4" />
                <span>Xác nhận tạo đoàn tự động</span>
            </button>
        </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center shadow-inner">
          <Stethoscope class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng đoàn khám</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ allGroups.length }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-blue-100 text-blue-600 rounded-2xl flex items-center justify-center shadow-inner">
          <Zap class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Đang thực hiện</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ openGroups.length }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-emerald-100 text-emerald-600 rounded-2xl flex items-center justify-center shadow-inner">
          <CheckCircle2 class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hoàn tất</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ closedGroups.length }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-purple-100 text-purple-600 rounded-2xl flex items-center justify-center shadow-inner">
          <Lock class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Khóa sổ</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ allGroups.filter(g => g.status === 'Locked').length }}</p>
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
        <div v-if="filteredGroups.length === 0" class="premium-card p-16 flex flex-col items-center justify-center">
            <div class="w-24 h-24 bg-slate-50 text-slate-300 rounded-[2rem] flex items-center justify-center mb-6 border border-slate-50 shadow-inner">
                <Stethoscope class="w-12 h-12" />
            </div>
            <h4 class="text-xl font-black text-slate-800 uppercase tracking-widest">{{ i18n.t('common.noData') }}</h4>
            <p class="text-[10px] font-black text-slate-400 mt-2 uppercase tracking-widest">Không có dữ liệu đoàn khám nào trong mục này</p>
        </div>

        <div v-for="group in filteredGroups" :key="group.groupId" 
             class="premium-card bg-white rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden group/card">
            
            <div class="p-5 bg-primary text-white flex justify-between items-center transition-all">
                <div class="flex items-center gap-5">
                    <div class="w-10 h-10 bg-white/20 backdrop-blur-md rounded-xl flex items-center justify-center text-white shadow-sm border border-white/10 shrink-0">
                        <Stethoscope class="w-5 h-5" />
                    </div>
                    <div class="min-w-0 pr-4">
                        <h4 class="text-base font-black truncate">{{ group.groupName }}</h4>
                        <p class="text-[9px] font-black text-white/70 uppercase tracking-widest mt-0.5 truncate italic">
                            [HĐ-{{ group.healthContractId }}] • {{ group.shortName || group.companyName }} • {{ formatDate(group.examDate) }}
                        </p>
                    </div>
                </div>
                <div class="flex items-center gap-3">
                    <span :class="['px-3 py-1 rounded-lg text-[8px] font-black uppercase tracking-tighter border border-white/20 shadow-sm leading-none flex items-center h-6', getStatusClass(group.status)]">
                        {{ getStatusLabel(group.status) }}
                    </span>
                   
                   <!-- Action Buttons for Status -->
                   <button v-if="group.status === 'Open' && can('DoanKham.Edit')" @click="updateStatus(group.groupId, 'Finished')" class="text-[9px] font-black bg-white text-primary hover:bg-slate-50 px-4 py-2 rounded-xl transition-all shadow-sm">{{ i18n.t('groups.btnFinish') }}</button>
                   
                   <button v-if="group.status === 'Finished' && can('DoanKham.Lock') && lockStatuses[group.groupId]?.isReadyToLock" 
                           @click="handleLockGroup(group.groupId)" 
                           class="text-[9px] font-black bg-emerald-500 text-white hover:bg-emerald-600 px-4 py-2 rounded-xl transition-all shadow-sm flex items-center gap-2">
                       <Lock class="w-3 h-3" /> KHÓA SỔ TÀI CHÍNH
                   </button>

                   <div v-if="group.status === 'Locked'" class="flex items-center gap-2 px-3 py-1.5 bg-white/20 rounded-xl border border-white/10 shadow-inner">
                       <CheckCircle2 class="w-3 h-3 text-white" />
                       <span class="text-[9px] font-black uppercase tracking-widest text-white">Dữ liệu đã chốt sổ</span>
                   </div>
                </div>
            </div>

            <div class="p-8">
                <!-- Tabs for Card -->
                <div class="flex gap-4 mb-6 border-b border-slate-100 pb-2 mt-2">
                    <button @click="cardTab[group.groupId] = 'staffs'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', (cardTab[group.groupId] || 'staffs') === 'staffs' ? 'bg-primary text-white shadow-md shadow-primary/20' : 'bg-white text-slate-400 hover:bg-slate-100']">
                        <Users class="w-3 h-3 inline mr-1" /> {{ i18n.t('groups.tabStaff') }}
                    </button>
                    <button @click="cardTab[group.groupId] = 'positions'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', cardTab[group.groupId] === 'positions' ? 'bg-primary text-white shadow-md shadow-primary/20' : 'bg-white text-slate-400 hover:bg-slate-100']">
                        <ShieldCheck class="w-3 h-3 inline mr-1" /> {{ i18n.t('groups.tabPositions') }}
                    </button>
                    <button @click="openSuppliesTab(group.groupId)" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all flex items-center gap-2', cardTab[group.groupId] === 'supplies' ? 'bg-emerald-600 text-white shadow-md shadow-emerald-100' : 'bg-white text-slate-400 hover:bg-slate-100']">
                        <Package class="w-3 h-3" /> Vật tư & Hao phí
                    </button>
                    <div class="flex-grow"></div>
                    <button @click="$router.push('/reception')" class="px-4 py-2 text-[9px] font-black uppercase tracking-widest text-indigo-500 hover:bg-indigo-50 rounded-xl transition-all flex items-center gap-2">
                        <ScanLine class="w-3 h-3" /> Cổng Báo Danh
                    </button>
                </div>

                <!-- Nhân sự List Table -->
                <div v-show="(cardTab[group.groupId] || 'staffs') === 'staffs'" class="space-y-6 animate-fade-in">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400 ">
                            <UsersIcon class="w-4 h-4" /> Đội ngũ đi khám
                        </h5>
                        <div class="flex items-center gap-2 pr-2">
                            <button v-if="group.status === 'Open' && can('DoanKham.StaffAssign')" @click="openStaffModal(group.groupId)" class="text-[8px] font-black text-primary uppercase tracking-tighter hover:bg-blue-50 px-2 py-1 rounded transition-all">Gán nhân sự</button>
                            <div v-if="can('DoanKham.StaffAssign')" class="w-px h-3 bg-slate-100"></div>
                            <button v-if="group.status === 'Open' && can('DoanKham.StaffAssign')" @click="handleAiSuggest(group.groupId)" :disabled="isAiLoading" class="text-[8px] font-black text-primary uppercase tracking-tighter hover:bg-blue-50 px-2 py-1 rounded transition-all flex items-center gap-1">
                                <Sparkles class="w-2.5 h-2.5" v-if="!isAiLoading" />
                                <RefreshCw class="w-2.5 h-2.5 animate-spin" v-else />
                                {{ isAiLoading ? '...' : 'AI Copilot' }}
                            </button>
                            <div class="w-px h-3 bg-slate-100"></div>
                            <button @click="exportGroupStaff(group.groupId, group.groupName)" class="text-[8px] font-black text-emerald-600 uppercase tracking-tighter hover:bg-emerald-50 px-2 py-1 rounded transition-all">Excel</button>
                            <div class="w-px h-3 bg-slate-100"></div>
                            <button v-if="group.status === 'Open' && can('ChamCong.QR')" @click="openQrModal(group.groupId)" class="text-[8px] font-black text-purple-600 uppercase tracking-tighter hover:bg-purple-50 px-2 py-1 rounded transition-all flex items-center gap-1">
                                <QrCode class="w-2.5 h-2.5" />
                                QR
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

                <!-- Supplies / Vật tư List -->
                <div v-show="cardTab[group.groupId] === 'supplies'" class="space-y-6 animate-fade-in">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400 ">
                            <Package class="w-4 h-4" /> THỰC TẾ TIÊU HAO SO VỚI QUY MÔ ĐOÀN
                        </h5>
                        <div class="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest bg-slate-50 text-slate-500 px-3 py-1.5 rounded-lg border border-slate-100">
                            Quy mô: <span class="bg-primary text-white px-2 py-0.5 rounded-md">{{ suppliesData[group.groupId]?.totalPatients || 0 }}</span> bệnh nhân
                        </div>
                    </div>

                    <div v-if="loadingSupplies[group.groupId]" class="flex justify-center p-8">
                        <Loader2 class="w-8 h-8 animate-spin text-primary opacity-50" />
                    </div>
                    
                    <div v-else-if="suppliesData[group.groupId]?.supplies?.length" class="grid gap-4">
                        <div v-for="item in suppliesData[group.groupId].supplies" :key="item.itemName" class="p-5 rounded-2xl border flex flex-col md:flex-row md:items-center gap-6 transition-all"
                            :class="{ 
                                'bg-rose-50 border-rose-100': item.status === 'CRITICAL',
                                'bg-amber-50 border-amber-100': item.status === 'WARNING',
                                'bg-emerald-50 border-emerald-100': item.status === 'NORMAL'
                            }">
                            
                            <div class="w-full md:w-1/3">
                                <div class="font-black text-slate-700 uppercase tracking-widest mb-1">{{ item.itemName }}</div>
                                <div class="text-[10px] font-black uppercase tracking-widest"
                                    :class="{
                                        'text-rose-500': item.status === 'CRITICAL',
                                        'text-amber-500': item.status === 'WARNING',
                                        'text-emerald-500': item.status === 'NORMAL'
                                    }">
                                    <template v-if="item.status === 'CRITICAL'"><AlertOctagon class="w-3 h-3 inline mr-1 -mt-0.5"/> THẤT THOÁT NGHIÊM TRỌNG</template>
                                    <template v-else-if="item.status === 'WARNING'">CẦN CHÚ Ý</template>
                                    <template v-else>TRONG ĐỊNH MỨC AN TOÀN</template>
                                </div>
                            </div>

                            <div class="w-full md:w-2/3 space-y-2">
                                <div class="flex justify-between text-[10px] font-black uppercase tracking-widest text-slate-500">
                                    <span>Đã dùng: <strong class="text-slate-800">{{ item.totalUsed }}</strong> {{ item.unit }}</span>
                                    <span>Định mức: <strong>{{ item.expectedUsage }}</strong> {{ item.unit }}</span>
                                </div>
                                <div class="h-2 w-full bg-slate-100 rounded-full flex overflow-hidden relative">
                                    <div class="h-full transition-all duration-1000"
                                        :class="{
                                            'bg-rose-500': item.status === 'CRITICAL',
                                            'bg-amber-500': item.status === 'WARNING',
                                            'bg-emerald-500': item.status === 'NORMAL'
                                        }"
                                        :style="`width: ${Math.min(item.usagePercentage, 100)}%`">
                                    </div>
                                    <div v-if="item.usagePercentage > 100" class="h-full bg-rose-600 opacity-50 absolute left-0 top-0"
                                        :style="`width: ${Math.min(item.usagePercentage, 100)}%`">
                                    </div>
                                </div>
                                <div class="text-right text-[9px] font-black uppercase tracking-widest"
                                     :class="item.usagePercentage > 100 ? 'text-rose-500' : 'text-slate-400'">
                                    Tỉ lệ hao phí: {{ item.usagePercentage }}%
                                </div>
                            </div>
                        </div>
                    </div>
                    <div v-else class="p-10 text-center bg-slate-50/50 rounded-2xl border border-slate-100">
                        <PackageSearch class="w-8 h-8 mx-auto text-slate-300 mb-3" />
                        <div class="text-[10px] font-black uppercase tracking-widest text-slate-400">Đoàn khám này chưa có phát sinh xuất kho vật tư</div>
                    </div>
                </div>

                <!-- Financial Lock Summary (Visible when Locked) -->
                <div v-if="group.status === 'Locked'" class="mt-8 p-6 bg-slate-900 rounded-3xl border border-white/5 relative overflow-hidden group/finance">
                    <div class="absolute -right-4 -bottom-4 opacity-10 transform group-hover/finance:scale-110 transition-transform">
                        <Scale class="w-32 h-32 text-white" />
                    </div>
                    <div class="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
                        <div class="space-y-1">
                            <h5 class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Giá trị quyết toán nhân sự</h5>
                            <div class="text-2xl font-black text-white italic">
                                {{ formatPrice(group.totalLaborCost || 0) }}
                            </div>
                        </div>
                        <div class="flex items-center gap-6">
                            <div class="text-right">
                                <p class="text-[9px] font-black text-slate-500 uppercase">Thực khám</p>
                                <p class="text-lg font-black text-white italic">{{ lockStatuses[group.groupId]?.completedPatients || 0 }} ca</p>
                            </div>
                            <div class="w-px h-10 bg-white/10"></div>
                            <div class="text-right">
                                <p class="text-[9px] font-black text-slate-500 uppercase">Nhân sự đi đoàn</p>
                                <p class="text-lg font-black text-white italic">{{ staffDetails[group.groupId]?.length || 0 }} người</p>
                            </div>
                        </div>
                        <div class="bg-emerald-500/10 border border-emerald-500/20 p-4 rounded-2xl">
                            <div class="flex items-center gap-3 text-emerald-400">
                                <CheckCircle2 class="w-5 h-5" />
                                <div>
                                    <p class="text-[9px] font-black uppercase tracking-widest">Trạng thái khóa</p>
                                    <p class="text-[11px] font-black text-white">ĐÃ XÁC THỰC LƯƠNG</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Ready to Lock Warning (Visible when Finished but not Locked) -->
                <div v-if="group.status === 'Finished' && lockStatuses[group.groupId]" class="mt-8 p-6 bg-slate-50 rounded-[2.5rem] border-2 border-slate-200/50 flex flex-col md:flex-row justify-between items-center gap-8 relative overflow-hidden group/lockbox">
                    <div class="absolute inset-0 bg-gradient-to-br from-indigo-50/50 to-emerald-50/50 opacity-0 group-hover/lockbox:opacity-100 transition-opacity"></div>
                    
                    <div class="flex items-center gap-6 relative z-10">
                        <div class="w-16 h-16 bg-white rounded-2xl flex items-center justify-center shadow-xl border border-slate-100">
                             <div class="relative">
                                <Stethoscope class="w-8 h-8 text-primary" />
                                <Lock v-if="!lockStatuses[group.groupId].isReadyToLock" class="absolute -right-2 -bottom-2 w-5 h-5 text-amber-500 fill-amber-500" />
                                <CheckCircle2 v-else class="absolute -right-2 -bottom-2 w-5 h-5 text-emerald-500 fill-emerald-500" />
                             </div>
                        </div>
                        <div>
                            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Kiểm tra điều kiện chốt sổ</p>
                            <h4 class="text-xl font-black text-slate-800 italic leading-none mb-1">
                                {{ lockStatuses[group.groupId].completedPatients }} / {{ lockStatuses[group.groupId].totalPatients }} Ca khám hoàn tất
                            </h4>
                            <p class="text-[10px] font-black tracking-widest" :class="lockStatuses[group.groupId].isReadyToLock ? 'text-emerald-500' : 'text-amber-500'">
                                {{ lockStatuses[group.groupId].isReadyToLock ? '✓ Đủ điều kiện khóa sổ tài chính' : '⚠ ' + lockStatuses[group.groupId].message }}
                            </p>
                        </div>
                    </div>

                    <div class="flex flex-col items-center md:items-end gap-3 relative z-10">
                        <button v-if="lockStatuses[group.groupId].isReadyToLock" 
                                @click="handleLockGroup(group.groupId)" 
                                class="btn-premium primary !bg-slate-900 !shadow-slate-200 !px-10 !py-4 !text-xs">
                            CHỐT KHÓA SỔ & TÍNH LƯƠNG
                        </button>
                        <router-link v-else :to="`/medical-records?groupId=${group.groupId}`" class="px-8 py-3 bg-white border-2 border-slate-200 text-slate-400 font-black text-[10px] uppercase tracking-widest rounded-2xl hover:border-primary hover:text-primary transition-all">
                            Xử lý ca tồn đọng
                        </router-link>
                        <p class="text-[9px] font-bold text-slate-400 italic">Dữ liệu sẽ không thể chỉnh sửa sau khi khóa.</p>
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
                                  <Stethoscope class="w-3 h-3 text-indigo-400" /> Vị trí làm việc
                              </label>
                              <select v-model="modals.staff.data.positionId" required @change="onPositionChange" class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                                  <option :value="null" disabled>-- Chọn vị trí trống --</option>
                                  <option v-for="p in groupPositions[modals.staff.groupId]" :key="p.positionId" :value="p.positionId">
                                      {{ p.positionName }} (Đã xếp: {{ p.assignedCount }}/{{ p.requiredCount }})
                                  </option>
                              </select>
                          </div>

                          <div class="space-y-2" v-if="modals.staff.data.positionId">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                                  <CheckCircle2 class="w-3 h-3 text-indigo-400" /> Chọn nhân viên (Đã lọc theo vị trí)
                              </label>
                              
                              <!-- Role Mismatch Warning -->
                              <div v-if="isRoleMismatch" class="flex items-center gap-2 px-3 py-2 bg-rose-50 rounded-xl border border-rose-100 mb-2 animate-pulse">
                                  <AlertCircle class="w-3 h-3 text-rose-600" />
                                  <span class="text-[9px] font-black text-rose-600 uppercase tracking-widest leading-tight">
                                      Cảnh báo: Nhân sự này có chuyên môn không khớp với vị trí {{ currentPositionName }}
                                  </span>
                              </div>

                              <select v-model="modals.staff.data.staffId" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all appearance-none">
                                  <option :value="null" disabled>-- Chọn nhân sự phù hợp --</option>
                                  
                                  <optgroup label="Gợi ý phù hợp nhất">
                                      <option v-for="s in recommendedStaff" :key="s.staffId" :value="s.staffId">
                                          🌟 {{ s.fullName }} ({{ s.staffType || 'Chưa phân loại' }})
                                      </option>
                                  </optgroup>

                                  <optgroup label="Tất cả nhân sự khác">
                                      <option v-for="s in otherStaff" :key="s.staffId" :value="s.staffId">
                                          {{ s.fullName }} ({{ s.staffType || '---' }})
                                      </option>
                                  </optgroup>
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
    FileIcon, X, Download, Upload as UploadIcon, LogIn, LogOut, Search, Edit3, 
    MapPin, Clock, Filter, ChevronRight, Activity, 
    Settings, CheckCircle, Package, History, 
    ChevronDown, ChevronUp, MoreHorizontal,
    Loader2, AlertCircle, Send
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { parseApiError } from '../services/errorHelper'
import { usePermission } from '@/composables/usePermission'
import { useToast } from '../composables/useToast'
import { useI18nStore } from '../stores/i18n'
import ConfirmDialog from '../components/ConfirmDialog.vue'


const authStore = useAuthStore()
const i18n = useI18nStore()
const { can } = usePermission()
const toast = useToast()
const groups = ref([])
const allGroups = computed(() => groups.value)
const loading = ref(false)
const contracts = ref([])
const staffList = ref([])
const staffDetails = ref({})
const showForm = ref(false)
const activeTab = ref('Open')
const importInput = ref(null)
const currentGroupId = ref(null)
const createMode = ref('manual') // manual, auto
const lockStatuses = ref({})

const autoAssignData = ref({
    healthContractId: null,
    groupName: '',
    examDate: '',
    status: 'Open'
})
const isAutoAssignLoading = ref(false)

const onAutoContractSelect = () => {
    if (!autoAssignData.value.healthContractId) return
    const contract = contracts.value.find(c => c.healthContractId === Number(autoAssignData.value.healthContractId))
    if (!contract) return
    
    // Đồng bộ ngày chuẩn 100%
    const rawDate = contract.startDate || contract.StartDate
    if (rawDate) {
        autoAssignData.value.examDate = rawDate.split('T')[0]
    }

    const contractGroups = groups.value.filter(g => g.healthContractId === Number(autoAssignData.value.healthContractId))
    const nextSequence = contractGroups.length + 1
    const companyBase = contract.shortName || contract.companyName || 'Công ty'
    
    // Đặt tên chuyên nghiệp: Đoàn khám [Tên Cty] - Lần [X]
    autoAssignData.value.groupName = `Đoàn khám ${companyBase} - Lần ${nextSequence}`
}

const handleAutoCreateOnly = async () => {
    if (!autoAssignData.value.healthContractId || !autoAssignData.value.groupName || !autoAssignData.value.examDate) {
        toast.error("Vui lòng điền đầy đủ thông tin")
        return
    }
    
    try {
        isAutoAssignLoading.value = true
        await apiClient.post('/api/MedicalGroups', {
            healthContractId: Number(autoAssignData.value.healthContractId),
            groupName: autoAssignData.value.groupName,
            examDate: autoAssignData.value.examDate,
            status: 'Open'
        })
        await fetchData()
        showForm.value = false
        toast.success("Hệ thống đã tự động tạo đoàn khám thành công!")
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        isAutoAssignLoading.value = false
    }
}

const getRequiredStaffType = (posName) => {
    if (!posName) return null;
    const clinicalKeywords = ['Khám', 'Siêu âm', 'Sản', 'Tai mũi họng', 'Bác sĩ', 'Chẩn đoán'];
    const technicalKeywords = ['Lấy máu', 'X-Quang', 'Xét nghiệm', 'Xét nghiệm máu', 'Kỹ thuật'];
    
    if (clinicalKeywords.some(k => posName.toLowerCase().includes(k.toLowerCase()))) return 'BacSi';
    if (technicalKeywords.some(k => posName.toLowerCase().includes(k.toLowerCase()))) return 'KyThuatVien';
    return 'DieuDuong'; // Default to Nursing/Support
}

const currentPositionName = computed(() => {
    const gid = modals.value.staff.groupId;
    const pid = modals.value.staff.data.positionId;
    if (!gid || !pid) return '';
    return groupPositions.value[gid]?.find(p => p.positionId === pid)?.positionName || '';
})

const requiredTypeForSelectedPos = computed(() => getRequiredStaffType(currentPositionName.value));

const recommendedStaff = computed(() => {
    if (!requiredTypeForSelectedPos.value) return [];
    return staffList.value.filter(s => s.staffType === requiredTypeForSelectedPos.value);
})

const otherStaff = computed(() => {
    if (!requiredTypeForSelectedPos.value) return staffList.value;
    return staffList.value.filter(s => s.staffType !== requiredTypeForSelectedPos.value);
})

const isRoleMismatch = computed(() => {
    const sid = modals.value.staff.data.staffId;
    const reqType = requiredTypeForSelectedPos.value;
    if (!sid || !reqType) return false;
    const staff = staffList.value.find(s => s.staffId === sid);
    if (!staff || !staff.staffType) return false;
    
    // Nếu yêu cầu Bác sĩ mà gán người khác -> Mismatch
    if (reqType === 'BacSi' && staff.staffType !== 'BacSi') return true;
    // Nếu Bác sĩ mà bị gán vào vị trí không yêu cầu Bác sĩ -> Mismatch (Lãng phí tài nguyên)
    if (staff.staffType === 'BacSi' && reqType !== 'BacSi') return true;
    
    return false;
})

const onPositionChange = () => {
    // Tự động clear nhân viên nếu đã chọn mà không khớp (optional, but warning is better)
}

const newGroup = ref({ healthContractId: null, groupName: '', examDate: '', status: 'Open' })
const cardTab = ref({})
const loadingSupplies = ref({})
const suppliesData = ref({})
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
        const rawData = res.data;
        let processedData = rawData;
        
        if (typeof rawData === 'string') {
            // Xóa dấu phẩy thừa trước ] hoặc } để tránh lỗi JSON.parse
            processedData = rawData.replace(/,(\s*[\]\}])/g, '$1');
        }
        
        aiSuggestions.value = typeof processedData === 'string' ? JSON.parse(processedData) : processedData;
        showAiModal.value = true;
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
    if (staffDetail.employeeCode?.toLowerCase() === authStore.user?.username?.toLowerCase()) return true
    return false
}

const canCheckOut = (staffDetail, group) => {
    if (group.status !== 'Open' || !staffDetail.checkInTime || staffDetail.checkOutTime) return false
    if (can('ChamCong.CheckInOut')) return true
    // MedicalStaff can only check-out for themselves — backend validates identity
    if (staffDetail.employeeCode?.toLowerCase() === authStore.user?.username?.toLowerCase()) return true
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
        case 'Finished': return 'Hòa tất khám'
        case 'Locked': return 'Đã khóa sổ'
        default: return status
    }
}

const fetchLockStatus = async (groupId) => {
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${groupId}/lock-status`)
        lockStatuses.value[groupId] = res.data
    } catch (e) {
        console.error("Lock Status Error:", e)
    }
}

const handleLockGroup = async (groupId) => {
    confirmData.value = {
        show: true,
        title: 'Khóa sổ tài chính',
        message: 'Hệ thống sẽ tự động tính toán lương dựa trên ca làm thực tế và khóa toàn bộ dữ liệu đoàn khám này. Bạn có chắc chắn muốn chốt số liệu không?',
        variant: 'warning',
        onConfirm: async () => {
            try {
                const res = await apiClient.post(`/api/MedicalGroups/${groupId}/lock`)
                toast.success("Đã khóa sổ đoàn khám thành công!")
                fetchData()
            } catch (e) {
                toast.error(parseApiError(e))
            }
        }
    }
}

const fetchData = async () => {
    try {
        if (authStore.hasRole('MedicalStaff')) {
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
        
        const cRes = await apiClient.get('/api/Contracts')
        contracts.value = cRes.data

        if (authStore.hasAnyRole('Admin', 'MedicalGroupManager', 'MedicalStaff', 'PersonnelManager')) {
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
            if (g.status === 'Finished' || g.status === 'Locked') {
                fetchLockStatus(g.groupId)
            }
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
        newGroup.value = { healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0], status: 'Open' }
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

// --- Smart Create Logic Removed as per request ---

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

const openSuppliesTab = async (groupId) => {
    cardTab.value[groupId] = 'supplies'
    loadingSupplies.value[groupId] = true
    try {
        const res = await apiClient.get(`/api/MedicalGroups/${groupId}/supplies-report`)
        suppliesData.value[groupId] = res.data
    } catch (e) {
        toast.error(parseApiError(e))
    } finally {
        loadingSupplies.value[groupId] = false
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
