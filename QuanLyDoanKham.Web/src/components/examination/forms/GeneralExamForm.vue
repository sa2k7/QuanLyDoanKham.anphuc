<template>
  <div class="space-y-4">
    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Ghi chú chi tiết kết quả</label>
      <textarea 
        v-model="data.details" 
        placeholder="Nhập ghi chú kết quả tại đây..."
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-4 px-4 text-slate-800 font-medium focus:border-indigo-400 outline-none transition-all min-h-[120px]"
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
  details: props.modelValue.details || ''
})

watch(data, (newVal) => {
  emit('update:modelValue', newVal)
}, { deep: true })
</script>
