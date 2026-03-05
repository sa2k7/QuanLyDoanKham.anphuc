<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="fixed inset-0 z-[9998] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4" @click.self="cancel">
        <div class="bg-white max-w-md w-full p-8 rounded-[2.5rem] shadow-2xl animate-pop-in">
          <div class="flex items-center space-x-4 mb-6">
            <div :class="['p-4 rounded-2xl', iconBgClass]">
              <component :is="iconComponent" class="w-8 h-8" :class="iconColorClass" />
            </div>
            <div class="flex-1">
              <h3 class="text-xl font-black text-slate-800">{{ title }}</h3>
            </div>
          </div>
          
          <p class="text-slate-600 font-bold mb-8 leading-relaxed">{{ message }}</p>
          
          <div class="flex space-x-4">
            <button @click="cancel" class="flex-1 px-6 py-3 rounded-xl font-black text-slate-500 bg-slate-100 hover:bg-slate-200 transition-all border border-slate-200">
              Hủy
            </button>
            <button @click="confirm" :class="['flex-1 px-6 py-3 rounded-xl font-black text-white transition-all', confirmBtnClass]">
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import { AlertTriangle, Trash2, CheckCircle } from 'lucide-vue-next'

const props = defineProps({
  modelValue: Boolean,
  title: { type: String, default: 'Xác nhận' },
  message: { type: String, required: true },
  confirmText: { type: String, default: 'Xác nhận' },
  variant: { type: String, default: 'danger' } // danger, warning, success
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const iconComponent = computed(() => {
  switch(props.variant) {
    case 'danger': return Trash2
    case 'warning': return AlertTriangle
    case 'success': return CheckCircle
    default: return AlertTriangle
  }
})

const iconBgClass = computed(() => {
  switch(props.variant) {
    case 'danger': return 'bg-rose-50'
    case 'warning': return 'bg-amber-50'
    case 'success': return 'bg-emerald-50'
    default: return 'bg-slate-50'
  }
})

const iconColorClass = computed(() => {
  switch(props.variant) {
    case 'danger': return 'text-rose-600'
    case 'warning': return 'text-amber-600'
    case 'success': return 'text-emerald-600'
    default: return 'text-slate-600'
  }
})

const confirmBtnClass = computed(() => {
  switch(props.variant) {
    case 'danger': return 'bg-rose-600 hover:bg-rose-700'
    case 'warning': return 'bg-amber-600 hover:bg-amber-700'
    case 'success': return 'bg-emerald-600 hover:bg-emerald-700'
    default: return 'bg-slate-900 hover:bg-black'
  }
})

const confirm = () => {
  emit('confirm')
  emit('update:modelValue', false)
}

const cancel = () => {
  emit('cancel')
  emit('update:modelValue', false)
}
</script>

<style scoped>
.modal-enter-active, .modal-leave-active {
  transition: opacity 0.3s ease;
}
.modal-enter-from, .modal-leave-to {
  opacity: 0;
}

.animate-pop-in {
  animation: popIn 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

@keyframes popIn {
  0% {
    opacity: 0;
    transform: scale(0.8);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
