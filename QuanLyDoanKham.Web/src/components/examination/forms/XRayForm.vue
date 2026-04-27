<template>
  <div class="space-y-6 animate-fade-in">
    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Vị trí chụp (Target region)</label>
      <select 
        v-model="result.region"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-indigo-500 outline-none appearance-none"
      >
        <option value="Tiêu chuẩn">Phổi thẳng (Standard Chest)</option>
        <option value="Cột sống cổ">Cột sống cổ (Cervical Spine)</option>
        <option value="Cột sống thắt lưng">Cột sống thắt lưng (Lumbar Spine)</option>
        <option value="Xương khớp">Xương khớp (Extremities)</option>
        <option value="Khác">Khác / Theo chỉ định</option>
      </select>
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div v-for="tag in commonFindings" :key="tag"
           @click="toggleFinding(tag)"
           :class="['px-4 py-2 rounded-xl border-2 text-[10px] font-black uppercase tracking-widest cursor-pointer transition-all text-center',
                    result.findings.includes(tag) ? 'border-amber-500 bg-amber-50 text-amber-700' : 'border-slate-100 bg-white text-slate-400']">
        {{ tag }}
      </div>
    </div>

    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Mô tả chi tiết hình ảnh</label>
      <textarea 
        v-model="result.detailedDescription"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-indigo-500 outline-none min-h-[80px]"
        placeholder="Mô tả các bất thường quan sát được..."
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

const commonFindings = [
  'Tim phổi bình thường',
  'Xơ hóa đỉnh phổi',
  'Dày dính màng phổi',
  'Vẹo cột sống',
  'Thoái hóa đốt sống',
  'Nghi ngờ lao'
]

const result = reactive({
  region: props.modelValue.region || 'Tiêu chuẩn',
  findings: props.modelValue.findings || [],
  detailedDescription: props.modelValue.detailedDescription || ''
})

const toggleFinding = (tag) => {
  const idx = result.findings.indexOf(tag)
  if (idx > -1) result.findings.splice(idx, 1)
  else result.findings.push(tag)
}

watch(result, (newVal) => {
  emit('update:modelValue', { ...newVal })
}, { deep: true })

onMounted(() => {
  emit('update:modelValue', { ...result })
})
</script>
