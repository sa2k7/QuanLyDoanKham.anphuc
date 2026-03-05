<template>
  <div class="space-y-6">
    <!-- Header Actions -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <div>
        <h2 class="text-3xl font-bold tracking-tight flex items-center gap-3 text-gray-800">
          <div class="bg-primary p-2 rounded-xl">
            <Building2 class="w-6 h-6 text-white" />
          </div>
          Đối tác Doanh nghiệp
        </h2>
        <p class="text-gray-500 font-medium mt-1">Quản lý danh sách khách hàng và đối tác</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
              @click="showForm = !showForm" 
              class="px-6 py-3 bg-primary hover:bg-primary-dark text-white rounded-xl font-bold transition-all shadow-md shadow-primary/20 flex items-center gap-2">
        <Plus class="w-5 h-5" />
        <span>THÊM ĐỐI TÁC</span>
      </button>
    </div>

    <!-- Company Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Tổng số pháp nhân</p>
        <p class="text-3xl font-bold text-gray-900">{{ list.length }} <span class="text-xs font-medium text-gray-400 ml-1">Công ty</span></p>
      </div>
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Kết quả tìm kiếm</p>
        <p class="text-3xl font-bold text-primary">{{ filteredList.length }}</p>
      </div>
      <div class="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
        <p class="text-[10px] font-bold uppercase tracking-widest text-gray-400 mb-2">Tình trạng hệ thống</p>
        <p class="text-3xl font-bold text-gray-900">Ổn định</p>
      </div>
    </div>

    <!-- Rule: Arrow pointing to form -->
    <div v-if="showForm" class="flex flex-col items-center animate-bounce mb-2">
        <ArrowDown class="text-primary w-8 h-8" />
    </div>

    <!-- Form Add/Edit Company -->
    <div v-if="showForm || editingCompany" class="bg-white p-8 rounded-2xl shadow-lg border border-gray-100 mb-8">
        <h3 class="text-xl font-bold mb-6 text-gray-800 flex items-center gap-3">
            <Building2 class="w-5 h-5 text-primary" />
            {{ editingCompany ? 'Hiệu chỉnh thông tin đối tác' : 'Khai báo thông tin đối tác mới' }}
        </h3>
        <form @submit.prevent="editingCompany ? updateCompany() : addCompany()" class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
                <label class="text-xs font-bold uppercase tracking-wider text-gray-400">Tên gọi pháp nhân</label>
                <input v-model="currentCompany.companyName" required :disabled="isLoading" class="input-premium" placeholder="Tên công ty..." />
            </div>
            <div class="space-y-2">
                <label class="text-xs font-bold uppercase tracking-wider text-gray-400">Mã số thuế</label>
                <input v-model="currentCompany.taxCode" required :disabled="isLoading" class="input-premium" placeholder="Mã số thuế..." />
            </div>
            <div class="space-y-2 md:col-span-2">
                <label class="text-xs font-bold uppercase tracking-wider text-gray-400">Địa chỉ</label>
                <input v-model="currentCompany.address" required :disabled="isLoading" class="input-premium" placeholder="Địa chỉ..." />
            </div>
            <div class="space-y-2">
                <label class="text-xs font-bold uppercase tracking-wider text-gray-400">Số điện thoại</label>
                <input 
                  type="text"
                  maxlength="12"
                  :value="formatPhone(currentCompany.phoneNumber)"
                  @input="handlePhoneInput"
                  required 
                  :disabled="isLoading"
                  class="input-premium" 
                  placeholder="0xxx.xxx.xxx"
                />
            </div>
            <div class="flex items-end justify-end gap-3 md:col-span-1">
                <button type="button" @click="cancelEdit" :disabled="isLoading" class="px-4 py-2 text-gray-400 font-bold text-sm uppercase">Hủy</button>
                <button type="submit" :disabled="isLoading" class="px-6 py-2 bg-primary text-white rounded-lg font-bold shadow-md shadow-primary/20">
                    {{ isLoading ? '...' : (editingCompany ? 'CẬP NHẬT' : 'LƯU LẠI') }}
                </button>
            </div>
        </form>
    </div>


    <!-- Table List -->
    <div class="premium-card overflow-hidden">
        <div class="p-8 border-b-2 border-slate-50 flex flex-col md:flex-row justify-between items-center gap-6 bg-slate-50/30">
            <div>
                <h3 class="text-xl font-black text-slate-800 tracking-tight">DANH SÁCH ĐỐI TÁC</h3>
                <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">Tổng cộng: {{ filteredList.length }} pháp nhân</p>
            </div>
            <div class="relative w-full md:w-80">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                <input v-model="searchQuery" placeholder="Tìm tên, mã số thuế..." class="input-premium pl-12 py-3 text-sm" />
            </div>
        </div>
        <div class="overflow-x-auto">
            <table class="professional-table">
                <thead>
                    <tr>
                        <th class="text-left w-1/3 border-r border-slate-100">Cơ quan / Doanh nghiệp</th>
                        <th class="text-center border-r border-slate-100">Mã số thuế</th>
                        <th class="text-left">Đầu mối liên hệ</th>
                        <th class="text-right"></th>
                    </tr>
                </thead>
                <tbody class="divide-y-2 divide-slate-50">
                    <tr v-for="item in filteredList" :key="item.companyId" class="group">
                        <td class="border-r border-slate-50">
                            <div class="flex items-center gap-4">
                                <div class="w-12 h-12 rounded-2xl bg-sky-50 text-sky-600 flex items-center justify-center font-black group-hover:bg-sky-600 group-hover:text-white transition-all shadow-sm">
                                    {{ item.companyName.charAt(0) }}
                                </div>
                                <div>
                                    <div class="font-black text-slate-800 leading-tight group-hover:text-primary transition-colors">{{ item.companyName }}</div>
                                    <div class="text-[10px] font-bold text-slate-400 flex items-center mt-1 uppercase tracking-wider">
                                        <MapPin class="w-3 h-3 mr-1" /> {{ item.address }}
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="text-center border-r border-slate-50">
                            <span class="px-3 py-1.5 rounded-lg bg-slate-100 text-slate-600 font-black text-[10px] tracking-widest border border-slate-200">
                                {{ item.taxCode }}
                            </span>
                        </td>
                        <td class="font-bold">
                            <div class="flex items-center text-indigo-600">
                                <div class="w-8 h-8 rounded-full bg-indigo-50 flex items-center justify-center mr-3">
                                    <Phone class="w-4 h-4" />
                                </div>
                                {{ formatPhone(item.phoneNumber) }}
                            </div>
                        </td>
                        <td class="text-right">
                            <div v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" class="flex justify-end gap-2 pr-4">
                                <button @click="editCompany(item)" class="p-3 text-slate-400 hover:text-indigo-600 hover:bg-white rounded-xl transition-all border border-transparent hover:border-indigo-100 shadow-sm shadow-transparent hover:shadow-indigo-100/50">
                                    <Edit2 class="w-5 h-5" />
                                </button>
                                <button @click="confirmDelete(item)" class="p-3 text-slate-400 hover:text-rose-600 hover:bg-white rounded-xl transition-all border border-transparent hover:border-rose-100 shadow-sm shadow-transparent hover:shadow-rose-100/50">
                                    <Trash2 class="w-5 h-5" />
                                </button>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div v-if="filteredList.length === 0" class="py-32 flex flex-col items-center justify-center gap-4 bg-slate-50/20">
            <div class="w-20 h-20 bg-slate-50 rounded-full flex items-center justify-center text-slate-200">
                <Building2 class="w-10 h-10" />
            </div>
            <p class="text-slate-400 font-black uppercase tracking-widest text-sm">
                {{ searchQuery ? 'Không tìm thấy kết quả phù hợp' : 'Chưa có dữ liệu đối tác' }}
            </p>
        </div>
    </div>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog 
      v-model="showDeleteConfirm"
      title="Xóa công ty?"
      :message="`Bạn có chắc muốn xóa &quot;${companyToDelete?.companyName}&quot;? Hành động này không thể hoàn tác.`"
      confirmText="Xóa ngay"
      variant="danger"
      @confirm="deleteCompany"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Plus, Search, MapPin, Phone, Edit2, Trash2, ArrowDown, Loader, Building2 } from 'lucide-vue-next'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useToast } from '../composables/useToast'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const searchQuery = ref('')
