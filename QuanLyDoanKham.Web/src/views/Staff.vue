<template>
  <div class="h-full flex flex-col bg-slate-50 relative animate-fade-in-up p-3 scrollbar-premium overflow-y-auto font-sans">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-2 mb-4">
      <div>
        <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
          <div class="w-8 h-8 bg-primary text-white rounded-lg flex items-center justify-center shadow-md">
            <UsersIcon class="w-4.5 h-4.5" />
          </div>
          {{ i18n.t('staff.title') }}
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[8px] mt-0.5">{{ i18n.t('staff.subtitle') }}</p>
      </div>
      <div class="flex items-center gap-2">
        <button v-if="can('NhanSu.Manage')" 
                @click="exportStaff" 
                class="btn-premium h-8 bg-white border border-slate-200 text-slate-600 px-3 shadow-sm hover:bg-slate-50">
          <Download class="w-3 h-3 mr-1" />
          <span class="text-[8px] font-black uppercase tracking-widest">XUẤT</span>
        </button>
        <button v-if="can('NhanSu.Manage')" 
                @click="triggerImport" 
                class="btn-premium h-8 bg-primary/10 text-primary px-3 shadow-sm hover:bg-primary/20">
          <UploadIcon class="w-3 h-3 mr-1" />
          <span class="text-[8px] font-black uppercase tracking-widest">NHẬP</span>
        </button>
        <button v-if="can('NhanSu.Manage')" 
                @click="openModal()" 
                class="btn-premium primary h-8 !rounded-lg !px-4 text-[9px] font-black">
          <Plus class="w-3.5 h-3.5 mr-1" />
          <span>{{ i18n.t('staff.addBtn') }}</span>
        </button>
      </div>
    </div>
    
    <!-- Stats Summary -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-4">
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-8 h-8 bg-primary/10 text-primary rounded-lg flex items-center justify-center shadow-inner">
          <UsersIcon class="w-4 h-4" />
        </div>
        <div>
          <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest">Tổng nhân sự</p>
          <p class="text-base font-black text-slate-900 tabular-nums">{{ list.length }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-8 h-8 bg-emerald-100 text-emerald-600 rounded-lg flex items-center justify-center shadow-inner">
          <CheckCircle2 class="w-4 h-4" />
        </div>
        <div>
          <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest">Đi đoàn</p>
          <p class="text-base font-black text-slate-900 tabular-nums">{{ list.filter(s => s.isWorking).length }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-8 h-8 bg-amber-100 text-amber-600 rounded-lg flex items-center justify-center shadow-inner">
          <Clock class="w-4 h-4" />
        </div>
        <div>
          <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest">Nghỉ</p>
          <p class="text-base font-black text-slate-900 tabular-nums">{{ list.filter(s => !s.isWorking).length }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-8 h-8 bg-blue-100 text-blue-600 rounded-lg flex items-center justify-center shadow-inner">
          <DollarSign class="w-4 h-4" />
        </div>
        <div>
          <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest">Tổng lương</p>
          <p class="text-base font-black text-slate-900 tabular-nums">{{ formatPrice(list.reduce((sum, s) => sum + (s.baseSalary || 0), 0)) }}</p>
        </div>
      </div>
    </div>

    <!-- Search & List -->
    <div class="premium-card overflow-hidden">
        <div class="p-2.5 border-b border-slate-50 flex flex-col md:flex-row gap-2 bg-slate-50/10">
            <div class="relative group flex-1">
                <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-300 w-3 h-3" />
                <input v-model="searchQuery" placeholder="Tìm tên hoặc mã nhân viên..." 
                       class="w-full pl-9 pr-3 py-1 rounded-lg bg-white border border-slate-200 focus:border-primary/20 outline-none font-bold text-[11px] text-slate-600 shadow-sm transition-all" />
            </div>
            <select v-model="activeTab" class="px-3 py-1 rounded-lg bg-white border border-slate-200 font-bold text-[11px] uppercase tracking-widest text-slate-500 outline-none min-w-[160px]">
                <option value="All">Tất cả chức danh ({{ list.length }})</option>
                <option v-for="role in roles" :key="role" :value="role">
                    {{ role }} ({{ list.filter(s => s.jobTitle === role).length }})
                </option>
            </select>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[8px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-2 text-center w-10">#</th>
                        <th class="p-2">Nhân sự</th>
                        <th class="p-2">Chức danh</th>
                        <th class="p-2">Vai trò</th>
                        <th class="p-2 text-center">Trạng thái</th>
                        <th class="p-2 text-right">Lương/Ngày</th>
                        <th class="p-2 text-center">Thao tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(item, index) in filteredList" :key="item.staffId" 
                        class="text-[10.5px] hover:bg-slate-50/50 transition-all cursor-pointer" @click="openModal(item)">
                        <td class="p-2 text-center font-black text-slate-400">{{ index + 1 }}</td>
                        <td class="p-2">
                            <div class="flex items-center gap-1.5">
                                <div class="w-7 h-7 rounded bg-slate-50 border border-slate-100 flex-shrink-0">
                                    <img v-if="item.avatarPath" :src="`/${item.avatarPath}`" class="w-full h-full object-cover" />
                                    <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${item.fullName}`" class="w-full h-full" />
                                </div>
                                <div>
                                    <div class="font-bold text-slate-800 uppercase tracking-wide truncate max-w-[150px]">{{ item.fullName }}</div>
                                    <div class="text-[7px] font-black text-slate-400 uppercase tracking-widest">{{ item.employeeCode }}</div>
                                </div>
                            </div>
                        </td>
                        <td class="p-2 font-bold text-slate-500 uppercase tracking-widest text-[8.5px]">{{ item.jobTitle }}</td>
                        <td class="p-2">
                            <span class="inline-flex px-1 py-0.5 rounded bg-slate-100 text-slate-600 font-black text-[7.5px] uppercase tracking-widest ">
                                {{ i18n.t('roles.' + (item.systemRole || 'MedicalStaff')) }}
                            </span>
                        </td>
                        <td class="p-2 text-center">
                            <span v-if="item.isWorking" class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-emerald-50 text-emerald-600 text-[8px] font-black uppercase tracking-widest ">
                                <span class="w-1 h-1 rounded-full bg-emerald-500"></span>
                                Đoàn
                            </span>
                            <span v-else class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded bg-slate-50 text-slate-400 text-[8px] font-black uppercase tracking-widest ">
                                Nghỉ
                            </span>
                        </td>
                        <td class="p-2 text-right font-bold text-slate-700 tabular-nums">
                            {{ formatPrice(item.baseSalary || 0) }}
                        </td>
                        <td class="p-2 text-center">
                            <button v-if="can('NhanSu.Manage')" 
                                    @click.stop="openModal(item)" class="p-1 rounded-lg bg-indigo-50 text-indigo-500 hover:bg-indigo-500 hover:text-white transition-all shadow-sm">
                                <Edit3 class="w-3.5 h-3.5" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="filteredList.length === 0">
                        <td colspan="7" class="py-20 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <UsersIcon class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không có dữ liệu nhân sự</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Staff Detail Modal -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay flex items-center justify-center p-3" @click.self="showModal = false">
        <div class="modal-box w-full max-w-4xl animate-scale-up !rounded-xl overflow-hidden flex flex-col max-h-[90vh]">
            <!-- Header Section -->
            <div class="bg-indigo-600 text-white p-3 relative shrink-0">
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                        <div class="w-8 h-8 bg-white/10 backdrop-blur-md rounded-lg flex items-center justify-center shadow-inner">
                            <UsersIcon class="w-4 h-4" />
                        </div>
                        <div>
                            <h2 class="text-base font-black uppercase tracking-tight italic leading-none">
                                {{ currentStaff.staffId ? currentStaff.fullName : i18n.t('staff.formTitle') }}
                            </h2>
                            <p class="text-[7.5px] font-bold text-white/60 uppercase tracking-widest mt-0.5">
                                ID: <span class="text-white font-black">{{ currentStaff.employeeCode || 'AUTO' }}</span>
                            </p>
                        </div>
                    </div>
                    <button @click="showModal = false" class="w-7 h-7 rounded-lg bg-black/10 hover:bg-black/20 flex items-center justify-center transition-all">
                        <X class="w-4 h-4" />
                    </button>
                </div>

                <!-- Tab Navigation inside Header -->
                <div class="flex gap-1.5 mt-3">
                    <button v-for="tab in modalTabs" :key="tab.id"
                            @click="activeModalTab = tab.id"
                            :class="['px-2.5 py-1 text-[8px] font-black uppercase tracking-widest transition-all rounded-md',
                                     activeModalTab === tab.id ? 'bg-white text-indigo-600 shadow-md' : 'text-white/70 hover:text-white hover:bg-white/10']">
                        {{ tab.label }}
                    </button>
                </div>
            </div>

            <!-- Modal Content -->
            <div class="p-4 overflow-y-auto custom-scrollbar flex-1">
                <!-- TAB 1: Thông tin cơ bản -->
                <div v-if="activeModalTab === 'info'" class="animate-fade-in">
                    <div class="grid grid-cols-1 lg:grid-cols-4 gap-5">
                        <!-- Basic Info Form -->
                        <div class="lg:col-span-3">
                            <form id="staffForm" @submit.prevent="saveStaff" class="grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-2.5">
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Họ và Tên *</label>
                                    <input v-model="currentStaff.fullName" required class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold" placeholder="Nguyễn Văn A" />
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Giới tính</label>
                                    <select v-model="currentStaff.gender" class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold">
                                        <option value="Nam">Nam</option>
                                        <option value="Nữ">Nữ</option>
                                    </select>
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Chức danh</label>
                                    <select v-model="jobCategory" required class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold">
                                        <option v-for="job in standardJobs" :key="job" :value="job">{{ job }}</option>
                                        <option value="Khác">Khác...</option>
                                    </select>
                                </div>
                                <div v-if="jobCategory === 'Khác'" class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-indigo-400 italic">Chức danh cụ thể *</label>
                                    <input v-model="currentStaff.jobTitle" required class="input-premium py-1.5 bg-slate-50 focus:bg-white w-full border-indigo-200 text-[11px] font-bold" placeholder="VD: Lái xe..." />
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Lương / Ngày công *</label>
                                    <CurrencyInput v-model="currentStaff.baseSalary" required class="!py-1.5 text-[11px] font-bold" />
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">CCCD / CMND</label>
                                    <input v-model="currentStaff.idCardNumber" class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold" placeholder="001..." />
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Email</label>
                                    <input v-model="currentStaff.email" type="email" class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold" placeholder="nhanvien@gmail.com" />
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Vai trò hệ thống</label>
                                    <select v-model="currentStaff.systemRole" class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold" required>
                                        <option value="Admin">{{ i18n.t('roles.Admin') }}</option>
                                        <option value="PersonnelManager">{{ i18n.t('roles.PersonnelManager') }}</option>
                                        <option value="ContractManager">{{ i18n.t('roles.ContractManager') }}</option>
                                        <option value="PayrollManager">{{ i18n.t('roles.PayrollManager') }}</option>
                                        <option value="MedicalGroupManager">{{ i18n.t('roles.MedicalGroupManager') }}</option>
                                        <option value="WarehouseManager">{{ i18n.t('roles.WarehouseManager') }}</option>
                                        <option value="MedicalStaff">{{ i18n.t('roles.MedicalStaff') }}</option>
                                        <option value="Customer">{{ i18n.t('roles.Customer') }}</option>
                                    </select>
                                </div>
                                <div class="flex flex-col gap-0.5">
                                    <label class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 ">Số điện thoại</label>
                                    <input v-model="currentStaff.phoneNumber" class="input-premium py-1.5 bg-slate-50 border-slate-200 focus:bg-white w-full text-[11px] font-bold" placeholder="09..." />
                                </div>
                            </form>
                        </div>
                        
                        <!-- Side Actions -->
                        <div class="space-y-3">
                            <div class="bg-white p-3 rounded-xl border border-slate-100 shadow-sm text-center">
                                <div @click="triggerAvatarUpload" class="w-16 h-16 mx-auto rounded-xl overflow-hidden bg-slate-50 border-2 border-slate-50 shadow-inner group cursor-pointer relative mb-1.5">
                                    <img v-if="currentStaff.avatarPath" :src="`/${currentStaff.avatarPath}`" class="w-full h-full object-cover" />
                                    <div v-else class="w-full h-full flex flex-col items-center justify-center text-slate-300">
                                        <Camera class="w-5 h-5 mb-0.5" />
                                        <span class="text-[6px] font-black uppercase tracking-widest">Tải lên</span>
                                    </div>
                                    <div class="absolute inset-0 bg-primary/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all text-white font-black text-[8px]">SỬA</div>
                                </div>
                                <input type="file" ref="avatarInput" class="hidden" @change="onAvatarChange" />
                                <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest ">Ảnh đại diện</p>
                            </div>

                            <div v-if="currentStaff.staffId" class="bg-indigo-600 text-white p-3 rounded-xl shadow-lg shadow-indigo-100 flex flex-col justify-between h-20 relative overflow-hidden">
                                <div class="relative z-10">
                                    <p class="text-[7px] font-black uppercase opacity-60 mb-0.5 tracking-[0.2em]">Thù lao tạm tính</p>
                                    <p class="text-base font-black ">{{ formatPrice(currentStaff.shifts?.reduce((sum, s) => sum + s.calculatedSalary, 0) || 0) }}</p>
                                </div>
                                <Wallet class="absolute -right-1 -bottom-1 w-12 h-12 opacity-10 rotate-12" />
                            </div>
                        </div>
                    </div>
                    
                    <!-- Lịch sử công tác (Read-only) -->
                    <div v-if="currentStaff.staffId" class="pt-8 mt-8 border-t border-slate-100">
                         <h4 class="text-xs font-black uppercase tracking-widest text-slate-400 mb-6 flex items-center gap-2">
                             <HistoryIcon class="w-4 h-4" /> Lịch sử đăng ký thực địa
                         </h4>
                         <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                             <div v-for="(day, idx) in currentStaff.workdays" :key="idx" class="flex justify-between items-center p-4 bg-white border border-slate-100 rounded-2xl hover:border-indigo-100 transition-all shadow-sm">
                                 <div>
                                     <p class="text-sm font-black text-slate-700">{{ formatDate(day.date) }}</p>
                                     <p class="text-[9px] font-black text-indigo-400 uppercase tracking-widest ">{{ day.workPosition || 'Vị trí: N/A' }}</p>
                                 </div>
                                 <span class="text-[10px] font-black px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg">{{ day.groupName }}</span>
                             </div>
                             <div v-if="!currentStaff.workdays?.length" class="col-span-full text-center py-12 text-slate-300 text-[10px] font-black uppercase tracking-widest italic bg-slate-50/50 rounded-3xl">Chưa có lịch sử làm việc</div>
                         </div>
                    </div>
                </div>

                <!-- TAB 2: Bảng Lương -->
                <div v-if="activeModalTab === 'payroll'" class="animate-fade-in">
                    <div v-if="payrollLoading" class="py-16 text-center text-slate-400">
                        <div class="w-8 h-8 border-2 border-sky-400 border-t-transparent rounded-full animate-spin mx-auto mb-3"></div>
                        <p class="text-xs font-black uppercase tracking-widest">Đang tải dữ liệu lương...</p>
                    </div>
                    <div v-else>
                        <div class="grid grid-cols-3 gap-4 mb-6">
                            <div class="bg-emerald-50 rounded-2xl p-5 text-center border border-emerald-100">
                                <p class="text-[9px] font-black uppercase tracking-widest text-emerald-600 mb-1">Tổng thù lao</p>
                                <p class="text-xl font-black text-emerald-700">{{ formatPrice(payrollData.totalSalary || 0) }}</p>
                            </div>
                            <div class="bg-sky-50 rounded-2xl p-5 text-center border border-sky-100">
                                <p class="text-[9px] font-black uppercase tracking-widest text-sky-600 mb-1">Số đoàn tham gia</p>
                                <p class="text-xl font-black text-sky-700">{{ payrollData.totalShifts || 0 }}</p>
                            </div>
                            <div class="bg-violet-50 rounded-2xl p-5 text-center border border-violet-100">
                                <p class="text-[9px] font-black uppercase tracking-widest text-violet-600 mb-1">Tổng ngày công</p>
                                <p class="text-xl font-black text-violet-700">{{ payrollData.totalDays || 0 }}</p>
                            </div>
                        </div>
                        <div class="bg-white rounded-2xl border border-slate-100 overflow-hidden">
                            <table class="w-full text-xs">
                                <thead class="bg-slate-50">
                                    <tr>
                                        <th class="p-3 text-left font-black text-slate-400 uppercase tracking-widest">Đoàn khám</th>
                                        <th class="p-3 text-left font-black text-slate-400 uppercase tracking-widest">Ngày</th>
                                        <th class="p-3 text-left font-black text-slate-400 uppercase tracking-widest">Vị trí</th>
                                        <th class="p-3 text-center font-black text-slate-400 uppercase tracking-widest">Ca</th>
                                        <th class="p-3 text-right font-black text-slate-400 uppercase tracking-widest">Thù lao</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-slate-50">
                                    <tr v-for="s in payrollData.shifts" :key="s.id" class="hover:bg-slate-50/50">
                                        <td class="p-3 font-black text-slate-700">{{ s.groupName }}</td>
                                        <td class="p-3 text-slate-500">{{ formatDate(s.examDate) }}</td>
                                        <td class="p-3 text-slate-500">{{ s.workPosition || '—' }}</td>
                                        <td class="p-3 text-center">
                                            <span class="px-2 py-1 rounded bg-indigo-50 text-indigo-600 font-black">
                                                {{ s.shiftType === 0.5 ? 'Nửa ca' : 'Cả ngày' }}
                                            </span>
                                        </td>
                                        <td class="p-3 text-right font-black text-slate-800">{{ formatPrice(s.calculatedSalary) }}</td>
                                    </tr>
                                    <tr v-if="!payrollData.shifts?.length">
                                        <td colspan="5" class="py-12 text-center text-slate-300 font-black text-[10px] uppercase tracking-widest">Chưa có dữ liệu lương</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <!-- TAB 3: Chấm công -->
                <div v-if="activeModalTab === 'attendance'" class="animate-fade-in">
                    <div class="flex gap-3 mb-5">
                        <select v-model="attendanceYear" @change="loadAttendance" class="px-4 py-2 rounded-xl bg-slate-50 border border-slate-200 font-black text-xs text-slate-600 outline-none">
                            <option v-for="y in yearOptions" :key="y" :value="y">Năm {{ y }}</option>
                        </select>
                        <select v-model="attendanceMonth" @change="loadAttendance" class="px-4 py-2 rounded-xl bg-slate-50 border border-slate-200 font-black text-xs text-slate-600 outline-none">
                            <option value="">Tất cả tháng</option>
                            <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }}</option>
                        </select>
                    </div>
                    <div v-if="attendanceLoading" class="py-16 text-center text-slate-400">
                        <div class="w-8 h-8 border-2 border-sky-400 border-t-transparent rounded-full animate-spin mx-auto mb-3"></div>
                    </div>
                    <div v-else class="space-y-3">
                        <div v-for="(rec, idx) in attendanceData" :key="idx"
                             class="flex items-center gap-4 p-4 bg-white border border-slate-100 rounded-2xl hover:border-sky-100 transition-all">
                            <div class="w-14 h-14 rounded-2xl flex flex-col items-center justify-center flex-shrink-0"
                                 :class="rec.checkInTime ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-50 text-slate-400'">
                                <span class="text-lg font-black leading-tight">{{ new Date(rec.examDate).getDate() }}</span>
                                <span class="text-[8px] font-black uppercase tracking-widest">Th{{ new Date(rec.examDate).getMonth()+1 }}</span>
                            </div>
                            <div class="flex-1">
                                <p class="font-black text-slate-800 text-sm">{{ rec.groupName }}</p>
                                <p class="text-[10px] text-slate-400 font-black uppercase tracking-widest mt-0.5">{{ rec.workPosition || 'Chưa rõ vị trí' }}</p>
                                <div class="flex gap-4 mt-1.5 text-[10px] font-black">
                                    <span v-if="rec.checkInTime" class="text-emerald-600">✓ Vào: {{ formatTime(rec.checkInTime) }}</span>
                                    <span v-else class="text-slate-300">Chưa check-in</span>
                                    <span v-if="rec.checkOutTime" class="text-sky-600">✓ Ra: {{ formatTime(rec.checkOutTime) }}</span>
                                </div>
                            </div>
                            <div class="text-right">
                                <span :class="rec.workStatus === 'Joined' ? 'text-emerald-600 bg-emerald-50' : 'text-slate-400 bg-slate-50'"
                                      class="px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-widest">
                                    {{ rec.workStatus }}
                                </span>
                                <p class="text-xs font-black text-slate-700 mt-2">{{ formatPrice(rec.calculatedSalary) }}</p>
                            </div>
                        </div>
                        <div v-if="!attendanceData.length" class="py-16 text-center text-slate-300">
                            <p class="font-black text-[10px] uppercase tracking-widest">Không có dữ liệu chấm công kỳ này</p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Footer Section -->
            <div class="p-3 border-t border-slate-50 flex justify-between gap-2 bg-slate-50/50 shrink-0">
                <button v-if="currentStaff.staffId && can('HeThong.UserManage')" @click="deleteStaff" type="button" class="h-8 w-8 rounded-lg bg-white text-rose-500 flex items-center justify-center hover:bg-rose-500 hover:text-white transition-all border border-rose-100 shadow-sm">
                    <Trash2 class="w-4 h-4" />
                </button>
                <div class="flex-1"></div>
                <div class="flex gap-2">
                    <button @click="showModal = false" class="px-4 py-1.5 text-slate-400 font-black uppercase tracking-widest text-[9px] hover:text-slate-600 transition-colors">
                        {{ i18n.t('common.cancel') }}
                    </button>
                    <button form="staffForm" type="submit" class="bg-slate-900 text-white px-6 py-1.5 rounded-lg font-black shadow-lg hover:bg-slate-800 transition-all uppercase tracking-widest text-[9px]">
                        {{ i18n.t('common.save') }}
                    </button>
                </div>
            </div>
        </div>
      </div>
    </Teleport>

    <!-- Hidden Elements -->
    <input type="file" ref="importInput" class="hidden" @change="handleImportFile" />
    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import apiClient from '../services/apiClient'
import { parseApiError } from '../services/errorHelper'
import { 
    Users as UsersIcon, Plus, Search, ArrowRight, X, Camera, Save, 
    History as HistoryIcon, Download, Upload as UploadIcon, Wallet, Stethoscope,
    Edit3, Trash2, RefreshCw, Filter, MoreHorizontal, UserPlus, FileSpreadsheet,
    Mail, Phone, MapPin, Briefcase, Calendar, ChevronRight, CheckCircle, CheckCircle2,
    AlertCircle, Clock, Loader2, ShieldAlert, UserCheck, DollarSign, Activity, LogOut, LogIn, Package
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { usePermission } from '../composables/usePermission'
import { useToast } from '../composables/useToast'

import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useI18nStore } from '../stores/i18n'
import CurrencyInput from '../components/CurrencyInput.vue'

const authStore = useAuthStore()
const { can } = usePermission()
const i18n = useI18nStore()
const toast = useToast()
const list = ref([])
const showModal = ref(false)
const searchQuery = ref('')
const activeTab = ref('All')
const currentStaff = ref({})
const avatarInput = ref(null)
const importInput = ref(null)
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const activeModalTab = ref('info')
const modalTabs = [
    { id: 'info', label: 'Thông tin' },
    { id: 'payroll', label: 'Bảng lương' },
    { id: 'attendance', label: 'Chấm công' },
]

// Payroll tab state
const payrollLoading = ref(false)
const payrollData = ref({ totalSalary: 0, totalShifts: 0, totalDays: 0, shifts: [] })

// Attendance tab state
const attendanceLoading = ref(false)
const attendanceData = ref([])
const attendanceYear = ref(new Date().getFullYear())
const attendanceMonth = ref('')
const yearOptions = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i)

const standardJobs = ['Bác sĩ', 'Điều dưỡng', 'Kỹ thuật viên', 'Dược sĩ', 'Nhân viên hỗ trợ']
const jobCategory = ref('Bác sĩ')

const roles = computed(() => {
    if (!list.value) return []
    return [...new Set(list.value.map(s => s.jobTitle).filter(Boolean))]
})

const totalPayroll = computed(() => {
    if (!list.value) return 0
    return list.value.reduce((acc, s) => acc + (s.baseSalary || 0), 0)
})

const filteredList = computed(() => {
    let res = list.value
    if (activeTab.value !== 'All') res = res.filter(s => s.jobTitle === activeTab.value)
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase()
        res = res.filter(s => s.fullName.toLowerCase().includes(q) || s.employeeCode.toLowerCase().includes(q))
    }
    return res
})

const fetchList = async () => {
    try {
        const res = await apiClient.get('/api/Staffs')
        list.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu nhân viên") }
}

const openModal = async (staff = null) => {
    if (staff) {
        try {
            const res = await apiClient.get(`/api/Staffs/${staff.staffId}`)
            const data = res.data
            currentStaff.value = {
                ...data,
                systemRole: data.systemRole || data.SystemRole || 'MedicalStaff'
            }
            if (standardJobs.includes(currentStaff.value.jobTitle)) {
                jobCategory.value = currentStaff.value.jobTitle
            } else {
                jobCategory.value = 'Khác'
            }
        } catch (e) { 
            currentStaff.value = { ...staff, workdays: [], shifts: [], systemRole: staff.systemRole || 'MedicalStaff' } 
            if (standardJobs.includes(currentStaff.value.jobTitle)) {
                jobCategory.value = currentStaff.value.jobTitle
            } else {
                jobCategory.value = 'Khác'
            }
        }
    } else {
        currentStaff.value = { fullName: '', email: '', phoneNumber: '', gender: 'Nam', jobTitle: 'Bác sĩ', baseSalary: 1000000, systemRole: 'MedicalStaff' }
        jobCategory.value = 'Bác sĩ'
    }
    showModal.value = true
    activeModalTab.value = 'info'
    if (staff?.staffId) {
        loadPayroll(staff.staffId)
        loadAttendance()
    }
}

watch(jobCategory, (newVal) => {
    if (newVal !== 'Khác') {
        currentStaff.value.jobTitle = newVal
    } else if (!currentStaff.value.jobTitle || standardJobs.includes(currentStaff.value.jobTitle)) {
        currentStaff.value.jobTitle = ''
    }
})

const saveStaff = async () => {
    try {
        if (currentStaff.value.staffId) {
            await apiClient.put(`/api/Staffs/${currentStaff.value.staffId}`, currentStaff.value)
        } else {
            await apiClient.post('/api/Staffs', currentStaff.value)
        }
        toast.success("Đã ghi nhận dữ liệu nhân sự!")
        showModal.value = false
        fetchList()
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const deleteStaff = async () => {
    if (!confirm("Xóa nhân viên này?")) return
    try {
        await apiClient.delete(`/api/Staffs/${currentStaff.value.staffId}`)
        toast.success("Đã gỡ bỏ!")
        showModal.value = false
        fetchList()
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const triggerAvatarUpload = () => avatarInput.value.click()
const onAvatarChange = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        const res = await apiClient.post('/api/Staffs/upload-avatar', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        })
        currentStaff.value.avatarPath = res.data.path
    } catch (e) { 
        toast.error(parseApiError(e)) 
    }
}

const exportStaff = async () => {
    try {
        const res = await apiClient.get('/api/Staffs/export', { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'DanhSachNhanSu.xlsx')
        document.body.appendChild(link)
        link.click()
        toast.success("Đã tải tệp Excel danh sách nhân sự!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const triggerImport = () => importInput.value.click()
const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        await apiClient.post('/api/Staffs/import', formData)
        toast.success("Đã nhập dữ liệu nhân sự thành công!")
        fetchList()
    } catch (e) { toast.error(parseApiError(e)) }
}

const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatTime = (d) => d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
const formatPrice = (p) => new Intl.NumberFormat('vi-VN').format(p) + ' đ'

const loadPayroll = async (staffId) => {
    if (!staffId) return
    payrollLoading.value = true
    try {
        const res = await apiClient.get(`/api/Staffs/${staffId}/payroll-summary`)
        payrollData.value = res.data
    } catch (e) { console.warn('Cannot load payroll', e) }
    finally { payrollLoading.value = false }
}

const loadAttendance = async () => {
    if (!currentStaff.value.staffId) return
    attendanceLoading.value = true
    try {
        const params = new URLSearchParams({ year: attendanceYear.value })
        if (attendanceMonth.value) params.append('month', attendanceMonth.value)
        const res = await apiClient.get(`/api/Staffs/${currentStaff.value.staffId}/attendance?${params}`)
        attendanceData.value = res.data
    } catch (e) { console.warn('Cannot load attendance', e) }
    finally { attendanceLoading.value = false }
}

onMounted(fetchList)
</script>
