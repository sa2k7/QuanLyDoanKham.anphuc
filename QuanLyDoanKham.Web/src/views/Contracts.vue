<template>
  <div class="space-y-6 animate-fade-in">
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <div>
        <h2 class="text-3xl font-bold tracking-tight flex items-center gap-3 text-gray-800">
          <div class="bg-primary p-2 rounded-xl">
            <FileText class="w-6 h-6 text-white" />
          </div>
          Hệ thống Hợp đồng
        </h2>
        <p class="text-gray-500 font-medium mt-1">Quản lý pháp lý và giá trị hợp đồng</p>
      </div>
      <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
              @click="showForm = !showForm" 
              class="px-6 py-3 bg-primary hover:bg-primary-dark text-white rounded-xl font-bold transition-all shadow-md shadow-primary/20 flex items-center gap-2">
        <Plus class="w-5 h-5" />
        <span>{{ showForm ? 'HỦY BỎ' : 'TẠO HỢP ĐỒNG' }}</span>
      </button>
    </div>

    <!-- Stats Summary Section -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">Tổng giá trị HĐ (Dự kiến)</p>
            <p class="text-3xl font-black text-slate-900 tracking-tighter">{{ formatPrice(list.reduce((sum, c) => sum + c.totalAmount, 0)) }}</p>
        </div>
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">HĐ Đang thực hiện</p>
            <p class="text-4xl font-black text-slate-900 tracking-tighter">
                {{ list.filter(c => ['Active', 'Pending'].includes(c.status)).length }} <span class="text-sm text-slate-400">Dự án</span>
            </p>
        </div>
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">Tổng quy mô khám</p>
            <p class="text-4xl font-black text-slate-900 tracking-tighter">{{ list.reduce((sum, c) => sum + (c.expectedQuantity || 0), 0) }} <span class="text-sm text-slate-400">Người</span></p>
        </div>
    </div>

    <!-- Creation Area -->
    <div v-if="showForm" class="premium-card p-10 bg-white border-4 border-primary/10 mb-12 animate-slide-up">
        <div class="flex items-center gap-4 mb-10">
            <div class="w-12 h-12 bg-primary/10 text-primary rounded-2xl flex items-center justify-center">
                <PlusCircle class="w-7 h-7" />
            </div>
            <div>
                <h3 class="text-2xl font-black text-slate-800 tracking-tight">Ký kết Hợp đồng mới</h3>
                <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">Soạn thảo hồ sơ pháp lý đối tác</p>
            </div>
        </div>
        <form @submit.prevent="addContract" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đối tác doanh nghiệp</label>
                <select v-model="newContract.companyId" required class="input-premium">
                    <option disabled value="null">-- Tuyển chọn đối tác --</option>
                    <option v-for="c in companies" :key="c.companyId" :value="c.companyId">{{ c.companyName }}</option>
                </select>
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Đơn giá (VNĐ/Người)</label>
                <input v-model="newContract.unitPrice" type="number" required class="input-premium" placeholder="0" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Số lượng dự kiến</label>
                <input v-model="newContract.expectedQuantity" type="number" required class="input-premium" placeholder="0" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày ký</label>
                <input v-model="newContract.signingDate" type="date" required class="input-premium" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày bắt đầu</label>
                <input v-model="newContract.startDate" type="date" required class="input-premium" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ml-1">Ngày kết thúc</label>
                <input v-model="newContract.endDate" type="date" required class="input-premium" />
            </div>
            <div class="lg:col-span-3 flex justify-between items-center bg-slate-50 p-6 rounded-2xl border-2 border-slate-100">
                <div>
                   <p class="text-[10px] font-black text-slate-400 uppercase">Tổng giá trị dự kiến</p>
                   <p class="text-2xl font-black text-primary">{{ formatPrice(newContract.unitPrice * newContract.expectedQuantity) }}</p>
                </div>
                <button type="submit" class="btn-premium bg-slate-900 text-white px-10">XÁC NHẬN KÝ KẾT</button>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-8">
        <button @click="activeTab = 'active'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'active' ? 'bg-primary text-white shadow-lg shadow-primary/20' : 'bg-white text-slate-400 border-2 border-slate-50 hover:bg-slate-50']">
            Đang triển khai ({{ filteredList.active.length }})
        </button>
        <button @click="activeTab = 'finished'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'finished' ? 'bg-emerald-500 text-white shadow-lg shadow-emerald-200' : 'bg-white text-slate-400 border-2 border-slate-50 hover:bg-slate-50']">
            Đã kết thúc/Khóa ({{ filteredList.finished.length }})
        </button>
    </div>

    <!-- Contract Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div v-for="item in (activeTab === 'active' ? filteredList.active : filteredList.finished)" :key="item.healthContractId" 
             class="premium-card p-10 group relative transition-all hover:bg-slate-50/50">
            
            <div class="absolute right-6 top-6">
                <span :class="['px-4 py-1.5 rounded-full text-[9px] font-black uppercase tracking-widest shadow-sm border', getStatusClass(item.status)]">
                    {{ item.status }}
                </span>
            </div>

            <div class="flex items-start gap-6 mb-10">
                <div class="bg-indigo-50 p-5 rounded-3xl text-indigo-600 shadow-inner group-hover:scale-110 transition-transform duration-500">
                    <FileText class="w-8 h-8" />
                </div>
                <div>
                    <h4 class="text-2xl font-black text-slate-800 leading-tight mb-2 group-hover:text-primary transition-colors tracking-tight">{{ item.companyName }}</h4>
                    <div class="flex items-center gap-3">
                        <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">HĐ-{{ item.healthContractId }}</span>
                        <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                        <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-1">
                            <Calendar class="w-3 h-3" /> {{ formatDate(item.signingDate) }}
                        </span>
                    </div>
                </div>
            </div>

            <div class="grid grid-cols-2 gap-6 mb-10">
                <div class="bg-white/50 border-2 border-slate-50 p-6 rounded-[2rem]">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-2">Giá trị dự kiến</p>
                    <p class="text-2xl font-black text-indigo-600 tracking-tighter">{{ formatPrice(item.totalAmount) }}</p>
                </div>
                <div class="bg-white/50 border-2 border-slate-50 p-6 rounded-[2rem]">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-2">Quy mô khám</p>
                    <p class="text-2xl font-black text-slate-800 tracking-tighter">{{ item.expectedQuantity }} <span class="text-xs text-slate-400">{{ item.unitName }}</span></p>
                </div>
            </div>

            <div class="flex items-center justify-between pt-8 border-t-2 border-slate-50">
                <div class="flex items-center gap-4">
                    <p class="text-[10px] font-black text-slate-400 uppercase">Hạn: {{ formatDate(item.endDate) }}</p>
                </div>
                <div class="flex items-center gap-2">
                    <button @click="openDetails(item)" class="btn-premium py-2 px-5 bg-white border-2 border-slate-100 text-slate-900 hover:border-primary/20 hover:text-primary transition-all text-xs">
                        CHI TIẾT <ArrowRight class="w-4 h-4 ml-1" />
                    </button>
                    <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
                            @click="triggerUpload(item.healthContractId)" 
                            class="p-2 bg-indigo-50 text-indigo-600 rounded-xl hover:bg-indigo-100 transition-all"
                            title="Tải lên file hợp đồng">
                        <Upload class="w-4 h-4" />
                    </button>
                </div>
            </div>
        </div>
        
        <div v-if="(activeTab === 'active' ? filteredList.active : filteredList.finished).length === 0" class="col-span-2 py-40 flex flex-col items-center justify-center gap-6 bg-slate-50/20 rounded-[3rem] border-4 border-dashed border-slate-100">
            <div class="w-24 h-24 bg-white rounded-full flex items-center justify-center text-slate-100 shadow-sm">
                <FileText class="w-12 h-12" />
            </div>
            <p class="text-slate-300 font-black uppercase tracking-[0.3em] text-sm">Không tìm thấy hợp đồng nào</p>
        </div>
    </div>

    <!-- Hidden File Input -->
    <input type="file" ref="fileInput" class="hidden" @change="handleFileUpload" accept=".pdf,.doc,.docx,.xlsx" />

    <!-- Contract Detail Modal -->
    <div v-if="detailsModal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-2xl p-10 rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden">
            <div class="absolute top-0 left-0 right-0 h-32 bg-gradient-to-r from-teal-400 to-teal-600"></div>
            <div class="absolute top-8 left-8 bg-white p-4 rounded-3xl shadow-lg">
                <FileText class="w-10 h-10 text-teal-600" />
            </div>

            <button @click="detailsModal.show = false" class="absolute top-6 right-6 bg-white/20 p-2 rounded-full hover:bg-white/40 transition-all text-white">
                <span class="font-black text-xl">✕</span>
            </button>

            <div class="mt-24 pt-4">
                <div class="flex justify-between items-start mb-8">
                    <div>
                        <h3 class="text-3xl font-black text-slate-800">{{ detailsModal.data.companyName }}</h3>
                        <p class="text-sm font-bold text-slate-400 uppercase tracking-widest mt-1">Hợp đồng #{{ detailsModal.data.healthContractId }}</p>
                    </div>
                    <span :class="['px-6 py-2 rounded-full text-sm font-black uppercase tracking-widest border-2', getStatusClass(detailsModal.data.status)]">
                        {{ detailsModal.data.status }}
                    </span>
                </div>

                <div v-if="!isEditing" class="grid grid-cols-2 gap-8 mb-8">
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Tổng giá trị</p>
                        <p class="text-2xl font-black text-primary">{{ formatPrice(detailsModal.data.totalAmount) }}</p>
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Quy mô (Dự kiến)</p>
                        <p class="text-2xl font-black text-slate-700">{{ detailsModal.data.expectedQuantity }} {{ detailsModal.data.unitName }}</p>
                    </div>
                </div>

                <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8 bg-slate-50 p-8 rounded-[2rem] border-2 border-primary/20">
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Đơn giá</label>
                        <input type="number" v-model="detailsModal.data.unitPrice" class="input-premium shadow-none" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Số lượng</label>
                        <input type="number" v-model="detailsModal.data.expectedQuantity" class="input-premium shadow-none" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Ngày bắt đầu</label>
                        <input type="date" v-model="detailsModal.data.startDate" class="input-premium shadow-none" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Ngày kết thúc</label>
                        <input type="date" v-model="detailsModal.data.endDate" class="input-premium shadow-none" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Trạng thái</label>
                        <select v-model="detailsModal.data.status" class="input-premium shadow-none">
                            <option value="Pending">Pending (Chờ duyệt)</option>
                            <option value="Active">Active (Hoạt động)</option>
                            <option value="Finished">Finished (Hoàn thành)</option>
                            <option value="Locked">Locked (Đã khóa)</option>
                        </select>
                    </div>
                </div>

                <div class="space-y-4">
                    <div v-if="detailsModal.data.filePath" class="flex items-center gap-3 p-4 bg-indigo-50 rounded-2xl border border-indigo-100">
                        <FileText class="w-6 h-6 text-indigo-600" />
                        <div class="flex-1">
                            <p class="text-[10px] font-black uppercase text-indigo-400">File đính kèm</p>
                            <p class="text-xs font-bold text-slate-700 truncate">{{ detailsModal.data.filePath.split('\\').pop() }}</p>
                        </div>
                        <a :href="'http://localhost:5283/' + detailsModal.data.filePath" target="_blank" class="px-4 py-2 bg-white text-indigo-600 rounded-xl text-[10px] font-black shadow-sm">XEM FILE</a>
                    </div>
                </div>

                <div class="mt-10 flex flex-wrap items-center gap-4">
                    <template v-if="!isEditing">
                        <button v-if="['Active', 'Pending'].includes(detailsModal.data.status) && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleLockContract(detailsModal.data.healthContractId)" 
                                class="flex-1 bg-slate-900 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2">
                            <Lock class="w-5 h-5" />
                            <span>KHÓA HỢP ĐỒNG</span>
                        </button>
                        <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
                                @click="isEditing = true" 
                                class="bg-indigo-600 text-white px-8 py-4 rounded-2xl font-black hover:bg-indigo-700 transition-all">
                            CHỈNH SỬA
                        </button>
                        <button v-if="authStore.role === 'Admin'" 
                                @click="handleDeleteContract(detailsModal.data.healthContractId)" 
                                class="bg-rose-50 text-rose-500 p-4 rounded-2xl hover:bg-rose-500 hover:text-white transition-all">
                            <Trash2 class="w-6 h-6" />
                        </button>
                    </template>
                    <template v-else>
                        <button @click="handleUpdateContract" 
                                class="flex-1 bg-primary text-white px-8 py-4 rounded-2xl font-black transition-all">LƯU THAY ĐỔI</button>
                        <button @click="isEditing = false" class="bg-slate-100 text-slate-500 px-8 py-4 rounded-2xl font-black">HỦY</button>
                    </template>
                    <button v-if="!isEditing" @click="detailsModal.show = false" class="bg-white border-2 border-slate-100 text-slate-400 px-8 py-4 rounded-2xl font-black">ĐÓNG</button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Confirm Dialog -->
    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Plus, FileText, Calendar, ArrowRight, Trash2, Save, PlusCircle, History, Sparkles, Clock, Lock, Upload, X } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const companies = ref([])
