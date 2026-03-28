<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Building2 class="w-6 h-6" />
          </div>
          Công ty Đối tác
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-indigo-600 font-black tabular-nums">{{ String(list.length).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-widest text-[10px] mt-2">Nội bộ: Quản lý thông tin pháp nhân khách hàng</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'"
              @click="showForm = !showForm"
               class="btn-premium bg-gradient-to-r from-teal-500 to-emerald-500 text-white px-8 py-3 rounded-xl shadow-lg shadow-teal-500/30 hover:shadow-teal-500/50 hover:-translate-y-1 transition-all font-black">
        <Plus class="w-5 h-5" />
        <span>{{ showForm ? 'HỦY BỎ' : 'THÊM CÔNG TY' }}</span>
      </button>
    </div>

    <!-- Stats Section -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
        <StatCard 
            title="Tổng đối tác pháp nhân"
            :value="String(list.length).padStart(3, '0')"
            :icon="Building2"
            variant="indigo"
            subtext="Công ty đối tác"
        />
        <StatCard 
            title="Mới trong tháng"
            :value="String(list.length > 0 ? 1 : 0).padStart(3, '0')"
            :icon="PlusCircle"
            variant="emerald"
            subtext="Khai báo mới"
        />
        <StatCard 
            title="Hợp đồng liên kết"
            :value="String(list.length).padStart(3, '0')"
            :icon="FileText"
            variant="sky"
            subtext="Dự án thực tế"
        />
    </div>

    <!-- Inline Add Form (like Contracts.vue) -->
    <div v-if="showForm" class="premium-card p-10 bg-white/80 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 animate-slide-up">
        <div class="flex items-center gap-4 mb-8">
            <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center">
                <Building2 class="w-6 h-6" />
            </div>
            <div>
                <h3 class="text-xl font-black text-slate-800">Khai báo Công ty mới</h3>
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">Thêm mới đối tác pháp nhân vào hệ thống</p>
            </div>
        </div>
        <form @submit.prevent="saveCompany" class="grid grid-cols-1 lg:grid-cols-3 gap-6 animate-slide-up">
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black tracking-widest text-slate-400 ml-1 uppercase">Tên Công Ty *</label>
                <input v-model="currentCompany.shortName" type="text" required
                       class="input-premium w-full !bg-white border-2 border-blue-600/20 focus:border-blue-600 shadow-sm"
                       placeholder="VD: VinCom, Techcombank..." />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black tracking-widest text-slate-400 ml-1 uppercase">Tên người đại diện *</label>
                <input v-model="currentCompany.companyName" type="text" required
                       class="input-premium w-full"
                       placeholder="Họ và tên người đại diện pháp luật" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã số thuế *</label>
                <CodeInput v-model="currentCompany.taxCode" :required="true" customClass="input-premium w-full" placeholder="0101..." />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số điện thoại liên hệ công ty</label>
                <CodeInput v-model="currentCompany.phoneNumber" customClass="input-premium w-full" placeholder="090..." />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số điện thoại người đại diện</label>
                <CodeInput v-model="currentCompany.contactPhone" customClass="input-premium w-full" placeholder="09x - sđt trực tiếp đại diện" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Địa chỉ trụ sở</label>
                <input v-model="currentCompany.address" class="input-premium w-full" placeholder="Số nhà, đường, quận..." />
            </div>
            <div class="lg:col-span-3 flex justify-end pt-2">
                 <button type="submit" class="btn-premium bg-gradient-to-r from-teal-500 to-emerald-500 text-white px-12 py-4 rounded-xl shadow-lg shadow-teal-500/30 hover:shadow-teal-500/50 hover:-translate-y-1 transition-all font-black">XÁC NHẬN KHAI BÁO</button>
            </div>
        </form>
    </div>

    <!-- Search & List -->
    <div class="premium-card bg-white/95 backdrop-blur-xl rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden mt-4">
        <div class="p-6 border-b border-slate-100 flex items-center gap-4 bg-white/50">
            <div class="relative group flex-1">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                <input v-model="searchQuery" placeholder="Tìm tên công ty, mã số thuế..."
                       class="w-full pl-10 pr-4 py-2 rounded-xl bg-white border border-slate-200 focus:border-indigo-600/20 outline-none font-black text-xs text-slate-600 shadow-sm transition-all" />
            </div>
            <div class="px-4 py-2 bg-white rounded-xl border border-slate-100 text-[10px] font-black text-slate-400 uppercase tracking-widest ">
                Kết quả: <span class="text-indigo-600">{{ filteredList.length }}</span>
            </div>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-4 text-center w-16">STT</th>
                        <th class="p-4">Tên công ty</th>
                        <th class="p-4">Người đại diện</th>
                        <th class="p-4">Mã số thuế</th>
                        <th class="p-4">SĐT Công ty</th>
                        <th class="p-4">SĐT Đại diện</th>
                        <th class="p-4 text-center">Tác vụ</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(item, index) in filteredList" :key="item.companyId"
                        class="text-xs hover:bg-slate-50/50 transition-all">
                        <td class="p-4 text-center font-black text-slate-400 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                        <td class="p-4">
                            <div class="flex items-center gap-3">
                                <div class="w-8 h-8 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center font-black">
                                    {{ (item.shortName || item.companyName).charAt(0) }}
                                </div>
                                <div class="font-black text-slate-800">{{ item.shortName || '—' }}</div>
                            </div>
                        </td>
                        <td class="p-4 text-slate-600">{{ item.companyName }}</td>
                        <td class="p-4 font-black text-slate-600 ">{{ item.taxCode }}</td>
                        <td class="p-4 font-black text-slate-500">{{ item.phoneNumber }}</td>
                        <td class="p-4 font-black text-slate-500">{{ item.contactPhone || '—' }}</td>
                        <td class="p-4 text-center">
                            <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
                                    @click="openModal(item)" class="btn-action-premium variant-indigo text-slate-400" title="Chỉnh sửa">
                                <Edit3 class="w-5 h-5" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="filteredList.length === 0">
                        <td colspan="6" class="py-20 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <Building2 class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Chưa có công ty phù hợp</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Modal Form (for UPDATE only) -->
    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/60 backdrop-blur-md p-4 overflow-y-auto">
          <div class="bg-white/95 backdrop-blur-3xl w-full max-w-2xl rounded-[2.5rem] border border-slate-100 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
              <!-- No Border Overlay for Glassmorphism -->
              
              <!-- Header Accent Line -->
              <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

              <!-- Close Button -->
              <button @click="showModal = false" class="absolute top-8 right-8 bg-slate-100 p-2 rounded-full hover:bg-slate-200 transition-all text-slate-500 z-[60] flex items-center justify-center border border-slate-200 shadow-sm">
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
                              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Chỉnh sửa thông tin hồ sơ đối tác</p>
                          </div>
                      </div>
                      
                      <form id="companyForm" @submit.prevent="saveCompany" class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-5">
                          <div class="md:col-span-2 flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên Công Ty *</label>
                              <input v-model="currentCompany.shortName" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="VD: VinCom..." />
                          </div>
                          <div class="flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Tên người đại diện *</label>
                              <input v-model="currentCompany.companyName" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="Họ và tên đại diện" />
                          </div>
                          <div class="flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Mã số thuế *</label>
                              <CodeInput v-model="currentCompany.taxCode" :required="true" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="0101..." />
                          </div>
                          <div class="flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">SĐT liên hệ công ty</label>
                              <CodeInput v-model="currentCompany.phoneNumber" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="090..." />
                          </div>
                          <div class="flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">SĐT người đại diện</label>
                              <CodeInput v-model="currentCompany.contactPhone" customClass="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="09x..." />
                          </div>
                          <div class="md:col-span-2 flex flex-col gap-2">
                              <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Địa chỉ trụ sở</label>
                              <input v-model="currentCompany.address" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="Số nhà, đường, quận..." />
                          </div>
                      </form>
                  </div>

                  <div class="px-10 pb-10 pt-4 bg-white relative z-20">
                      <div class="flex flex-wrap items-center gap-4">
                          <button v-if="currentCompany.companyId && authStore.role === 'Admin'" type="button" @click="deleteCompany" 
                                  class="w-14 h-14 bg-rose-50 text-rose-500 rounded-2xl hover:bg-rose-500 hover:text-white transition-all shadow-sm flex items-center justify-center border border-rose-100">
                              <Trash2 class="w-6 h-6" />
                          </button>
                          <div class="flex-1"></div>
                          <button type="button" @click="showModal = false" 
                                  class="bg-white border-2 border-slate-100 text-slate-400 px-8 py-4 rounded-2xl font-black hover:bg-slate-50 transition-all uppercase tracking-widest text-xs">HỦY</button>
                           <button form="companyForm" type="submit" 
                                   class="flex-1 bg-gradient-to-r from-indigo-500 to-indigo-600 text-white px-10 py-4 rounded-2xl font-black shadow-lg shadow-indigo-500/30 uppercase tracking-widest text-xs hover:shadow-indigo-500/50 hover:-translate-y-1 active:scale-95 transition-all">LƯU THÔNG TIN</button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Building2, Plus, Search, MapPin, FileText, PlusCircle, X, Edit3, Trash2 } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import StatCard from '../components/StatCard.vue'
