<template>
  <div class="h-full flex flex-col dashboard-gradient relative animate-fade-in pb-4 p-3 scrollbar-premium overflow-y-auto font-sans">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-2 mb-3">
      <div>
        <h2 class="text-base font-bold text-slate-800 flex items-center gap-2">
          <div class="w-7 h-7 bg-primary text-white rounded-lg flex items-center justify-center shadow-md">
            <FileText class="w-4 h-4" />
          </div>
          Theo Dõi Bệnh Án
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[7.5px] mt-0.5">Tiến độ khám sức khỏe theo hợp đồng &amp; đoàn khám</p>
      </div>
      <div class="flex gap-2">
        <button @click="handleExport" class="btn-premium secondary h-7.5 !rounded-lg !px-2.5 shadow-sm border border-white/40 text-[8px] font-black uppercase">
          <Download class="w-3 h-3" /> XUẤT EXCEL
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="flex flex-col md:flex-row gap-2 mb-2.5">
      <div class="relative flex-1 group">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-300 w-3 h-3" />
        <input v-model="search" @input="debouncedSearch" type="text"
          placeholder="Tìm tên, CCCD, số định danh..." class="w-full pl-9 pr-3 py-1.5 rounded-lg bg-white/50 border border-slate-100 focus:bg-white focus:border-primary/20 outline-none font-bold text-[11px] text-slate-600 shadow-sm transition-all" />
      </div>
      <select v-model="filterContractId" @change="onContractChange" class="px-3 py-1.5 rounded-lg bg-white/50 border border-slate-100 font-bold text-[11px] text-slate-500 outline-none shadow-sm min-w-[160px]">
        <option value="">Tất cả hợp đồng</option>
        <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
          {{ c.contractName }} (Mã: {{ c.healthContractId }})
        </option>
      </select>
      <select v-model="filterGroupId" @change="loadRecords" class="px-3 py-1.5 rounded-lg bg-white/50 border border-slate-100 font-bold text-[11px] text-slate-500 outline-none shadow-sm min-w-[160px]" :disabled="!filterContractId">
        <option value="">Tất cả đoàn khám</option>
        <option v-for="g in groups" :key="g.groupId" :value="g.groupId">
          {{ g.groupName }}
        </option>
      </select>
      <button @click="loadRecords" class="w-7.5 h-7.5 bg-white rounded-lg shadow-sm border border-slate-100 text-slate-400 hover:text-primary transition-all flex items-center justify-center">
        <RefreshCw class="w-3 h-3" :class="{ 'animate-spin': loading }" />
      </button>
    </div>

    <!-- Contract Progress Tracking -->
    <div v-if="selectedContract" class="premium-card p-3 mb-3">
      <div class="flex items-center justify-between mb-2">
        <div class="flex items-center gap-2">
          <div class="w-6 h-6 bg-indigo-100 text-indigo-600 rounded-lg flex items-center justify-center">
            <Hash class="w-3.5 h-3.5" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-800 uppercase tracking-tight">{{ selectedContract.contractName }}</p>
            <p class="text-[8px] font-bold text-slate-400 uppercase">{{ selectedContract.companyName || selectedContract.shortName || '' }}</p>
          </div>
        </div>
        <div class="text-right">
          <p class="text-[18px] font-black text-slate-900 tabular-nums leading-none">
            {{ total }} <span class="text-[10px] font-bold text-slate-400">/ {{ selectedContract.expectedQuantity || '?' }}</span>
          </p>
          <p class="text-[7px] font-black uppercase tracking-widest" :class="progressPercent >= 100 ? 'text-emerald-600' : progressPercent >= 50 ? 'text-sky-600' : 'text-amber-600'">
            {{ progressPercent }}% đã có bệnh án
          </p>
        </div>
      </div>
      <div class="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
        <div class="h-full rounded-full transition-all duration-700 ease-out"
          :class="progressPercent >= 100 ? 'bg-emerald-500' : progressPercent >= 50 ? 'bg-sky-500' : 'bg-amber-500'"
          :style="{ width: Math.min(progressPercent, 100) + '%' }"
        ></div>
      </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-2 mb-3">
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-primary/10 text-primary rounded-lg flex items-center justify-center shadow-inner">
          <Users class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Tổng bệnh án</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ total.toLocaleString() }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-emerald-100 text-emerald-600 rounded-lg flex items-center justify-center shadow-inner">
          <CheckCircle class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Hoàn thành</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ stats.completed }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-sky-100 text-sky-600 rounded-lg flex items-center justify-center shadow-inner">
          <Clock class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Đang khám</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ stats.inProgress }}</p>
        </div>
      </div>
      <div class="premium-card p-2 flex items-center gap-2.5">
        <div class="w-7 h-7 bg-amber-100 text-amber-600 rounded-lg flex items-center justify-center shadow-inner">
          <AlertCircle class="w-3.5 h-3.5" />
        </div>
        <div>
          <p class="text-[7px] font-black text-slate-400 uppercase tracking-widest">Chờ khám</p>
          <p class="text-[14px] font-black text-slate-900 tabular-nums">{{ stats.created }}</p>
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
      <div v-if="!loading && records.length === 0" class="flex-grow flex flex-col items-center justify-center p-8 text-center animate-scale-up">
        <div class="w-16 h-16 bg-slate-50 rounded-[1.5rem] flex items-center justify-center mb-4 shadow-inner border border-slate-100/50">
          <FileText class="w-8 h-8 text-slate-300" />
        </div>
        <h3 class="text-base font-black text-slate-800 uppercase tracking-tight mb-2 italic">Chưa có bệnh án nào</h3>
        <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest max-w-sm leading-relaxed mb-2">
          Bệnh án được tạo tự động khi import danh sách vào Đoàn khám <br/> hoặc khi bệnh nhân check-in tại phòng khám.
        </p>
      </div>

      <!-- Data Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full border-collapse">
          <thead class="text-left text-[7.5px] font-black uppercase tracking-widest text-slate-400 bg-slate-50/50 border-b border-slate-100">
            <tr>
              <th class="px-2 py-2 text-center w-8">#</th>
              <th class="px-2 py-2">Bệnh án</th>
              <th class="px-2 py-2">GT</th>
              <th class="px-2 py-2">Ngày sinh</th>
              <th class="px-2 py-2">CCCD/CMND</th>
              <th class="px-2 py-2">Đơn vị</th>
              <th class="px-2 py-2">Trạng thái</th>
              <th class="px-2 py-2">Đoàn khám</th>
              <th class="px-2 py-2 text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="(r, i) in records" :key="r.medicalRecordId" class="group hover:bg-slate-50/50 transition-all duration-300">
              <td class="px-2 py-1 text-center text-slate-400 text-[9px] font-bold">{{ (page - 1) * pageSize + i + 1 }}</td>
              <td class="px-2 py-1">
                <div class="flex items-center gap-1.5">
                  <div class="avatar-circle w-5.5 h-5.5 text-[7px] font-black">{{ r.fullName?.charAt(0) }}</div>
                  <span class="font-bold text-slate-800 text-[10.5px] uppercase tracking-tight">{{ r.fullName }}</span>
                </div>
              </td>
              <td class="px-2 py-1">
                <span :class="['badge inline-block text-[7px] font-black uppercase px-1 py-0.5', r.gender === 'Nam' ? 'badge-blue' : r.gender === 'Nữ' ? 'badge-pink' : 'badge-gray']">
                  {{ r.gender === 'Nam' ? 'M' : r.gender === 'Nữ' ? 'F' : 'O' }}
                </span>
              </td>
              <td class="px-2 py-1 text-slate-600 text-[10px] tabular-nums font-medium">{{ formatDate(r.dateOfBirth) }}</td>
              <td class="px-2 py-1 text-slate-600 font-mono text-[9.5px]">{{ r.idCardNumber || '—' }}</td>
              <td class="px-2 py-1 text-slate-500 text-[9.5px] truncate max-w-[120px]">{{ r.department || '—' }}</td>
              <td class="px-2 py-1">
                <span :class="['badge', getStatusBadge(r.status)]">{{ formatStatus(r.status) }}</span>
              </td>
              <td class="px-2 py-1 text-[9px] text-indigo-600 font-black truncate max-w-[140px] uppercase tracking-tighter">
                {{ r.groupName || '—' }}
                <p class="text-[7px] text-slate-400 font-bold tracking-normal">{{ r.contractName }}</p>
              </td>
              <td class="px-2 py-1 text-center">
                <div class="flex items-center justify-center gap-1">
                  <button @click="viewDetail(r)" class="p-1 rounded-lg bg-blue-50 text-blue-500 hover:bg-blue-500 hover:text-white transition-all shadow-sm border border-blue-100" title="Xem hồ sơ">
                    <Eye class="w-3 h-3" />
                  </button>
                  <button @click="openEdit(r)" class="p-1 rounded-lg bg-indigo-50 text-indigo-500 hover:bg-indigo-500 hover:text-white transition-all shadow-sm border border-indigo-100" title="Sửa">
                    <Edit class="w-3 h-3" />
                  </button>
                  <button @click="confirmDelete(r)" class="p-1 rounded-lg bg-rose-50 text-rose-500 hover:bg-rose-500 hover:text-white transition-all shadow-sm border border-rose-100" title="Xóa">
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
          Hiển thị {{ records.length }} / {{ total }} bệnh án
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

    <!-- Modals -->
    <Teleport to="body">
      <!-- Edit Modal (only for editing existing records) -->
      <div v-if="showModal && editMode" class="modal-overlay">
        <div class="modal-box max-w-lg">
          <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-slate-50/30">
            <h3 class="text-sm font-black text-slate-800 uppercase tracking-tight flex items-center gap-2">
              <div class="w-6 h-6 rounded-lg flex items-center justify-center bg-amber-100 text-amber-600">
                <Edit class="w-3.5 h-3.5" />
              </div>
              CẬP NHẬT BỆNH ÁN
            </h3>
            <button @click="showModal = false" class="text-slate-300 hover:text-rose-500 transition-colors">
              <X class="w-5 h-5" />
            </button>
          </div>
          
          <div class="form-grid">
            <div class="form-group col-span-2" style="grid-column: span 2;">
              <label>Họ và tên <span class="text-rose-500">*</span></label>
              <input v-model="form.fullName" type="text" class="form-input" placeholder="Nhập họ tên đầy đủ..." />
            </div>
            <div class="form-group">
              <label>Giới tính</label>
              <select v-model="form.gender" class="form-input">
                <option value="">Chọn giới tính</option>
                <option value="Nam">Nam</option>
                <option value="Nữ">Nữ</option>
                <option value="Khác">Khác</option>
              </select>
            </div>
            <div class="form-group">
              <label>Ngày sinh</label>
              <input v-model="form.dateOfBirth" type="date" class="form-input" />
            </div>
            <div class="form-group">
              <label>CCCD / CMND</label>
              <input v-model="form.iDCardNumber" type="text" class="form-input" placeholder="Nhập số định danh..." />
            </div>
            <div class="form-group">
              <label>Phòng ban / Tổ</label>
              <input v-model="form.department" type="text" class="form-input" placeholder="Nhập phòng ban..." />
            </div>
          </div>

          <div v-if="formError" class="px-5 py-2 mx-5 bg-rose-50 border border-rose-100 rounded-xl flex items-center gap-2 text-rose-500 text-[10px] font-bold italic">
            <AlertCircle class="w-3.5 h-3.5" /> {{ formError }}
          </div>
          
          <div class="p-4 flex gap-3">
            <button @click="showModal = false" class="flex-1 h-10 rounded-xl text-[11px] font-black uppercase text-slate-400 hover:bg-slate-50 transition-colors">
              Hủy bỏ
            </button>
            <button @click="submitForm" :disabled="saving" class="flex-1 btn-premium primary h-10 shadow-lg shadow-primary/20">
              <Save v-if="!saving" class="w-4 h-4" />
              <RefreshCw v-else class="w-4 h-4 animate-spin" />
              LƯU THAY ĐỔI
            </button>
          </div>
        </div>
      </div>

      <!-- Detail Modal -->
      <div v-if="showDetail" class="modal-overlay">
        <div class="modal-box max-w-2xl">
          <div class="p-4 border-b border-slate-50 flex justify-between items-center bg-primary/5">
            <h3 class="text-sm font-black text-slate-800 uppercase tracking-tight flex items-center gap-2">
              <div class="w-6 h-6 bg-primary text-white rounded-lg flex items-center justify-center">
                <Eye class="w-3.5 h-3.5" />
              </div>
              CHI TIẾT BỆNH ÁN
            </h3>
            <button @click="showDetail = false" class="text-slate-300 hover:text-rose-500 transition-colors">
              <X class="w-5 h-5" />
            </button>
          </div>
          <div v-if="!detailData" class="p-20 flex flex-col items-center justify-center">
            <RefreshCw class="w-8 h-8 text-primary/20 animate-spin" />
          </div>
          <div v-else class="p-6">
            <div class="grid grid-cols-3 gap-6 mb-8">
              <div class="col-span-1 flex flex-col items-center gap-3">
                <div class="w-24 h-24 rounded-3xl bg-primary/10 flex items-center justify-center text-4xl font-black text-primary border-4 border-white shadow-xl shadow-primary/10">
                  {{ detailData.fullName?.charAt(0) }}
                </div>
                <span :class="['badge', getStatusBadge(detailData.status)]">{{ formatStatus(detailData.status) }}</span>
              </div>
              <div class="col-span-2 space-y-4">
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest block mb-1">Họ và Tên</label>
                    <p class="text-sm font-black text-slate-800 uppercase italic">{{ detailData.fullName }}</p>
                  </div>
                  <div>
                    <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest block mb-1">CCCD / CMND</label>
                    <p class="text-sm font-black text-slate-600 tabular-nums">{{ detailData.idCardNumber || '—' }}</p>
                  </div>
                  <div>
                    <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest block mb-1">Giới tính / Ngày sinh</label>
                    <p class="text-[11px] font-bold text-slate-600 uppercase">{{ detailData.gender }} • {{ formatDate(detailData.dateOfBirth) }}</p>
                  </div>
                  <div>
                    <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest block mb-1">Phòng ban</label>
                    <p class="text-[11px] font-bold text-slate-600 uppercase">{{ detailData.department || '—' }}</p>
                  </div>
                </div>
                <div class="p-3 bg-slate-50 rounded-2xl border border-slate-100">
                  <div class="flex items-center gap-2 mb-2">
                    <Building2 class="w-3.5 h-3.5 text-primary" />
                    <span class="text-[9px] font-black text-slate-800 uppercase tracking-widest">Thông tin đoàn khám</span>
                  </div>
                  <p class="text-[11px] font-black text-slate-700 uppercase italic">{{ detailData.groupName }}</p>
                  <p class="text-[9px] font-bold text-slate-400 uppercase tracking-tight">{{ detailData.contractName }}</p>
                </div>
              </div>
            </div>

            <!-- Medical History -->
            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <History class="w-4 h-4 text-slate-400" />
                <h4 class="text-[10px] font-black text-slate-800 uppercase tracking-widest">Diễn biến hồ sơ</h4>
              </div>
              <div v-if="detailData.medicalHistory && detailData.medicalHistory.length > 0" class="space-y-2">
                <div v-for="h in detailData.medicalHistory" :key="h.medicalRecordId" class="p-3 rounded-xl border border-slate-100 flex justify-between items-center">
                  <div>
                    <p class="text-[10px] font-black text-slate-700 uppercase">{{ h.groupName }}</p>
                    <p class="text-[8px] font-bold text-slate-400">{{ formatDate(h.examDate) }}</p>
                  </div>
                  <span :class="['badge', getStatusBadge(h.status)]">{{ formatStatus(h.status) }}</span>
                </div>
              </div>
              <div v-else class="p-8 bg-slate-50 rounded-xl text-center">
                <p class="text-[9px] font-bold text-slate-400 uppercase italic">Chưa có lịch sử khám khác</p>
              </div>
            </div>
          </div>
          <div class="p-4 bg-slate-50/50 flex justify-end gap-2">
            <button @click="showDetail = false" class="px-6 h-9 rounded-xl text-[10px] font-black uppercase text-slate-400 hover:bg-slate-100 transition-colors">Đóng</button>
            <button v-if="detailData && (detailData.status === 'COMPLETED' || detailData.status === 'QC_PASSED')" @click="handleExportPdf(detailData.medicalRecordId)" class="btn-premium primary h-9 px-4">
              <Download class="w-3.5 h-3.5" /> XUẤT KẾT QUẢ PDF
            </button>
          </div>
        </div>
      </div>

      <!-- Delete Confirm -->
      <div v-if="deleteTarget" class="modal-overlay">
        <div class="modal-box max-w-sm p-6 text-center">
          <div class="w-16 h-16 bg-rose-50 text-rose-500 rounded-3xl flex items-center justify-center mx-auto mb-4 border-2 border-rose-100 shadow-lg shadow-rose-500/10">
            <Trash2 class="w-8 h-8" />
          </div>
          <h3 class="text-base font-black text-slate-800 uppercase italic mb-2">XÁC NHẬN XÓA?</h3>
          <p class="text-[11px] font-bold text-slate-400 leading-relaxed mb-8">
            Bạn đang yêu cầu xóa bệnh án của <span class="text-slate-900 font-black uppercase">{{ deleteTarget.fullName }}</span>.<br/> Hành động này không thể hoàn tác.
          </p>
          <div class="flex gap-3">
            <button @click="deleteTarget = null" class="flex-1 h-11 rounded-2xl text-[11px] font-black uppercase text-slate-400 hover:bg-slate-50 transition-all">Hủy</button>
            <button @click="doDelete" :disabled="saving" class="flex-1 h-11 rounded-2xl bg-rose-500 text-white font-black text-[11px] uppercase tracking-widest shadow-lg shadow-rose-500/20 hover:bg-rose-600 transition-all flex items-center justify-center gap-2">
              <RefreshCw v-if="saving" class="w-4 h-4 animate-spin" />
              XÁC NHẬN XÓA
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, RefreshCw, UserRound, ArrowLeft, ArrowRight, Eye, Edit, Trash2, X, AlertCircle, Users, UserPlus, Save, History, Download, CheckCircle, Clock, Hash, Building2, FileText } from 'lucide-vue-next'
import apiClient from '@/services/apiClient'
import { useToast } from '@/composables/useToast'
import { parseApiError } from '@/services/errorHelper'