const showForm = ref(false)
const fileInput = ref(null)
const currentUploadId = ref(null)

const newContract = ref({
    companyId: null,
    signingDate: new Date().toISOString().split('T')[0],
    startDate: '',
    endDate: '',
    unitPrice: 0,
    expectedQuantity: 0,
    unitName: 'Người',
    status: 'Pending'
})

const detailsModal = ref({ show: false, data: {} })
const isEditing = ref(false)
const activeTab = ref('active')
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const filteredList = computed(() => {
    return {
        active: list.value.filter(c => ['Pending', 'Active'].includes(c.status)),
        finished: list.value.filter(c => ['Finished', 'Locked'].includes(c.status))
    }
})

const getStatusClass = (status) => {
    switch(status) {
        case 'Active': return 'bg-sky-50 text-sky-600 border-sky-100'
        case 'Finished': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'Locked': return 'bg-slate-50 text-slate-500 border-slate-200'
        default: return 'bg-amber-50 text-amber-600 border-amber-100'
    }
}

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/HealthContracts')
        list.value = res.data
    } catch (e) { toast.error("Lỗi khi tải danh sách hợp đồng") }
}

const fetchCompanies = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Companies')
        companies.value = res.data
    } catch (e) { console.error(e) }
}

const addContract = async () => {
    try {
        if (!newContract.value.companyId) return toast.warning("Chưa chọn đối tác!")
        const payload = { ...newContract.value, totalAmount: newContract.value.unitPrice * newContract.value.expectedQuantity };
        await axios.post('http://localhost:5283/api/HealthContracts', payload)
        toast.success("Tạo hợp đồng thành công!")
        fetchList()
        showForm.value = false
        resetForm()
    } catch (e) { toast.error("Lỗi khi tạo hợp đồng") }
}