const showForm = ref(false)
const isLoading = ref(false)
const editingCompany = ref(null)
const showDeleteConfirm = ref(false)
const companyToDelete = ref(null)

const currentCompany = ref({
    companyName: '',
    taxCode: '',
    address: '',
    phoneNumber: ''
})

const filteredList = computed(() => {
    if (!searchQuery.value) return list.value
    const query = searchQuery.value.toLowerCase()
    return list.value.filter(c => 
        c.companyName.toLowerCase().includes(query) ||
        c.taxCode.toLowerCase().includes(query) ||
        c.address.toLowerCase().includes(query) ||
        c.phoneNumber.includes(query)
    )
})

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Companies')
        list.value = res.data
    } catch (e) { 
        console.error(e)
        toast.error('Không thể tải danh sách công ty')
    }
}

const addCompany = async () => {
    try {
        const { companyName, taxCode, address, phoneNumber } = currentCompany.value;
        
        if (!companyName) return toast.warning('Bạn chưa nhập tên công ty!')
        if (!taxCode) return toast.warning('Bạn chưa nhập mã số thuế!')
        if (!address) return toast.warning('Bạn chưa nhập địa chỉ!')
        if (!phoneNumber || phoneNumber.length < 10) return toast.warning('Số điện thoại phải đủ 10 chữ số!')

        isLoading.value = true
        await axios.post('http://localhost:5283/api/Companies', currentCompany.value)
        toast.success('Đã thêm đối tác mới thành công!')
        await fetchList()
        showForm.value = false
        resetForm()
    } catch (e) { 
        console.error(e) 
        toast.error('Không thể lưu thông tin công ty')
    } finally {
        isLoading.value = false
    }
}

