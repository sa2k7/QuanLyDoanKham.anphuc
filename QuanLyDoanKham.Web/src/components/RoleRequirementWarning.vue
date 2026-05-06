<template>
  <div v-if="warnings.length > 0" class="flex flex-wrap gap-1.5">
    <div
      v-for="w in warnings"
      :key="w.role"
      :class="[
        'flex items-center gap-1.5 px-2 py-1 rounded-lg border text-[9px] font-black uppercase tracking-widest',
        w.type === 'under'
          ? 'bg-amber-50 border-amber-200 text-amber-700'
          : 'bg-rose-50 border-rose-200 text-rose-700'
      ]"
    >
      <component :is="w.type === 'under' ? AlertTriangle : AlertCircle" class="w-3 h-3 flex-shrink-0" />
      <span>{{ w.role }}</span>
      <span class="opacity-60">{{ w.assigned }}/{{ w.required }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { AlertTriangle, AlertCircle } from 'lucide-vue-next'

/**
 * RoleRequirementWarning
 *
 * Displays a warning badge per role where assignedCount !== requiredCount.
 * - Under-assigned (assigned < required): amber badge with AlertTriangle
 * - Over-assigned  (assigned > required): rose badge with AlertCircle
 * - Exact match: no badge rendered
 *
 * Props:
 *   requirements: RoleRequirementWithCountDto[]
 *     { role: string, requiredCount: number, assignedCount: number }[]
 */
const props = defineProps({
  requirements: {
    type: Array,
    default: () => []
  }
})

const warnings = computed(() => {
  if (!props.requirements) return []
  return props.requirements
    .filter(r => r.assignedCount !== r.requiredCount)
    .map(r => ({
      role: r.role,
      required: r.requiredCount,
      assigned: r.assignedCount,
      type: r.assignedCount < r.requiredCount ? 'under' : 'over'
    }))
})
</script>
