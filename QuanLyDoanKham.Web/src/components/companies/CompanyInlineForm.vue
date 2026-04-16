<template>
  <div class="premium-card p-10 bg-white/80 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 animate-slide-up">
      <div class="flex items-center gap-4 mb-8">
          <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center">
              <Building2 class="w-6 h-6" />
          </div>
          <div>
              <h3 class="text-xl font-black text-slate-800">{{ i18n.t('companies.formTitle') }}</h3>
              <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ i18n.t('companies.formSubtitle') }}</p>
          </div>
      </div>
      <form @submit.prevent="handleSave" class="grid grid-cols-1 lg:grid-cols-3 gap-6 animate-slide-up">
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black tracking-widest text-slate-400 ml-1 uppercase">Tên Công Ty *</label>
              <input v-model="formData.shortName" type="text" required
                     class="input-premium w-full !bg-white border-2 border-blue-600/20 focus:border-blue-600 shadow-sm"
                     placeholder="VD: VinCom, Techcombank..." />
          </div>
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black tracking-widest text-slate-400 ml-1 uppercase">Tên người đại diện *</label>
              <input v-model="formData.companyName" type="text" required
                     class="input-premium w-full"
                     placeholder="Họ và tên người đại diện pháp luật" />
          </div>
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã số thuế *</label>
              <CodeInput v-model="formData.taxCode" :required="true" customClass="input-premium w-full" placeholder="0101..." />
          </div>
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số điện thoại liên hệ công ty</label>
              <CodeInput v-model="formData.phoneNumber" :maxlength="10" customClass="input-premium w-full" placeholder="090..." />
          </div>
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số điện thoại người đại diện</label>
              <CodeInput v-model="formData.contactPhone" :maxlength="10" customClass="input-premium w-full" placeholder="09x - sđt trực tiếp đại diện" />
          </div>
          <div class="flex flex-col gap-2">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Địa chỉ trụ sở</label>
              <input v-model="formData.address" class="input-premium w-full" placeholder="Số nhà, đường, quận..." />
          </div>
          <div class="lg:col-span-3 flex justify-end pt-2 gap-4">
               <button type="button" @click="$emit('cancel')" 
                       class="bg-rose-50 border-2 border-rose-100 text-rose-600 px-8 py-4 rounded-xl font-black hover:bg-rose-600 hover:text-white transition-all uppercase tracking-widest text-xs shadow-sm shadow-rose-100/50">HỦY BỎ</button>
               <button type="submit" class="btn-premium bg-gradient-to-r from-teal-500 to-emerald-500 text-white px-12 py-4 rounded-xl shadow-lg shadow-teal-500/30 hover:shadow-teal-500/50 hover:-translate-y-1 transition-all font-black">XÁC NHẬN KHAI BÁO</button>
          </div>
      </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Building2 } from 'lucide-vue-next'
import { useI18nStore } from '../../stores/i18n'
import CodeInput from '../CodeInput.vue'

const i18n = useI18nStore()

const emit = defineEmits(['cancel', 'save'])

const getInitialData = () => ({ shortName: '', companyName: '', taxCode: '', address: '', phoneNumber: '', contactPhone: '' })
const formData = ref(getInitialData())

const handleSave = () => {
  emit('save', formData.value)
  // Optionally reset after saving; though the parent might destroy the component
  formData.value = getInitialData()
}
</script>
