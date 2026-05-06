<template>
  <div class="group-operational-modals">
    <!-- 1. Staff Selection Modal -->
    <Teleport to="body">
      <div v-if="modals.staff.show" class="modal-overlay">
          <div class="modal-content max-w-xl border-teal-500">
              <button @click="$emit('close', 'staff')" class="btn-close-modal">
                  <X class="w-5 h-5" />
              </button>

              <div class="modal-body">
                  <div class="flex items-center gap-4 mb-8">
                      <div class="icon-box bg-teal-50 text-teal-600">
                          <UsersIcon class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="modal-title">Điều động nhân sự</h3>
                          <p class="modal-subtitle">Phân bổ vị trí & Ca làm tại đoàn khám</p>
                      </div>
                  </div>

                  <form id="staffForm" @submit.prevent="$emit('add-staff')" class="space-y-6">
                      <div class="space-y-4">
                              <div class="space-y-3">
                                  <div class="flex justify-between items-center">
                                      <div class="flex items-center gap-2">
                                          <label class="input-label !mb-0"><ShieldCheck class="w-3 h-3 text-indigo-400 inline" /> 0. Chọn vị trí trực</label>
                                          <span v-if="groupPositions.length > 0" 
                                                class="px-1.5 py-0.5 bg-slate-100 text-slate-500 rounded text-[8px] font-black tracking-tighter border border-slate-200">
                                              {{ groupPositions.length }}/7
                                          </span>
                                      </div>
                                      <div class="flex gap-1" v-if="groupPositions.length > 0">
                                          <button @click="$emit('open-position-modal')" type="button" class="text-[9px] font-bold text-indigo-600 hover:bg-indigo-50 px-2 py-1 rounded transition-all border border-transparent hover:border-indigo-100">
                                              + Thêm vị trí
                                          </button>
                                          <button @click="$emit('quick-init-positions')" type="button" class="text-[9px] font-bold text-amber-600 hover:bg-amber-50 px-2 py-1 rounded transition-all border border-transparent hover:border-amber-100 flex items-center gap-1" title="Khởi tạo các vị trí chuẩn còn thiếu">
                                              <Zap class="w-3 h-3" /> Auto
                                          </button>
                                      </div>
                                  </div>
                                  
                                  <div v-if="groupPositions.length === 0" class="bg-indigo-50/30 p-4 rounded-2xl border border-dashed border-indigo-200 text-center space-y-3">
                                      <p class="text-[11px] font-semibold text-indigo-500 uppercase tracking-wider">Đoàn khám chưa có cơ cấu vị trí</p>
                                      <div class="flex gap-2 justify-center">
                                          <button @click="$emit('open-position-modal')" type="button" class="px-3 py-2 bg-white border border-indigo-100 rounded-xl text-[9px] font-black text-indigo-600 hover:bg-indigo-50 transition-all uppercase tracking-widest flex items-center gap-1.5 shadow-sm">
                                              <Plus class="w-3 h-3" /> Thêm thủ công
                                          </button>
                                          <button @click="$emit('quick-init-positions')" type="button" class="px-3 py-2 bg-indigo-600 border border-indigo-600 rounded-xl text-[9px] font-black text-white hover:bg-indigo-700 transition-all uppercase tracking-widest flex items-center gap-1.5 shadow-md shadow-indigo-100">
                                              <Zap class="w-3 h-3" /> Khởi tạo nhanh (6 vị trí)
                                          </button>
                                      </div>
                                  </div>

                                  <select v-else v-model="modals.staff.data.positionId" class="select-premium" @change="$emit('position-change')">
                                      <option :value="null">-- Chọn vị trí trong đoàn --</option>
                                      <option v-for="p in groupPositions" :key="p.positionId" :value="p.positionId">
                                          {{ p.positionName }} (Cần: {{ p.requiredCount }} người)
                                      </option>
                                  </select>
                              </div>

                          <div class="space-y-2">
                              <label class="input-label"><UsersIcon class="w-3 h-3 text-indigo-400" /> 1. Chọn nhân sự</label>
                              <select v-model="modals.staff.data.staffId" required class="select-premium">
                                  <option :value="null" disabled>-- Chọn nhân sự tham gia --</option>
                                  <optgroup v-if="recommendedStaff.length > 0" label="Phù hợp vị trí">
                                      <option v-for="s in recommendedStaff" :key="s.staffId" :value="s.staffId">
                                          {{ s.fullName }} ({{ s.jobTitle || s.staffType || '---' }})
                                      </option>
                                  </optgroup>
                                  <optgroup v-if="otherStaff.length > 0" label="Nhân sự khác">
                                      <option v-for="s in otherStaff" :key="s.staffId" :value="s.staffId">
                                          {{ s.fullName }} ({{ s.jobTitle || s.staffType || '---' }})
                                      </option>
                                  </optgroup>
                                  <option v-if="recommendedStaff.length === 0 && otherStaff.length === 0" disabled>
                                      Không có quyền xem danh sách nhân sự
                                  </option>
                              </select>
                          </div>

                          <div v-if="isRoleMismatch" class="alert-warning">
                              <AlertTriangle class="w-4 h-4 flex-shrink-0" />
                              Nhân sự được chọn không phù hợp với vị trí yêu cầu
                          </div>

                          <div class="space-y-2" v-if="modals.staff.data.staffId">
                              <label class="input-label"><Stethoscope class="w-3 h-3 text-indigo-400" /> 2. Vai trò tại đoàn</label>
                              <div class="relative">
                                  <input v-model="modals.staff.data.workPosition" 
                                         required 
                                         placeholder="VD: Khám nội, Siêu âm, Lấy máu..." 
                                         class="input-premium !pl-12" />
                                  <div class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400">
                                      <ShieldCheck class="w-5 h-5" />
                                  </div>
                              </div>
                              <p class="text-[9px] font-bold text-slate-400 mt-1 uppercase tracking-widest italic ml-1">
                                  * Hệ thống sẽ tự động đồng bộ với hồ sơ chuyên môn
                              </p>
                          </div>

                          <div class="grid grid-cols-2 gap-4" v-if="modals.staff.data.staffId">
                              <div class="space-y-2">
                                  <label class="input-label"><RefreshCw class="w-3 h-3 text-indigo-400" /> Ca làm</label>
                                  <select v-model="modals.staff.data.shiftType" class="select-premium">
                                      <option :value="1.0">Cả ngày (1.0)</option>
                                      <option :value="0.5">Sáng (0.5)</option>
                                      <option :value="0.51">Chiều (0.5)</option>
                                  </select>
                              </div>
                              <div class="space-y-2">
                                  <label class="input-label"><CheckCircle2 class="w-3 h-3 text-indigo-400" /> Trạng thái</label>
                                  <select v-model="modals.staff.data.workStatus" class="select-premium">
                                      <option value="Đã tham gia">Đã tham gia</option>
                                      <option value="Đang chờ">Đang chờ</option>
                                      <option value="Vắng mặt">Vắng mặt</option>
                                  </select>
                              </div>
                          </div>

                          <div class="space-y-2" v-if="modals.staff.data.staffId">
                              <label class="input-label"><MapPin class="w-3 h-3 text-indigo-400" /> Điểm đón nhân sự</label>
                              <input v-model="modals.staff.data.pickupLocation" 
                                     placeholder="VD: Cổng chính, VP Công ty, Tự túc..." 
                                     class="input-premium" />
                          </div>
                      </div>
                  </form>
                  <div class="modal-footer">
                      <button @click="$emit('close', 'staff')" class="btn-cancel">Hủy bỏ</button>
                      <button form="staffForm" type="submit" class="btn-confirm bg-slate-900 border-slate-900 shadow-slate-900 hover:bg-teal-600">XÁC NHẬN ĐIỀU ĐỘNG</button>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- 4. Edit Group Info Modal -->
    <Teleport to="body">
      <div v-if="modals.editGroup && modals.editGroup.show" class="modal-overlay">
          <div class="modal-content max-w-xl border-indigo-600">
              <button @click="$emit('close', 'editGroup')" class="btn-close-modal">
                  <X class="w-5 h-5" />
              </button>
              <div class="modal-body">
                  <div class="flex items-center gap-4 mb-8">
                      <div class="icon-box bg-indigo-50 text-indigo-600">
                          <Settings2 class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="modal-title">Sửa thông tin đoàn</h3>
                          <p class="modal-subtitle">Cập nhật lịch trình & Nội dung khám</p>
                      </div>
                  </div>

                  <form id="editGroupForm" @submit.prevent="$emit('update-group')" class="space-y-5">
                      <div class="space-y-2">
                          <label class="input-label">Tên đoàn khám</label>
                          <input v-model="modals.editGroup.data.groupName" required class="input-premium" />
                      </div>
                      <div class="grid grid-cols-2 gap-4">
                          <div class="space-y-2">
                              <label class="input-label">Ngày triển khai</label>
                              <input v-model="modals.editGroup.data.examDate" type="date" required class="input-premium" />
                          </div>
                          <div class="space-y-2">
                              <label class="input-label">Trạng thái</label>
                              <select v-model="modals.editGroup.data.status" class="select-premium">
                                  <option value="Open">Đang triển khai</option>
                                  <option value="Finished">Đã kết thúc</option>
                                  <option value="Locked">Đã khóa sổ</option>
                              </select>
                          </div>
                      </div>
                      <div class="grid grid-cols-2 gap-4">
                          <div class="space-y-2">
                              <label class="input-label">Giờ bắt đầu khám</label>
                              <input v-model="modals.editGroup.data.startTime" placeholder="VD: 07:30" class="input-premium" />
                          </div>
                          <div class="space-y-2">
                              <label class="input-label">Giờ xuất phát</label>
                              <input v-model="modals.editGroup.data.departureTime" placeholder="VD: 05:30" class="input-premium" />
                          </div>
                      </div>
                      <div class="space-y-2">
                          <label class="input-label">Nội dung khám</label>
                          <input v-model="modals.editGroup.data.examContent" placeholder="VD: TỔNG QUÁT..." class="input-premium" />
                      </div>
                  </form>
                  <div class="modal-footer">
                      <button @click="$emit('close', 'editGroup')" class="btn-cancel">Hủy bỏ</button>
                      <button form="editGroupForm" type="submit" class="btn-confirm bg-slate-900">LƯU THAY ĐỔI</button>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- 2. Position Creation Modal -->
    <Teleport to="body">
      <div v-if="modals.position.show" class="modal-overlay">
          <div class="modal-content max-w-lg border-indigo-500">
              <button @click="$emit('close', 'position')" class="btn-close-modal">
                  <X class="w-5 h-5" />
              </button>

              <div class="modal-body">
                  <div class="flex items-center gap-4 mb-8">
                      <div class="icon-box bg-indigo-50 text-indigo-600">
                          <ShieldCheck class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="modal-title">{{ modals.position.data.positionId ? 'Sửa vị trí trực' : 'Thêm vị trí trực' }}</h3>
                          <p class="modal-subtitle">{{ modals.position.data.positionId ? 'Cập nhật cấu hình nhân sự' : 'Thiết lập cơ cấu nhân sự' }}</p>
                      </div>
                  </div>

                  <form id="positionForm" @submit.prevent="$emit('add-position')" class="space-y-6">
                      <div class="space-y-2">
                          <label class="input-label">Tên vị trí (Trạm)</label>
                          <input v-model="modals.position.data.positionName" list="standard-positions" required placeholder="Chọn hoặc nhập tên vị trí..." class="input-premium" />
                          <datalist id="standard-positions">
                              <option value="Trưởng đoàn"></option>
                              <option value="Tiếp nhận"></option>
                              <option value="Khám nội"></option>
                              <option value="Khám ngoại"></option>
                              <option value="Lấy máu"></option>
                              <option value="Khám sản"></option>
                              <option value="Siêu âm"></option>
                          </datalist>
                      </div>
                      <div class="space-y-2">
                          <label class="input-label">Số lượng cần thiết</label>
                          <input v-model.number="modals.position.data.requiredCount" type="number" min="1" required class="input-premium" />
                      </div>
                  </form>
                  <div class="modal-footer !justify-between">
                      <div class="flex gap-2">
                          <button @click="$emit('close', 'position')" class="btn-cancel">Hủy bỏ</button>
                          <button v-if="modals.position.data.positionId" 
                                  @click="$emit('remove-position', modals.position.data.positionId, modals.position.groupId)" 
                                  type="button" 
                                  class="px-4 py-2 bg-rose-50 text-rose-600 rounded-xl text-[10px] font-black hover:bg-rose-100 transition-all flex items-center gap-2 border border-rose-100">
                              <Trash2 class="w-3.5 h-3.5" /> XÓA VỊ TRÍ
                          </button>
                      </div>
                      <button form="positionForm" type="submit" class="btn-confirm bg-slate-900 border-slate-900 shadow-slate-900 hover:bg-indigo-600">
                          {{ modals.position.data.positionId ? 'CẬP NHẬT VỊ TRÍ' : 'LƯU VỊ TRÍ' }}
                      </button>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- 3. QR Attendance Modal -->
    <Teleport to="body">
      <div v-if="modals.qr.show" class="modal-overlay">
          <div class="modal-content max-w-sm border-purple-500">
              <button @click="$emit('close', 'qr')" class="btn-close-modal">
                  <X class="w-5 h-5" />
              </button>
              <div class="p-10 text-center">
                  <div class="mb-6 mt-4">
                      <div class="icon-box bg-indigo-50 text-indigo-600 mx-auto mb-4">
                          <QrCode class="w-10 h-10" />
                      </div>
                      <h3 class="text-xl font-black text-slate-800 uppercase tracking-tighter">QR CHẤM CÔNG</h3>
                      <p class="modal-subtitle">{{ qrData?.groupName || 'Đoàn khám' }}</p>
                  </div>

                  <div v-if="qrData" class="space-y-6">
                      <div class="bg-white p-6 rounded-[2.5rem] border-4 border-slate-900 shadow-[8px_8px_0px_#0f172a] inline-block mb-4">
                          <img :src="qrData.pngBase64" class="w-48 h-48 mx-auto" alt="QR" />
                      </div>
                      <div class="pt-4 flex flex-col gap-3">
                          <button @click="$emit('copy-qr-url')" class="w-full py-3 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-2xl font-black text-[10px] uppercase tracking-widest transition-all">Sao chép Link</button>
                          <button @click="$emit('close', 'qr')" class="btn-confirm bg-slate-900 border-slate-900 w-full">ĐÓNG</button>
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
  </div>
