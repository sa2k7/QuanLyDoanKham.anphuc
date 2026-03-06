<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-primary text-white rounded-2xl flex items-center justify-center shadow-lg">
            <UsersIcon class="w-6 h-6" />
          </div>
          Quản lý Nhân sự
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[9px] mt-2">Nội bộ: Lịch sử công tác & Thù lao thù lao</p>
      </div>
      <div class="flex items-center gap-3">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="exportStaff" 
                class="btn-premium bg-white border border-slate-200 text-slate-600 px-6 py-3 rounded-xl shadow-sm">
          <Download class="w-4 h-4 mr-2" />
          <span>XUẤT EXCEL</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="triggerImport" 
                class="btn-premium bg-indigo-50 text-indigo-600 px-6 py-3 rounded-xl shadow-sm">
          <UploadIcon class="w-4 h-4 mr-2" />
          <span>NHẬP EXCEL</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="openModal()" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 rounded-xl shadow-lg">
          <Plus class="w-5 h-5" />
          <span>THÊM NHÂN SỰ</span>
        </button>
      </div>
    </div>

    <!-- Search & Filters -->
    <div class="flex flex-col lg:flex-row gap-4 mb-8">
        <div class="relative flex-1 group">
            <Search class="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 w-5 h-5" />
            <input v-model="searchQuery" placeholder="Tìm tên hoặc mã nhân viên..." 
                   class="w-full pl-16 pr-8 py-4 rounded-xl bg-white border-2 border-slate-50 focus:border-primary/20 outline-none font-bold text-slate-600 shadow-sm" />
        </div>
        <select v-model="activeTab" class="px-6 py-4 rounded-xl bg-white border-2 border-slate-50 font-black text-xs uppercase text-slate-500">
            <option value="All">Tất cả chức danh</option>
            <option v-for="role in roles" :key="role" :value="role">{{ role }}</option>
        </select>
    </div>

    <!-- Staff List (List Mode) -->
    <div class="space-y-4">
        <div v-for="item in filteredList" :key="item.staffId" 
             @click="openModal(item)"
             class="premium-card p-6 bg-white border border-slate-50 flex flex-col md:flex-row items-center gap-6 group cursor-pointer hover:border-primary/20 transition-all">
            
            <div class="w-16 h-16 rounded-2xl overflow-hidden border-2 border-white shadow-md bg-slate-50">
                <img v-if="item.avatarPath" :src="`http://localhost:5283/${item.avatarPath}`" class="w-full h-full object-cover" />
                <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${item.fullName}`" class="w-full h-full" />
            </div>

            <div class="flex-1 text-center md:text-left">
                <h4 class="text-lg font-black text-slate-800 leading-tight group-hover:text-primary transition-colors">{{ item.fullName }}</h4>
                <div class="flex items-center justify-center md:justify-start gap-3 mt-1">
                    <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ item.employeeCode }}</span>
                    <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                    <span class="text-[10px] font-black text-indigo-500 uppercase tracking-widest">{{ item.jobTitle }}</span>
                </div>
            </div>

            <div class="flex items-center gap-8">
                <div class="text-right hidden md:block">
                    <p class="text-[9px] font-black text-slate-300 uppercase">Trạng thái</p>
                    <div v-if="item.isWorking" class="flex items-center gap-2 text-emerald-500">
                        <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                        <p class="text-[10px] font-black uppercase">Đang đi đoàn</p>
                    </div>
                    <div v-else class="text-slate-400">
                        <p class="text-[10px] font-black uppercase text-center">Nghỉ</p>
                    </div>
                </div>
                <div class="text-right">
                    <p class="text-[9px] font-black text-slate-300 uppercase">Lương cơ bản</p>
                    <p class="font-black text-slate-700 tracking-tight">{{ formatPrice(item.baseSalary || 0) }}</p>
                </div>
                <div class="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-300 group-hover:bg-primary group-hover:text-white transition-all">
                    <ArrowRight class="w-5 h-5" />
                </div>
            </div>
        </div>

        <div v-if="filteredList.length === 0" class="py-40 bg-slate-50/50 rounded-[3rem] border-2 border-dashed border-slate-100 flex flex-col items-center justify-center gap-4">
            <UsersIcon class="w-12 h-12 text-slate-200" />
            <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không có dữ liệu nhân sự</p>
        </div>
    </div>

    <!-- Staff Detail Modal -->
    <div v-if="showModal" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-4xl max-h-[90vh] overflow-hidden rounded-[2.5rem] shadow-2xl flex flex-col animate-fade-in-up">
            <div class="p-8 border-b border-slate-50 flex justify-between items-center bg-white">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 bg-indigo-600 text-white rounded-xl flex items-center justify-center">
                        <UsersIcon class="w-6 h-6" />
                    </div>
                    <div>
                        <h3 class="text-xl font-black text-slate-800">{{ currentStaff.staffId ? currentStaff.fullName : 'Thêm Nhân sự mới' }}</h3>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ currentStaff.employeeCode || 'TỰ ĐỘNG' }}</p>
                    </div>
                </div>
                <button @click="showModal = false" class="p-2 hover:bg-slate-50 rounded-xl"><X class="w-6 h-6 text-slate-300" /></button>
            </div>

            <div class="flex-1 overflow-y-auto p-8 bg-slate-50/30">
                <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    <!-- Basic Info Form -->
                    <div class="lg:col-span-2 space-y-6">
                        <form id="staffForm" @submit.prevent="saveStaff" class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">Họ và Tên (Có dấu) *</label>
                                <input v-model="currentStaff.fullName" required class="input-premium" placeholder="VD: Nguyễn Văn A" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">Giới tính</label>
                                <select v-model="currentStaff.gender" class="input-premium">
                                    <option value="Nam">Nam</option>
                                    <option value="Nữ">Nữ</option>
                                </select>
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">Chức danh môn</label>
                                <input v-model="currentStaff.jobTitle" required class="input-premium" placeholder="VD: Bác sĩ, Điều dưỡng..." />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">Lương / Ngày công *</label>
                                <input v-model.number="currentStaff.baseSalary" type="number" required class="input-premium" />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">CCCD / CMND</label>
                                <input v-model="currentStaff.idCardNumber" class="input-premium" placeholder="001..." />
                            </div>
                            <div class="space-y-2">
                                <label class="text-[10px] font-black uppercase text-slate-400">Vai trò hệ thống</label>
                                <select v-model="currentStaff.systemRole" class="input-premium" required>
                                    <option value="MedicalStaff">MedicalStaff (Đi đoàn)</option>
                                    <option value="PersonnelManager">PersonnelManager (NS)</option>
                                    <option value="ContractManager">ContractManager (KD)</option>
                                    <option value="WarehouseManager">WarehouseManager (Kho)</option>
                                    <option value="Admin">Admin (Quản trị)</option>
                                </select>
                            </div>
                        </form>

                        <!-- Lịch sử công tác (Read-only) -->
                        <div v-if="currentStaff.staffId" class="pt-6 border-t border-slate-100">
                             <h4 class="text-xs font-black uppercase text-slate-400 mb-4 flex items-center gap-2">
                                 <HistoryIcon class="w-4 h-4" /> Lịch sử đăng ký thực địa
                             </h4>
                             <div class="space-y-3">
                                 <div v-for="(day, idx) in currentStaff.workdays" :key="idx" class="flex justify-between items-center p-4 bg-white rounded-2xl shadow-sm border border-slate-50">
                                     <div>
                                         <p class="text-sm font-bold text-slate-700">{{ formatDate(day.date) }}</p>
                                         <p class="text-[9px] font-black text-indigo-400 uppercase tracking-widest">{{ day.workPosition || 'Vị trí: N/A' }}</p>
                                     </div>
                                     <span class="text-[10px] font-black px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg">{{ day.groupName }}</span>
                                 </div>
                                 <div v-if="!currentStaff.workdays?.length" class="text-center py-6 text-slate-300 text-[10px] font-bold uppercase italic">Chưa có lịch sử làm việc</div>
                             </div>
                        </div>
                    </div>

                    <!-- Side Actions -->
                    <div class="space-y-6">
                        <div class="bg-white p-6 rounded-3xl border border-slate-100 shadow-sm text-center">
                            <div @click="triggerAvatarUpload" class="w-32 h-32 mx-auto rounded-[2rem] overflow-hidden bg-slate-50 border-4 border-slate-50 shadow-inner group cursor-pointer relative mb-4">
                                <img v-if="currentStaff.avatarPath" :src="`http://localhost:5283/${currentStaff.avatarPath}`" class="w-full h-full object-cover" />
                                <div v-else class="w-full h-full flex flex-col items-center justify-center text-slate-300">
                                    <Camera class="w-8 h-8 mb-1" />
                                    <span class="text-[8px] font-black uppercase">Click Tải lên</span>
                                </div>
                                <div class="absolute inset-0 bg-primary/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all text-white font-black text-[10px]">THAY ĐỔI</div>
                            </div>
                            <input type="file" ref="avatarInput" class="hidden" @change="onAvatarChange" />
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Ảnh đại diện nội bộ</p>
                        </div>

                        <div v-if="currentStaff.staffId" class="bg-indigo-600 text-white p-6 rounded-3xl shadow-lg shadow-indigo-200">
                             <p class="text-[9px] font-black uppercase opacity-60 mb-2">Thù lao tạm tính (Tổng)</p>
                             <p class="text-2xl font-black tracking-tighter">{{ formatPrice(currentStaff.shifts?.reduce((sum, s) => sum + s.calculatedSalary, 0) || 0) }}</p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="p-8 border-t border-slate-50 flex justify-between gap-4 bg-white">
                <button v-if="currentStaff.staffId && authStore.role === 'Admin'" @click="deleteStaff" type="button" class="px-6 py-3 text-rose-500 font-bold hover:bg-rose-50 rounded-xl transition-all">Gỡ bỏ</button>
                <div class="flex-1"></div>
                <button @click="showModal = false" class="px-8 py-3 text-slate-400 font-black">QUAY LẠI</button>
                <button form="staffForm" type="submit" class="bg-slate-900 text-white px-10 py-3 rounded-xl font-black shadow-lg">LƯU THÔNG TIN</button>
            </div>
        </div>
    </div>

    <!-- Hidden Elements -->
    <input type="file" ref="importInput" class="hidden" @change="handleImportFile" />
    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { Users as UsersIcon, Plus, Search, ArrowRight, X, Camera, Save, History as HistoryIcon, Download, Upload as UploadIcon } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()
