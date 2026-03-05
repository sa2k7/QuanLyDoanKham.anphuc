<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-primary text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-primary/20">
            <UsersIcon class="w-8 h-8" />
          </div>
          Đội ngũ Nhân sự
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">Điều phối & Quản lý chuyên môn y tế</p>
      </div>
      <button v-if="authStore.role === 'Admin'" 
              @click="openModal()" 
              class="btn-premium bg-slate-900 text-white hover:bg-black shadow-2xl shadow-slate-200 py-4 px-10">
        <Plus class="w-6 h-6" />
        <span>THÊM NHÂN SỰ MỚI</span>
      </button>
    </div>

    <!-- Stats Section -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-8 mb-12">
      <div v-for="stat in stats" :key="stat.label" class="premium-card p-8 bg-white border-2 border-slate-50 group hover:border-primary/20 transition-all">
        <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4 group-hover:text-primary transition-colors">{{ stat.label }}</p>
        <div class="flex items-end gap-2">
            <span class="text-4xl font-black text-slate-900 tracking-tighter leading-none">{{ stat.value }}</span>
            <span class="text-xs font-bold text-slate-400 mb-1 uppercase tracking-widest">Thành viên</span>
        </div>
      </div>
    </div>

    <!-- Interface Controls -->
    <div class="flex flex-col lg:flex-row justify-between items-stretch lg:items-center gap-6 mb-12">
        <div class="flex p-1.5 bg-slate-100 rounded-[1.5rem] w-fit">
            <button v-for="tab in tabs" :key="tab.value"
                    @click="activeTab = tab.value"
                    :class="['px-8 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all', 
                             activeTab === tab.value ? 'bg-white text-primary shadow-xl shadow-slate-200/50' : 'text-slate-400 hover:text-slate-600']">
                {{ tab.label }}
            </button>
        </div>
        <div class="relative flex-1 max-w-xl group">
            <Search class="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 w-5 h-5 group-focus-within:text-primary transition-colors" />
            <input v-model="searchQuery" placeholder="Tìm theo tên, mã NV hoặc CCCD..." 
                   class="w-full pl-16 pr-8 py-5 rounded-[1.5rem] bg-white border-2 border-slate-50 focus:border-primary/20 focus:ring-8 focus:ring-primary/5 outline-none font-bold text-slate-600 placeholder:text-slate-300 transition-all shadow-sm shadow-slate-100" />
        </div>
    </div>

    <!-- Staff Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
        <div v-for="item in filteredList" :key="item.staffId" 
             class="premium-card p-1 group cursor-pointer hover:scale-[1.02] transition-all duration-500"
             @click="openModal(item)">
            
            <div class="p-8 pb-10">
                <div class="flex justify-center mb-8 relative">
                    <div class="w-24 h-24 rounded-[2rem] overflow-hidden border-4 border-white shadow-2xl bg-slate-50 group-hover:rotate-3 transition-transform duration-500">
                        <img v-if="item.avatarPath" :src="`http://localhost:5283/${item.avatarPath}`" class="w-full h-full object-cover" />
                        <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${item.fullName}`" class="w-full h-full" />
                    </div>
                    <div v-if="item.isWorking" class="absolute -bottom-2 right-1/2 translate-x-12 px-3 py-1 bg-emerald-500 text-white text-[8px] font-black uppercase tracking-widest rounded-full border-2 border-white shadow-lg animate-pulse">
                        Online
                    </div>
                </div>

                <div class="text-center space-y-2">
                    <p class="text-[9px] font-black text-slate-300 uppercase tracking-[0.2em]">{{ item.employeeCode || 'System ID' }}</p>
                    <h4 class="text-xl font-black text-slate-800 tracking-tight leading-tight px-4">{{ item.fullName }}</h4>
                    <p class="text-xs font-bold text-indigo-500 uppercase tracking-widest">{{ item.jobTitle }}</p>
                </div>
            </div>

            <div v-if="item.isWorking" class="mx-6 mb-6 p-4 bg-indigo-50/50 rounded-2xl border border-indigo-100/50 group-hover:bg-indigo-600 group-hover:border-indigo-600 transition-all duration-500">
                <div class="flex items-center gap-3">
                    <div class="p-2 bg-white rounded-xl text-indigo-600 shadow-sm">
                        <Activity class="w-4 h-4" />
                    </div>
                    <div>
                        <p class="text-[8px] font-black text-indigo-300 uppercase tracking-widest leading-none mb-1 group-hover:text-indigo-100">Đang trực tại</p>
                        <p class="text-[10px] font-black text-indigo-900 truncate max-w-[140px] group-hover:text-white">{{ item.currentGroupName }}</p>
                    </div>
                </div>
            </div>
            <div v-else class="mx-6 mb-6 p-4 bg-slate-50 rounded-2xl border border-slate-100 flex items-center gap-3 opacity-60">
                <div class="p-2 bg-white rounded-xl text-slate-300">
                    <Clock class="w-4 h-4" />
                </div>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Đang nghỉ ngơi</p>
            </div>

            <div class="px-8 py-6 bg-slate-50/50 border-t border-slate-100 flex items-center justify-between rounded-b-[2.5rem]">
                <div class="flex items-center gap-2">
                    <Wallet class="w-4 h-4 text-emerald-500" />
                    <span class="text-xs font-black text-slate-700 tracking-tight">{{ formatPrice(item.baseSalary) }}</span>
                </div>
                <div class="w-8 h-8 rounded-full bg-white flex items-center justify-center text-slate-300 group-hover:bg-primary group-hover:text-white transition-all shadow-sm">
                    <ArrowRight class="w-4 h-4" />
                </div>
            </div>
        </div>
        
        <div v-if="filteredList.length === 0" class="col-span-full py-40 bg-white rounded-[3rem] border-4 border-dashed border-slate-50 flex flex-col items-center justify-center gap-6">
            <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center text-slate-100">
                <UsersIcon class="w-12 h-12" />
            </div>
            <p class="text-xl font-black text-slate-300 uppercase tracking-widest">Không có dữ liệu nhân sự</p>
        </div>
    </div>

    <!-- Modal Form -->
    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 md:p-10 backdrop-blur-xl bg-slate-900/40 animate-fade-in">
        <div class="bg-white w-full max-w-5xl max-h-[90vh] overflow-hidden rounded-[3rem] shadow-2xl flex flex-col animate-scale-up">
            <!-- Modal Header -->
            <div class="p-8 border-b border-slate-100 flex justify-between items-center bg-white sticky top-0 z-10">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-indigo-200">
                        <UserPlus v-if="!currentStaff.staffId" class="w-6 h-6" />
                        <UserCheck v-else class="w-6 h-6" />
                    </div>
                    <div>
                        <h3 class="text-2xl font-black text-slate-800">{{ currentStaff.staffId ? 'Cập nhật thông tin' : 'Thêm Nhân sự mới' }}</h3>
                        <p class="text-sm font-medium text-slate-500">Mã nhân viên: <span class="text-indigo-600 font-bold">{{ currentStaff.employeeCode || 'TỰ ĐỘNG' }}</span></p>
                    </div>
                </div>
                <button @click="showModal = false" class="p-4 hover:bg-slate-50 rounded-2xl transition-all">
                    <X class="w-6 h-6 text-slate-400" />
                </button>
            </div>

            <!-- Hidden Avatar Input -->
            <input type="file" ref="avatarInput" @change="onAvatarChange" accept="image/*" class="hidden" />

            <!-- Modal Body -->
            <div class="p-8 overflow-y-auto custom-scrollbar bg-slate-50/30">
                <form id="staffForm" @submit.prevent="saveStaff" class="space-y-10">
                    <!-- section 1: Basic Info -->
                    <div class="space-y-6">
                        <div class="flex items-center gap-3">
                            <div class="w-1 h-6 bg-indigo-500 rounded-full"></div>
                            <h4 class="text-sm font-black uppercase tracking-[0.2em] text-slate-400">Thông tin cá nhân</h4>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Họ và Tên (Có dấu) *</label>
                                <input v-model="currentStaff.fullName" :disabled="authStore.role !== 'Admin'" required placeholder="Nguyễn Văn A" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Họ và Tên (Không dấu)</label>
                                <input v-model="currentStaff.fullNameUnsigned" :disabled="authStore.role !== 'Admin'" placeholder="NGUYEN VAN A" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Mã Nhân Viên</label>
                                <input v-model="currentStaff.employeeCode" :disabled="authStore.role !== 'Admin'" placeholder="NV001" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Năm sinh</label>
                                <input v-model.number="currentStaff.birthYear" :disabled="authStore.role !== 'Admin'" type="number" placeholder="1990" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Giới tính</label>
                                <select v-model="currentStaff.gender" :disabled="authStore.role !== 'Admin'" class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold disabled:bg-slate-50 disabled:text-slate-500 cursor-not-allowed">
                                    <option value="Nam">Nam</option>
                                    <option value="Nữ">Nữ</option>
                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Số CMND/CCCD</label>
                                <input v-model="currentStaff.idCardNumber" :disabled="authStore.role !== 'Admin'" maxlength="20" placeholder="001..." 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Số Điện Thoại</label>
                                <input v-model="currentStaff.phoneNumber" :disabled="authStore.role !== 'Admin'" placeholder="090..." 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2 md:col-start-1">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Ảnh đại diện</label>
                                <div @click="authStore.role === 'Admin' && avatarInput.click()" 
                                     class="relative w-32 h-32 rounded-[2rem] overflow-hidden border-4 border-white shadow-lg bg-slate-100 group cursor-pointer transition-all hover:scale-105 active:scale-95">
                                    <img v-if="currentStaff.avatarPath" :src="`http://localhost:5283/${currentStaff.avatarPath}`" class="w-full h-full object-cover" />
                                    <div v-else class="w-full h-full flex flex-col items-center justify-center text-slate-300">
                                        <Camera class="w-8 h-8 mb-1" />
                                        <span class="text-[8px] font-black uppercase">Click để tải</span>
                                    </div>
                                    <div v-if="authStore.role === 'Admin'" class="absolute inset-0 bg-indigo-600/60 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                        <Upload class="w-8 h-8 text-white" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- section 2: Work & Financial -->
                    <div class="space-y-6">
                        <div class="flex items-center gap-3">
                            <div class="w-1 h-6 bg-blue-500 rounded-full"></div>
                            <h4 class="text-sm font-black uppercase tracking-[0.2em] text-slate-400">Công việc & Tài chính</h4>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Chức danh</label>
                                <select v-model="currentStaff.jobTitle" :disabled="authStore.role !== 'Admin'" class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold disabled:bg-slate-50 disabled:text-slate-500">
                                    <option v-if="currentStaff.systemRole === 'MedicalStaff'" value="Bác sĩ">Bác sĩ</option>
                                    <option v-if="currentStaff.systemRole === 'MedicalStaff'" value="Điều dưỡng">Điều dưỡng</option>
                                    <option v-if="currentStaff.systemRole === 'MedicalStaff'" value="Kỹ thuật viên">Kỹ thuật viên</option>
                                    
                                    <option v-if="currentStaff.systemRole === 'PersonnelManager'" value="Quản lý Nhân sự">Quản lý Nhân sự</option>
                                    <option v-if="currentStaff.systemRole === 'PersonnelManager'" value="Chuyên viên Nhân sự">Chuyên viên Nhân sự</option>
                                    
                                    <option v-if="currentStaff.systemRole === 'ContractManager'" value="Quản lý Kinh doanh">Quản lý Kinh doanh</option>
                                    <option v-if="currentStaff.systemRole === 'ContractManager'" value="Chuyên viên Hợp đồng">Chuyên viên Hợp đồng</option>
                                    
                                    <option v-if="currentStaff.systemRole === 'WarehouseManager'" value="Quản lý Vật tư">Quản lý Vật tư</option>
                                    <option v-if="currentStaff.systemRole === 'WarehouseManager'" value="Thủ kho">Thủ kho</option>
                                    
                                    <option v-if="currentStaff.systemRole === 'PayrollManager'" value="Kế toán">Kế toán</option>
                                    <option v-if="currentStaff.systemRole === 'PayrollManager'" value="Quản lý Tiền lương">Quản lý Tiền lương</option>
                                    
                                    <option v-if="currentStaff.systemRole === 'MedicalGroupManager'" value="Điều phối viên">Điều phối viên</option>
                                    <option v-if="currentStaff.systemRole === 'MedicalGroupManager'" value="Quản lý Vận hành">Quản lý Vận hành</option>

                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Đơn vị / Phòng ban</label>
                                <select v-model="currentStaff.department" :disabled="authStore.role !== 'Admin'" class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold disabled:bg-slate-50 disabled:text-slate-500">
                                    <option v-if="currentStaff.systemRole === 'MedicalStaff'" value="Khối Chuyên môn">Khối Chuyên môn</option>
                                    <option v-if="currentStaff.systemRole === 'PersonnelManager'" value="Phòng Hành chính - Nhân sự">Phòng Hành chính - Nhân sự</option>
                                    <option v-if="currentStaff.systemRole === 'ContractManager'" value="Phòng Kinh doanh & Dự án">Phòng Kinh doanh & Dự án</option>
                                    <option v-if="currentStaff.systemRole === 'WarehouseManager'" value="Phòng Vật tư & Thiết bị">Phòng Vật tư & Thiết bị</option>
                                    <option v-if="currentStaff.systemRole === 'PayrollManager'" value="Phòng Kế toán - Tài chính">Phòng Kế toán - Tài chính</option>
                                    <option v-if="currentStaff.systemRole === 'MedicalGroupManager'" value="Phòng Vận hành Đoàn khám">Phòng Vận hành Đoàn khám</option>
                                    <option value="Ban Giám Đốc">Ban Giám Đốc</option>
                                    <option value="Khác">Khác</option>
                                </select>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Loại nhân sự</label>
                                <select v-model="currentStaff.employeeType" :disabled="authStore.role !== 'Admin'" class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold disabled:bg-slate-50 disabled:text-slate-500">
                                    <option value="Nội bộ">Nội bộ</option>
                                    <option value="Thuê ngoài">Thuê ngoài</option>
                                </select>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-rose-400 px-1 font-black">Vai trò hệ thống (Phân quyền) *</label>
                                <select v-model="currentStaff.systemRole" :disabled="authStore.role !== 'Admin'" required class="w-full px-6 py-4 rounded-2xl border-2 border-rose-100 bg-white shadow-sm focus:border-rose-500 outline-none transition-all font-black text-rose-600">
                                    <option value="MedicalStaff">NV Đi khám (Chuyên môn)</option>
                                    <option value="PersonnelManager">NV Quản lý NHÂN SỰ</option>
                                    <option value="ContractManager">NV Quản lý HỢP ĐỒNG & CÔNG TY</option>
                                    <option value="WarehouseManager">NV Quản lý VẬT TƯ</option>
                                    <option value="PayrollManager">NV Quản lý TÍNH LƯƠNG (Kế toán)</option>
                                    <option value="MedicalGroupManager">NV Quản lý VẬN HÀNH ĐOÀN KHÁM</option>
                                </select>
                                <p class="text-[9px] text-slate-400 px-1 italic">* Vai trò này quyết định các mục nhân viên thấy trên thanh Menu.</p>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Lương cơ bản (1 buổi) *</label>
                                <div class="relative">
                                    <input 
                                        type="text" 
                                        :value="currentStaff.baseSalary?.toLocaleString('vi-VN')" 
                                        :disabled="authStore.role !== 'Admin'"
                                        @input="e => {
                                            const val = e.target.value.replace(/\D/g, '');
                                            currentStaff.baseSalary = val ? parseInt(val) : 0;
                                            e.target.value = currentStaff.baseSalary.toLocaleString('vi-VN');
                                        }"
                                        required 
                                        class="w-full pl-6 pr-14 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-black text-indigo-600 text-lg disabled:bg-slate-50 disabled:text-slate-500" 
                                        placeholder="1.000.000" 
                                    />
                                    <span class="absolute right-6 top-1/2 -translate-y-1/2 text-xs font-black text-slate-400">VNĐ</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- section 3: Banking -->
                    <div class="space-y-6">
                        <div class="flex items-center gap-3">
                            <div class="w-1 h-6 bg-emerald-500 rounded-full"></div>
                            <h4 class="text-sm font-black uppercase tracking-[0.2em] text-slate-400">Thông tin tài khoản ngân hàng</h4>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Số tài khoản</label>
                                <input v-model="currentStaff.bankAccountNumber" :disabled="authStore.role !== 'Admin'" placeholder="123456..." 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Tên chủ tài khoản</label>
                                <input v-model="currentStaff.bankAccountName" :disabled="authStore.role !== 'Admin'" placeholder="VIET NAM A" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Tên ngân hàng</label>
                                <input v-model="currentStaff.bankName" :disabled="authStore.role !== 'Admin'" placeholder="VCB, Techcombank..." 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 px-1">Mã số thuế</label>
                                <input v-model="currentStaff.taxCode" :disabled="authStore.role !== 'Admin'" placeholder="8...123" 
                                       class="w-full px-6 py-4 rounded-2xl border-2 border-white bg-white shadow-sm focus:border-indigo-500/20 outline-none transition-all font-bold placeholder:text-slate-300 disabled:bg-slate-50 disabled:text-slate-500" />
                            </div>
                        </div>
                    </div>

                    <!-- section 4: Working History (Read-only for both, but specifically for Staff details) -->
                    <div v-if="currentStaff.staffId" class="space-y-6">
                        <div class="flex items-center gap-3">
                            <div class="w-1 h-6 bg-purple-500 rounded-full"></div>
                            <h4 class="text-sm font-black uppercase tracking-[0.2em] text-slate-400">Lịch sử công tác & Thù lao tạm tính</h4>
                        </div>
                        
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                            <!-- Workdays -->
                            <div class="bg-indigo-50/30 rounded-3xl p-6 border border-indigo-100/50">
                                <h5 class="text-xs font-black uppercase text-indigo-400 mb-4 flex items-center gap-2">
                                    <Calendar class="w-4 h-4" /> Danh sách ngày làm việc
                                </h5>
                                <div class="space-y-2 max-h-60 overflow-y-auto custom-scrollbar pr-2">
                                    <div v-for="(day, idx) in currentStaff.workdays" :key="idx" 
                                         class="flex justify-between items-center p-3 bg-white rounded-xl shadow-sm">
                                        <span class="text-sm font-bold text-slate-700">{{ new Date(day.date).toLocaleDateString('vi-VN') }}</span>
                                        <span class="text-[10px] font-black px-2 py-1 bg-indigo-50 text-indigo-600 rounded-lg">{{ day.groupName }}</span>
                                    </div>
                                    <p v-if="!currentStaff.workdays?.length" class="text-sm font-medium text-slate-400 italic text-center py-4">Chưa có lịch sử làm việc</p>
                                </div>
                            </div>

                            <!-- Shifts/Salary -->
                            <div class="bg-emerald-50/30 rounded-3xl p-6 border border-emerald-100/50">
                                <h5 class="text-xs font-black uppercase text-emerald-400 mb-4 flex items-center gap-2">
                                    <Activity class="w-4 h-4" /> Chi tiết thù lao từng đoàn
                                </h5>
                                <div class="space-y-3 max-h-60 overflow-y-auto custom-scrollbar pr-2">
                                    <div v-for="(shift, idx) in currentStaff.shifts" :key="idx" 
                                         class="p-4 bg-white rounded-2xl shadow-sm border border-slate-50">
                                        <div class="flex justify-between items-start mb-2">
                                            <span class="text-sm font-black text-slate-800">{{ shift.groupName }}</span>
                                            <span class="text-[10px] font-black px-2 py-0.5 bg-emerald-50 text-emerald-600 rounded-full">Ca: {{ shift.shiftType }}</span>
                                        </div>
                                        <div class="flex justify-between items-center text-xs">
                                            <span class="text-slate-400 font-bold">Lương tạm tính:</span>
                                            <span class="text-emerald-600 font-black">{{ formatPrice(shift.calculatedSalary) }}</span>
                                        </div>
                                    </div>
                                    <p v-if="!currentStaff.shifts?.length" class="text-sm font-medium text-slate-400 italic text-center py-4">Chưa có dữ liệu ca trực</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </form>
            </div>

            <!-- Modal Footer -->
            <div class="p-8 border-t border-slate-100 flex justify-between items-center bg-white sticky bottom-0 z-10 gap-4">
                <button v-if="currentStaff.staffId && authStore.role === 'Admin'" @click="removeItem" type="button" 
                        class="text-red-500 font-bold px-6 py-4 rounded-2xl hover:bg-red-50 transition-all flex items-center gap-2">
                    <Trash2 class="w-5 h-5" />
                    Xóa nhân viên
                </button>
                <div class="flex-1"></div>
                <button @click="showModal = false" type="button" class="px-8 py-4 rounded-2xl font-black text-slate-400 hover:bg-slate-50 transition-all">
                    {{ authStore.role === 'Admin' ? 'Hủy' : 'Đóng' }}
                </button>
                <button v-if="authStore.role === 'Admin'" form="staffForm" type="submit" 
                        class="bg-indigo-600 text-white px-12 py-4 rounded-2xl font-black shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all flex items-center gap-3 active:scale-95">
                    <Save class="w-5 h-5" />
                    {{ currentStaff.staffId ? 'Lưu thay đổi' : 'Tạo nhân viên' }}
                </button>
            </div>
        </div>
    </div>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog 
      v-model="showDeleteConfirm"
      title="Xóa nhân viên?"
      :message="`Bạn có chắc muốn xóa nhân viên &quot;${currentStaff.fullName}&quot;?`"
      confirmText="Xóa ngay"
      variant="danger"
      @confirm="deleteStaff"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { 
  Users, Plus, Search, ChevronRight, X, Save, Trash2, 
  Camera, Upload, UserCheck, Activity
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()
const showDeleteConfirm = ref(false)

const list = ref([])
const showModal = ref(false)
const activeTab = ref('All')
const searchQuery = ref('')
const currentStaff = ref({})
const avatarInput = ref(null)

// Auto-sync Job Title with System Role
watch(() => currentStaff.value.systemRole, (newRole) => {
    if (!newRole) return;
    
    const roleMapping = {
        'MedicalStaff': { title: 'Bác sĩ', dept: 'Khối Chuyên môn' },
        'PersonnelManager': { title: 'Quản lý Nhân sự', dept: 'Phòng Hành chính - Nhân sự' },
        'ContractManager': { title: 'Quản lý Kinh doanh', dept: 'Phòng Kinh doanh & Dự án' },
        'WarehouseManager': { title: 'Quản lý Vật tư', dept: 'Phòng Vật tư & Thiết bị' },
        'PayrollManager': { title: 'Kế toán', dept: 'Phòng Kế toán - Tài chính' },
        'MedicalGroupManager': { title: 'Điều phối viên', dept: 'Phòng Vận hành Đoàn khám' }
    };
    
    // Only auto-update if it's a new staff or certain conditions met
    if (roleMapping[newRole]) {
        currentStaff.value.jobTitle = roleMapping[newRole].title;
        currentStaff.value.department = roleMapping[newRole].dept;
    }
});

const stats = computed(() => [
  { label: 'Tổng nhân sự', value: list.value.length },
  { label: 'Bác sĩ', value: list.value.filter(s => s.jobTitle === 'Bác sĩ').length },
  { label: 'Điều dưỡng', value: list.value.filter(s => s.jobTitle === 'Điều dưỡng').length },
  { label: 'Kỹ thuật viên', value: list.value.filter(s => s.jobTitle === 'Kỹ thuật viên').length }
])

const tabs = [
    { label: 'Tất cả', value: 'All' },
    { label: 'Bác sĩ', value: 'Bác sĩ' },
    { label: 'Điều dưỡng', value: 'Điều dưỡng' },
    { label: 'Kỹ thuật viên', value: 'Kỹ thuật viên' }
]

const filteredList = computed(() => {
    let result = list.value
    
    if (activeTab.value !== 'All') {
        result = result.filter(s => s.jobTitle === activeTab.value)
    }
    
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase()
        result = result.filter(s => 
            s.fullName?.toLowerCase().includes(q) || 
            s.employeeCode?.toLowerCase().includes(q) || 
            s.idCardNumber?.includes(q) ||
            s.phoneNumber?.includes(q)
        )
    }
    
    return result
})

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Staffs')
        list.value = res.data
    } catch (e) { console.error(e) }
}