const toast = useToast()

// ─── State ────────────────────────────────────────────────────────────────────
const loading = ref(false)
const saving = ref(false)
const records = ref([])
const contracts = ref([])
const groups = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(30)
const search = ref('')
const filterContractId = ref('')
const filterGroupId = ref('')

const stats = ref({
  completed: 0,
  inProgress: 0,
  created: 0
})

const showModal = ref(false)
const showDetail = ref(false)
const editMode = ref(false)
const detailData = ref(null)
const deleteTarget = ref(null)
const formError = ref('')
const modalGroups = ref([])

const form = ref(resetForm())

// ─── Computed ─────────────────────────────────────────────────────────────────
const totalPages = computed(() => Math.ceil(total.value / pageSize.value))
// Hiển thị các hợp đồng đang hoạt động (Active) để theo dõi tiến độ
const approvedContracts = computed(() => {
  return contracts.value
    .filter(c => c.status === 'Active')
    .sort((a, b) => a.contractName.localeCompare(b.contractName))
})

// Modal chỉ hiện HĐ đã duyệt (Approved) hoặc đang hoạt động (Active) để tạo đoàn khám
const modalContracts = computed(() => {
  return contracts.value
    .filter(c => ['Approved', 'Active'].includes(c.status))
    .sort((a, b) => a.contractName.localeCompare(b.contractName))
})

