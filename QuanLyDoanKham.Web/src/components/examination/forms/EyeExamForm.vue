<template>
  <div class="grid grid-cols-2 gap-6">
    <!-- Mắt Phải -->
    <div class="bg-blue-50 p-4 rounded-2xl border border-blue-100">
      <h4 class="font-black text-blue-800 mb-4 flex items-center gap-2">MẮT PHẢI (MP)</h4>
      
      <div class="space-y-4">
        <div class="form-group">
          <label class="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Không Kính</label>
          <div class="flex items-center gap-2">
            <input v-model="data.rightEyeUncorrected" type="number" step="0.1" max="10" placeholder="10" class="w-full bg-white border border-blue-200 rounded-xl py-2 px-3 text-slate-800 font-bold focus:border-blue-500 outline-none"/>
            <span class="text-slate-400 font-black">/ 10</span>
          </div>
        </div>
        
        <div class="form-group">
          <label class="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Có Kính</label>
          <div class="flex items-center gap-2">
            <input v-model="data.rightEyeCorrected" type="number" step="0.1" max="10" placeholder="10" class="w-full bg-white border border-blue-200 rounded-xl py-2 px-3 text-slate-800 font-bold focus:border-blue-500 outline-none"/>
            <span class="text-slate-400 font-black">/ 10</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Mắt Trái -->
    <div class="bg-emerald-50 p-4 rounded-2xl border border-emerald-100">
      <h4 class="font-black text-emerald-800 mb-4 flex items-center gap-2">MẮT TRÁI (MT)</h4>
      
      <div class="space-y-4">
        <div class="form-group">
          <label class="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Không Kính</label>
          <div class="flex items-center gap-2">
            <input v-model="data.leftEyeUncorrected" type="number" step="0.1" max="10" placeholder="10" class="w-full bg-white border border-emerald-200 rounded-xl py-2 px-3 text-slate-800 font-bold focus:border-emerald-500 outline-none"/>
            <span class="text-slate-400 font-black">/ 10</span>
          </div>
        </div>
        
        <div class="form-group">
          <label class="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Có Kính</label>
          <div class="flex items-center gap-2">
            <input v-model="data.leftEyeCorrected" type="number" step="0.1" max="10" placeholder="10" class="w-full bg-white border border-emerald-200 rounded-xl py-2 px-3 text-slate-800 font-bold focus:border-emerald-500 outline-none"/>
            <span class="text-slate-400 font-black">/ 10</span>
          </div>
        </div>
      </div>
    </div>
    
    <div class="col-span-2 form-group mt-2">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Ghi chú thêm (Các bệnh lý về mắt)</label>
      <textarea 
        v-model="data.eyeDiseases" 
        placeholder="Cận thị, Viễn thị, Mù màu..."
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-medium focus:border-indigo-400 outline-none min-h-[80px]"
      ></textarea>
    </div>
  </div>
</template>

<script setup>
import { reactive, watch } from 'vue'

const props = defineProps({
  modelValue: { type: Object, required: true }
})

const emit = defineEmits(['update:modelValue'])

const data = reactive({
  rightEyeUncorrected: props.modelValue.rightEyeUncorrected || '',
  leftEyeUncorrected: props.modelValue.leftEyeUncorrected || '',
  rightEyeCorrected: props.modelValue.rightEyeCorrected || '',
  leftEyeCorrected: props.modelValue.leftEyeCorrected || '',
  eyeDiseases: props.modelValue.eyeDiseases || ''
})

watch(data, (newVal) => {
  emit('update:modelValue', newVal)
}, { deep: true })
</script>