const openModal = async (staff = null) => {
    if (staff) {
        // Fetch full details including Workdays and Shifts
        try {
            const res = await axios.get(`http://localhost:5283/api/Staffs/${staff.staffId}`)
            currentStaff.value = res.data
        } catch (e) {
            console.error("Failed to fetch staff details", e)
            currentStaff.value = { ...staff, workdays: [], shifts: [] }
        }
    } else {
        currentStaff.value = {
            fullName: '',
            jobTitle: 'Bác sĩ',
            employeeType: 'Nội bộ',
            gender: 'Nam',
            baseSalary: 1000000,
            systemRole: 'MedicalStaff',
            workdays: [],
            shifts: []
        }
    }
    showModal.value = true
}

const saveStaff = async () => {
    try {
        const staff = { ...currentStaff.value };
        
        // Validation logic
        if (staff.idCardNumber && !/^\d{9,12}$/.test(staff.idCardNumber)) {
            return toast.warning("Số CMND/CCCD phải là số (9-12 ký tự)!")
        }
        if (staff.phoneNumber && !/^0\d{9,10}$/.test(staff.phoneNumber)) {
            return toast.warning("Số điện thoại không đúng định dạng!")
        }

        if (staff.staffId) {
            await axios.put(`http://localhost:5283/api/Staffs/${staff.staffId}`, staff)
            toast.success("Đã cập nhật thông tin nhân viên!")
        } else {
            await axios.post('http://localhost:5283/api/Staffs', staff)
            toast.success("Đã tạo nhân viên mới!")
        }
        
        showModal.value = false
        fetchList()
    } catch (e) { 
        console.error(e)
        toast.error("Không thể lưu thông tin nhân viên.")
    }
}

const removeItem = () => {
    showDeleteConfirm.value = true
}

const deleteStaff = async () => {
    try {
        await axios.delete(`http://localhost:5283/api/Staffs/${currentStaff.value.staffId}`)
        toast.success("Đã xóa nhân viên thành công!")
        showModal.value = false
        fetchList()
    } catch (e) {
        console.error(e)
        toast.error("Không thể xóa nhân viên.")
    }
}

const onAvatarChange = async (e) => {
    const file = e.target.files[0]
    if (!file) return

    const formData = new FormData()
    formData.append('file', file)

    try {
        const res = await axios.post('http://localhost:5283/api/Staffs/upload-avatar', formData)
        currentStaff.value.avatarPath = res.data.path
    } catch (e) {
        console.error("Avatar upload failed", e)
        toast.error("Không thể tải ảnh lên.")
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchList)
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 8px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

@keyframes fade-in {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes scale-up {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.animate-fade-in {
  animation: fade-in 0.4s ease-out forwards;
}

.animate-scale-up {
  animation: scale-up 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
