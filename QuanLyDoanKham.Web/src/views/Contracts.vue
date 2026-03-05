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
        <span>TẠO HỢP ĐỒNG</span>
      </button>
    </div>

    <!-- Stats Summary Section -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <div class="absolute -right-4 -bottom-4 w-24 h-24 rotate-12 opacity-[0.03] group-hover:opacity-10 transition-all group-hover:scale-125 text-primary">
                <FileText class="w-full h-full" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">Tổng giá trị Hợp đồng</p>
            <p class="text-3xl font-black text-slate-900 tracking-tighter">{{ formatPrice(list.reduce((sum, c) => sum + c.totalAmount, 0)) }}</p>
            <div class="mt-4 flex items-center gap-2 text-[10px] font-black text-emerald-600 bg-emerald-50 px-3 py-1 rounded-full w-fit">
               <Sparkles class="w-3 h-3" /> TĂNG TRƯỞNG DƯƠNG
            </div>
        </div>
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <div class="absolute -right-4 -bottom-4 w-24 h-24 rotate-12 opacity-[0.03] group-hover:opacity-10 transition-all group-hover:scale-125 text-indigo-600">
                <Clock class="w-full h-full" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">HĐ Đang triển khai</p>
            <p class="text-4xl font-black text-slate-900 tracking-tighter">{{ list.filter(c => !c.isFinished).length }} <span class="text-sm text-slate-400">Dự án</span></p>
            <div class="mt-4 flex items-center gap-2 text-[10px] font-black text-indigo-600 bg-indigo-50 px-3 py-1 rounded-full w-fit">
               VẬN HÀNH ỔN ĐỊNH
            </div>
        </div>
        <div class="premium-card p-8 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <div class="absolute -right-4 -bottom-4 w-24 h-24 rotate-12 opacity-[0.03] group-hover:opacity-10 transition-all group-hover:scale-125 text-teal-600">
                <UsersIcon class="w-full h-full" />
            </div>
            <p class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 mb-4">Tổng quy mô khám</p>
            <p class="text-4xl font-black text-slate-900 tracking-tighter">{{ list.reduce((sum, c) => sum + (c.patientCount || 0), 0) }} <span class="text-sm text-slate-400">Bệnh nhân</span></p>
            <div class="mt-4 flex items-center gap-2 text-[10px] font-black text-teal-600 bg-teal-50 px-3 py-1 rounded-full w-fit">
               DỮ LIỆU THỰC TẾ
            </div>
        </div>
    </div>

    <!-- Creation Area Toggle -->
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
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Chọn Đối tác doanh nghiệp</label>
                <select v-model="newContract.companyId" required class="input-premium">
                    <option v-if="availableCompanies.length === 0" disabled value="null" selected>Tất cả công ty đã có hợp đồng</option>
                    <option v-else disabled value="null" selected>-- Tuyển chọn đối tác --</option>
                    <option v-for="c in availableCompanies" :key="c.companyId" :value="c.companyId">{{ c.companyName }}</option>
                </select>
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Giá trị hợp đồng (VNĐ)</label>
                <input 
                  type="text" 
                  :value="formattedTotalAmount"
                  @input="handleAmountInput"
                  required 
                  class="input-premium text-indigo-600 text-lg" 
                  placeholder="0"
                />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Số lượng nhân sự khám</label>
                <input v-model="newContract.patientCount" type="number" required class="input-premium" placeholder="0" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Ngày bắt đầu</label>
                <input v-model="newContract.startDate" type="date" required class="input-premium" />
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Ngày kết thúc dự kiến</label>
                <input v-model="newContract.endDate" type="date" required class="input-premium" />
            </div>
            <div class="flex items-end">
                <button type="submit" class="w-full btn-premium bg-slate-900 text-white hover:bg-black shadow-xl shadow-slate-200">XÁC NHẬN KÝ KẾT</button>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-8">
        <button @click="activeTab = 'active'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'active' ? 'bg-primary text-white shadow-lg shadow-primary/20' : 'bg-white text-slate-400 border-2 border-slate-50 hover:bg-slate-50']">
            Hợp đồng đang thực hiện ({{ filteredList.active.length }})
        </button>
        <button @click="activeTab = 'finished'" 
                :class="['px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'finished' ? 'bg-emerald-500 text-white shadow-lg shadow-emerald-200' : 'bg-white text-slate-400 border-2 border-slate-50 hover:bg-slate-50']">
            Lịch sử hoàn tất ({{ filteredList.finished.length }})
        </button>
    </div>

    <!-- Contract Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div v-for="item in (activeTab === 'active' ? filteredList.active : filteredList.finished)" :key="item.healthContractId" 
             class="premium-card p-10 group relative transition-all hover:bg-slate-50/50">
            
            <div v-if="item.isFinished" class="absolute -right-12 top-6 rotate-45 bg-emerald-500 text-white text-[9px] font-black px-12 py-1.5 uppercase tracking-widest shadow-sm">
                Hoàn tất
            </div>
            <div v-else class="absolute -right-12 top-6 rotate-45 bg-primary text-white text-[9px] font-black px-12 py-1.5 uppercase tracking-widest shadow-sm">
                Active
            </div>

            <div class="flex items-start gap-6 mb-10">
                <div class="bg-indigo-50 p-5 rounded-3xl text-indigo-600 shadow-inner group-hover:scale-110 transition-transform duration-500">
                    <FileText class="w-8 h-8" />
                </div>
                <div>
                    <h4 class="text-2xl font-black text-slate-800 leading-tight mb-2 group-hover:text-primary transition-colors tracking-tight">{{ item.companyName }}</h4>
                    <div class="flex items-center gap-3">
                        <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Mã HĐ: #HD-{{ item.healthContractId }}</span>
                        <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                        <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest flex items-center gap-1">
                            <Calendar class="w-3 h-3" /> {{ formatDate(item.startDate) }}
                        </span>
                    </div>
                </div>
            </div>

            <div class="grid grid-cols-2 gap-6 mb-10">
                <div class="bg-white/50 border-2 border-slate-50 p-6 rounded-[2rem]">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-2">Giá trị HĐ</p>
                    <p class="text-2xl font-black text-indigo-600 tracking-tighter">{{ formatPrice(item.totalAmount) }}</p>
                </div>
                <div class="bg-white/50 border-2 border-slate-50 p-6 rounded-[2rem]">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-2">Quy mô khám</p>
                    <p class="text-2xl font-black text-slate-800 tracking-tighter">{{ item.patientCount }} <span class="text-xs text-slate-400">Người</span></p>
                </div>
            </div>

            <div class="flex items-center justify-between pt-8 border-t-2 border-slate-50">
                <div class="flex items-center gap-2">
                    <div class="w-2 h-2 rounded-full animate-pulse" :class="item.isFinished ? 'bg-emerald-500' : 'bg-primary'"></div>
                    <span class="text-[10px] font-black uppercase text-slate-500 tracking-widest">{{ item.isFinished ? 'Đã thanh quyết toán' : 'Đang trong quá trình thực hiện' }}</span>
                </div>
                <button @click="openDetails(item)" class="btn-premium py-2 px-5 bg-white border-2 border-slate-100 text-slate-900 hover:border-primary/20 hover:text-primary transition-all text-xs">
                    Chi tiết <ArrowRight class="w-4 h-4" />
                </button>
            </div>
        </div>
        
        <div v-if="(activeTab === 'active' ? filteredList.active : filteredList.finished).length === 0" class="col-span-2 py-40 flex flex-col items-center justify-center gap-6 bg-slate-50/20 rounded-[3rem] border-4 border-dashed border-slate-100">
            <div class="w-24 h-24 bg-white rounded-full flex items-center justify-center text-slate-100 shadow-sm">
                <FileText v-if="activeTab === 'active'" class="w-12 h-12" />
                <History v-else class="w-12 h-12" />
            </div>
            <p class="text-slate-300 font-black uppercase tracking-[0.3em] text-sm">{{ activeTab === 'active' ? 'Chưa có hợp đồng đang triển khai' : 'Chưa có hợp đồng nào được hoàn tất' }}</p>
        </div>
    </div>
    <!-- Contract Detail Modal -->
    <div v-if="detailsModal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-2xl p-10 rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden">
            <!-- Decorative Header -->
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
                        <p class="text-sm font-bold text-slate-400 uppercase tracking-widest mt-1">Mã HĐ: #{{ detailsModal.data.healthContractId }}</p>
                    </div>
                    <span :class="['px-6 py-2 rounded-full text-sm font-black uppercase tracking-widest shadow-sm border-2', detailsModal.data.isFinished ? 'bg-emerald-500 text-white border-emerald-600' : 'bg-primary/10 text-primary border-primary/20']">
                        {{ detailsModal.data.isFinished ? 'Đã hoàn thành' : 'Đang thực hiện' }}
                    </span>
                </div>

                <div v-if="!isEditing" class="grid grid-cols-2 gap-8 mb-8">
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Tổng giá trị</p>
                        <p class="text-3xl font-black text-primary">{{ formatPrice(detailsModal.data.totalAmount) }}</p>
                    </div>
                    <div class="bg-slate-50 p-6 rounded-[2rem]">
                        <p class="text-xs font-black text-slate-400 uppercase tracking-widest mb-2">Quy mô khám</p>
                        <p class="text-3xl font-black text-slate-700">{{ detailsModal.data.patientCount }} <span class="text-base text-slate-400">nhân sự</span></p>
                    </div>
                </div>

                <!-- Editing Mode -->
                <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8 bg-slate-50 p-8 rounded-[2rem] border-2 border-primary/20">
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Giá trị (VNĐ)</label>
                        <input type="number" v-model="detailsModal.data.totalAmount" class="w-full px-4 py-3 rounded-xl border-2 border-white focus:border-primary/20 outline-none font-bold" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Số lượng người</label>
                        <input type="number" v-model="detailsModal.data.patientCount" class="w-full px-4 py-3 rounded-xl border-2 border-white focus:border-primary/20 outline-none font-bold" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Ngày bắt đầu</label>
                        <input type="date" v-model="detailsModal.data.startDate" class="w-full px-4 py-3 rounded-xl border-2 border-white focus:border-primary/20 outline-none font-bold" />
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Ngày kết thúc</label>
                        <input type="date" v-model="detailsModal.data.endDate" class="w-full px-4 py-3 rounded-xl border-2 border-white focus:border-primary/20 outline-none font-bold" />
                    </div>
                    <div class="md:col-span-2 flex items-center space-x-2">
                        <input type="checkbox" v-model="detailsModal.data.isFinished" id="isFinishedEdit" />
                        <label for="isFinishedEdit" class="text-sm font-bold text-slate-600">Đã hoàn thành hợp đồng</label>
                    </div>
                </div>

                <div class="space-y-4">
                    <div class="flex items-center space-x-4 p-4 rounded-2xl border border-slate-100">
                        <Calendar class="w-6 h-6 text-slate-400" />
                        <div>
                            <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">Thời gian thực hiện</p>
                            <p class="font-bold text-slate-700 text-lg">
                                {{ formatDate(detailsModal.data.startDate) }} <span class="mx-2 text-slate-300">➔</span> {{ formatDate(detailsModal.data.endDate) }}
                            </p>
                        </div>
                    </div>
                </div>

                <div class="mt-10 flex flex-wrap items-center gap-4">
                    <template v-if="!isEditing">
                        <button v-if="!detailsModal.data.isFinished && (authStore.role === 'Admin' || authStore.role === 'ContractManager')" 
                                @click="handleFinishContract(detailsModal.data.healthContractId)" 
                                class="flex-1 bg-emerald-500 hover:bg-emerald-600 text-white px-8 py-4 rounded-2xl font-black transition-all flex items-center justify-center space-x-2 shadow-lg shadow-emerald-200">
                            <CheckCircle class="w-5 h-5" />
                            <span>KẾT THÚC HĐ</span>
                        </button>
                        <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
                                @click="isEditing = true" 
                                class="bg-slate-800 text-white px-8 py-4 rounded-2xl font-black hover:bg-slate-900 transition-all flex items-center space-x-2">
                            <span>CHỈNH SỬA</span>
                        </button>
                        <button v-if="authStore.role === 'Admin' || authStore.role === 'ContractManager'" 
                                @click="handleDeleteContract(detailsModal.data.healthContractId)" 
                                class="bg-rose-50 text-rose-500 p-4 rounded-2xl hover:bg-rose-500 hover:text-white transition-all shadow-sm">
                            <Trash2 class="w-6 h-6" />
                        </button>
                    </template>
                    <template v-else>
                        <button @click="handleUpdateContract" 
                                class="flex-1 bg-primary text-white px-8 py-4 rounded-2xl font-black hover:bg-primary-dark transition-all flex items-center justify-center space-x-2">
                            <Save class="w-5 h-5" />
                            <span>LƯU THAY ĐỔI</span>
                        </button>
                        <button @click="isEditing = false" 
                                class="bg-slate-100 text-slate-500 px-8 py-4 rounded-2xl font-black hover:bg-slate-200 transition-all">
                            HỦY
                        </button>
                    </template>
                    
                    <button v-if="!isEditing" @click="detailsModal.show = false" class="bg-slate-100 text-slate-500 hover:bg-slate-200 px-8 py-4 rounded-2xl font-black transition-all">ĐÓNG</button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Custom Confirm Dialog -->
    <ConfirmDialog 
        v-model="confirmData.show"
        :title="confirmData.title"
        :message="confirmData.message"
        :variant="confirmData.variant"
        @confirm="confirmData.onConfirm"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Plus, FileText, Calendar, ArrowRight, CheckCircle, Trash2, Save, PlusCircle, History, Sparkles, Clock, Users as UsersIcon } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const companies = ref([])