const editCompany = (company) => {
    editingCompany.value = company
    currentCompany.value = { ...company }
    showForm.value = false
}

const updateCompany = async () => {
    try {
        isLoading.value = true
        await axios.put(`http://localhost:5283/api/Companies/${editingCompany.value.companyId}`, currentCompany.value)
        toast.success('Cập nhật thông tin thành công!')
        await fetchList()
        cancelEdit()
    } catch (e) {
        console.error(e)
        toast.error('Không thể cập nhật thông tin công ty')
    } finally {
        isLoading.value = false
    }
}

const confirmDelete = (company) => {
    companyToDelete.value = company
    showDeleteConfirm.value = true
}

const deleteCompany = async () => {
    try {
        isLoading.value = true
        await axios.delete(`http://localhost:5283/api/Companies/${companyToDelete.value.companyId}`)
        toast.success(`Đã xóa "${companyToDelete.value.companyName}"`)
        await fetchList()
        companyToDelete.value = null
    } catch (e) {
        console.error(e)
        toast.error('Không thể xóa công ty này')
    } finally {
        isLoading.value = false
    }
}

const cancelEdit = () => {
    editingCompany.value = null
    showForm.value = false
    resetForm()
}

const resetForm = () => {
    currentCompany.value = { companyName: '', taxCode: '', address: '', phoneNumber: '' }
}

const handlePhoneInput = (e) => {
    let value = e.target.value.replace(/\D/g, '')
    currentCompany.value.phoneNumber = value.slice(0, 10)
    
    let formatted = currentCompany.value.phoneNumber
    if (formatted.length > 4) formatted = formatted.slice(0, 4) + '.' + formatted.slice(4)
    if (formatted.length > 8) formatted = formatted.slice(0, 8) + '.' + formatted.slice(8)
    
    e.target.value = formatted
}

const formatPhone = (val) => {
    if (!val) return ''
    let cleaned = val.replace(/\D/g, '')
    return cleaned.replace(/(\d{4})(\d{3})(\d{3})/, '$1.$2.$3')
}

onMounted(fetchList)
</script>
