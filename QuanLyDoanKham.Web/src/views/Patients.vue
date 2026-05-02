<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in pb-4 p-3 scrollbar-premium overflow-y-auto font-sans">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-2 mb-3">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <div class="w-7 h-7 bg-primary text-white rounded-lg flex items-center justify-center shadow-md">
            <Users class="w-4 h-4" />
          </div>
          Quản Lý Bệnh Nhân
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[7.5px] mt-0.5">Lý lịch & Hồ sơ chuẩn y khoa (Medical Database)</p>
      </div>
      <div class="flex gap-2">
        <button @click="handleExport" class="btn-premium secondary h-7.5 !rounded-lg !px-2.5 shadow-sm border border-white/40 text-[8px] font-black uppercase">
          <Download class="w-3 h-3" /> XUẤT
        </button>
        <button @click="openAdd" class="btn-premium primary h-7.5 !rounded-lg !px-3 shadow-lg shadow-primary/20 text-[8px] font-black uppercase">
          <UserPlus class="w-3.5 h-3.5" /> THÊM MỚI
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="flex flex-col md:flex-row gap-2 mb-2.5">
      <div class="relative flex-1 group">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-300 w-3 h-3" />
        <input v-model="search" @input="debouncedSearch" type="text"
          placeholder="Tìm tên, CCCD, số điện thoại..." class="w-full pl-9 pr-3 py-1.5 rounded-lg bg-white/50 border border-slate-100 focus:bg-white focus:border-primary/20 outline-none font-bold text-[11px] text-slate-600 shadow-sm transition-all" />
      </div>
      <select v-model="filterContractId" @change="loadPatients" class="px-3 py-1.5 rounded-lg bg-white/50 border border-slate-100 font-bold text-[11px] text-slate-500 outline-none shadow-sm min-w-[160px]">
        <option value="">Tất cả hợp đồng</option>
        <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
          {{ c.contractName }}
        </option>
      </select>
      <button @click="loadPatients" class="w-7.5 h-7.5 bg-white rounded-lg shadow-sm border border-slate-100 text-slate-400 hover:text-primary transition-all flex items-center justify-center">
        <RefreshCw class="w-3 h-3" :class="{ 'animate-spin': loading }" />
      </button>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-2 mb-3">
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-primary/10 text-primary rounded-lg flex items-center justify-center shadow-inner">
          <Users class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Tổng bệnh nhân</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ total.toLocaleString() }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-emerald-100 text-emerald-600 rounded-lg flex items-center justify-center shadow-inner">
          <FileText class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Có hồ sơ</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.hasRecord).length }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-sky-100 text-sky-600 rounded-lg flex items-center justify-center shadow-inner">
          <CheckCircle class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Khám xong</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.status === 'COMPLETED').length }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-amber-100 text-amber-600 rounded-lg flex items-center justify-center shadow-inner">
          <Clock class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Chờ khám</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.status !== 'COMPLETED' && p.status !== 'CREATED').length }}</p>
        </div>
      </div>
    </div>

    <!-- Table / Content Area -->
    <div class="premium-card min-h-[400px] flex flex-col relative overflow-hidden">
      <!-- Loading Overlay -->
      <div v-if="loading" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-50 flex flex-col items-center justify-center animate-fade-in">
        <div class="relative">
          <RefreshCw class="w-10 h-10 text-primary animate-spin" />
          <div class="absolute inset-0 blur-xl bg-primary/20 animate-pulse"></div>
        </div>
        <p class="mt-4 text-[11px] font-black text-slate-800 uppercase tracking-[0.2em] animate-pulse">Đang truy xuất dữ liệu...</p>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && patients.length === 0" class="flex-grow flex flex-col items-center justify-center p-8 text-center animate-scale-up">
        <div class="w-16 h-16 bg-slate-50 rounded-[1.5rem] flex items-center justify-center mb-4 shadow-inner border border-slate-100/50">
          <UserRound class="w-8 h-8 text-slate-300" />
        </div>
        <h3 class="text-base font-black text-slate-800 uppercase tracking-tight mb-2 italic">Chưa có bệnh nhân nào</h3>
        <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest max-w-sm leading-relaxed mb-6">
          Hệ thống chưa ghi nhận dữ liệu bệnh nhân nào phù hợp. <br/> Bấm "Thêm Bệnh Nhân" để bắt đầu nhập liệu ngay.
        </p>
        <button @click="openAdd" class="btn-premium primary shadow-lg shadow-primary/20 !px-6 !py-2.5 text-[11px]">
          <UserPlus class="w-3.5 h-3.5" /> Thêm bệnh nhân mới
        </button>
      </div>

      <!-- Data Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full border-collapse">
          <thead class="text-left text-[7.5px] font-black uppercase tracking-widest text-slate-400 bg-slate-50/50 border-b border-slate-100">
            <tr>
              <th class="px-2 py-2 text-center w-8">#</th>
              <th class="px-2 py-2">Bệnh nhân</th>
              <th class="px-2 py-2">GT</th>
              <th class="px-2 py-2">Ngày sinh</th>
              <th class="px-2 py-2">CCCD/CMND</th>
              <th class="px-2 py-2">Điện thoại</th>
              <th class="px-2 py-2">Đơn vị</th>
              <th class="px-2 py-2">Hợp đồng</th>
              <th class="px-2 py-2 text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(p, i) in patients" :key="p.patientId" class="group hover:bg-slate-50/50 transition-all duration-300">
              <td class="px-2 py-1 text-center text-slate-400 text-[9px] font-bold">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td class="px-2 py-1">
                <div class="flex items-center gap-1.5">
                  <div class="avatar-circle w-5.5 h-5.5 text-[7px] font-black">{{ p.fullName?.charAt(0) }}</div>
                  <span class="font-bold text-slate-800 text-[10.5px] uppercase tracking-tight">{{ p.fullName }}</span>
                </div>
              </td>
              <td class="px-2 py-1">
                <span :class="['badge inline-block text-[7px] font-black uppercase px-1 py-0.5', p.gender === 'Nam' ? 'badge-blue' : p.gender === 'Nữ' ? 'badge-pink' : 'badge-gray']">
                  {{ p.gender === 'Nam' ? 'M' : p.gender === 'Nữ' ? 'F' : 'O' }}
                </span>
              </td>
              <td class="px-2 py-1 text-slate-600 text-[10px] tabular-nums font-medium">{{ formatDate(p.dateOfBirth) }}</td>
              <td class="px-2 py-1 text-slate-600 font-mono text-[9.5px]">{{ p.iDCardNumber || '—' }}</td>
              <td class="px-2 py-1 text-slate-600 text-[10px] tabular-nums font-medium">{{ p.phoneNumber || '—' }}</td>
              <td class="px-2 py-1 text-slate-500 text-[9.5px] truncate max-w-[120px]">{{ p.department || '—' }}</td>
              <td class="px-2 py-1 text-[9px] text-indigo-600 font-black truncate max-w-[140px] uppercase tracking-tighter">{{ p.contractName || '—' }}</td>
              <td class="px-2 py-1 text-center">
                <div class="flex items-center justify-center gap-1">
                  <button @click="viewDetail(p)" class="p-1 rounded-lg bg-blue-50 text-blue-500 hover:bg-blue-500 hover:text-white transition-all shadow-sm border border-blue-100" title="Xem hồ sơ">
                    <Eye class="w-3 h-3" />
                  </button>
                  <button @click="openEdit(p)" class="p-1 rounded-lg bg-indigo-50 text-indigo-500 hover:bg-indigo-500 hover:text-white transition-all shadow-sm border border-indigo-100" title="Sửa">
                    <Edit class="w-3 h-3" />
                  </button>
                  <button @click="confirmDelete(p)" class="p-1 rounded-lg bg-rose-50 text-rose-500 hover:bg-rose-500 hover:text-white transition-all shadow-sm border border-rose-100" title="Xóa">
                    <Trash2 class="w-3 h-3" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Footer -->
      <div v-if="totalPages > 1" class="p-3 border-t border-slate-100 bg-slate-50/30 flex items-center justify-between">
        <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest leading-none">
          Hiển thị {{ patients.length }} / {{ total }} bệnh nhân
        </p>
        <div class="flex items-center gap-1.5">
          <button @click="goPage(page - 1)" :disabled="page <= 1" class="w-8.5 h-8.5 rounded-xl flex items-center justify-center border border-slate-200 bg-white text-slate-400 hover:text-primary disabled:opacity-30 transition-all">
            <ArrowLeft class="w-3.5 h-3.5" />
          </button>
          <div class="px-3 py-1.5 rounded-xl bg-white border border-slate-200 text-[11px] font-black text-slate-700 tabular-nums">
            {{ page }} / {{ totalPages }}
          </div>
          <button @click="goPage(page + 1)" :disabled="page >= totalPages" class="w-8.5 h-8.5 rounded-xl flex items-center justify-center border border-slate-200 bg-white text-slate-400 hover:text-primary disabled:opacity-30 transition-all">
            <ArrowRight class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>

    <!-- MODAL: Thêm / Sửa Bệnh Nhân -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay flex items-center justify-center p-3" @click.self="showModal = false">
        <div class="modal-box max-w-lg animate-scale-up !rounded-xl overflow-hidden max-h-[95vh] flex flex-col">
          <div class="bg-indigo-600 text-white p-3 shrink-0">
            <div class="flex items-center justify-between w-full">
              <div class="flex items-center gap-2.5">
                <div class="w-8 h-8 bg-white/10 backdrop-blur-md rounded-lg flex items-center justify-center shadow-inner">
                  <UserPlus v-if="!editMode" class="w-4 h-4" />
                  <Edit v-else class="w-4 h-4" />
                </div>
                <div>
                  <h2 class="text-base font-black uppercase tracking-tight italic leading-none">
                    {{ editMode ? 'Cập Nhật Hồ Sơ' : 'Thêm Bệnh Nhân' }}
                  </h2>
                  <p class="text-[7.5px] font-bold text-white/60 uppercase tracking-widest mt-0.5">Khai báo thông tin bệnh nhân</p>
                </div>
              </div>
              <button @click="showModal = false" class="w-7 h-7 rounded-lg bg-black/10 hover:bg-black/20 flex items-center justify-center transition-all">
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>

          <form @submit.prevent="submitForm" class="p-3.5 overflow-y-auto">
            <div class="grid grid-cols-2 gap-x-3 gap-y-2 mb-4">
              <!-- Họ tên -->
              <div class="form-group col-span-2">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Họ và tên <span class="text-rose-500">*</span></label>
                <input v-model="form.fullName" type="text" required
                  placeholder="Nguyễn Văn A" class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200" />
              </div>

              <!-- Giới tính -->
              <div class="form-group">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Giới tính</label>
                <select v-model="form.gender" class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200">
                  <option value="">— Chọn —</option>
                  <option>Nam</option>
                  <option>Nữ</option>
                  <option>Khác</option>
                </select>
              </div>

              <!-- Ngày sinh -->
              <div class="form-group">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Ngày sinh</label>
                <input v-model="form.dateOfBirth" type="date" class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200" />
              </div>

              <!-- CCCD -->
              <div class="form-group">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">CCCD / CMND</label>
                <input v-model="form.iDCardNumber" type="text"
                  placeholder="012345678901" class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200" />
              </div>

              <!-- SĐT -->
              <div class="form-group">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Số điện thoại</label>
                <input v-model="form.phoneNumber" type="tel" maxlength="10"
                  @input="form.phoneNumber = form.phoneNumber.replace(/[^\d]/g, '').slice(0, 10)"
                  placeholder="0901234567" class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200" />
              </div>

              <!-- Phòng ban -->
              <div class="form-group col-span-2">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Phòng ban / Đơn vị</label>
                <input v-model="form.department" type="text"
                  placeholder="Phòng Kế toán, Ban Giám đốc..." class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200" />
              </div>

              <!-- Hợp đồng -->
              <div class="form-group col-span-2">
                <label class="text-[8px] font-black uppercase text-slate-400 tracking-widest">Thuộc Hợp Đồng <span class="text-rose-500">*</span></label>
                <select v-model="form.healthContractId" required class="form-input !py-1.5 !text-[11px] font-bold bg-slate-50 border-slate-200">
                  <option value="">— Chọn hợp đồng —</option>
                  <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                    {{ c.contractName }} ({{ c.companyName }})
                  </option>
                </select>
              </div>
            </div>

            <div v-if="formError" class="flex items-center gap-2 p-2 bg-rose-50 border border-rose-100 rounded-lg mb-3 animate-shake">
              <AlertCircle class="w-3.5 h-3.5 text-rose-500" />
              <p class="text-[8px] font-black text-rose-600 uppercase tracking-widest">{{ formError }}</p>
            </div>

            <div class="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
              <button type="button" @click="showModal = false" class="px-4 py-1.5 text-slate-400 font-black text-[9px] uppercase tracking-widest hover:text-slate-600 transition-all">Hủy</button>
              <button type="submit" :disabled="saving" class="btn-premium primary !px-5 !py-2 !rounded-lg text-[9px] font-black uppercase tracking-widest">
                <RefreshCw v-if="saving" class="w-3 h-3 animate-spin mr-1.5" />
                <Save v-else class="w-3 h-3 mr-1.5" />
                <span>{{ editMode ? 'Lưu hồ sơ' : 'Tạo mới' }}</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- MODAL: Chi tiết / Hồ sơ Bệnh Nhân -->
    <Teleport to="body">
      <div v-if="showDetail && detailData" class="modal-overlay flex items-center justify-center p-3 z-[100]" @click.self="showDetail = false">
        <div class="modal-box max-w-xl w-full animate-scale-up overflow-hidden !rounded-xl max-h-[90vh] flex flex-col">
          <div class="modal-header bg-indigo-600 text-white p-4 rounded-t-xl relative shrink-0">
            <div class="flex items-center justify-between w-full relative z-10">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 bg-white/20 backdrop-blur-md rounded-lg flex items-center justify-center text-lg font-black shadow-inner border border-white/30">
                  {{ detailData.fullName?.charAt(0) }}
                </div>
                <div>
                  <h2 class="text-base font-black italic uppercase tracking-tighter leading-none mb-1">{{ detailData.fullName }}</h2>
                  <div class="flex items-center gap-2">
                    <span class="px-1.5 py-0.5 bg-white/20 rounded-md text-[7px] font-black uppercase tracking-widest">{{ detailData.gender }}</span>
                    <div class="w-1 h-1 rounded-full bg-white/40"></div>
                    <span class="text-indigo-100 text-[9px] font-bold">{{ formatDate(detailData.dateOfBirth) }}</span>
                  </div>
                </div>
              </div>
              <button @click="showDetail = false" class="w-7 h-7 rounded-lg bg-black/10 hover:bg-black/20 flex items-center justify-center transition-all">
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>

          <div class="p-4 overflow-y-auto">
            <!-- Thông tin cơ bản -->
            <div class="grid grid-cols-2 gap-2 mb-4">
              <div class="p-2.5 rounded-lg bg-slate-50 border border-slate-100">
                <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest mb-0.5 flex items-center gap-1">
                  <Hash class="w-2 h-2" /> CCCD/CMND
                </p>
                <p class="font-bold text-slate-800 text-[11px] tabular-nums">{{ detailData.iDCardNumber || '—' }}</p>
              </div>
              <div class="p-2.5 rounded-lg bg-slate-50 border border-slate-100">
                <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest mb-0.5 flex items-center gap-1">
                  <Phone class="w-2 h-2" /> Điện thoại
                </p>
                <p class="font-bold text-slate-800 text-[11px] tabular-nums">{{ detailData.phoneNumber || '—' }}</p>
              </div>
              <div class="p-2.5 rounded-lg bg-slate-50 border border-slate-100">
                <p class="text-[7.5px] font-black text-slate-400 uppercase tracking-widest mb-0.5 flex items-center gap-1">
                  <Building2 class="w-2 h-2" /> Phòng ban
                </p>
                <p class="font-bold text-slate-700 text-[11px] truncate">{{ detailData.department || '—' }}</p>
              </div>
              <div class="p-2.5 rounded-lg bg-indigo-50/50 border border-indigo-100">
                <p class="text-[7.5px] font-black text-indigo-400 uppercase tracking-widest mb-0.5 flex items-center gap-1">
                  <FileText class="w-2 h-2" /> Hợp đồng
                </p>
                <p class="font-black text-indigo-600 text-[10px] truncate uppercase tracking-tighter">{{ detailData.contractName || '—' }}</p>
              </div>
            </div>

            <!-- Lịch sử khám -->
            <div class="flex flex-col">
              <div class="flex items-center justify-between mb-2.5">
                <h3 class="text-[11px] font-black text-slate-800 uppercase tracking-widest flex items-center gap-1.5 italic">
                  <History class="w-3.5 h-3.5 text-indigo-500" /> Lịch Sử Khám
                </h3>
                <span class="px-1.5 py-0.5 bg-slate-100 rounded text-[7.5px] font-black text-slate-500 tabular-nums">
                  {{ (detailData.medicalHistory || []).length }} PHIÊN
                </span>
              </div>

              <div v-if="!detailData.medicalHistory?.length" class="flex flex-col items-center justify-center py-8 bg-slate-50 rounded-lg border-2 border-dashed border-slate-200">
                <Inbox class="w-6 h-6 text-slate-300 mb-1.5" />
                <p class="text-[8px] font-bold text-slate-400 uppercase tracking-widest">Trống</p>
              </div>

              <div v-else class="space-y-1.5">
                <div v-for="h in detailData.medicalHistory" :key="h.medicalRecordId"
                  class="p-2.5 bg-white rounded-lg border border-slate-100 flex gap-2.5 group hover:border-primary/20 transition-all shadow-sm">
                  <div class="w-1 h-auto rounded-full shrink-0" :class="getStatusColor(h.status)"></div>
                  <div class="flex-1 min-w-0">
                    <div class="flex justify-between items-start gap-2">
                      <p class="font-bold text-slate-800 text-[10.5px] truncate">{{ h.groupName || 'Đoàn khám' }}</p>
                      <span :class="getStatusBadge(h.status)" class="badge text-[7px] px-1 py-0.5 font-black uppercase shrink-0">
                        {{ formatStatus(h.status) }}
                      </span>
                    </div>
                    <div class="flex items-center gap-2 mt-0.5 text-[8.5px] text-slate-400 font-bold">
                      <span>{{ formatDate(h.examDate) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="p-3 border-t bg-slate-50 flex justify-end gap-2 shrink-0">
            <button @click="openEdit(detailData); showDetail = false" class="btn-premium secondary !px-4 !py-1.5 text-[9px] font-black uppercase tracking-widest">
              <Edit class="w-3 h-3 mr-1" /> Sửa
            </button>
            <button @click="showDetail = false" class="btn-premium primary !px-5 !py-1.5 text-[9px] font-black uppercase tracking-widest">Đóng</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete Confirm Modal -->
    <Teleport to="body">
      <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
        <div class="modal-box max-w-sm animate-scale-up !rounded-2xl">
          <div class="p-6 text-center">
            <div class="w-14 h-14 bg-rose-100 text-rose-500 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-inner">
              <Trash2 class="w-7 h-7" />
            </div>
            <h3 class="text-lg font-black text-slate-800 italic uppercase tracking-tight mb-1">Xác nhận xóa</h3>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-6 leading-relaxed">
              Xóa bệnh nhân <span class="text-rose-500 underline">{{ deleteTarget.fullName }}</span>? <br/>
              Hành động này không thể khôi phục.
            </p>
            <div class="flex gap-2 justify-center">
              <button @click="deleteTarget = null" class="btn-premium secondary !px-5 !py-2 text-[10px]">Hủy</button>
              <button @click="doDelete" :disabled="saving" class="btn-premium primary !bg-rose-500 !shadow-rose-200 !px-6 !py-2 text-[10px]">
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
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, RefreshCw, UserRound, ArrowLeft, ArrowRight, UserPlus, FileText, CheckCircle, Clock, Calendar, Hash, Phone, Building2, Eye, Edit, Trash2, X, AlertCircle, FileDigit, Users, Save, History, Inbox, Upload, Download } from 'lucide-vue-next'
import apiClient from '@/services/apiClient'
import { useToast } from '@/composables/useToast'
import { parseApiError } from '@/services/errorHelper'

const toast = useToast()

// ─── State ────────────────────────────────────────────────────────────────────
const loading = ref(false)
const saving = ref(false)
const patients = ref([])
const contracts = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(30)
const search = ref('')
const filterContractId = ref('')

// Modals & UI State
const showModal = ref(false)
const editMode = ref(false)
const showDetail = ref(false)
const detailData = ref(null)
const deleteTarget = ref(null)
const formError = ref('')
const importing = ref(false)
const fileInput = ref(null)


const form = ref(resetForm())

// ─── Computed ─────────────────────────────────────────────────────────────────
const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const approvedContracts = computed(() => {
  // Ưu tiên hiển thị các hợp đồng đã được phê duyệt hoặc đang hoạt động.
  // Loại bỏ các 'Hợp đồng chưa đặt tên' trừ khi chúng đang ở trạng thái Active để giảm nhiễu.
  return contracts.value.filter(c => {
    const isUnnamed = c.contractName?.toLowerCase().includes('chưa đặt tên')
    const isRejected = c.status === 'REJECTED'
    
    if (isRejected) return false
    if (isUnnamed && c.status !== 'ACTIVE' && c.status !== 'Active') return false
    
    return true
  }).sort((a, b) => {
    // Đưa các hợp đồng có tên thật lên đầu
    const aUnnamed = a.contractName?.toLowerCase().includes('chưa đặt tên')
    const bUnnamed = b.contractName?.toLowerCase().includes('chưa đặt tên')
    if (aUnnamed && !bUnnamed) return 1
    if (!aUnnamed && bUnnamed) return -1
    return 0
  })
})

// ─── Init ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadContracts(), loadPatients()])
})