const showForm = ref(false)
const newContract = ref({
    companyId: null,
    totalAmount: 0,
    patientCount: 0,
    startDate: '',
    endDate: ''
})
const detailsModal = ref({ show: false, data: {} })
const isEditing = ref(false)
const activeTab = ref('active')
const confirmData = ref({
    show: false,
    title: '',
    message: '',
    variant: 'warning',
    onConfirm: () => {}
})

const filteredList = computed(() => {
    return {
        active: list.value.filter(c => !c.isFinished),
        finished: list.value.filter(c => c.isFinished)
    }
})

const availableCompanies = computed(() => {
    // Chỉ hiện thị công ty chưa có bất kỳ hợp đồng nào trong danh sách
    const companyIdsWithContract = list.value.map(c => c.companyId);
    return companies.value.filter(c => !companyIdsWithContract.includes(c.companyId));
})

const formattedTotalAmount = computed(() => {
    return (newContract.value.totalAmount || 0).toLocaleString('vi-VN')
})

const handleAmountInput = (e) => {
    const val = e.target.value.replace(/\D/g, '')
    newContract.value.totalAmount = val ? parseInt(val) : 0
    e.target.value = (newContract.value.totalAmount || 0).toLocaleString('vi-VN')
}

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/HealthContracts')
        list.value = res.data
    } catch (e) { 
        console.error(e)
        toast.error("Không thể tải danh sách hợp đồng. Hãy kiểm tra Backend.")
    }
}

