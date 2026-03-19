<template>
  <div class="space-y-8 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-200">
            <Stethoscope class="w-6 h-6" />
          </div>
          Điều hành Đoàn khám
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-indigo-600 font-black tabular-nums">{{ String(groups.length).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[9px] mt-2">Nội bộ: Quản lý nhân sự, Vị trí trực & Trạng thái làm việc</p>
      </div>

      <div class="flex items-center gap-4">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                @click="exportGroups" 
                class="btn-premium bg-white border border-slate-200 text-slate-600 px-6 py-3 rounded-xl shadow-sm hover:bg-slate-50 transition-all">
            <Download class="w-4 h-4 mr-2" />
            <span class="text-[10px] font-black uppercase tracking-widest">Xuất DS Đoàn</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                @click="showForm = !showForm" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 rounded-xl shadow-xl hover:shadow-2xl transition-all">
            <Plus class="w-5 h-5 mr-2" />
            <span class="text-[10px] font-black uppercase tracking-widest ">Khởi tạo đoàn</span>
        </button>
      </div>
    </div>

    <!-- Stats Overview -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard 
            title="Tổng số đoàn khám"
            :value="String(groups.length).padStart(3, '0')"
            :icon="Building2"
            variant="indigo"
            subtext="Danh mục đoàn"
        />
        <StatCard 
            title="Đang triển khai"
            :value="String(openGroups.length).padStart(3, '0')"
            :icon="Calendar"
            variant="emerald"
            subtext="Đoàn hoạt động"
        />
        <StatCard 
            title="Tổng nhân sự điều động"
            :value="String(Object.values(staffDetails).flat().length).padStart(3, '0')"
            :icon="UsersIcon"
            variant="amber"
            subtext="Lượt quân số"
        />
    </div>

    <!-- Smart Create Form -->
    <div v-if="showForm" class="premium-card p-10 bg-white border-4 border-indigo-50/50 mb-10 animate-slide-up relative overflow-hidden">
        <div class="absolute top-0 right-0 p-4">
            <button @click="showForm = false" class="p-2 hover:bg-slate-100 rounded-full transition-all">
                <X class="w-5 h-5 text-slate-400" />
            </button>
        </div>

        <!-- Smart Create Section -->
        <div class="mb-8 pb-8 border-b border-slate-100">
            <div class="flex items-center gap-3 mb-6">
                <div class="w-10 h-10 bg-emerald-50 text-emerald-600 rounded-2xl flex items-center justify-center">
                    <Zap class="w-5 h-5" />
                </div>
                <div>
                    <h4 class="font-black text-slate-800 uppercase tracking-widest text-sm">Tạo nhanh từ Hợp đồng</h4>
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">Tự động điền thông tin từ hợp đồng đã phê duyệt</p>
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase text-indigo-400 tracking-widest">Chọn Hợp đồng (Đã phê duyệt)</label>
                    <select v-model="selectedContractForAuto" @change="onSmartContractSelect" class="w-full px-4 py-3 rounded-xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                        <option :value="null" disabled>-- Chọn hợp đồng --</option>
                        <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                            {{ c.companyName }}
                        </option>
                    </select>
                    <p v-if="approvedContracts.length === 0" class="text-[9px] font-black text-rose-400">Chưa có hợp đồng nào được phê duyệt.</p>
                </div>

                <!-- Smart Preview -->
                <div v-if="smartPreview" class="bg-indigo-50 rounded-2xl p-5 border-2 border-indigo-100 space-y-3">
                    <p class="text-[9px] font-black text-indigo-400 uppercase tracking-widest">Xem trước thông tin đoàn</p>
                    <div class="space-y-2">
                        <div class="flex items-center gap-2">
                            <span class="text-[9px] font-black text-slate-500 uppercase tracking-widest w-20">Tên đoàn:</span>
                            <input v-model="smartPreview.groupName" class="flex-1 bg-white rounded-lg px-3 py-1.5 text-xs font-black text-slate-800 border border-indigo-100 outline-none focus:border-indigo-400 transition-all" />
                        </div>
                        <div class="flex items-center gap-2">
                            <span class="text-[9px] font-black text-slate-500 uppercase tracking-widest w-20">Ngày KH:</span>
                            <input v-model="smartPreview.examDate" type="date" class="flex-1 bg-white rounded-lg px-3 py-1.5 text-xs font-black text-slate-800 border border-indigo-100 outline-none focus:border-indigo-400 transition-all" />
                        </div>
                    </div>

                    <!-- Clone Staff Option -->
                    <div v-if="lastGroupOfContract" class="flex items-center gap-3 p-3 bg-white rounded-xl border border-indigo-100">
                        <input type="checkbox" id="cloneStaff" v-model="doCloneStaff" class="w-4 h-4 accent-indigo-600" />
                        <label for="cloneStaff" class="text-[10px] font-black text-slate-700 cursor-pointer">
                            Clone nhân sự từ đoàn cũ: <span class="text-indigo-500">{{ lastGroupOfContract.groupName }}</span>
                        </label>
                    </div>

                    <!-- Conflict Warnings -->
                    <div v-if="conflictWarnings.length > 0" class="space-y-1">
                        <p class="text-[9px] font-black text-rose-500 uppercase tracking-widest">⚠ Cảnh báo trùng lịch:</p>
                        <div v-for="w in conflictWarnings" :key="w.staffId" class="bg-rose-50 rounded-lg px-3 py-1.5 border border-rose-100">
                            <span class="text-[9px] font-black text-rose-600">{{ w.fullName }} — đã có lịch trong đoàn khác</span>
                        </div>
                    </div>

                    <button @click="confirmSmartCreate" class="w-full bg-emerald-600 text-white py-3 rounded-xl font-black text-[10px] uppercase tracking-widest shadow-lg shadow-emerald-100 active:scale-95 transition-all flex items-center justify-center gap-2">
                        <Zap class="w-4 h-4" /> XÁC NHẬN TẠO NHANH
                    </button>
                </div>
            </div>
        </div>

        <div class="flex items-center gap-4 mb-8">
            <div class="h-px flex-1 bg-slate-100"></div>
            <h3 class="text-[10px] font-black text-slate-300 uppercase tracking-widest">Hoặc khai báo thủ công</h3>
            <div class="h-px flex-1 bg-slate-100"></div>
        </div>

        <form @submit.prevent="addGroup" class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Hợp đồng mục tiêu</label>
                <select v-model="newGroup.healthContractId" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                    <option :value="null" disabled>-- Chọn hợp đồng --</option>
                    <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">{{ c.shortName || c.companyName }}</option>
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
                <button type="submit" class="btn-premium bg-indigo-600 text-white px-12 py-4 rounded-2xl shadow-xl shadow-indigo-100 hover:shadow-2xl transition-all active:scale-95 uppercase tracking-widest text-[11px] font-black ">KÍCH HOẠT ĐOÀN MỚI</button>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-6">
        <button @click="activeTab = 'Open'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Open' ? 'bg-primary text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Đang thực hiện ({{ openGroups.length }})
        </button>
        <button @click="activeTab = 'Finished'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Finished' ? 'bg-slate-800 text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Đã hoàn tất ({{ closedGroups.length }})
        </button>
    </div>

    <!-- Danh sách Đoàn -->
    <div class="space-y-6">
        <div v-for="group in filteredGroups" :key="group.groupId" 
             class="premium-card bg-white border-2 border-slate-50 overflow-hidden group/card shadow-sm">
            
            <div class="p-8 bg-slate-900 text-white flex justify-between items-center">
                <div class="flex items-center gap-6">
                    <div class="w-16 h-16 bg-indigo-500 rounded-2xl flex items-center justify-center text-white shadow-xl">
                        <Stethoscope class="w-8 h-8" />
                    </div>
                    <div>
                        <h4 class="text-2xl font-black ">{{ group.groupName }}</h4>
                        <p class="text-xs font-black text-indigo-300 uppercase tracking-widest ">{{ group.shortName || group.companyName }} • {{ formatDate(group.examDate) }}</p>
                    </div>
                </div>
                <div class="flex items-center gap-3">
                   <span :class="['px-4 py-1.5 rounded-full text-[9px] font-black uppercase tracking-widest border', getStatusClass(group.status)]">{{ getStatusLabel(group.status) }}</span>
                   <button v-if="group.status === 'Open'" @click="updateStatus(group.groupId, 'Finished')" class="text-[9px] font-black bg-white/10 hover:bg-white/20 px-4 py-2 rounded-xl transition-all">HOÀN TẤT ĐOÀN</button>
                </div>
            </div>

            <div class="p-8">
                <!-- Nhân sự List Table -->
                <div class="space-y-6">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400 ">
                            <UsersIcon class="w-4 h-4" /> Đội ngũ đi khám & Vị trí trực
                        </h5>
                        <div class="flex items-center gap-3">
                            <button v-if="group.status === 'Open'" @click="openStaffModal(group.groupId)" class="text-[9px] font-black text-indigo-600 uppercase tracking-widest hover:underline">+ ĐIỀU ĐỘNG & GÁN VỊ TRÍ</button>
                            <div class="w-px h-3 bg-slate-200"></div>
                            <button @click="exportGroupStaff(group.groupId, group.groupName)" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest hover:underline">XUẤT DS ĐI ĐOÀN (EXCEL)</button>
                        </div>
                    </div>
                    
                    <div class="overflow-x-auto border border-slate-100 rounded-2xl">
                        <table class="w-full text-left bg-white">
                            <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                                <tr>
                                    <th class="p-4 text-center w-16">STT</th>
                                    <th class="p-4">Nhân sự</th>
                                    <th class="p-4">Vị trí làm việc tại đoàn</th>
                                    <th class="p-4 text-center">Ca làm</th>
                                    <th class="p-4 text-center">Trạng thái</th>
                                    <th class="p-4 text-center">Tác vụ</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-slate-50 text-xs">
                                <tr v-for="(s, index) in staffDetails[group.groupId]" :key="s.id" class="hover:bg-slate-50/50 transition-all">
                                    <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                                    <td class="p-4">
                                        <div class="font-black text-slate-800 uppercase tracking-widest">{{ s.fullName }}</div>
                                        <div class="text-[9px] text-slate-400 uppercase tracking-widest font-black mt-1">{{ s.jobTitle }}</div>
                                    </td>
                                    <td class="p-4">
                                        <span class="px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg font-black uppercase tracking-widest text-[9px]">
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
                                            <button v-if="(authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager') && group.status === 'Open' && !s.checkInTime" 
                                                    @click="checkIn(s.id, group.groupId)"
                                                    class="btn-action-premium variant-indigo text-slate-400" title="Vào đoàn">
                                                <LogIn class="w-5 h-5" />
                                            </button>
                                            <button v-if="(authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager') && group.status === 'Open' && s.checkInTime && !s.checkOutTime" 
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
                             <a :href="'http://localhost:5283/' + group.importFilePath" target="_blank" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest underline ml-2">Tải về</a>
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
      <div v-if="modals.staff.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-xl p-4 overflow-y-auto">
          <div class="bg-white w-full max-w-xl rounded-[3.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Gradient -->
              <div class="absolute top-0 left-0 right-0 h-32 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

              <button @click="modals.staff.show = false" class="absolute top-6 right-6 p-3 hover:bg-white/20 rounded-full transition-all text-white z-10">
                  <X class="w-6 h-6" />
              </button>

              <div class="mt-32 relative z-10 pt-4">
                  <div class="p-12 pt-4 pb-0">
                  <div class="flex items-center gap-5 mb-10">
                      <div class="w-16 h-16 bg-white rounded-[1.5rem] flex items-center justify-center text-teal-600 shadow-lg border-2 border-teal-50">
                          <UsersIcon class="w-8 h-8" />
                      </div>
                      <div>
                          <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest ">Điều động nhân sự</h3>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Phân bổ vị trí & Ca làm tại đoàn khám</p>
                      </div>
                  </div>

              <form id="staffForm" @submit.prevent="addStaff" class="space-y-8">
                  <div class="space-y-3">
                      <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                          <CheckCircle2 class="w-3 h-3 text-indigo-400" /> Chọn nhân viên (Đảm bảo không trùng lịch)
                      </label>
                      <select v-model="modals.staff.data.staffId" required class="w-full px-6 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all appearance-none">
                          <option :value="null" disabled>-- Chọn nhân sự từ danh sách --</option>
                          <option v-for="s in staffList" :key="s.staffId" :value="s.staffId">{{ s.fullName }} — {{ s.jobTitle }}</option>
                      </select>
                  </div>
                  
                  <div class="grid grid-cols-2 gap-6">
                      <div class="space-y-3">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                              <Clock class="w-3 h-3 text-indigo-400" /> Loại ca làm
                          </label>
                          <select v-model="modals.staff.data.shiftType" class="w-full px-6 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                              <option :value="1.0">Cả ngày (Hệ số 1.0)</option>
                              <option :value="0.5">Nửa ngày (Hệ số 0.5)</option>
                          </select>
                      </div>
                      <div class="space-y-3">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                              <RefreshCw class="w-3 h-3 text-indigo-400" /> Trạng thái ban đầu
                          </label>
                          <select v-model="modals.staff.data.workStatus" class="w-full px-6 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                              <option value="Đang chờ">Đang chờ (Pending)</option>
                              <option value="Đã tham gia">Đã tham gia (Active)</option>
                          </select>
                      </div>
                  </div>

                  <div class="space-y-3">
                      <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 flex items-center gap-2">
                          <Stethoscope class="w-3 h-3 text-indigo-400" /> Vị trí làm việc tại đoàn
                      </label>
                      <select v-model="modals.staff.data.workPosition" required class="w-full px-6 py-4 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-indigo-500/20 focus:bg-white outline-none font-black text-slate-700 transition-all">
                          <option value="Tiếp nhận">Trạm 01: Tiếp nhận & Phân loại</option>
                          <option value="Cân đo huyết áp">Trạm 02: Cân đo & Huyết áp</option>
                          <option value="Khám nội">Trạm 03: Khám Nội tổng quát</option>
                          <option value="Khám ngoại">Trạm 04: Khám Ngoại / Da liễu</option>
                          <option value="Lấy máu">Trạm 05: Lấy máu (Cận lâm sàng)</option>
                          <option value="Siêu âm">Trạm 06: Siêu âm tổng quát</option>
                          <option value="Khám sản phụ khoa">Trạm 07: Sản phụ khoa</option>
                          <option value="Hậu cần">Trạm 08: Hậu cần & Trả kết quả</option>
                          <option value="Khác">Phụ trợ / Khác</option>
                      </select>
                  </div>

                  </form>
                  </div>

                  <div class="px-12 pb-12 pt-4 bg-white relative z-20">
                      <div class="flex gap-4 pt-4">
                          <button type="button" @click="modals.staff.show = false" class="flex-1 py-5 text-slate-400 font-black text-xs uppercase tracking-widest hover:text-slate-600 transition-all underline decoration-2 underline-offset-8">Hủy bỏ</button>
                          <button form="staffForm" type="submit" class="flex-[2] bg-indigo-600 text-white py-5 rounded-2xl font-black text-xs uppercase tracking-[0.2em] shadow-xl shadow-indigo-100 transition-all active:scale-95 hover:bg-slate-900">XÁC NHẬN ĐIỀU ĐỘNG</button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- Hidden Input for Import -->
    <input type="file" ref="importInput" class="hidden" @change="handleImportFile" />

    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { 
    Stethoscope, Plus, Building2, Calendar, Users as UsersIcon, FileText, Trash2, 
    FileIcon, X, Download, Upload as UploadIcon, LogIn, LogOut, CheckCircle2, Clock, Zap
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import StatCard from '../components/StatCard.vue'

const authStore = useAuthStore()
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

const newGroup = ref({ healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0] })
const modals = ref({
    staff: { show: false, groupId: null, data: { staffId: null, shiftType: 1.0, workPosition: 'Tiếp nhận', workStatus: 'Đang chờ' } }
})
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const openGroups = computed(() => groups.value.filter(g => g.status === 'Open'))
const closedGroups = computed(() => groups.value.filter(g => g.status !== 'Open'))
const filteredGroups = computed(() => activeTab.value === 'Open' ? openGroups.value : closedGroups.value)

