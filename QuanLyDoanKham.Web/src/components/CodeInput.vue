<template>
  <div class="relative w-full">
    <input
      type="text"
      :value="displayValue"
      @input="onInput"
      @blur="onBlur"
      :placeholder="placeholder"
      :class="customClass"
      :required="required"
    />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  placeholder: {
    type: String,
    default: ''
  },
  customClass: {
    type: String,
    default: 'input-premium w-full'
  },
  required: {
    type: Boolean,
    default: false
  },
  maxlength: {
    type: [Number, String],
    default: null
  }
})

const emit = defineEmits(['update:modelValue'])

const formatCode = (val) => {
  if (val === null || val === undefined || val === '') return ''
  // Strip non-digits
  let raw = String(val).replace(/[^\d]/g, '')
  
  // Áp dụng giới hạn độ dài nếu có
  if (props.maxlength) {
    raw = raw.slice(0, Number(props.maxlength))
  }

  // Thêm dấu chấm mỗi 3 số từ dưới lên (ví dụ: 090.123.456)
  return raw.replace(/\B(?=(\d{3})+(?!\d))/g, '.')
}

const displayValue = ref(formatCode(props.modelValue))

watch(() => props.modelValue, (newVal) => {
  const formatted = formatCode(newVal)
  if (formatted !== displayValue.value) {
    displayValue.value = formatted
  }
})

const onInput = (event) => {
  let value = event.target.value
  let rawValue = value.replace(/[^\d]/g, '') // Luôn trả về chuỗi thuần túy (gôc) cho backend
  
  // Áp dụng giới hạn độ dài nếu có
  if (props.maxlength) {
    rawValue = rawValue.slice(0, Number(props.maxlength))
  }

  displayValue.value = formatCode(rawValue)
  emit('update:modelValue', rawValue)
}


const onBlur = () => {
  displayValue.value = formatCode(props.modelValue)
}
</script>
