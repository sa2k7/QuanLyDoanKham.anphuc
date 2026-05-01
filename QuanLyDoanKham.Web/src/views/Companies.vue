<template>
  <div class="space-y-4 animate-fade-in p-3 bg-slate-50 relative overflow-y-auto h-full scrollbar-premium">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-3 mb-3">
      <div>
        <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
          <div class="w-8 h-8 bg-primary text-white rounded-lg flex items-center justify-center shadow-md">
            <Building2 class="w-4.5 h-4.5" />
          </div>
          {{ i18n.t('companies.title') }}
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[7.5px] mt-0.5">{{ i18n.t('companies.subtitle') }}</p>
      </div>
      <button v-if="authStore.hasAnyRole('Admin', 'ContractManager')"
              @click="showForm = !showForm"
              :disabled="isLoading"
               class="h-8 px-3 bg-primary text-white rounded-lg font-black text-[9px] uppercase shadow-sm flex items-center gap-1.5 hover:bg-primary/90 transition-all">
        <Plus class="w-4 h-4" />
        <span>{{ showForm ? i18n.t('companies.cancelBtn') : i18n.t('companies.addBtn') }}</span>
      </button>
    </div>

    <!-- Inline Add Form -->
    <CompanyInlineForm v-if="showForm" @cancel="showForm = false" @save="onSaveCompany" />

    <!-- Search & List Component -->
    <div class="premium-card bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden relative">
        <!-- Loading Overlay -->
        <div v-if="isLoading" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-50 flex items-center justify-center rounded-xl">
          <div class="flex flex-col items-center gap-2">
            <div class="w-8 h-8 border-2 border-slate-200 border-t-primary rounded-full animate-spin"></div>
            <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">Đang tải...</p>
          </div>
        </div>

        <div class="p-3 border-b border-slate-100 flex items-center gap-3 bg-white/50">
            <div class="relative group flex-1">
                <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-300 w-3.5 h-3.5" />
                <input v-model="searchQuery" placeholder="Tìm tên công ty, mã số thuế..."
                       class="w-full pl-9 pr-3 py-1.5 rounded-lg bg-white border border-slate-200 focus:border-primary/30 outline-none font-bold text-[11px] text-slate-600 shadow-sm transition-all" />
            </div>
            <div class="px-3 py-1.5 bg-slate-50 rounded-lg border border-slate-100 text-[8px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">
                Kết quả: <span class="text-primary font-bold">{{ filteredList.length }}</span>
            </div>
        </div>

        <CompanyListTable :items="filteredList" :loading="isLoading" @edit="openEditModal" />
    </div>

    <!-- Modal Form (for UPDATE only) -->
    <Teleport to="body">
      <CompanyFormModal 
        v-if="showModal" 
        :company="currentCompany" 
        @close="showModal = false" 
        @save="onSaveCompany" 
        @delete="onDeleteCompany" 
      />
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Building2, Plus, Search } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'
import { useCompanies } from '../composables/useCompanies'

// UI Components
import CompanyListTable from '../components/companies/CompanyListTable.vue'
import CompanyInlineForm from '../components/companies/CompanyInlineForm.vue'
import CompanyFormModal from '../components/companies/CompanyFormModal.vue'

const authStore = useAuthStore()
const i18n = useI18nStore()
const { filteredList, searchQuery, isLoading, fetchList, saveCompany, deleteCompany } = useCompanies()

const showModal = ref(false)
const showForm = ref(false)
const currentCompany = ref({})

// Mode Edit (Add handled by Inline form)
const openEditModal = (company) => {
    currentCompany.value = { ...company }
    showModal.value = true
}

// Global Handlers
const onSaveCompany = async (companyData) => {
    const success = await saveCompany(companyData)
    if (success) {
        showModal.value = false
        showForm.value = false
    }
}

const onDeleteCompany = async (companyId) => {
    const success = await deleteCompany(companyId)
    if (success) {
        showModal.value = false
    }
}

onMounted(() => {
    fetchList()
})
</script>
