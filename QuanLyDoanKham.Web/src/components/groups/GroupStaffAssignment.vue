<template>
  <div class="space-y-6 animate-fade-in">
    <div class="flex justify-between items-center">
        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400">
            <UsersIcon class="w-4 h-4" /> Đội ngũ đi đoàn
        </h5>
        <div class="flex items-center gap-2 pr-2">
            <button v-if="status === 'Open' && can('DoanKham.StaffAssign')" 
                    @click="$emit('open-modal', 'staff', groupId)" 
                    class="btn-mini-action">Gán nhân sự</button>
            <div v-if="can('DoanKham.StaffAssign')" class="divider"></div>
            <button v-if="status === 'Open' && can('DoanKham.StaffAssign')" 
                    @click="$emit('ai-suggest', groupId)" 
                    :disabled="isAiLoading" 
                    class="btn-mini-action text-primary">
                <Sparkles v-if="!isAiLoading" class="w-2.5 h-2.5" />
                <RefreshCw v-else class="w-2.5 h-2.5 animate-spin" />
                {{ isAiLoading ? 'Đang tính...' : 'AI Copilot' }}
            </button>
            <div class="divider"></div>
            <button @click="$emit('export-staff', groupId)" class="btn-mini-action text-emerald-600">Excel</button>
            <div class="divider"></div>
            <button v-if="status === 'Open' && can('ChamCong.QR')" 
                    @click="$emit('open-modal', 'qr', groupId)" 
                    class="btn-mini-action text-purple-600">
                <QrCode class="w-2.5 h-2.5" /> QR
            </button>
        </div>
    </div>
    
    <div class="overflow-x-auto border border-slate-100 rounded-2xl">
        <table class="w-full text-left bg-white">
            <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                <tr>
                    <th class="p-4 text-center w-16">STT</th>
                    <th class="p-4">Nhân viên</th>
                    <th class="p-4">Vị trí trực</th>
                    <th class="p-4 text-center">Ca làm</th>
                    <th class="p-4 text-center">Trạng thái</th>
                    <th class="p-4 text-center">Tác vụ</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-slate-50 text-xs text-slate-600">
                <tr v-if="staff.length === 0">
                    <td colspan="6" class="p-8 text-center bg-slate-50/50">
                        <div class="flex flex-col items-center justify-center text-slate-400">
                            <UsersIcon class="w-8 h-8 mb-3 opacity-20" />
                            <span class="text-[10px] font-black uppercase tracking-widest">Chưa gán nhân sự nào</span>
                        </div>
                    </td>
                </tr>
                <tr v-for="(s, index) in staff" :key="s.id" class="hover:bg-slate-50/50 transition-all">
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
                            <span :class="['px-3 py-1 rounded-lg font-black text-[9px] uppercase tracking-widest ', getWorkStatusClass(s.workStatus)]">
                                {{ s.workStatus || 'Đang chờ' }}
                            </span>
                            <span v-if="s.checkInTime" class="text-[8px] font-black text-slate-400">Vào: {{ formatTime(s.checkInTime) }}</span>
                        </div>
                    </td>
                    <td class="p-4">
                        <div class="flex items-center justify-center gap-2">
                            <button v-if="canCheckIn(s)" 
                                    @click="$emit('check-in', s.id, groupId)"
                                    class="icon-btn-indigo" title="Vào đoàn">
                                <LogIn class="w-4 h-4" />
                            </button>
                            <button v-if="canCheckOut(s)" 
                                    @click="$emit('check-out', s.id, groupId)"
                                    class="icon-btn-rose" title="Rời đoàn">
                                <LogOut class="w-4 h-4" />
                            </button>
                            <button v-if="status === 'Open' && can('DoanKham.StaffAssign')" 
                                    @click="$emit('remove-staff', s.id, groupId)"
                                    class="icon-btn-slate" title="Xóa khỏi đoàn">
                                <Trash2 class="w-4 h-4" />
                            </button>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
  </div>
</template>

<script setup>
import { Users as UsersIcon, Sparkles, RefreshCw, QrCode, LogIn, LogOut, Trash2 } from 'lucide-vue-next'

const props = defineProps({
  groupId: { type: Number, required: true },
  staff: { type: Array, required: true },
  can: { type: Function, required: true },
  isAiLoading: { type: Boolean, default: false },
  status: { type: String, default: 'Open' }
})

defineEmits(['open-modal', 'ai-suggest', 'export-staff', 'check-in', 'check-out', 'remove-staff'])

const getWorkStatusClass = (status) => {
  if (status === 'Đã tham gia') return 'bg-emerald-100 text-emerald-600'
  if (status === 'Vắng mặt') return 'bg-rose-100 text-rose-600'
  return 'bg-slate-100 text-slate-400'
}

const formatTime = (date) => {
  return new Date(date).toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})
}

const canCheckIn = (s) => {
  return !s.checkInTime && props.status === 'Open'
}

const canCheckOut = (s) => {
  return s.checkInTime && !s.checkOutTime && props.status === 'Open'
}
</script>

<style scoped>
.btn-mini-action {
  font-size: 8px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: -0.02em;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.btn-mini-action:hover {
  background: #f1f5f9;
}

.divider {
  width: 1px;
  height: 0.75rem;
  background: #e2e8f0;
}

.icon-btn-indigo {
  padding: 0.5rem;
  color: #6366f1;
  border-radius: 8px;
  transition: all 0.2s;
}

.icon-btn-indigo:hover {
  background: #e0e7ff;
}

.icon-btn-rose:hover {
  background: #fff1f2;
}

.icon-btn-slate {
  padding: 0.5rem;
  color: #94a3b8;
  border-radius: 8px;
  transition: all 0.2s;
}

.icon-btn-slate:hover {
  background: #f1f5f9;
  color: #e11d48;
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
