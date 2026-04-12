<template>
  <div class="space-y-6 animate-fade-in pb-20 p-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-bold text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Building2 class="w-6 h-6" />
          </div>
          {{ i18n.t('companies.title') }}
        </h2>
        <p class="text-slate-400 font-semibold uppercase tracking-widest text-[10px] mt-2">{{ i18n.t('companies.subtitle') }}</p>
      </div>
      <button v-if="authStore.hasAnyRole('Admin', 'ContractManager')"
              @click="showForm = !showForm"
              :disabled="isLoading"
               class="btn-premium primary shadow-lg shadow-primary/30 hover:shadow-primary/50 hover:-translate-y-1 transition-all">
        <Plus class="w-5 h-5" />
        <span>{{ showForm ? i18n.t('companies.cancelBtn') : i18n.t('companies.addBtn') }}</span>
      </button>
    </div>

    <!-- Inline Add Form -->
    <CompanyInlineForm v-if="showForm" @cancel="showForm = false" @save="onSaveCompany" />

    <!-- Search & List Component -->
    <div class="premium-card bg-white/95 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden relative">
        <!-- Loading Overlay -->
        <div v-if="isLoading" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-50 flex items-center justify-center rounded-[2rem]">
          <div class="flex flex-col items-center gap-3">
            <div class="w-10 h-10 border-3 border-slate-200 border-t-primary rounded-full animate-spin"></div>
            <p class="text-xs font-semibold text-slate-600 uppercase tracking-widest">Đang tải...</p>
          </div>
        </div>

        <div class="p-6 border-b border-slate-100 flex items-center gap-4 bg-white/50">
            <div class="relative group flex-1">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                <input v-model="searchQuery" placeholder="Tìm tên công ty, mã số thuế..."
                       class="w-full pl-10 pr-4 py-2 rounded-xl bg-white border border-slate-200 focus:border-primary/30 focus:ring-2 focus:ring-primary/10 outline-none font-semibold text-sm text-slate-600 shadow-sm transition-all" />
            </div>
            <div class="px-4 py-2 bg-white rounded-xl border border-slate-100 text-[10px] font-semibold text-slate-400 uppercase tracking-widest whitespace-nowrap">
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
