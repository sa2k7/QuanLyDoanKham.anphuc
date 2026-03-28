<template>
  <div 
    class="bg-white/95 backdrop-blur-md p-8 rounded-[2.5rem] border border-slate-100 shadow-[0_8px_30px_rgb(0,0,0,0.04)] relative overflow-hidden group transition-all hover:shadow-[0_15px_40px_-15px_rgba(0,0,0,0.08)] hover:-translate-y-1"
    :class="containerClass"
  >
    <!-- Background Decor Icon -->
    <div 
      v-if="icon" 
      class="absolute -right-4 -bottom-4 w-24 h-24 rotate-12 opacity-[0.03] group-hover:opacity-10 transition-all group-hover:scale-110 pointer-events-none"
    >
      <component :is="icon" class="w-full h-full" />
    </div>

    <!-- Label -->
    <p class="text-[10px] font-black uppercase tracking-[0.3em] text-slate-400 mb-4 flex items-center gap-2">
      <template v-if="smallIcon">
        <component :is="smallIcon" class="w-3 h-3" />
      </template>
      {{ title || '---' }}
    </p>

    <!-- Value & Trend -->
    <div class="flex items-baseline gap-3 flex-wrap">
      <p 
        class="text-3xl font-black tabular-nums leading-none transition-colors"
        :class="valueClass"
      >
        {{ value }}
      </p>
      
      <span 
        v-if="trend" 
        :class="['text-[10px] font-black px-3 py-1 rounded-full uppercase tracking-widest', trendClass]"
      >
        {{ trend }}
      </span>
    </div>

    <!-- Subtext / Progress -->
    <div v-if="subtext || progress !== undefined" class="mt-4">
      <p v-if="subtext" class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ subtext }}</p>
      
      <div v-if="progress !== undefined" class="mt-3 w-full bg-slate-100 rounded-full h-1.5 overflow-hidden">
        <div 
          class="h-full rounded-full transition-all duration-1000"
          :class="progressClass"
          :style="{ width: progress + '%' }"
        ></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  title: String,
  value: [String, Number],
  icon: [Object, Function],
  smallIcon: [Object, Function],
  trend: String,
  trendColor: {
    type: String,
    default: 'emerald' // emerald, rose, sky, amber, slate
  },
  subtext: String,
  progress: Number,
  variant: {
    type: String,
    default: 'default' // default, indigo, emerald, rose, sky, amber
  }
})

const valueClass = computed(() => {
  switch(props.variant) {
    case 'indigo': return 'text-indigo-600'
    case 'emerald': return 'text-emerald-600'
    case 'rose': return 'text-rose-600'
    case 'sky': return 'text-sky-600'
    case 'amber': return 'text-amber-600'
    default: return 'text-slate-900'
  }
})

const trendClass = computed(() => {
  switch(props.trendColor) {
    case 'rose': return 'bg-rose-50 text-rose-600'
    case 'sky': return 'bg-sky-50 text-sky-600'
    case 'amber': return 'bg-amber-50 text-amber-600'
    case 'slate': return 'bg-slate-100 text-slate-500'
    default: return 'bg-emerald-50 text-emerald-600'
  }
})

const progressClass = computed(() => {
  switch(props.variant) {
    case 'indigo': return 'bg-indigo-600'
    case 'emerald': return 'bg-emerald-500'
    case 'rose': return 'bg-rose-500'
    case 'sky': return 'bg-sky-500'
    case 'amber': return 'bg-amber-500'
    default: return 'bg-primary'
  }
})

const containerClass = computed(() => {
  // Can add conditional borders or shadows if needed
  return ''
})
</script>