// ─── Helpers ──────────────────────────────────────────────────────────────────
function resetForm() {
  return {
    patientId: null,
    fullName: '',
    gender: '',
    dateOfBirth: '',
    iDCardNumber: '',
    phoneNumber: '',
    department: '',
    healthContractId: ''
  }
}

let searchTimer = null
function debouncedSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => { page.value = 1; loadPatients() }, 400)
}

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '—'
const formatDateShort = (d) => d ? new Date(d).toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'}) : '—'
const formatDateTime = (d) => d ? new Date(d).toLocaleString('vi-VN') : '—'

const formatStatus = (s) => {
  const map = {
    CREATED: 'Mới tạo', READY: 'Sẵn sàng', CHECKED_IN: 'Đã check-in',
    IN_PROGRESS: 'Đang khám', STATION_DONE: 'Xong trạm',
    QC_PENDING: 'Chờ QC', QC_PASSED: 'QC đạt', COMPLETED: 'Hoàn thành'
  }
  return map[s] || s
}

const getStatusColor = (s) => {
  if (['COMPLETED', 'QC_PASSED'].includes(s)) return 'bg-emerald-400'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'bg-blue-400'
  if (s === 'CHECKED_IN') return 'bg-indigo-400'
  return 'bg-slate-300'
}

