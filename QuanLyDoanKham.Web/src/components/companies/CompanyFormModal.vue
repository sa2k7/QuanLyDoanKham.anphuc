<template>
  <div class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-md p-4 overflow-y-auto">
      <div class="bg-white/95 backdrop-blur-3xl w-full max-w-2xl rounded-[2.5rem] border border-slate-100 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
          <!-- Header Accent Line -->
          <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

          <!-- Close Button -->
          <button @click="$emit('close')" class="absolute top-8 right-8 bg-white p-2 rounded-full hover:bg-rose-50 hover:text-rose-600 transition-all text-slate-600 z-[60] flex items-center justify-center border-2 border-slate-100 shadow-sm">
              <X class="w-5 h-5" />
          </button>

          <div class="relative z-10 pt-8">
              <div class="p-10 pb-6">
                  <div class="flex items-center gap-4 mb-8">
                      <div class="w-14 h-14 bg-teal-50 text-teal-600 rounded-3xl flex items-center justify-center shadow-inner border border-teal-100">
                          <Building2 class="w-7 h-7" />
                      </div>
                      <div>
                          <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest">Cập nhật công ty</h3>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Chỉnh sửa thông tin hồ sơ doanh nghiệp</p>
                      </div>
                  </div>
                  
                  <form id="companyModalForm" @submit.prevent="handleSave" class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-5">
                      <div class="md:col-span-2 flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên Công Ty *</label>
                          <input v-model="formData.shortName" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="VD: VinCom..." />
                      </div>
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên người đại diện *</label>
                          <input v-model="formData.companyName" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="Họ và tên đại diện" />
                      </div>
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã số thuế *</label>
                          <CodeInput v-model="formData.taxCode" :required="true" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="0101..." />
                      </div>
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">SĐT liên hệ công ty</label>
                          <CodeInput v-model="formData.phoneNumber" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="090..." />
                      </div>
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">SĐT người đại diện</label>
                          <CodeInput v-model="formData.contactPhone" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="09x..." />
                      </div>
                      <div class="md:col-span-2 flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Địa chỉ trụ sở</label>
                          <input v-model="formData.address" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="Số nhà, đường, quận..." />
                      </div>
                  </form>
              </div>

              <div class="px-10 pb-10 pt-4 bg-white relative z-20">
                  <div class="flex flex-wrap items-center gap-4">
                      <button v-if="formData.companyId && authStore.isAdmin" type="button" @click="$emit('delete', formData.companyId)" 
                              class="w-14 h-14 bg-rose-50 text-rose-500 rounded-2xl hover:bg-rose-500 hover:text-white transition-all shadow-sm flex items-center justify-center border border-rose-100">
                          <Trash2 class="w-6 h-6" />
                      </button>
                      <div class="flex-1"></div>
                      <button type="button" @click="$emit('close')" 
                              class="bg-rose-50 border-2 border-rose-100 text-rose-600 px-8 py-4 rounded-2xl font-black hover:bg-rose-600 hover:text-white transition-all uppercase tracking-widest text-xs shadow-sm shadow-rose-100/50">
                          {{ i18n.t('common.cancel') }}
                      </button>
                       <button form="companyModalForm" type="submit" 
                               class="flex-1 bg-gradient-to-r from-indigo-500 to-indigo-600 text-white px-10 py-4 rounded-2xl font-black shadow-lg shadow-indigo-500/30 uppercase tracking-widest text-xs hover:shadow-indigo-500/50 hover:-translate-y-1 active:scale-95 transition-all">{{ i18n.t('common.save') }}</button>
                  </div>
              </div>
          </div>
      </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Building2, X, Trash2 } from 'lucide-vue-next'
import { useAuthStore } from '../../stores/auth'
import { useI18nStore } from '../../stores/i18n'
import CodeInput from '../CodeInput.vue'

const authStore = useAuthStore()
const i18n = useI18nStore()

const props = defineProps({
  company: {
      type: Object,
      required: true
  }
})

const emit = defineEmits(['close', 'save', 'delete'])

const formData = ref({ ...props.company })

watch(() => props.company, (newVal) => {
  formData.value = { ...newVal }
}, { deep: true })

const handleSave = () => {
  emit('save', formData.value)
}
</script>
