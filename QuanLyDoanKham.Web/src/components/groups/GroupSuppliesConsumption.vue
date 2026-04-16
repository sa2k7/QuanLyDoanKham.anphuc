<template>
  <div class="group-supplies-consumption space-y-6 animate-fade-in">
    <div class="flex justify-between items-center">
        <h5 class="flex items-center gap-2 text-xs font-black uppercase tracking-widest text-slate-400">
            <Package class="w-4 h-4" /> THỰC TẾ TIÊU HAO SO VỚI QUY MÔ ĐOÀN
        </h5>
        <div v-if="suppliesData" class="flex items-center gap-2 text-[10px] font-black uppercase tracking-widest bg-slate-50 text-slate-500 px-3 py-1.5 rounded-lg border border-slate-100">
            Quy mô: <span class="bg-primary text-white px-2 py-0.5 rounded-md">{{ suppliesData.totalPatients || 0 }}</span> bệnh nhân
        </div>
    </div>

    <div v-if="isLoading" class="flex justify-center p-8">
        <Loader2 class="w-8 h-8 animate-spin text-primary opacity-50" />
    </div>
    
    <div v-else-if="suppliesData?.supplies?.length" class="grid gap-4">
        <div v-for="item in suppliesData.supplies" :key="item.itemName" class="p-5 rounded-2xl border flex flex-col md:flex-row md:items-center gap-6 transition-all"
            :class="getConsumptionBgClass(item.status)">
            
            <div class="w-full md:w-1/3">
                <div class="font-black text-slate-700 uppercase tracking-widest mb-1">{{ item.itemName }}</div>
                <div class="text-[10px] font-black uppercase tracking-widest" :class="getConsumptionTextClass(item.status)">
                    <template v-if="item.status === 'CRITICAL'"><AlertOctagon class="w-3 h-3 inline mr-1 -mt-0.5"/> THẤT THOÁT NGHIÊM TRỌNG</template>
                    <template v-else-if="item.status === 'WARNING'">CẦN CHÚ Ý</template>
                    <template v-else>TRONG ĐỊNH MỨC AN TOÀN</template>
                </div>
            </div>

            <div class="w-full md:w-2/3 space-y-2">
                <div class="flex justify-between text-[10px] font-black uppercase tracking-widest text-slate-500">
                    <span>Đã dùng: <strong class="text-slate-800">{{ item.totalUsed }}</strong> {{ item.unit }}</span>
                    <span>Định mức: <strong>{{ item.expectedUsage }}</strong> {{ item.unit }}</span>
                </div>
                <!-- Progress Bar -->
                <div class="h-2 w-full bg-slate-100 rounded-full flex overflow-hidden relative">
                    <div class="h-full transition-all duration-1000"
                        :class="getProgressBarClass(item.status)"
                        :style="`width: ${Math.min(item.usagePercentage, 100)}%`">
                    </div>
                    <div v-if="item.usagePercentage > 100" class="h-full bg-rose-600 opacity-50 absolute left-0 top-0"
                        :style="`width: ${Math.min(item.usagePercentage - 100, 100)}%` ">
                    </div>
                </div>
                <div class="text-right text-[9px] font-black uppercase tracking-widest"
                     :class="item.usagePercentage > 100 ? 'text-rose-500' : 'text-slate-400'">
                    Tỉ lệ hao phí: {{ item.usagePercentage }}%
                </div>
            </div>
        </div>
    </div>
    <div v-else class="p-10 text-center bg-slate-50/50 rounded-2xl border border-slate-100">
        <PackageSearch class="w-8 h-8 mx-auto text-slate-300 mb-3" />
        <div class="text-[10px] font-black uppercase tracking-widest text-slate-400">Đoàn khám này chưa có phát sinh xuất kho vật tư</div>
    </div>
  </div>
</template>

<script setup>
import { Package, Loader2, AlertOctagon, PackageSearch } from 'lucide-vue-next'

defineProps({
  groupId: { type: Number, required: true },
  suppliesData: { type: Object, default: null },
  isLoading: { type: Boolean, default: false }
})

const getConsumptionBgClass = (status) => {
  if (status === 'CRITICAL') return 'bg-rose-50 border-rose-100'
  if (status === 'WARNING') return 'bg-amber-50 border-amber-100'
  return 'bg-emerald-50 border-emerald-100'
}

const getConsumptionTextClass = (status) => {
  if (status === 'CRITICAL') return 'text-rose-500'
  if (status === 'WARNING') return 'text-amber-500'
  return 'text-emerald-500'
}

const getProgressBarClass = (status) => {
  if (status === 'CRITICAL') return 'bg-rose-500'
  if (status === 'WARNING') return 'bg-amber-500'
  return 'bg-emerald-500'
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