const fetchCompanies = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Companies')
        companies.value = res.data
    } catch (e) { console.error(e) }
}

const addContract = async () => {
    try {
        // Validation logic
        const { companyId, totalAmount, patientCount, startDate, endDate } = newContract.value;
        
        if (!companyId) return toast.warning("Bạn chưa chọn đối tác!")
        if (totalAmount <= 0) return toast.warning("Giá trị hợp đồng phải lớn hơn 0!")
        if (patientCount <= 0) return toast.warning("Số lượng nhân sự khám phải lớn hơn 0!")
        if (!startDate) return toast.warning("Bạn chưa chọn ngày bắt đầu!")
        if (!endDate) return toast.warning("Bạn chưa chọn ngày kết thúc!")
        if (new Date(startDate) > new Date(endDate)) return toast.warning("Ngày bắt đầu không thể lớn hơn ngày kết thúc!")

        console.log("Sending contract data:", newContract.value);
        const response = await axios.post('http://localhost:5283/api/HealthContracts', newContract.value)
        
        toast.success("Tạo hợp đồng thành công!")
        fetchList()
        showForm.value = false
        newContract.value = { companyId: null, totalAmount: 0, patientCount: 0, startDate: '', endDate: '' }
    } catch (e) { 
        console.error("API Error Detail:", e.response?.data);
        toast.error("Lỗi hệ thống: Không thể tạo hợp đồng. Vui lòng kiểm tra lại kết nối Backend.")
    }
}