// Hợp đồng đang được chọn trong bộ lọc
const selectedContract = computed(() => {
  if (!filterContractId.value) return null
  return contracts.value.find(c => c.healthContractId === parseInt(filterContractId.value)) || null
})

// Phần trăm tiến độ: tổng bệnh án / số lượng dự kiến HĐ
const progressPercent = computed(() => {
  if (!selectedContract.value || !selectedContract.value.expectedQuantity) return 0
  return Math.round((total.value / selectedContract.value.expectedQuantity) * 100)
})

// ─── Init ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadContracts(), loadRecords()])
})

// ─── Helpers ──────────────────────────────────────────────────────────────────
function resetForm() {
  return {
    medicalRecordId: null,
    contractId: filterContractId.value || '',
    groupId: filterGroupId.value || '',
    fullName: '',
    gender: '',
    dateOfBirth: '',
    iDCardNumber: '',
    department: ''
  }
}

let searchTimer = null
function debouncedSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => { page.value = 1; loadRecords() }, 400)
}

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '—'

const formatStatus = (s) => {
  const map = {
    CREATED: 'Mới tạo', READY: 'Sẵn sàng', CHECKED_IN: 'Đã tiếp đón',
    IN_PROGRESS: 'Đang khám', STATION_DONE: 'Xong trạm',
    QC_PENDING: 'Chờ QC', QC_PASSED: 'QC đạt', QC_REWORK: 'QC trả về',
    COMPLETED: 'Hoàn thành', REPORTED: 'Đã trả KQ', CLOSED: 'Đã đóng',
    NO_SHOW: 'Vắng mặt', CANCELLED: 'Đã hủy'
  }
  return map[s] || s
}

