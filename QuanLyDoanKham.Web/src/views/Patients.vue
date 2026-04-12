<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in-up pb-12 scrollbar-premium overflow-y-auto font-sans p-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-8 glass-header p-6 rounded-[2rem] shadow-sm">
      <div>
        <h1 class="text-3xl font-bold text-slate-900 tracking-tight italic uppercase">Quản Lý Bệnh Nhân</h1>
        <p class="text-[10px] font-semibold text-slate-400 uppercase tracking-widest mt-1">Lý lịch, hồ sơ chuẩn y khoa và lịch sử thăm khám</p>
      </div>
      <div class="flex gap-3">
        <button @click="handleExport" class="btn-premium secondary">
          <Download class="w-4 h-4" /> Xuất Excel
        </button>
        <button @click="$refs.fileInput.click()" class="btn-premium secondary" :disabled="importing">
          <Upload v-if="!importing" class="w-4 h-4" />
          <RefreshCw v-else class="w-4 h-4 animate-spin" />
          <span>{{ importing ? 'Đang nhập...' : 'Nhập Excel' }}</span>
        </button>
        <input type="file" ref="fileInput" class="hidden" accept=".xlsx, .xls" @change="onFileChange" />
        <button @click="openEnrollModal" class="btn-premium secondary !bg-indigo-50 !text-indigo-600 border-indigo-100">
          <Calendar class="w-4 h-4" /> Đăng ký khám đoàn
        </button>
        <button @click="openAdd" class="btn-premium primary">
          <UserPlus class="w-5 h-5" /> Thêm Bệnh Nhân
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="flex flex-col md:flex-row gap-4 mb-8">
      <div class="relative flex-1 group">
        <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4 group-focus-within:text-primary transition-colors" />
        <input v-model="search" @input="debouncedSearch" type="text"
          placeholder="Tìm tên, CCCD, số điện thoại..." class="w-full pl-12 pr-4 py-3 rounded-2xl bg-white/50 border border-slate-100 focus:bg-white focus:border-primary/20 outline-none font-bold text-sm text-slate-600 shadow-sm transition-all" />
      </div>
      <select v-model="filterContractId" @change="loadPatients" class="px-6 py-3 rounded-2xl bg-white/50 border border-slate-100 font-bold text-sm text-slate-500 outline-none shadow-sm min-w-[240px]">
        <option value="">Tất cả hợp đồng</option>
        <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
          {{ c.contractName }}
        </option>
      </select>
      <button @click="loadPatients" class="w-12 h-12 bg-white rounded-2xl shadow-sm border border-slate-100 text-slate-400 hover:text-primary transition-all flex items-center justify-center">
        <RefreshCw class="w-5 h-5" :class="{ 'animate-spin': loading }" />
      </button>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center shadow-inner">
          <Users class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng bệnh nhân</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ total.toLocaleString() }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-emerald-100 text-emerald-600 rounded-2xl flex items-center justify-center shadow-inner">
          <FileText class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Có hồ sơ</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.hasRecord).length }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-sky-100 text-sky-600 rounded-2xl flex items-center justify-center shadow-inner">
          <CheckCircle class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Khám xong</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.status === 'COMPLETED').length }}</p>
        </div>
      </div>
      <div class="premium-card p-6 flex items-center gap-4">
        <div class="w-12 h-12 bg-amber-100 text-amber-600 rounded-2xl flex items-center justify-center shadow-inner">
          <Clock class="w-6 h-6" />
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Chờ khám</p>
          <p class="text-2xl font-black text-slate-900 tabular-nums">{{ patients.filter(p => p.status !== 'COMPLETED' && p.status !== 'CREATED').length }}</p>
        </div>
      </div>
    </div>

    <!-- Table / Content Area -->
    <div class="premium-card min-h-[400px] flex flex-col relative overflow-hidden">
      <!-- Loading Overlay -->
      <div v-if="loading" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-50 flex flex-col items-center justify-center animate-fade-in">
        <div class="relative">
          <RefreshCw class="w-12 h-12 text-primary animate-spin" />
          <div class="absolute inset-0 blur-xl bg-primary/20 animate-pulse"></div>
        </div>
        <p class="mt-6 text-sm font-black text-slate-800 uppercase tracking-[0.3em] animate-pulse">Đang truy xuất dữ liệu...</p>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && patients.length === 0" class="flex-grow flex flex-col items-center justify-center p-12 text-center animate-scale-up">
        <div class="w-24 h-24 bg-slate-50 rounded-[2.5rem] flex items-center justify-center mb-8 shadow-inner border border-slate-100/50">
          <UserRound class="w-10 h-10 text-slate-300" />
        </div>
        <h3 class="text-xl font-black text-slate-800 uppercase tracking-tight mb-3 italic">Chưa có bệnh nhân nào</h3>
        <p class="text-xs font-bold text-slate-400 uppercase tracking-widest max-w-sm leading-relaxed mb-8">
          Hệ thống chưa ghi nhận dữ liệu bệnh nhân nào phù hợp. <br/> Bấm "Thêm Bệnh Nhân" để bắt đầu nhập liệu ngay.
        </p>
        <button @click="openAdd" class="btn-premium primary shadow-lg shadow-primary/20">
          <UserPlus class="w-4 h-4" /> Thêm bệnh nhân mới
        </button>
      </div>

      <!-- Data Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full border-collapse">
          <thead class="text-left text-[10px] font-semibold uppercase tracking-widest text-slate-500 bg-slate-50 border-b border-slate-100">
            <tr>
              <th class="px-4 py-5 text-center w-16">#</th>
              <th class="px-4 py-5">Họ và Tên</th>
              <th class="px-4 py-5">Giới tính</th>
              <th class="px-4 py-5">Ngày sinh</th>
              <th class="px-4 py-5">CCCD/CMND</th>
              <th class="px-4 py-5">Điện thoại</th>
              <th class="px-4 py-5">Phòng ban</th>
              <th class="px-4 py-5">Hợp đồng</th>
              <th class="px-4 py-5 text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(p, i) in patients" :key="p.patientId" class="group hover:bg-slate-50/50 transition-all duration-300">
              <td class="px-4 py-4 text-center text-slate-400 text-xs">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="avatar-circle">{{ p.fullName?.charAt(0) }}</div>
                  <span class="font-semibold text-slate-800">{{ p.fullName }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['badge inline-block', p.gender === 'Nam' ? 'badge-blue' : p.gender === 'Nữ' ? 'badge-pink' : 'badge-gray']">
                  {{ p.gender || '—' }}
                </span>
              </td>
              <td class="px-4 py-4 text-slate-600 text-xs tabular-nums">{{ formatDate(p.dateOfBirth) }}</td>
              <td class="px-4 py-4 text-slate-600 font-mono text-xs">{{ p.iDCardNumber || '—' }}</td>
              <td class="px-4 py-4 text-slate-600 text-xs tabular-nums">{{ p.phoneNumber || '—' }}</td>
              <td class="px-4 py-4 text-slate-500 text-xs">{{ p.department || '—' }}</td>
              <td class="px-4 py-4 text-[11px] text-indigo-600 font-black line-clamp-2">{{ p.contractName || '—' }}</td>
              <td class="px-4 py-4 text-center">
                <div class="flex items-center justify-center gap-1 group">
                  <button @click="viewDetail(p)" class="p-2.5 rounded-xl bg-blue-50/50 text-blue-500 hover:bg-blue-500 hover:text-white transition-all shadow-sm border border-blue-100 hover:border-blue-500" title="Xem hồ sơ">
                    <Eye class="w-4 h-4" />
                  </button>
                  <button @click="openEnrollModal(p)" class="p-2.5 rounded-xl bg-emerald-50/50 text-emerald-600 hover:bg-emerald-600 hover:text-white transition-all shadow-sm border border-emerald-100 hover:border-emerald-600" title="Ghi danh vào đoàn">
                    <Calendar class="w-4 h-4" />
                  </button>
                  <button @click="openEdit(p)" class="p-2.5 rounded-xl bg-indigo-50/50 text-indigo-500 hover:bg-indigo-500 hover:text-white transition-all shadow-sm border border-indigo-100 hover:border-indigo-500" title="Sửa">
                    <Edit class="w-4 h-4" />
                  </button>
                  <button @click="confirmDelete(p)" class="p-2.5 rounded-xl bg-rose-50/50 text-rose-500 hover:bg-rose-500 hover:text-white transition-all shadow-sm border border-rose-100 hover:border-rose-500" title="Xóa">
                    <Trash2 class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Footer -->
      <div v-if="totalPages > 1" class="p-6 border-t border-slate-100 bg-slate-50/30 flex items-center justify-between">
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none">
          Hiển thị {{ patients.length }} / {{ total }} bệnh nhân
        </p>
        <div class="flex items-center gap-2">
          <button @click="goPage(page - 1)" :disabled="page <= 1" class="w-10 h-10 rounded-xl flex items-center justify-center border border-slate-200 bg-white text-slate-400 hover:text-primary disabled:opacity-30 transition-all">
            <ArrowLeft class="w-4 h-4" />
          </button>
          <div class="px-4 py-2 rounded-xl bg-white border border-slate-200 text-xs font-black text-slate-700 tabular-nums">
            {{ page }} / {{ totalPages }}
          </div>
          <button @click="goPage(page + 1)" :disabled="page >= totalPages" class="w-10 h-10 rounded-xl flex items-center justify-center border border-slate-200 bg-white text-slate-400 hover:text-primary disabled:opacity-30 transition-all">
            <ArrowRight class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- MODAL: Thêm / Sửa Bệnh Nhân -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay flex items-center justify-center" @click.self="showModal = false">
        <div class="modal-box max-w-xl animate-scale-up">
          <div class="modal-header bg-gradient-to-r from-primary to-indigo-600 text-white p-8 rounded-t-[2.5rem]">
            <div class="flex items-center justify-between w-full">
              <div class="flex items-center gap-4">
                <div class="w-14 h-14 bg-white/20 backdrop-blur-md rounded-2xl flex items-center justify-center shadow-inner">
                  <UserPlus v-if="!editMode" class="w-7 h-7" />
                  <Edit v-else class="w-7 h-7" />
                </div>
                <div>
                  <h2 class="text-2xl font-black uppercase tracking-tight italic leading-none mb-1">
                    {{ editMode ? 'Cập Nhật Hồ Sơ' : 'Thêm Bệnh Nhân' }}
                  </h2>
                  <p class="text-xs font-bold text-white/60 uppercase tracking-widest">Khai báo thông tin định danh bệnh nhân</p>
                </div>
              </div>
              <button @click="showModal = false" class="w-10 h-10 rounded-xl bg-black/10 hover:bg-black/20 flex items-center justify-center transition-all">
                <X class="w-5 h-5" />
              </button>
            </div>
          </div>

          <form @submit.prevent="submitForm" class="modal-body">
            <div class="form-grid">
              <!-- Họ tên -->
              <div class="form-group col-span-2">
                <label>Họ và tên <span class="required">*</span></label>
                <input v-model="form.fullName" type="text" required
                  placeholder="Nguyễn Văn A" class="form-input" />
              </div>

              <!-- Giới tính -->
              <div class="form-group">
                <label>Giới tính</label>
                <select v-model="form.gender" class="form-input">
                  <option value="">— Chọn —</option>
                  <option>Nam</option>
                  <option>Nữ</option>
                  <option>Khác</option>
                </select>
              </div>

              <!-- Ngày sinh -->
              <div class="form-group">
                <label>Ngày sinh</label>
                <input v-model="form.dateOfBirth" type="date" class="form-input" />
              </div>

              <!-- CCCD -->
              <div class="form-group">
                <label>CCCD / CMND</label>
                <input v-model="form.iDCardNumber" type="text"
                  placeholder="012345678901" class="form-input" />
              </div>

              <!-- SĐT -->
              <div class="form-group">
                <label>Số điện thoại</label>
                <input v-model="form.phoneNumber" type="tel"
                  placeholder="0901234567" class="form-input" />
              </div>

              <!-- Phòng ban -->
              <div class="form-group col-span-2">
                <label>Phòng ban / Đơn vị</label>
                <input v-model="form.department" type="text"
                  placeholder="Phòng Kế toán, Ban Giám đốc..." class="form-input" />
              </div>

              <!-- Hợp đồng -->
              <div class="form-group col-span-2">
                <label>Thuộc Hợp Đồng <span class="required">*</span></label>
                <select v-model="form.healthContractId" required class="form-input">
                  <option value="">— Chọn hợp đồng —</option>
                  <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                    {{ c.contractName }} ({{ c.companyName }})
                  </option>
                </select>
              </div>
            </div>

            <div v-if="formError" class="flex items-center gap-3 p-4 bg-rose-50 border border-rose-100 rounded-2xl mb-6 animate-shake">
              <AlertCircle class="w-5 h-5 text-rose-500" />
              <p class="text-xs font-bold text-rose-600 uppercase tracking-widest">{{ formError }}</p>
            </div>

            <div class="flex items-center justify-end gap-3 p-8 border-t border-slate-100 bg-slate-50/50 rounded-b-[2.5rem]">
              <button type="button" @click="showModal = false" class="px-8 py-3 text-slate-400 font-black text-xs uppercase tracking-widest hover:text-slate-600 transition-all">Hủy bỏ</button>
              <button type="submit" :disabled="saving" class="btn-premium primary px-10">
                <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
                <Save v-else class="w-4 h-4" />
                <span>{{ editMode ? 'Lưu thay đổi' : 'Thêm bệnh nhân' }}</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- MODAL: Chi tiết / Hồ sơ Bệnh Nhân -->
    <Teleport to="body">
      <div v-if="showDetail && detailData" class="modal-overlay flex items-center justify-center p-6 z-[100]" @click.self="showDetail = false">
        <div class="modal-box max-w-2xl w-full animate-scale-up overflow-hidden">
          <div class="modal-header bg-gradient-to-r from-indigo-600 to-violet-600 text-white p-10 rounded-t-[2.5rem] relative">
            <div class="absolute inset-0 bg-[url('https://www.transparenttextures.com/patterns/carbon-fibre.png')] opacity-10"></div>
            <div class="flex items-center justify-between w-full relative z-10">
              <div class="flex items-center gap-6">
                <div class="w-20 h-20 bg-white/20 backdrop-blur-md rounded-[2rem] flex items-center justify-center text-3xl font-black shadow-inner border border-white/30">
                  {{ detailData.fullName?.charAt(0) }}
                </div>
                <div>
                  <h2 class="text-3xl font-black italic uppercase tracking-tighter leading-none mb-2">{{ detailData.fullName }}</h2>
                  <div class="flex items-center gap-3">
                    <span class="px-3 py-1 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-widest">{{ detailData.gender }}</span>
                    <div class="w-1 h-1 rounded-full bg-white/40"></div>
                    <span class="text-indigo-100 text-xs font-bold">{{ formatDate(detailData.dateOfBirth) }}</span>
                  </div>
                </div>
              </div>
              <button @click="showDetail = false" class="w-12 h-12 rounded-2xl bg-black/10 hover:bg-black/20 flex items-center justify-center transition-all">
                <X class="w-6 h-6" />
              </button>
            </div>
          </div>

          <div class="p-8">
            <!-- Thông tin cơ bản -->
            <div class="grid grid-cols-2 gap-4 mb-10">
              <div class="p-6 rounded-[2rem] bg-slate-50 border border-slate-100 group hover:border-indigo-200 transition-all">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-2">
                  <Hash class="w-3 h-3 text-indigo-400" /> CCCD/CMND
                </p>
                <p class="font-black text-slate-800 tabular-nums">{{ detailData.iDCardNumber || '—' }}</p>
              </div>
              <div class="p-6 rounded-[2rem] bg-slate-50 border border-slate-100 group hover:border-indigo-200 transition-all">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-2">
                  <Phone class="w-3 h-3 text-indigo-400" /> Điện thoại
                </p>
                <p class="font-black text-slate-800 tabular-nums">{{ detailData.phoneNumber || '—' }}</p>
              </div>
              <div class="p-6 rounded-[2rem] bg-slate-50 border border-slate-100 group hover:border-indigo-200 transition-all">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1 flex items-center gap-2">
                  <Building2 class="w-3 h-3 text-indigo-400" /> Phòng ban
                </p>
                <p class="font-bold text-slate-700">{{ detailData.department || '—' }}</p>
              </div>
              <div class="p-6 rounded-[2rem] bg-indigo-50/30 border border-indigo-100 group hover:border-indigo-300 transition-all">
                <p class="text-[9px] font-black text-indigo-400 uppercase tracking-widest mb-1 flex items-center gap-2">
                  <FileText class="w-3 h-3 text-indigo-400" /> Hợp đồng
                </p>
                <p class="font-black text-indigo-600 text-sm whitespace-nowrap overflow-hidden text-ellipsis">{{ detailData.contractName || '—' }}</p>
              </div>
            </div>

            <!-- Lịch sử khám -->
            <div class="flex flex-col flex-grow">
              <div class="flex items-center justify-between mb-6">
                <h3 class="text-sm font-black text-slate-800 uppercase tracking-widest flex items-center gap-3 italic">
                  <History class="w-5 h-5 text-indigo-500" /> Lịch Sử Khám
                </h3>
                <span class="px-3 py-1 bg-slate-100 rounded-full text-[10px] font-black text-slate-500 tabular-nums">
                  {{ (detailData.medicalHistory || []).length }} PHIÊN KHÁM
                </span>
              </div>

              <div v-if="!detailData.medicalHistory?.length" class="flex flex-col items-center justify-center py-16 bg-slate-50 rounded-[2rem] border-2 border-dashed border-slate-200 opacity-60">
                <Inbox class="w-12 h-12 text-slate-300 mb-4" />
                <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Chưa có lịch sử khám nào</p>
              </div>

              <div v-else class="history-list">
                <div v-for="h in detailData.medicalHistory" :key="h.medicalRecordId"
                  class="history-item">
                  <div class="history-dot" :class="getStatusColor(h.status)"></div>
                  <div class="flex-1">
                    <div class="flex justify-between items-start">
                      <p class="font-semibold text-slate-800">{{ h.groupName || 'Đoàn khám' }}</p>
                      <span :class="getStatusBadge(h.status)" class="badge text-xs">
                        {{ formatStatus(h.status) }}
                      </span>
                    </div>
                    <p class="text-xs text-slate-400 mt-1">
                      <i class="fas fa-calendar-alt mr-1"></i>
                      {{ formatDate(h.examDate) }}
                      <span v-if="h.checkInAt" class="ml-3">
                        <i class="fas fa-sign-in-alt mr-1"></i> Check-in: {{ formatDateTime(h.checkInAt) }}
                      </span>
                    </p>
                    <div class="mt-1.5">
                      <div class="progress-bar-wrap">
                        <div class="progress-bar" :style="`width: ${h.tasksDone && h.tasksTotal ? (h.tasksDone/h.tasksTotal*100) : 0}%`"></div>
                      </div>
                      <p class="text-xs text-slate-400 mt-0.5">Trạm: {{ h.tasksDone || 0 }}/{{ h.tasksTotal || 0 }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="modal-footer border-t px-8 pb-8 flex justify-end gap-3 pt-6">
            <button @click="openEdit(detailData); showDetail = false" class="btn-premium secondary">
              <Edit class="w-4 h-4" /> Sửa thông tin
            </button>
            <button @click="showDetail = false" class="btn-premium primary">Đóng</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete Confirm Modal -->
    <Teleport to="body">
      <div v-if="deleteTarget" class="modal-overlay" @click.self="deleteTarget = null">
        <div class="modal-box max-w-md">
          <div class="p-8 text-center">
            <div class="w-20 h-20 bg-rose-100 text-rose-500 rounded-[2rem] flex items-center justify-center mx-auto mb-6 shadow-inner">
              <Trash2 class="w-10 h-10" />
            </div>
            <h3 class="text-2xl font-black text-slate-800 italic uppercase tracking-tight mb-2">Xác nhận xóa</h3>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-8 leading-relaxed">
              Bạn chắc chắn muốn xóa bệnh nhân <span class="text-rose-500 underline">{{ deleteTarget.fullName }}</span>? <br/>
              Hành động này không thể khôi phục.
            </p>
            <div class="flex gap-3 justify-center">
              <button @click="deleteTarget = null" class="btn-premium secondary">Hủy</button>
              <button @click="doDelete" :disabled="saving" class="btn-premium primary !bg-rose-500 !shadow-rose-200">
                <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
                <Trash2 v-else class="w-4 h-4" />
                <span>Xóa vĩnh viễn</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- ENROLL MODAL -->
    <Teleport to="body">
      <div v-if="showEnrollModal" class="modal-overlay flex items-center justify-center p-6 z-[120]" @click.self="showEnrollModal = false">
        <div class="modal-box max-w-lg w-full animate-scale-up">
          <div class="modal-header bg-gradient-to-r from-emerald-600 to-teal-600 text-white p-8 rounded-t-[2.5rem]">
            <div class="flex items-center gap-4">
              <div class="w-14 h-14 bg-white/20 backdrop-blur-md rounded-2xl flex items-center justify-center shadow-inner">
                <Calendar class="w-7 h-7" />
              </div>
              <div>
                <h2 class="text-xl font-black uppercase tracking-tight leading-none mb-1">Ghi Danh Khám Đoàn</h2>
                <p class="text-[10px] font-bold text-white/60 uppercase tracking-widest">Đăng ký bệnh nhân vào một đợt khám tập trung</p>
              </div>
            </div>
          </div>

          <div class="p-8">
            <div class="mb-6 p-5 bg-emerald-50 rounded-2xl border border-emerald-100 flex items-center gap-4" v-if="enrollTarget">
                <div class="avatar-circle">{{ enrollTarget.fullName?.charAt(0) }}</div>
                <div>
                   <p class="text-[10px] font-black text-emerald-600 uppercase tracking-widest">Bệnh nhân đang chọn</p>
                   <p class="text-lg font-black text-slate-800">{{ enrollTarget.fullName }}</p>
                </div>
            </div>

            <div v-else class="mb-6">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2 block">1. Tìm bệnh nhân (Chưa bắt buộc)</label>
                <div class="relative">
                    <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                    <input type="text" v-model="enrollSearch" @input="searchEnrollTarget" placeholder="Tìm theo tên hoặc CCCD..." class="form-input !pl-12" />
                </div>
                <!-- Search Result Dropdown (Simple) -->
                <div v-if="enrollSearchResults.length > 0" class="mt-2 bg-white border border-slate-100 rounded-xl shadow-lg max-h-40 overflow-y-auto">
                    <div v-for="p in enrollSearchResults" :key="p.patientId" @click="enrollTarget = p; enrollSearchResults = []" 
                        class="p-3 hover:bg-slate-50 cursor-pointer flex items-center gap-3 border-b border-slate-50 last:border-0">
                        <div class="w-8 h-8 bg-indigo-50 text-indigo-600 rounded-lg flex items-center justify-center text-[10px] font-black">{{ p.fullName?.charAt(0) }}</div>
                        <div>
                            <p class="text-xs font-black text-slate-800">{{ p.fullName }}</p>
                            <p class="text-[9px] text-slate-400 font-bold uppercase tracking-widest">{{ p.iDCardNumber || '—' }}</p>
                        </div>
                    </div>
                </div>
            </div>

            <div>
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2 block">2. Chọn Đoàn Khám Đang Mở</label>
              <select v-model="selectedEnrollGroupId" class="form-input !py-4 !text-base">
                <option value="">— Chọn đoàn khám mục tiêu —</option>
                <option v-for="g in openGroups" :key="g.groupId" :value="g.groupId">
                  [{{ formatDateShort(g.examDate) }}] {{ g.groupName }}
                </option>
              </select>
              <p v-if="openGroups.length === 0" class="text-[10px] font-bold text-rose-500 uppercase mt-2 italic">Hiện không có đoàn khám nào đang mở.</p>
            </div>
          </div>

          <div class="p-8 border-t bg-slate-50/50 rounded-b-[2.5rem] flex gap-3">
             <button @click="showEnrollModal = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest">Hủy bỏ</button>
             <button @click="submitEnroll" :disabled="!selectedEnrollGroupId || !enrollTarget || enrollLoading" 
                     class="flex-[2] btn-premium primary !bg-emerald-600 !shadow-emerald-100">
                <Loader2 v-if="enrollLoading" class="w-4 h-4 animate-spin" />
                <span v-else>XÁC NHẬN GHI DANH</span>
             </button>
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

// Enroll State
const showEnrollModal = ref(false)
const enrollTarget = ref(null)
const selectedEnrollGroupId = ref('')
const openGroups = ref([])
const enrollLoading = ref(false)
const enrollSearch = ref('')
const enrollSearchResults = ref([])

const form = ref(resetForm())

// ─── Computed ─────────────────────────────────────────────────────────────────
const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
const approvedContracts = computed(() => {
  // Cho phép hiện tất cả hợp đồng có tên, hoặc cho phép cả hợp đồng chưa đặt tên 
  // để sếp có thể nhập liệu bệnh nhân ngay lập tức mà không bị chặn.
  return contracts.value.filter(c => 
    c.status !== 'REJECTED' // Chỉ lọc bỏ các hợp đồng đã bị từ chối hẳn
  )
})

// ─── Init ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadContracts(), loadPatients(), loadOpenGroups()])
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
    QC_PENDING: 'Chờ QC', QC_PASSED: 'QC đạt', REPORTED: 'Đã báo cáo', CLOSED: 'Đóng'
  }
  return map[s] || s
}