const getStatusBadge = (s) => {
  if (['COMPLETED', 'QC_PASSED'].includes(s)) return 'badge-green'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'badge-blue'
  if (s === 'CHECKED_IN') return 'badge-indigo'
  return 'badge-gray'
}

// ─── API Calls ────────────────────────────────────────────────────────────────
const loadContracts = async () => {
  try {
    const res = await apiClient.get('/api/Contracts')
    contracts.value = res.data
  } catch (e) {
    console.warn('Không tải được danh sách HĐ:', e)
  }
}


const loadPatients = async () => {
  loading.value = true
  try {
    const params = new URLSearchParams({
      page: page.value,
      pageSize: pageSize.value
    })
    if (search.value) params.append('search', search.value)
    if (filterContractId.value) params.append('contractId', filterContractId.value)

    const res = await apiClient.get(`/api/Patients?${params}`)
    patients.value = res.data.items
    total.value = res.data.total
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    loading.value = false
  }
}

const viewDetail = async (p) => {
  detailData.value = null
  showDetail.value = true
  try {
    const res = await apiClient.get(`/api/Patients/${p.patientId}`)
    detailData.value = res.data
  } catch (e) {
    toast.error('Không thể tải hồ sơ bệnh nhân')
    showDetail.value = false
  }
}