const getStatusBadge = (s) => {
  if (['COMPLETED', 'QC_PASSED', 'REPORTED', 'CLOSED'].includes(s)) return 'badge-green'
  if (['IN_PROGRESS', 'STATION_DONE'].includes(s)) return 'badge-blue'
  if (s === 'CHECKED_IN' || s === 'READY') return 'badge-indigo'
  if (s === 'QC_PENDING' || s === 'QC_REWORK') return 'badge-pink'
  if (s === 'NO_SHOW' || s === 'CANCELLED') return 'badge-gray'
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

const onContractChange = async () => {
  filterGroupId.value = ''
  groups.value = []
  if (filterContractId.value) {
    try {
      const res = await apiClient.get(`/api/MedicalGroups/by-contract/${filterContractId.value}`)
      groups.value = res.data
    } catch (e) {
      console.warn('Không tải được danh sách đoàn:', e)
    }
  }
  page.value = 1
  loadRecords()
}

const loadRecords = async () => {
  loading.value = true
  try {
    const params = new URLSearchParams({
      page: page.value,
      pageSize: pageSize.value
    })
    if (search.value) params.append('search', search.value)
    if (filterContractId.value) params.append('contractId', filterContractId.value)
    if (filterGroupId.value) params.append('groupId', filterGroupId.value)

    const res = await apiClient.get(`/api/MedicalRecords/all?${params}`)
    records.value = res.data.items
    total.value = res.data.total
    
    // Update stats from the records currently in view (or we could fetch aggregate stats)
    // For simplicity, we use the ones in the current result set
    stats.value = {
        completed: records.value.filter(r => ['COMPLETED', 'QC_PASSED', 'REPORTED', 'CLOSED'].includes(r.status)).length,
        inProgress: records.value.filter(r => ['IN_PROGRESS', 'STATION_DONE', 'CHECKED_IN'].includes(r.status)).length,
        created: records.value.filter(r => r.status === 'CREATED' || r.status === 'READY').length
    }
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    loading.value = false
  }
}

const viewDetail = async (r) => {
  detailData.value = null
  showDetail.value = true
  try {
    const res = await apiClient.get(`/api/MedicalRecords/${r.medicalRecordId}`)
    detailData.value = res.data
  } catch (e) {
    toast.error('Không thể tải hồ sơ bệnh án')
    showDetail.value = false
  }
}

// ─── Form Actions ─────────────────────────────────────────────────────────────
const openAdd = async () => {
  editMode.value = false
  form.value = resetForm()
  formError.value = ''
  modalGroups.value = []
  
  if (form.value.contractId) {
    await onModalContractChange()
  }
  
  showModal.value = true
}

const onModalContractChange = async () => {
  form.value.groupId = ''
  modalGroups.value = []
  if (form.value.contractId) {
    try {
      const res = await apiClient.get(`/api/MedicalGroups/by-contract/${form.value.contractId}`)
      modalGroups.value = res.data
    } catch (e) {
      console.warn('Không tải được danh sách đoàn trong modal:', e)
    }
  }
}

const openEdit = (r) => {
  editMode.value = true
  form.value = {
    medicalRecordId: r.medicalRecordId,
    fullName: r.fullName,
    gender: r.gender || '',
    dateOfBirth: r.dateOfBirth ? r.dateOfBirth.split('T')[0] : '',
    iDCardNumber: r.idCardNumber || '',
    department: r.department || ''
  }
  formError.value = ''
  showModal.value = true
}

const submitForm = async () => {
  if (!form.value.fullName?.trim()) { formError.value = 'Họ tên không được để trống.'; return }
  if (!editMode.value && !form.value.groupId) { formError.value = 'Vui lòng chọn đoàn khám.'; return }

  saving.value = true
  formError.value = ''
  try {
    if (editMode.value) {
      const payload = {
        fullName: form.value.fullName.trim(),
        gender: form.value.gender || null,
        dateOfBirth: form.value.dateOfBirth || null,
        iDCardNumber: form.value.iDCardNumber || null,
        department: form.value.department || null
      }
      await apiClient.put(`/api/MedicalRecords/${form.value.medicalRecordId}/basic`, payload)
      toast.success('Cập nhật bệnh án thành công!')
    } else {
      const payload = {
        groupId: form.value.groupId,
        records: [{
          fullName: form.value.fullName.trim(),
          gender: form.value.gender || null,
          dateOfBirth: form.value.dateOfBirth || null,
          iDCardNumber: form.value.iDCardNumber || null,
          department: form.value.department || null
        }]
      }
      await apiClient.post('/api/MedicalRecords/batch-ingest', payload)
      toast.success('Thêm bệnh án mới thành công!')
    }
    
    showModal.value = false
    await loadRecords()
  } catch (e) {
    formError.value = parseApiError(e)
  } finally {
    saving.value = false
  }
}

const confirmDelete = (r) => { deleteTarget.value = r }

const doDelete = async () => {
  if (!deleteTarget.value) return
  saving.value = true
  try {
    await apiClient.delete(`/api/MedicalRecords/${deleteTarget.value.medicalRecordId}`)
    toast.success('Đã xóa bệnh án thành công!')
    deleteTarget.value = null
    await loadRecords()
  } catch (e) {
    toast.error(parseApiError(e))
  } finally {
    saving.value = false
  }
}

const handleExport = async () => {
    try {
        const params = new URLSearchParams()
        if (filterContractId.value) params.append('contractId', filterContractId.value)
        if (filterGroupId.value) params.append('groupId', filterGroupId.value)
        if (search.value) params.append('search', search.value)

        const res = await apiClient.get('/api/MedicalRecords/export', {
            params,
            responseType: 'blob'
        })
        
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `DanhSachBenhAn_${new Date().toISOString().slice(0,10)}.xlsx`)
        document.body.appendChild(link)
        link.click()
        link.remove()
        window.URL.revokeObjectURL(url)
        toast.success('Đã xuất file Excel thành công!')
    } catch (e) {
        toast.error(parseApiError(e))
    }
}

const handleExportPdf = async (id) => {
    try {
        const res = await apiClient.get(`/api/MedicalRecords/${id}/export-pdf`, { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `KetQuaKham_${id}.pdf`)
        document.body.appendChild(link)
        link.click()
        link.remove()
        window.URL.revokeObjectURL(url)
    } catch (e) {
        toast.error('Lỗi khi xuất PDF')
    }
}

const goPage = (p) => {
  if (p < 1 || p > totalPages.value) return
  page.value = p
  loadRecords()
}
</script>

<style scoped>
.patients-page { padding: 0; }
.avatar-circle {
  width: 32px; height: 32px; border-radius: 8px; background: linear-gradient(135deg, var(--primary), #8b5cf6);
  color: white; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 0.75rem; flex-shrink: 0;
}

.premium-card {
  background: white; border-radius: 1.25rem; border: 1px solid #f1f5f9;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
}

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