import CodeInput from '../components/CodeInput.vue'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const showModal = ref(false)
const showForm = ref(false)
const searchQuery = ref('')
const currentCompany = ref({})

const filteredList = computed(() => {
    if (!searchQuery.value) return list.value
    const q = searchQuery.value.toLowerCase()
    return list.value.filter(c =>
        c.companyName.toLowerCase().includes(q) ||
        (c.shortName || '').toLowerCase().includes(q) ||
        c.taxCode.includes(q)
    )
})

const fetchList = async () => {
    try {
        const res = await axios.get('/api/Companies')
        list.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu đối tác") }
}

// Open modal only for EDIT; add is handled by inline form
const openModal = (company = null) => {
    if (!company) return // Add is handled by showForm inline
    currentCompany.value = company ? { ...company } : { shortName: '', companyName: '', taxCode: '', address: '', phoneNumber: '' }
    showModal.value = true
}

const saveCompany = async () => {
    if (!currentCompany.value.companyId) {
        // Duplicate check for new entry
        const duplicate = list.value.find(c =>
            c.companyName.toLowerCase() === currentCompany.value.companyName.toLowerCase() ||
            c.taxCode === currentCompany.value.taxCode
        )
        if (duplicate) {
            toast.warning(`Phát hiện trùng lặp với công ty: ${duplicate.companyName}`)
            return
        }
    }

    try {
        if (currentCompany.value.companyId) {
            await axios.put(`/api/Companies/${currentCompany.value.companyId}`, currentCompany.value)
        } else {
            await axios.post('/api/Companies', currentCompany.value)
        }
        toast.success("Đã ghi nhận dữ liệu đối tác!")
        showModal.value = false
        showForm.value = false
        currentCompany.value = {}
        fetchList()
    } catch (e) {
        toast.error(e.response?.data || "Lỗi khi lưu dữ liệu")
    }
}

const deleteCompany = async () => {
    if (!confirm("Bạn có chắc chắn muốn xóa đối tác này?")) return
    try {
        await axios.delete(`/api/Companies/${currentCompany.value.companyId}`)
        toast.success("Đã xóa!")
        showModal.value = false
        fetchList()
    } catch (e) {
        toast.error(e.response?.data || "Không thể xóa đối tác này")
    }
}

onMounted(fetchList)
</script>