// ─── Form Actions ─────────────────────────────────────────────────────────────
const openAdd = () => {
  form.value = resetForm()
  editMode.value = false
  formError.value = ''
  showModal.value = true
}

const openEdit = (p) => {
  form.value = {
    patientId: p.patientId,
    fullName: p.fullName,
    gender: p.gender || '',
    dateOfBirth: p.dateOfBirth ? p.dateOfBirth.split('T')[0] : '',
    iDCardNumber: p.iDCardNumber || '',
    phoneNumber: p.phoneNumber || '',
    department: p.department || '',
    healthContractId: p.healthContractId
  }
  editMode.value = true
  formError.value = ''
  showModal.value = true
}

const submitForm = async () => {
  if (!form.value.fullName.trim()) { formError.value = 'Họ tên không được để trống.'; return }
  if (!form.value.healthContractId) { formError.value = 'Vui lòng chọn hợp đồng.'; return }

  saving.value = true
  formError.value = ''
  try {
    const payload = {
      healthContractId: Number(form.value.healthContractId),
      fullName: form.value.fullName.trim(),
      gender: form.value.gender || null,
      dateOfBirth: form.value.dateOfBirth || null,
      iDCardNumber: form.value.iDCardNumber || null,
      phoneNumber: form.value.phoneNumber || null,
      department: form.value.department || null
    }

    if (editMode.value) {
      await apiClient.put(`/api/Patients/${form.value.patientId}`, payload)
      toast.success('Cập nhật thông tin thành công!')
    } else {
      await apiClient.post('/api/Patients', payload)
      toast.success('Thêm bệnh nhân thành công!')
    }

    showModal.value = false
    await loadPatients()
  } catch (e) {
    formError.value = parseApiError(e)
  } finally {
    saving.value = false
  }
}