</template>

<script setup>
import {
  X, Users as UsersIcon, Stethoscope, CheckCircle2, AlertTriangle,
  ShieldCheck, QrCode, RefreshCw, MapPin, Settings2, Plus, Zap, Trash2
} from 'lucide-vue-next'

const props = defineProps({
  modals: { type: Object, required: true },
  groupPositions: { type: Array, default: () => [] },
  recommendedStaff: { type: Array, default: () => [] },
  otherStaff: { type: Array, default: () => [] },
  qrData: { type: Object, default: null },
  isRoleMismatch: { type: Boolean, default: false },
  currentPositionName: { type: String, default: '' }
})

defineEmits(['close', 'add-staff', 'add-position', 'position-change', 'copy-qr-url', 'update-group', 'open-position-modal', 'quick-init-positions'])
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(8px);
  padding: 1rem;
}

.modal-content {
  background: white;
  width: 100%;
  border-radius: 2.5rem;
  border-width: 2px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
  position: relative;
  overflow: hidden;
  animation: slideUp 0.3s ease-out;
}

.modal-body {
  padding: 2.5rem;
  padding-top: 3rem;
}

.modal-title {
  font-size: 1.5rem;
  font-weight: 900;
  color: #1e293b;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.modal-subtitle {
  font-size: 10px;
  font-weight: 900;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  margin-top: 0.25rem;
}

