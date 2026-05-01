<template>
  <div 
    class="bg-white p-2.5 rounded-lg border border-slate-100 relative overflow-hidden group transition-all hover:shadow-sm"
    :class="[containerClass, topBorderClass]"
  >
    <!-- Background Decor Icon -->
    <div 
      v-if="icon" 
      class="absolute -right-2 -bottom-2 w-12 h-12 rotate-12 opacity-[0.02] group-hover:opacity-5 transition-all group-hover:scale-110 pointer-events-none"
    >
      <component :is="icon" class="w-full h-full" />
    </div>

    <!-- Label -->
    <p class="text-[7.5px] font-black uppercase tracking-widest text-slate-400 mb-1 flex items-center gap-1">
      <template v-if="smallIcon">
        <component :is="smallIcon" class="w-2.5 h-2.5" />
      </template>
      {{ title || '---' }}
    </p>

    <!-- Value & Trend -->
    <div class="flex items-baseline gap-1.5 flex-wrap">
      <p 
        class="text-lg font-black tabular-nums leading-none transition-colors"
        :class="valueClass"
      >
        {{ value }}
      </p>
      
      <span 
        v-if="trend" 
        :class="['text-[7px] font-black px-1 py-0.5 rounded uppercase tracking-widest', trendClass]"
      >
        {{ trend }}
      </span>
    </div>

    <!-- Subtext / Progress -->
    <div v-if="subtext || progress !== undefined" class="mt-1.5">
      <p v-if="subtext" class="text-[7px] font-bold text-slate-400 uppercase tracking-widest leading-tight italic">{{ subtext }}</p>
      
      <div v-if="progress !== undefined" class="mt-1.5 w-full bg-slate-50 rounded-full h-1 overflow-hidden border border-slate-50">
        <div 
          class="h-full rounded-full transition-all duration-1000 shadow-inner"
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
  return ''
})

const topBorderClass = computed(() => {
  switch(props.variant) {
    case 'indigo': return 'border-t-2 border-t-indigo-500'
    case 'emerald': return 'border-t-2 border-t-emerald-500'
    case 'rose': return 'border-t-2 border-t-rose-500'
    case 'sky': return 'border-t-2 border-t-sky-500'
    case 'amber': return 'border-t-2 border-t-amber-500'
    default: return 'border-t-2 border-t-slate-500'
  }
})
</script>
