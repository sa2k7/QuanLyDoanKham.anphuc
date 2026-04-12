<template>
  <div class="overflow-x-auto">
      <table class="w-full text-left">
          <thead class="bg-slate-50 text-[10px] font-semibold uppercase tracking-widest text-slate-500 border-b border-slate-100">
              <tr>
                  <th class="p-4 text-center w-16">{{ i18n.t('common.stt') }}</th>
                  <th class="p-4">{{ i18n.t('companies.table.info') }}</th>
                  <th class="p-4">Người đại diện</th>
                  <th class="p-4">{{ i18n.t('companies.table.taxCode') }}</th>
                  <th class="p-4">SĐT Công ty</th>
                  <th class="p-4">SĐT Đại diện</th>
                  <th class="p-4 text-center">{{ i18n.t('common.actions') }}</th>
              </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
              <tr v-for="(item, index) in items" :key="item.companyId"
                  class="text-sm hover:bg-slate-50/70 transition-colors duration-200">
                  <td class="p-4 text-center font-semibold text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                  <td class="p-4">
                      <div class="flex items-center gap-3">
                          <div class="w-10 h-10 rounded-lg bg-gradient-to-br from-primary/20 to-primary/10 text-primary flex items-center justify-center font-bold text-sm shadow-sm">
                              {{ (item.shortName || item.companyName || 'C').charAt(0).toUpperCase() }}
                          </div>
                          <div class="font-semibold text-slate-800">{{ item.shortName || '—' }}</div>
                      </div>
                  </td>
                  <td class="p-4 text-slate-700 font-medium">{{ item.companyName }}</td>
                  <td class="p-4 font-semibold text-slate-600 font-mono text-xs">{{ item.taxCode }}</td>
                  <td class="p-4 font-medium text-slate-600">{{ item.phoneNumber || '—' }}</td>
                  <td class="p-4 font-medium text-slate-600">{{ item.contactPhone || '—' }}</td>
                  <td class="p-4 text-center">
                      <button v-if="authStore.hasAnyRole('Admin', 'ContractManager')" 
                              @click="$emit('edit', item)" 
                              class="btn-action-premium variant-indigo" 
                              title="Chỉnh sửa">
                          <Edit3 class="w-5 h-5" />
                      </button>
                  </td>
              </tr>
              <tr v-if="items.length === 0">
                  <td colspan="7" class="py-20 text-center">
                      <div class="flex flex-col items-center justify-center gap-4">
                          <Building2 class="w-12 h-12 text-slate-200" />
                          <p class="text-slate-400 font-semibold uppercase tracking-widest text-xs">Chưa có công ty phù hợp</p>
                      </div>
                  </td>
              </tr>
          </tbody>
      </table>
  </div>
</template>

<script setup>
import { Building2, Edit3 } from 'lucide-vue-next'
import { useAuthStore } from '../../stores/auth'
import { useI18nStore } from '../../stores/i18n'

const authStore = useAuthStore()
const i18n = useI18nStore()

defineProps({
  items: {
      type: Array,
      required: true
  },
  loading: {
      type: Boolean,
      default: false
  }
})

defineEmits(['edit'])
</script>
