<template>
  <div class="space-y-6 animate-fade-in">
    <div class="grid grid-cols-2 gap-4">
      <div v-for="sample in samples" :key="sample.id" 
           @click="toggleSample(sample.id)"
           :class="['p-4 rounded-2xl border-2 cursor-pointer transition-all flex items-center justify-between', 
                    result[sample.id] ? 'border-indigo-500 bg-indigo-50 text-indigo-700' : 'border-slate-100 bg-white text-slate-400']">
        <div class="flex items-center gap-3">
          <div :class="['p-2 rounded-xl', result[sample.id] ? 'bg-indigo-100' : 'bg-slate-50']">
            <component :is="sample.icon" :size="20" />
          </div>
          <span class="font-black text-xs uppercase tracking-widest">{{ sample.label }}</span>
        </div>
        <Check v-if="result[sample.id]" :size="18" />
        <Circle v-else :size="18" class="opacity-20" />
      </div>
    </div>
    
    <div class="form-group mt-4">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Ghi chú bệnh phẩm</label>
      <input 
        v-model="result.sampleNote"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-indigo-500 outline-none"
        placeholder="Mô tả tình trạng mẫu (vỡ, thiếu,...) nếu có"
      />
    </div>
  </div>
</template>

<script setup>
import { reactive, watch, onMounted } from 'vue'
import { Droplets, TestTube, Check, Circle } from 'lucide-vue-next'

const props = defineProps({
  modelValue: { type: Object, default: () => ({}) }
})

const emit = defineEmits(['update:modelValue'])

const samples = [
  { id: 'blood', label: 'Máu (Blood)', icon: Droplets },
  { id: 'urine', label: 'Nước tiểu (Urine)', icon: TestTube },
]

const result = reactive({
  blood: props.modelValue.blood || false,
  urine: props.modelValue.urine || false,
  sampleNote: props.modelValue.sampleNote || ''
})

const toggleSample = (id) => {
  result[id] = !result[id]
}

watch(result, (newVal) => {
  emit('update:modelValue', { ...newVal })
}, { deep: true })

onMounted(() => {
  emit('update:modelValue', { ...result })
})
</script>
