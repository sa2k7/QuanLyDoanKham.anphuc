<template>
  <div class="group-position-structure space-y-6 animate-fade-in">
    <div class="flex justify-between items-center">
        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400">
            <ShieldCheck class="w-4 h-4" /> Cơ cấu vị trí trực tại đoàn
        </h5>
        <div class="flex items-center gap-3">
            <button v-if="status === 'Open' && can('DoanKham.SetPosition')" 
                    @click="$emit('open-modal', 'position', groupId)" 
                    class="btn-add-text">
                PHÂN BỔ VỊ TRÍ MỚI
            </button>
        </div>
    </div>
    
    <div class="overflow-x-auto border border-slate-100 rounded-2xl">
        <table class="w-full text-left bg-white">
            <thead class="bg-indigo-50/50 text-[10px] font-black uppercase tracking-widest text-primary">
                <tr>
                    <th class="p-4 w-16 text-center">STT</th>
                    <th class="p-4">Tên Vị trí (Trạm)</th>
                    <th class="p-4 text-center">Số lượng cần</th>
                    <th class="p-4 text-center">Đã phân công</th>
                    <th class="p-4 text-center">Tác vụ</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-slate-50 text-xs">
                <tr v-if="positions.length === 0">
                    <td colspan="5" class="p-10 text-center text-slate-300 font-black uppercase tracking-widest text-[10px]">Chưa có cấu hình vị trí cho đoàn này.</td>
                </tr>
                <tr v-for="(p, index) in positions" :key="p.positionId" class="hover:bg-slate-50/50 transition-all">
                    <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                    <td class="p-4 font-black text-slate-800 uppercase tracking-widest">{{ p.positionName }}</td>
                    <td class="p-4 text-center font-black">{{ p.requiredCount }}</td>
                    <td class="p-4 text-center">
                       <span :class="['px-3 py-1 rounded-lg font-black text-[9px]', p.assignedCount >= p.requiredCount ? 'bg-emerald-50 text-emerald-600' : 'bg-indigo-50 text-primary']">
                          {{ p.assignedCount }} / {{ p.requiredCount }}
                       </span>
                    </td>
                    <td class="p-4 text-center">
                        <button v-if="status === 'Open' && can('DoanKham.SetPosition')" 
                                @click="$emit('remove-position', p.positionId, groupId)"
                                class="text-rose-400 hover:text-rose-600 p-2 transition-colors" title="Xóa vị trí">
                            <Trash2 class="w-4 h-4" />
                        </button>
                        <span v-else class="text-slate-300">---</span>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
  </div>
</template>

<script setup>
import { ShieldCheck, Trash2 } from 'lucide-vue-next'

defineProps({
  groupId: { type: Number, required: true },
  positions: { type: Array, required: true },
  status: { type: String, default: 'Open' },
  can: { type: Function, required: true }
})

defineEmits(['open-modal', 'remove-position'])
</script>

<style scoped>
.btn-add-text {
  font-size: 9px;
  font-weight: 900;
  color: #6366f1;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: transparent;
  border-bottom: 2px solid transparent;
  transition: all 0.2s;
}

.btn-add-text:hover {
  border-bottom-color: #6366f1;
  transform: translateY(-1px);
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