const confirmDelete = (p) => { deleteTarget.value = p }

const doDelete = async () => {
  if (!deleteTarget.value) return
  saving.value = true
  try {
    await apiClient.delete(`/api/Patients/${deleteTarget.value.patientId}`)
    toast.success('Đã xóa bệnh nhân thành công!')
    deleteTarget.value = null
    await loadPatients()
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    saving.value = false
  }
}

const onFileChange = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    
    // Nếu chưa chọn hợp đồng ở filter thì yêu cầu sếp chọn để biết import vào đâu
    if (!filterContractId.value) {
        toast.warning('Vui lòng chọn Hợp đồng ở mục lọc trước khi nhập Excel để hệ thống biết bệnh nhân thuộc hợp đồng nào.')
        e.target.value = ''
        return
    }

    importing.value = true
    const formData = new FormData()
    formData.append('file', file)

    try {
        const res = await apiClient.post(`/api/Patients/import?contractId=${filterContractId.value}`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        })
        toast.success(res.data.message)
        await loadPatients()
    } catch (err) {
        toast.error(err.response?.data?.message || 'Lỗi khi nhập Excel')
    } finally {
        importing.value = false
        e.target.value = ''
    }
}

const handleExport = async () => {
    try {
        const params = new URLSearchParams()
        if (filterContractId.value) params.append('contractId', filterContractId.value)
        if (search.value) params.append('search', search.value)

        const res = await apiClient.get('/api/Patients/export', {
            params,
            responseType: 'blob'
        })
        
        // Tạo URL tạm thời để trigger download
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `DanhSachBenhNhan_${new Date().toISOString().slice(0,10)}.xlsx`)
        document.body.appendChild(link)
        link.click()
        
        // Dọn dẹp
        link.remove()
        window.URL.revokeObjectURL(url)
        toast.success('Đã xuất file Excel thành công!')
    } catch (e) {
        toast.error(parseApiError(e))
    }
}

