<template>
  <div class="relative">
    <input
      type="text"
      :value="displayValue"
      @input="onInput"
      @blur="onBlur"
      ref="inputRef"
      :placeholder="placeholder"
      :class="customClass"
      :required="required"
    />
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: [Number, String],
    default: 0
  },
  placeholder: {
    type: String,
    default: '0'
  },
  customClass: {
    type: String,
    default: 'input-premium w-full'
  },
  required: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue'])
const inputRef = ref(null)

// Format number to string with dots: 1000000 -> 1.000.000
const formatNumber = (val) => {
  if (val === null || val === undefined || val === '') return ''
  const number = parseFloat(String(val).replace(/[^\d]/g, ''))
  if (isNaN(number)) return ''
  return new Intl.NumberFormat('vi-VN').format(number)
}

// Internal state for display
const displayValue = ref(formatNumber(props.modelValue))

watch(() => props.modelValue, (newVal) => {
  const formatted = formatNumber(newVal)
  if (formatted !== displayValue.value) {
    displayValue.value = formatted
  }
})

const onInput = (event) => {
  const value = event.target.value
  // Remove non-digits to get raw number
  const rawValue = value.replace(/[^\d]/g, '')
  const numberValue = rawValue === '' ? 0 : parseInt(rawValue, 10)

  // Update display (formatted)
  displayValue.value = formatNumber(numberValue)
  
  // Emit raw number to parent
  emit('update:modelValue', numberValue)
}

const onBlur = () => {
  // Ensure consistency on blur
  displayValue.value = formatNumber(props.modelValue)
}
</script>
