<template>
  <div class="space-y-6 animate-fade-in">
    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Vùng siêu âm</label>
      <div class="flex flex-wrap gap-2">
        <button v-for="mode in modes" :key="mode"
                @click="result.mode = mode"
                :class="['px-4 py-2 rounded-xl border-2 text-[10px] font-black uppercase tracking-widest transition-all',
                         result.mode === mode ? 'border-primary bg-primary/5 text-primary' : 'border-slate-100 bg-white text-slate-400']">
          {{ mode }}
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-4">
      <div v-for="field in dynamicFields" :key="field.key" class="form-group flex items-center justify-between p-3 bg-slate-50 rounded-xl">
        <span class="text-[10px] font-black text-slate-500 uppercase tracking-widest">{{ field.label }}</span>
        <select v-model="result.data[field.key]" class="bg-transparent border-none text-slate-800 font-bold text-sm outline-none text-right">
          <option value="Normal">Bình thường</option>
          <option value="Abnormal">Bất thường</option>
          <option value="Observation">Theo dõi</option>
          <option value="N/A">N/A</option>
        </select>
      </div>
    </div>

    <div class="form-group mt-2">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Ghi nhận hình ảnh</label>
      <textarea 
        v-model="result.observation"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-primary outline-none min-h-[80px]"
        placeholder="Ghi nhận chi tiết kết quả siêu âm..."
      ></textarea>
    </div>
  </div>
</template>

<script setup>
import { reactive, watch, onMounted } from 'vue'

const props = defineProps({
  modelValue: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue'])

const modes = ['Bụng tổng quát', 'Tuyến giáp', 'Vú', 'Tim', 'Khác']

const dynamicFields = [
  { key: 'liver',   label: 'Gan (Liver)' },
  { key: 'gall',    label: 'Túi mật (Gallbladder)' },
  { key: 'kidney',  label: 'Thận (Kidneys)' },
  { key: 'pancreas',label: 'Tụy (Pancreas)' },
  { key: 'spleen',  label: 'Lách (Spleen)' },
]

const result = reactive({
  mode: props.modelValue.mode || 'Bụng tổng quát',
  data: props.modelValue.data || {
    liver: 'Normal',
    gall: 'Normal',
    kidney: 'Normal',
    pancreas: 'Normal',
    spleen: 'Normal'
  },
  observation: props.modelValue.observation || ''
})

watch(result, (newVal) => {
  emit('update:modelValue', { ...newVal })
}, { deep: true })

onMounted(() => {
  emit('update:modelValue', { ...result })
})
</script>