const getStatusColor = (s) => {
  if (['CLOSED', 'REPORTED', 'QC_PASSED'].includes(s)) return 'bg-emerald-400'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'bg-blue-400'
  if (s === 'CHECKED_IN') return 'bg-indigo-400'
  return 'bg-slate-300'
}

const getStatusBadge = (s) => {
  if (['CLOSED', 'REPORTED', 'QC_PASSED'].includes(s)) return 'badge-green'
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

const loadOpenGroups = async () => {
    try {
        const res = await apiClient.get('/api/MedicalGroups')
        openGroups.value = res.data.filter(g => g.status === 'Open')
    } catch (e) { console.error('Lỗi tải đoàn khám:', e) }
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
    console.error('Lỗi tải danh sách bệnh nhân:', e.response?.data || e.message)
    toast.error('Không thể tải danh sách bệnh nhân. Vui lòng kiểm tra quyền truy cập.')
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

// ─── Enroll Actions ───────────────────────────────────────────────────────────
const openEnrollModal = (p = null) => {
    enrollTarget.value = p
    enrollSearch.value = ''
    enrollSearchResults.value = []
    selectedEnrollGroupId.value = ''
    showEnrollModal.value = true
}

const searchEnrollTarget = async () => {
    if (enrollSearch.value.length < 2) {
        enrollSearchResults.value = []
        return
    }
    try {
        const res = await apiClient.get(`/api/Patients?search=${enrollSearch.value}&pageSize=5`)
        enrollSearchResults.value = res.data.items || []
    } catch (e) { console.error(e) }
}

const submitEnroll = async () => {
    if (!enrollTarget.value || !selectedEnrollGroupId.value) return
    enrollLoading.value = true
    try {
        const p = enrollTarget.value
        const payload = {
            groupId: Number(selectedEnrollGroupId.value),
            records: [{
                fullName: p.fullName,
                iDCardNumber: p.iDCardNumber,
                gender: p.gender,
                dateOfBirth: p.dateOfBirth,
                department: p.department
            }]
        }
        await apiClient.post('/api/MedicalRecords/batch-ingest', payload)
        toast.success(`Đã ghi danh ${p.fullName} vào đoàn khám thành công!`)
        showEnrollModal.value = false
        // Nếu đang mở trang detail thì load lại để thấy history mới
        if (showDetail.value && detailData.value?.patientId === p.patientId) {
            viewDetail(p)
        }
    } catch (e) {
        toast.error(e.response?.data?.message || 'Có lỗi khi ghi danh.')
    } finally {
        enrollLoading.value = false
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
    formError.value = e.response?.data?.message || 'Có lỗi xảy ra. Vui lòng thử lại.'
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
    toast.error(e.response?.data?.message || 'Không thể xóa bệnh nhân này.')
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
        console.error('Lỗi xuất Excel:', e)
        toast.error('Không thể xuất Excel. Vui lòng thử lại sau.')
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
  width: 36px; height: 36px; border-radius: 12px; background: linear-gradient(135deg, var(--primary), #8b5cf6);
  color: white; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 0.85rem; flex-shrink: 0;
}

/* Modal Premium Styles */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
  animation: fadeIn 0.3s ease;
}

.modal-box {
  background: white; border-radius: 2.5rem; overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  width: 95%; max-width: 600px; animation: scaleUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.5rem; padding: 2rem; }
.form-group { display: flex; flex-col: column; gap: 0.5rem; }
.form-group label { font-size: 10px; font-weight: 800; text-transform: uppercase; color: #94a3b8; letter-spacing: 0.1em; }
.form-input { 
  width: 100%; padding: 0.75rem 1rem; border-radius: 1rem; border: 1px solid #e2e8f0;
  outline: none; transition: all 0.2s; font-size: 13px; font-weight: 600;
}
.form-input:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1); }

.badge {
  padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 10px; font-weight: 800;
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