// Only show approved contracts for group creation
const approvedContracts = computed(() => contracts.value.filter(c => 
    c.status === 'Approved' || c.status === 'Active' || c.status === 'InProgress' ||
    !['Pending', 'Draft'].includes(c.status)
))

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
        const [gRes, cRes, sRes] = await Promise.all([
            axios.get('http://localhost:5283/api/MedicalGroups'),
            axios.get('http://localhost:5283/api/HealthContracts'),
            axios.get('http://localhost:5283/api/Staffs')
        ]);
        groups.value = gRes.data
        contracts.value = cRes.data
        staffList.value = sRes.data
        groups.value.forEach(g => fetchGroupStaff(g.groupId))
    } catch (e) { toast.error("Lỗi khi tải dữ liệu") }
}

const fetchGroupStaff = async (id) => {
    try {
        const res = await axios.get(`http://localhost:5283/api/MedicalGroups/${id}/staffs`)
        staffDetails.value[id] = res.data
    } catch (e) { console.error(e) }
}

const addGroup = async () => {
    try {
        await axios.post('http://localhost:5283/api/MedicalGroups', newGroup.value)
        toast.success("Khởi tạo đoàn thành công!")
        showForm.value = false
        fetchData()
    } catch (e) { toast.error("Lỗi khi tạo đoàn khám") }
}

