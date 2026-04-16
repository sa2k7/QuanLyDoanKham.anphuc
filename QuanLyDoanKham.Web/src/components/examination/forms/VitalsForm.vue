<template>
  <div class="grid grid-cols-2 gap-4">
    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Mạch (Lần/phút)</label>
      <input 
        v-model="data.pulse" 
        type="number"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-rose-400 outline-none"
      />
    </div>

    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Huyết Áp (mmHg)</label>
      <div class="flex items-center gap-2">
        <input 
          v-model="data.bloodPressureSystolic" 
          type="number"
          placeholder="Tâm thu"
          class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-rose-400 outline-none"
        />
        <span class="text-slate-400 font-black">/</span>
        <input 
          v-model="data.bloodPressureDiastolic" 
          type="number"
          placeholder="Tâm trương"
          class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-rose-400 outline-none"
        />
      </div>
    </div>

    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Chiều cao (cm)</label>
      <input 
        v-model="data.height" 
        type="number"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-indigo-400 outline-none"
      />
    </div>

    <div class="form-group">
      <label class="block text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Cân nặng (kg)</label>
      <input 
        v-model="data.weight" 
        type="number"
        class="w-full bg-slate-50 border-2 border-slate-100 rounded-2xl py-3 px-4 text-slate-800 font-bold focus:border-indigo-400 outline-none"
      />
    </div>

    <div class="form-group col-span-2 bg-indigo-50 p-4 rounded-2xl border border-indigo-100 flex items-center justify-between">
      <span class="text-sm font-bold text-indigo-800">Chỉ số BMI:</span>
      <span class="text-2xl font-black" :class="bmiColor">{{ bmiResult }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed, watch, reactive } from 'vue'

const props = defineProps({
  modelValue: { type: Object, required: true }
})

const emit = defineEmits(['update:modelValue'])

const data = reactive({
  pulse: props.modelValue.pulse || '',
  bloodPressureSystolic: props.modelValue.bloodPressureSystolic || '',
  bloodPressureDiastolic: props.modelValue.bloodPressureDiastolic || '',
  height: props.modelValue.height || '',
  weight: props.modelValue.weight || ''
})

const bmiResult = computed(() => {
  if (data.height && data.weight) {
    const h = data.height / 100
    const bmi = data.weight / (h * h)
    return bmi.toFixed(1)
  }
  return '--'
})

const bmiColor = computed(() => {
  if (bmiResult.value === '--') return 'text-slate-400'
  const val = parseFloat(bmiResult.value)
  if (val < 18.5) return 'text-orange-500' // Underweight
  if (val < 25) return 'text-emerald-500' // Normal
  if (val < 30) return 'text-rose-400' // Overweight
  return 'text-rose-600' // Obese
})

watch(data, (newVal) => {
  emit('update:modelValue', { ...newVal, bmi: bmiResult.value })
}, { deep: true })
</script>
