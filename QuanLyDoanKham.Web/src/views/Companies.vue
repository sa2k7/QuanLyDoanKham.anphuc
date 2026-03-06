<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Building2 class="w-6 h-6" />
          </div>
          Công ty Khám sức khỏe
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[9px] mt-2">Nội bộ: Quản lý thông tin pháp nhân khách hàng</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
              @click="openModal()" 
              class="btn-premium bg-slate-900 text-white px-8 py-3 rounded-xl shadow-lg">
        <Plus class="w-5 h-5" />
        <span>THÊM CÔNG TY</span>
      </button>
    </div>

    <!-- Search -->
    <div class="relative group">
        <Search class="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 w-5 h-5" />
        <input v-model="searchQuery" placeholder="Tìm tên công ty, mã số thuế..." 
               class="w-full pl-16 pr-8 py-4 rounded-xl bg-white border-2 border-slate-50 focus:border-indigo-600/20 outline-none font-bold text-slate-600 shadow-sm transition-all" />
    </div>

    <!-- List -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="item in filteredList" :key="item.companyId" 
             @click="openModal(item)"
             class="premium-card p-6 bg-white border border-slate-50 group cursor-pointer hover:border-indigo-600/20 transition-all flex flex-col justify-between">
            
            <div class="flex justify-between items-start mb-4">
                <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-xl flex items-center justify-center font-black group-hover:bg-indigo-600 group-hover:text-white transition-all">
                    {{ item.companyName.charAt(0) }}
                </div>
                <div class="text-right">
                    <p class="text-[9px] font-black text-slate-300 uppercase">Mã số thuế</p>
                    <p class="text-xs font-black text-slate-700 tracking-widest">{{ item.taxCode }}</p>
                </div>
            </div>

            <h4 class="text-lg font-black text-slate-800 leading-tight mb-4 group-hover:text-indigo-600 transition-colors uppercase">{{ item.companyName }}</h4>
            
            <div class="pt-4 border-t border-slate-50 flex items-center gap-4 text-slate-400">
                <div class="flex items-center gap-1">
                    <MapPin class="w-3 h-3" />
                    <span class="text-[9px] font-bold truncate max-w-[150px]">{{ item.address }}</span>
                </div>
                <div class="flex items-center gap-1 ml-auto">
                    <Phone class="w-3 h-3" />
                    <span class="text-[9px] font-bold">{{ item.phoneNumber }}</span>
                </div>
            </div>
        </div>

        <div v-if="filteredList.length === 0" class="col-span-full py-40 bg-slate-50/50 rounded-[3rem] border-2 border-dashed border-slate-100 flex flex-col items-center justify-center gap-4">
            <Building2 class="w-12 h-12 text-slate-200" />
            <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Chưa có công ty phù hợp</p>
        </div>
    </div>

    <!-- Modal Form -->
    <div v-if="showModal" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-lg p-8 rounded-[2.5rem] shadow-2xl animate-fade-in-up">
            <h3 class="text-xl font-black text-slate-800 mb-6 uppercase">{{ currentCompany.companyId ? 'Cập nhật công ty' : 'Khai báo công ty mới' }}</h3>
            <form @submit.prevent="saveCompany" class="space-y-6">
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Tên pháp nhân đầy đủ *</label>
                    <input v-model="currentCompany.companyName" required class="input-premium" placeholder="Công ty TNHH..." />
                </div>
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Mã số thuế *</label>
                    <input v-model="currentCompany.taxCode" required class="input-premium" placeholder="0101..." />
                </div>
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Địa chỉ trụ sở</label>
                    <input v-model="currentCompany.address" required class="input-premium" placeholder="Số nhà, đường, quận..." />
                </div>
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Số điện thoại liên hệ</label>
                    <input v-model="currentCompany.phoneNumber" required class="input-premium" placeholder="090..." />
                </div>

                <div class="flex gap-3 pt-6">
                    <button v-if="currentCompany.companyId && authStore.role === 'Admin'" type="button" @click="deleteCompany" class="px-6 py-3 text-rose-500 font-bold hover:bg-rose-50 rounded-xl transition-all">Xóa</button>
                    <div class="flex-1"></div>
                    <button type="button" @click="showModal = false" class="px-8 py-3 text-slate-400 font-black uppercase text-xs underline">HỦY</button>
                    <button type="submit" class="bg-indigo-600 text-white px-10 py-3 rounded-xl font-black shadow-lg uppercase text-xs tracking-widest">LƯU THÔNG TIN</button>
                </div>
            </form>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Building2, Plus, Search, MapPin, Phone, X } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const showModal = ref(false)
const searchQuery = ref('')
const currentCompany = ref({})

const filteredList = computed(() => {
    if (!searchQuery.value) return list.value
    const q = searchQuery.value.toLowerCase()
    return list.value.filter(c => c.companyName.toLowerCase().includes(q) || c.taxCode.includes(q))
})

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Companies')
        list.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu đối tác") }
}

const openModal = (company = null) => {
    currentCompany.value = company ? { ...company } : { companyName: '', taxCode: '', address: '', phoneNumber: '' }
    showModal.value = true
}

const saveCompany = async () => {
    try {
        if (currentCompany.value.companyId) {
            await axios.put(`http://localhost:5283/api/Companies/${currentCompany.value.companyId}`, currentCompany.value)
        } else {
            await axios.post('http://localhost:5283/api/Companies', currentCompany.value)
        }
        toast.success("Đã ghi nhận dữ liệu đối tác!")
        showModal.value = false
        fetchList()
    } catch (e) { toast.error("Lỗi khi lưu dữ liệu") }
}

const deleteCompany = async () => {
    if (!confirm("Bạn có chắc chắn muốn xóa đối tác này?")) return
    try {
        await axios.delete(`http://localhost:5283/api/Companies/${currentCompany.value.companyId}`)
        toast.success("Đã xóa!")
        showModal.value = false
        fetchList()
    } catch (e) { toast.error("Không thể xóa đối tác này") }
}

onMounted(fetchList)
</script>