const autoCreateGroup = async () => {
    if (!selectedContractForAuto.value) return
    try {
        await axios.post(`http://localhost:5283/api/MedicalGroups/auto-create/${selectedContractForAuto.value}`)
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

    // Auto-fill group name and date
    const now = new Date()
    const month = String(now.getMonth() + 1).padStart(2, '0')
    const year = now.getFullYear()
    smartPreview.value = {
        groupName: `Đoàn khám - ${contract.companyName} - ${month}/${year}`,
        examDate: contract.examDate ? contract.examDate.split('T')[0] : 
                  new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    }

    // Find last group for this contract to offer clone
    const contractGroups = groups.value.filter(g => g.healthContractId === selectedContractForAuto.value)
    lastGroupOfContract.value = contractGroups.length > 0 ? contractGroups[contractGroups.length - 1] : null
    doCloneStaff.value = false
    conflictWarnings.value = []
}

const confirmSmartCreate = async () => {
    if (!smartPreview.value || !selectedContractForAuto.value) return
    try {
        // Create the group
        const res = await axios.post('http://localhost:5283/api/MedicalGroups', {
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
                        await axios.post(`http://localhost:5283/api/MedicalGroups/${newGroupId}/staffs`, {
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
        fetchData()
    } catch (e) { toast.error(e.response?.data || 'Lỗi tạo nhanh đoàn') }
}

const updateStatus = async (id, status) => {
    try {
        await axios.put(`http://localhost:5283/api/MedicalGroups/${id}/status`, { status: status })
        toast.success(`Đã cập nhật trạng thái: ${getStatusLabel(status)}`)
        fetchData()
    } catch (e) { 
        const msg = e.response?.data?.message || e.response?.data || "Lỗi cập nhật trạng thái"
        toast.error(msg) 
    }
}

const openStaffModal = (gid) => {
    modals.value.staff.groupId = gid
    modals.value.staff.show = true
    modals.value.staff.data = { staffId: null, shiftType: 1.0, workPosition: 'Tiếp nhận', workStatus: 'Đang chờ' }
}

const addStaff = async () => {
    try {
        const gid = modals.value.staff.groupId
        await axios.post(`http://localhost:5283/api/MedicalGroups/${gid}/staffs`, modals.value.staff.data)
        toast.success("Đã phân công nhân sự!")
        modals.value.staff.show = false
        fetchGroupStaff(gid)
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi khi phân công. Nhân viên có thể bị trùng lịch!") 
    }
}

const exportGroups = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/MedicalGroups/export', { responseType: 'blob' })
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
        const res = await axios.get(`http://localhost:5283/api/MedicalGroups/${id}/export-staff`, { responseType: 'blob' })
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
        await axios.delete(`http://localhost:5283/api/MedicalGroups/staffs/${detailId}`)
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
        await axios.post(`http://localhost:5283/api/MedicalGroups/staffs/${detailId}/checkin`)
        toast.success("Đã ghi nhận giờ vào đoàn!")
        fetchGroupStaff(gid)
    } catch (e) { toast.error(e.response?.data || "Lỗi Check-in") }
}

const checkOut = async (detailId, gid) => {
    try {
        const res = await axios.post(`http://localhost:5283/api/MedicalGroups/staffs/${detailId}/checkout`)
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
        const res = await axios.post(`http://localhost:5283/api/MedicalGroups/upload-data`, formData)
        await axios.put(`http://localhost:5283/api/MedicalGroups/${currentGroupId.value}`, { importFilePath: res.data.path })
        toast.success("Đã Import dữ liệu đoàn khám!")
        fetchData()
    } catch (e) { toast.error("Lỗi Import file") }
}

const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchData)
</script>
