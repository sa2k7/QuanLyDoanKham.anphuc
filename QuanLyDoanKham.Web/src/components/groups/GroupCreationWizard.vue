<template>
  <div class="premium-card p-4 mb-6 overflow-hidden border-indigo-100 bg-indigo-50/10 shadow-sm transition-all duration-300">
    <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center shadow-md shadow-indigo-100 shrink-0">
                <Zap class="w-4 h-4 text-white" />
            </div>
            <h3 class="text-sm font-black text-slate-800 tracking-tight uppercase">Cài đặt đoàn khám mới</h3>
        </div>
        <div class="flex items-center gap-3">
            <div class="flex p-1 bg-white rounded-xl border border-slate-100 shadow-sm">
                <button @click="createMode = 'manual'" :class="['px-4 py-1.5 rounded-lg font-black text-[9px] uppercase tracking-tighter transition-all', createMode === 'manual' ? 'bg-slate-100 text-slate-900 shadow-inner' : 'text-slate-400']">Thủ công</button>
                <button @click="createMode = 'auto'" :class="['px-4 py-1.5 rounded-lg font-black text-[9px] uppercase tracking-tighter transition-all', createMode === 'auto' ? 'bg-indigo-600 text-white shadow-lg' : 'text-slate-400']">Tự động (AI)</button>
            </div>
            <button @click="$emit('close')" class="p-1.5 hover:bg-slate-200/50 rounded-lg transition-all text-slate-400">
                <X class="w-4 h-4" />
            </button>
        </div>
    </div>

    <!-- Mode: Manual -->
    <form v-if="createMode === 'manual'" @submit.prevent="handleManualSubmit" class="flex items-end gap-4 min-w-full">
        <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Hợp đồng mục tiêu</label>
            <select v-model="manualData.healthContractId" required class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] uppercase px-4 focus:ring-2 focus:ring-indigo-500/10 outline-none">
                <option :value="null" disabled>-- Chọn hợp đồng --</option>
                <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                    [{{ c.contractCode || '---' }}] {{ c.contractName || c.companyName }}
                </option>
            </select>
        </div>
        <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên đoàn khám</label>
            <input v-model="manualData.groupName" required class="input-premium bg-white border-slate-200 w-full h-11 text-[11px] font-black px-4" placeholder="VD: Khám sức khỏe CB-CNV đợt 1" />
        </div>
        <div class="flex-[2] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày triển khai</label>
            <input v-model="manualData.examDate" type="date" required class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] px-4" />
        </div>
        <button type="submit" :disabled="isLoading" class="h-11 px-8 rounded-xl bg-slate-800 text-white font-black text-[10px] uppercase tracking-widest hover:bg-slate-900 transition-all shadow-lg active:scale-95 shrink-0 flex items-center justify-center gap-2">
            <Loader2 v-if="isLoading" class="w-3 h-3 animate-spin" />
            KÍCH HOẠT ĐOÀN MỚI
        </button>
    </form>

    <!-- Mode: Auto-Create -->
    <div v-if="createMode === 'auto'" class="flex items-end gap-4 min-w-full animate-fade-in">
        <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Chọn hợp đồng nguồn</label>
            <select v-model="autoData.healthContractId" @change="onAutoContractSelect" 
                    class="input-premium bg-white border-slate-200 w-full cursor-pointer font-black text-[11px] uppercase px-4 h-11 transition-all focus:ring-2 focus:ring-indigo-500/10 outline-none">
                <option :value="null">-- Chọn hợp đồng --</option>
                <option v-for="c in approvedContracts" :key="c.healthContractId" :value="c.healthContractId">
                    [{{ c.contractCode || '---' }}] {{ c.contractName || c.companyName }}
                </option>
            </select>
        </div>
        <div class="flex-[3] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên đoàn (Tự động)</label>
            <input v-model="autoData.groupName" class="input-premium bg-white border-slate-200 w-full h-11 text-[11px] font-black px-4" />
        </div>
        <div class="flex-[2] min-w-0 flex flex-col gap-1.5">
            <label class="text-[9px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày triển khai</label>
            <input type="date" v-model="autoData.examDate" class="input-premium bg-white border-slate-200 w-full h-11 font-black text-[11px] px-4" />
        </div>

        <button @click="handleAutoSubmit" :disabled="isLoading || !autoData.healthContractId" 
                class="h-11 px-8 rounded-xl font-black transition-all flex items-center justify-center gap-2 text-[10px] uppercase tracking-widest shrink-0"
                :class="[!autoData.healthContractId ? 'bg-slate-100 text-slate-400 border border-slate-200' : 'bg-blue-600 text-white hover:bg-blue-700 shadow-lg shadow-blue-500/20 active:scale-95 animate-pulse-subtle']">
            <Loader2 v-if="isLoading" class="w-4 h-4 animate-spin" />
            <Zap v-else class="w-4 h-4" />
            <span>Xác nhận tạo đoàn bằng AI</span>
        </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Zap, X, Loader2 } from 'lucide-vue-next'

const props = defineProps({
  approvedContracts: {
    type: Array,
    required: true
  },
  isLoading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['manual-create', 'auto-create', 'close'])

const createMode = ref('manual')

const manualData = ref({
  healthContractId: null,
  groupName: '',
  examDate: new Date().toISOString().split('T')[0]
})

const autoData = ref({
  healthContractId: null,
  groupName: '',
  examDate: new Date().toISOString().split('T')[0]
})

const onAutoContractSelect = () => {
    const contract = props.approvedContracts.find(c => c.healthContractId === autoData.value.healthContractId);
    if (contract) {
        autoData.value.groupName = `ĐOÀN KHÁM ${contract.shortName || contract.companyName}`.toUpperCase();
    }
}

const handleManualSubmit = () => {
  emit('manual-create', manualData.value)
}

const handleAutoSubmit = () => {
  emit('auto-create', autoData.value)
}
</script>

<style scoped>
.premium-card {
  background: white;
  border-radius: 2rem;
  border: 1px solid rgba(226, 232, 240, 0.8);
}

.input-premium {
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  transition: all 0.2s;
}

.animate-pulse-subtle {
    animation: pulse-subtle 3s infinite;
}

@keyframes pulse-subtle {
    0% { transform: scale(1); }
    50% { transform: scale(1.02); }
    100% { transform: scale(1); }
}

.animate-fade-in {
    animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(5px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