const list = ref([])
const showModal = ref(false)
const searchQuery = ref('')
const activeTab = ref('All')
const currentStaff = ref({})
const avatarInput = ref(null)
const importInput = ref(null)
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const roles = computed(() => [...new Set(list.value.map(s => s.jobTitle))])

const filteredList = computed(() => {
    let res = list.value
    if (activeTab.value !== 'All') res = res.filter(s => s.jobTitle === activeTab.value)
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase()
        res = res.filter(s => s.fullName.toLowerCase().includes(q) || s.employeeCode.toLowerCase().includes(q))
    }
    return res
})

const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Staffs')
        list.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu nhân viên") }
}

const openModal = async (staff = null) => {
    if (staff) {
        try {
            const res = await axios.get(`http://localhost:5283/api/Staffs/${staff.staffId}`)
            currentStaff.value = res.data
        } catch (e) { currentStaff.value = { ...staff, workdays: [], shifts: [] } }
    } else {
        currentStaff.value = { fullName: '', gender: 'Nam', jobTitle: 'Bác sĩ', baseSalary: 1000000, systemRole: 'MedicalStaff' }
    }
    showModal.value = true
}

const saveStaff = async () => {
    try {
        if (currentStaff.value.staffId) {
            await axios.put(`http://localhost:5283/api/Staffs/${currentStaff.value.staffId}`, currentStaff.value)
        } else {
            await axios.post('http://localhost:5283/api/Staffs', currentStaff.value)
        }
        toast.success("Đã ghi nhận dữ liệu nhân sự!")
        showModal.value = false
        fetchList()
    } catch (e) { toast.error("Lỗi khi lưu dữ liệu") }
}

const deleteStaff = async () => {
    if (!confirm("Xóa nhân viên này?")) return
    try {
        await axios.delete(`http://localhost:5283/api/Staffs/${currentStaff.value.staffId}`)
        toast.success("Đã gỡ bỏ!")
        showModal.value = false
        fetchList()
    } catch (e) { toast.error("Không thể xóa") }
}

const triggerAvatarUpload = () => avatarInput.value.click()
const onAvatarChange = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        const res = await axios.post('http://localhost:5283/api/Staffs/upload-avatar', formData)
        currentStaff.value.avatarPath = res.data.path
    } catch (e) { toast.error("Lỗi tải ảnh") }
}

const exportStaff = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Staffs/export', { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'DanhSachNhanSu.xlsx')
        document.body.appendChild(link)
        link.click()
        toast.success("Đã tải tệp Excel danh sách nhân sự!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const triggerImport = () => importInput.value.click()
const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        await axios.post('http://localhost:5283/api/Staffs/import', formData)
        toast.success("Đã nhập dữ liệu nhân sự thành công!")
        fetchList()
    } catch (e) { toast.error("Lỗi nhập dữ liệu từ Excel") }
}

const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchList)
</script>