const resetForm = () => {
    newContract.value = { companyId: null, signingDate: new Date().toISOString().split('T')[0], startDate: '', endDate: '', unitPrice: 0, expectedQuantity: 0, unitName: 'Người', status: 'Pending' }
}

const openDetails = (contract) => {
    const data = { ...contract };
    if (data.signingDate) data.signingDate = data.signingDate.split('T')[0];
    if (data.startDate) data.startDate = data.startDate.split('T')[0];
    if (data.endDate) data.endDate = data.endDate.split('T')[0];
    detailsModal.value.data = data
    detailsModal.value.show = true
    isEditing.value = false
}

const handleUpdateContract = async () => {
    try {
        await axios.put(`http://localhost:5283/api/HealthContracts/${detailsModal.value.data.healthContractId}`, detailsModal.value.data)
        toast.success("Đã cập nhật!")
        fetchList()
        detailsModal.value.show = false
    } catch (e) { toast.error("Lỗi khi cập nhật") }
}

const triggerUpload = (id) => {
    currentUploadId.value = id
    fileInput.value.click()
}

const handleFileUpload = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        await axios.post(`http://localhost:5283/api/HealthContracts/${currentUploadId.value}/upload`, formData)
        toast.success("Đã tải lên file hợp đồng!")
        fetchList()
    } catch (err) { toast.error("Lỗi khi tải file") }
}

const handleLockContract = (id) => {
    confirmData.value = {
        show: true,
        title: 'Khóa hợp đồng',
        message: 'Khi đã khóa, bạn sẽ không thể chỉnh sửa thông tin tài chính này nữa. Tiếp tục?',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await axios.put(`http://localhost:5283/api/HealthContracts/${id}/lock`)
                toast.success("Đã khóa hợp đồng")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { toast.error("Không thể khóa hợp đồng") }
        }
    }
}

const handleDeleteContract = (id) => {
    confirmData.value = {
        show: true, title: 'Xóa hợp đồng', message: 'Hành động này không thể hoàn tác. Xóa?', variant: 'danger',
        onConfirm: async () => {
            try {
                await axios.delete(`http://localhost:5283/api/HealthContracts/${id}`)
                toast.success("Đã xóa")
                fetchList()
                detailsModal.value.show = false
            } catch (e) { toast.error("Lỗi khi xóa") }
        }
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)
const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '---'

onMounted(() => {
    fetchList()
    fetchCompanies()
})
</script>