.icon-box {
  width: 3.5rem;
  height: 3.5rem;
  border-radius: 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.05);
}

.input-label {
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: #94a3b8;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-left: 0.25rem;
}

.input-premium, .select-premium {
  width: 100%;
  padding: 1rem 1.25rem;
  border-radius: 1.25rem;
  background: #f8fafc;
  border: 2px solid transparent;
  outline: none;
  font-weight: 900;
  color: #334155;
  transition: all 0.2s;
}

.input-premium:focus, .select-premium:focus {
  border-color: rgba(99, 102, 241, 0.2);
  background: white;
}

.alert-warning {
  padding: 0.5rem 0.75rem;
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 9px;
  font-weight: 900;
  color: #b45309;
  text-transform: uppercase;
}

.modal-footer {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.btn-cancel {
  flex: 1;
  padding: 1rem;
  font-size: 12px;
  font-weight: 900;
  color: #94a3b8;
  text-transform: uppercase;
  text-decoration: underline;
  text-underline-offset: 8px;
}

.btn-confirm {
  flex: 2;
  padding: 1rem;
  border-radius: 1.25rem;
  color: white;
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  border: 2px solid transparent;
  box-shadow: 4px 4px 0px rgba(15, 23, 42, 0.1);
  transition: all 0.2s;
}

.btn-confirm:active {
  transform: scale(0.95);
}

.btn-close-modal {
  position: absolute;
  top: 1.5rem;
  right: 1.5rem;
  padding: 0.5rem;
  background: white;
  border: 2px solid #0f172a;
  border-radius: 50%;
  color: #94a3b8;
  z-index: 10;
  box-shadow: 2px 2px 0px #0f172a;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