const openDetails = (contract) => {
    // Format dates for input[type=date]
    const data = { ...contract };
    if (data.startDate) data.startDate = data.startDate.split('T')[0];
    if (data.endDate) data.endDate = data.endDate.split('T')[0];
    
    detailsModal.value.data = data
    detailsModal.value.show = true
    isEditing.value = false
}

const handleUpdateContract = async () => {
    try {
        const id = detailsModal.value.data.healthContractId;
        await axios.put(`http://localhost:5283/api/HealthContracts/${id}`, detailsModal.value.data)
        toast.success("Cập nhật hợp đồng thành công!")
        fetchList()
        isEditing.value = false
        detailsModal.value.show = false
    } catch (e) {
        console.error(e)
        toast.error("Lỗi khi cập nhật hợp đồng")
    }
}

const handleDeleteContract = (id) => {
    confirmData.value = {
        show: true,
        title: 'Xác nhận xóa',
        message: 'Bạn có chắc chắn muốn xóa hợp đồng này? Toàn bộ dữ liệu liên quan có thể bị ảnh hưởng.',
        variant: 'danger',
        onConfirm: async () => {
            try {
                await axios.delete(`http://localhost:5283/api/HealthContracts/${id}`)
                toast.success("Đã xóa hợp đồng!")
                fetchList()
                detailsModal.value.show = false
            } catch (e) {
                console.error(e)
                toast.error("Lỗi khi xóa hợp đồng")
            }
        }
    }
}

const handleFinishContract = (id) => {
    confirmData.value = {
        show: true,
        title: 'Hoàn thành hợp đồng',
        message: 'Bạn có chắc chắn muốn hoàn thành hợp đồng này? Hãy đảm bảo tất cả các đoàn khám đã kết thúc.',
        variant: 'success',
        onConfirm: async () => {
            try {
                await axios.put(`http://localhost:5283/api/HealthContracts/${id}/finish`)
                toast.success("Hợp đồng đã hoàn thành!")
                fetchList()
                detailsModal.value.show = false
            } catch (e) {
                console.error(e)
                const msg = e.response?.data?.message || "Lỗi khi kết thúc hợp đồng";
                toast.error(msg);
            }
        }
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)
const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')

onMounted(() => {
    fetchList()
    fetchCompanies()
})
</script>
