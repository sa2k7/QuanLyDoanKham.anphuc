<template>
  <Transition name="toast">
    <div v-if="visible" 
         :class="['fixed bottom-10 right-10 z-[100000] px-8 py-5 rounded-[2.5rem] shadow-[0_25px_60px_rgba(0,0,0,0.15)] border-2 backdrop-blur-3xl flex items-center space-x-6 max-w-md animate-slide-up', variantClasses]">
      <div :class="['p-3 rounded-2xl bg-white/50', iconColorClass.replace('text-', 'text-')]">
        <component :is="icon" class="w-6 h-6" />
      </div>
      <div class="flex-1">
        <p class="font-black text-sm ">{{ message }}</p>
      </div>
      <button @click="close" class="p-2 hover:bg-black/5 rounded-xl transition-all">
        <X class="w-5 h-5 opacity-40" />
      </button>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { CheckCircle, XCircle, AlertCircle, Info, X } from 'lucide-vue-next'

const props = defineProps({
  message: { type: String, required: true },
  type: { type: String, default: 'success' }, // success, error, warning, info
  duration: { type: Number, default: 4000 }
})

const emit = defineEmits(['close'])

const visible = ref(false)

const variantClasses = computed(() => {
  switch(props.type) {
    case 'success': return 'bg-success/10 border-success text-success'
    case 'error': return 'bg-rose-50 border-rose-300 text-rose-700'
    case 'warning': return 'bg-amber-50 border-amber-300 text-amber-700'
    case 'info': return 'bg-sky-50 border-sky-300 text-sky-700'
    default: return 'bg-slate-50 border-slate-300 text-slate-700'
  }
})

const iconColorClass = computed(() => {
  switch(props.type) {
    case 'success': return 'text-success'
    case 'error': return 'text-rose-600'
    case 'warning': return 'text-amber-600'
    case 'info': return 'text-sky-600'
    default: return 'text-slate-600'
  }
})

const icon = computed(() => {
  switch(props.type) {
    case 'success': return CheckCircle
    case 'error': return XCircle
    case 'warning': return AlertCircle
    case 'info': return Info
    default: return Info
  }
})

const close = () => {
  visible.value = false
  setTimeout(() => emit('close'), 300)
}

onMounted(() => {
  visible.value = true
  if (props.duration > 0) {
    setTimeout(close, props.duration)
  }
})
</script>

<style scoped>
.toast-enter-active, .toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(100px);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(100px);
}

.animate-slide-up {
  animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(50px) scale(0.9);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