const goPage = (p) => {
  if (p < 1 || p > totalPages.value) return
  page.value = p
  loadPatients()
}
</script>

<style scoped>
/* Keeping only locally scoped unique logic if any, but transitioning to global classes */
.patients-page { padding: 0; }
.avatar-circle {
  width: 32px; height: 32px; border-radius: 8px; background: linear-gradient(135deg, var(--primary), #8b5cf6);
  color: white; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 0.75rem; flex-shrink: 0;
}

/* Modal Premium Styles */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
  animation: fadeIn 0.3s ease;
}

.modal-box {
  background: white; border-radius: 1.5rem; overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  width: 95%; max-width: 600px; animation: scaleUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; padding: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: 0.25rem; }
.form-group label { font-size: 9px; font-weight: 800; text-transform: uppercase; color: #94a3b8; letter-spacing: 0.1em; }
.form-input { 
  width: 100%; padding: 0.5rem 0.75rem; border-radius: 0.75rem; border: 1px solid #e2e8f0;
  outline: none; transition: all 0.2s; font-size: 12px; font-weight: 600;
}
.form-input:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1); }

.badge {
  padding: 0.2rem 0.6rem; border-radius: 9999px; font-size: 9px; font-weight: 800;
  text-transform: uppercase; letter-spacing: 0.05em;
}
.badge-blue { background: #eff6ff; color: #3b82f6; }
.badge-pink { background: #fdf2f8; color: #db2777; }
.badge-green { background: #f0fdf4; color: #16a34a; }
.badge-indigo { background: #eef2ff; color: #4f46e5; }
.badge-gray { background: #f8fafc; color: #64748b; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes scaleUp { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>
