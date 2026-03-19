<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-sky-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <UsersIcon class="w-6 h-6" />
          </div>
          Quản lý Nhân sự
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-indigo-600 font-black tabular-nums">{{ String(list.length).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-widest text-[9px] mt-2">Nội bộ: Lịch sử công tác & Thù lao nhân sự</p>
      </div>
      <div class="flex items-center gap-3">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="exportStaff" 
                class="btn-premium bg-white border border-slate-200 text-slate-600 px-6 py-3 shadow-sm hover:bg-slate-50">
          <Download class="w-4 h-4 mr-2" />
          <span class="text-[10px] font-black uppercase tracking-widest">XUẤT EXCEL</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="triggerImport" 
                class="btn-premium bg-indigo-50 text-indigo-600 px-6 py-3 shadow-sm hover:bg-indigo-100">
          <UploadIcon class="w-4 h-4 mr-2" />
          <span class="text-[10px] font-black uppercase tracking-widest">NHẬP EXCEL</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                @click="openModal()" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 shadow-lg">
          <Plus class="w-5 h-5" />
          <span>THÊM NHÂN SỰ</span>
        </button>
      </div>
    </div>
    
    <!-- Stats Section -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
        <StatCard 
            title="Tổng nhân sự hệ thống"
            :value="String(list.length).padStart(3, '0')"
            :icon="UsersIcon"
            variant="indigo"
            subtext="Toàn bộ danh sách"
        />
        <StatCard 
            title="Nhân sự đang đi đoàn"
            :value="String(list.filter(s => s.isWorking).length).padStart(3, '0')"
            :icon="Stethoscope"
            variant="emerald"
            subtext="Vận hành thực địa"
        />
        <StatCard 
            title="Tổng quỹ lương (Cơ bản)"
            :value="formatPrice(list.reduce((sum, s) => sum + (s.baseSalary || 0), 0))"
            :icon="Wallet"
            variant="rose"
            subtext="Chi phí định kỳ"
        />
    </div>

    <!-- Search & List in Table Format -->
    <div class="premium-card bg-white border border-slate-100 overflow-hidden">
        <div class="p-6 border-b border-slate-50 flex flex-col md:flex-row gap-4 bg-slate-50/30">
            <div class="relative group flex-1">
                <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                <input v-model="searchQuery" placeholder="Tìm tên hoặc mã nhân viên..." 
                       class="w-full pl-10 pr-4 py-2 rounded-xl bg-white border border-slate-200 focus:border-indigo-600/20 outline-none font-black text-xs text-slate-600 shadow-sm transition-all" />
            </div>
            <select v-model="activeTab" class="px-4 py-2 rounded-xl bg-white border border-slate-200 font-black text-xs uppercase tracking-widest text-slate-500 outline-none min-w-[200px]">
                <option value="All">Tất cả chức danh ({{ list.length }})</option>
                <option v-for="role in roles" :key="role" :value="role">
                    {{ role }} ({{ list.filter(s => s.jobTitle === role).length }})
                </option>
            </select>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-4 text-center w-16">STT</th>
                        <th class="p-4">Thông tin nhân sự</th>
                        <th class="p-4">Chức danh</th>
                        <th class="p-4">Vai trò</th>
                        <th class="p-4 text-center">Trạng thái</th>
                        <th class="p-4 text-right">Lương cơ bản</th>
                        <th class="p-4 text-center">Tác vụ</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(item, index) in filteredList" :key="item.staffId" 
                        class="text-xs hover:bg-slate-50/50 transition-all cursor-pointer" @click="openModal(item)">
                        <td class="p-4 text-center font-black text-slate-400">{{ String(index + 1).padStart(3, '0') }}</td>
                        <td class="p-4">
                            <div class="flex items-center gap-3">
                                <div class="w-10 h-10 rounded-xl overflow-hidden border border-slate-100 bg-slate-50">
                                    <img v-if="item.avatarPath" :src="`http://localhost:5283/${item.avatarPath}`" class="w-full h-full object-cover" />
                                    <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${item.fullName}`" class="w-full h-full" />
                                </div>
                                <div>
                                    <div class="font-black text-slate-800 uppercase tracking-widest group-hover:text-indigo-600">{{ item.fullName }}</div>
                                    <div class="text-[9px] font-black text-slate-400 mt-1 uppercase tracking-widest">{{ item.employeeCode }}</div>
                                </div>
                            </div>
                        </td>
                        <td class="p-4 font-black text-slate-500 uppercase tracking-widest">{{ item.jobTitle }}</td>
                        <td class="p-4">
                            <span class="inline-flex px-2 py-1 rounded-md bg-slate-100 text-slate-600 font-black text-[9px] uppercase tracking-widest ">
                                {{ i18n.t('roles.' + (item.systemRole || 'MedicalStaff')) }}
                            </span>
                        </td>
                        <td class="p-4 text-center">
                            <span v-if="item.isWorking" class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-emerald-50 text-emerald-600 text-[10px] font-black uppercase tracking-widest ">
                                <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                                Đi đoàn
                            </span>
                            <span v-else class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-slate-50 text-slate-400 text-[10px] font-black uppercase tracking-widest ">
                                Nghỉ
                            </span>
                        </td>
                        <td class="p-4 text-right font-black text-slate-700">
                            {{ formatPrice(item.baseSalary || 0) }}
                        </td>
                        <td class="p-4 text-center">
                            <button v-if="authStore.role === 'Admin' || authStore.role === 'PersonnelManager'" 
                                    @click="openModal(item)" class="btn-action-premium variant-indigo text-slate-400" title="Cập nhật">
                                <Edit3 class="w-5 h-5" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="filteredList.length === 0">
                        <td colspan="7" class="py-20 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <UsersIcon class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không có dữ liệu nhân sự</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Staff Detail Modal -->
    <div v-if="showModal" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4 overflow-y-auto">
        <div class="bg-white w-full max-w-4xl overflow-hidden rounded-[2.5rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative mt-auto mb-auto">
            <!-- Border Overlay -->
            <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
            
            <!-- Header Gradient -->
            <div class="absolute top-0 left-0 right-0 h-32 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>
            
            <button @click="showModal = false" class="absolute top-6 right-6 p-2 bg-white/20 hover:bg-white/40 rounded-full transition-all text-white z-20 flex items-center justify-center">
                <X class="w-6 h-6" />
            </button>

            <div class="p-8 pb-4 flex justify-between items-center bg-transparent relative z-10">
                <div class="flex items-center gap-4 text-white">
                    <div class="w-12 h-12 bg-white text-teal-600 rounded-xl flex items-center justify-center shadow-lg">
                        <UsersIcon class="w-6 h-6" />
                    </div>
                    <div>
                        <h3 class="text-xl font-black text-white">{{ currentStaff.staffId ? currentStaff.fullName : 'Thêm Nhân sự mới' }}</h3>
                        <p class="text-[10px] font-black opacity-80 uppercase tracking-widest text-white">{{ currentStaff.employeeCode || 'TỰ ĐỘNG' }}</p>
                    </div>
                </div>
            </div>

            <div class="p-8 bg-slate-50/30">
                <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    <!-- Basic Info Form -->
                    <div class="lg:col-span-2 space-y-6">
                        <form id="staffForm" @submit.prevent="saveStaff" class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Họ và Tên (Có dấu) *</label>
                                <input v-model="currentStaff.fullName" required class="input-premium w-full" placeholder="VD: Nguyễn Văn A" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Giới tính</label>
                                <select v-model="currentStaff.gender" class="input-premium w-full">
                                    <option value="Nam">Nam</option>
                                    <option value="Nữ">Nữ</option>
                                </select>
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Chức danh</label>
                                <select v-model="jobCategory" required class="input-premium w-full text-xs">
                                    <option v-for="job in standardJobs" :key="job" :value="job">{{ job }}</option>
                                    <option value="Khác">Khác...</option>
                                </select>
                            </div>
                            <div v-if="jobCategory === 'Khác'" class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-indigo-400 italic">Nhập chức danh cụ thể *</label>
                                <input v-model="currentStaff.jobTitle" required class="input-premium w-full border-indigo-200" placeholder="VD: Lái xe, Tạp vụ..." />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Lương / Ngày công *</label>
                                <input v-model.number="currentStaff.baseSalary" type="number" required class="input-premium w-full" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">CCCD / CMND</label>
                                <input v-model="currentStaff.idCardNumber" class="input-premium w-full" placeholder="001..." />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Địa chỉ Email (Nhận thông báo)</label>
                                <input v-model="currentStaff.email" type="email" class="input-premium w-full" placeholder="VD: nhanvien@gmail.com" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Vai trò hệ thống</label>
                                <select v-model="currentStaff.systemRole" class="input-premium w-full" required>
                                    <option value="MedicalStaff">{{ i18n.t('roles.MedicalStaff') }}</option>
                                    <option value="PersonnelManager">{{ i18n.t('roles.PersonnelManager') }}</option>
                                    <option value="ContractManager">{{ i18n.t('roles.ContractManager') }}</option>
                                    <option value="MedicalGroupManager">{{ i18n.t('roles.MedicalGroupManager') }}</option>
                                    <option value="WarehouseManager">{{ i18n.t('roles.WarehouseManager') }}</option>
                                    <option value="PayrollManager">{{ i18n.t('roles.PayrollManager') }}</option>
                                    <option value="Admin">{{ i18n.t('roles.Admin') }}</option>
                                    <option value="Customer">{{ i18n.t('roles.Customer') }}</option>
                                </select>
                            </div>
                        </form>

                        <!-- Lịch sử công tác (Read-only) -->
                        <div v-if="currentStaff.staffId" class="pt-6 border-t border-slate-100">
                             <h4 class="text-xs font-black uppercase tracking-widest text-slate-400 mb-4 flex items-center gap-2">
                                 <HistoryIcon class="w-4 h-4" /> Lịch sử đăng ký thực địa
                             </h4>
                             <div class="space-y-3">
                                 <div v-for="(day, idx) in currentStaff.workdays" :key="idx" class="flex justify-between items-center p-4 bg-white rounded-2xl shadow-sm border border-slate-50">
                                     <div>
                                         <p class="text-sm font-black text-slate-700">{{ formatDate(day.date) }}</p>
                                         <p class="text-[9px] font-black text-indigo-400 uppercase tracking-widest ">{{ day.workPosition || 'Vị trí: N/A' }}</p>
                                     </div>
                                     <span class="text-[10px] font-black px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg">{{ day.groupName }}</span>
                                 </div>
                                 <div v-if="!currentStaff.workdays?.length" class="text-center py-6 text-slate-300 text-[10px] font-black uppercase tracking-widest italic">Chưa có lịch sử làm việc</div>
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
                                    <span class="text-[8px] font-black uppercase tracking-widest">Click Tải lên</span>
                                </div>
                                <div class="absolute inset-0 bg-primary/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all text-white font-black text-[10px]">THAY ĐỔI</div>
                            </div>
                            <input type="file" ref="avatarInput" class="hidden" @change="onAvatarChange" />
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest ">Ảnh đại diện nội bộ</p>
                        </div>

                        <div v-if="currentStaff.staffId" class="bg-indigo-600 text-white p-6 rounded-3xl shadow-xl shadow-indigo-100 flex flex-col justify-between h-40 relative overflow-hidden">
                             <div class="relative z-10">
                                <p class="text-[9px] font-black uppercase opacity-60 mb-2 tracking-[0.2em]">Thù lao tạm tính (Tổng)</p>
                                <p class="text-3xl font-black ">{{ formatPrice(currentStaff.shifts?.reduce((sum, s) => sum + s.calculatedSalary, 0) || 0) }}</p>
                             </div>
                             <div class="relative z-10 border-t border-white/10 pt-4 flex justify-between items-center">
                                <span class="text-[10px] font-black opacity-60 uppercase tracking-widest">{{ currentStaff.shifts?.length || 0 }} Buổi làm việc</span>
                                <div class="w-8 h-8 bg-white/10 rounded-lg flex items-center justify-center">
                                    <ArrowRight class="w-4 h-4" />
                                </div>
                             </div>
                             <Wallet class="absolute -right-4 -bottom-4 w-24 h-24 opacity-10 rotate-12" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="p-8 border-t border-slate-50 flex justify-between gap-4 bg-white">
                <button v-if="currentStaff.staffId && authStore.role === 'Admin'" @click="deleteStaff" type="button" class="px-6 py-3 text-rose-500 font-black hover:bg-rose-50 rounded-xl transition-all">Gỡ bỏ</button>
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
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { 
  Users as UsersIcon, Plus, Search, ArrowRight, X, Camera, Save, 
  History as HistoryIcon, Download, Upload as UploadIcon, Wallet, Stethoscope 
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import StatCard from '../components/StatCard.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useI18nStore } from '../stores/i18n'

const authStore = useAuthStore()
const i18n = useI18nStore()
const toast = useToast()
const list = ref([])
const showModal = ref(false)
const searchQuery = ref('')
const activeTab = ref('All')
const currentStaff = ref({})
const avatarInput = ref(null)
const importInput = ref(null)
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const standardJobs = ['Bác sĩ', 'Điều dưỡng', 'Kỹ thuật viên', 'Dược sĩ', 'Nhân viên hỗ trợ']
const jobCategory = ref('Bác sĩ')

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
            // Đảm bảo systemRole luôn tồn tại và map đúng kể cả nếu backend trả PascalCase
            const data = res.data
            currentStaff.value = {
                ...data,
                systemRole: data.systemRole || data.SystemRole || 'MedicalStaff'
            }
            // Đồng bộ jobCategory
            if (standardJobs.includes(currentStaff.value.jobTitle)) {
                jobCategory.value = currentStaff.value.jobTitle
            } else {
                jobCategory.value = 'Khác'
            }
        } catch (e) { 
            currentStaff.value = { ...staff, workdays: [], shifts: [], systemRole: staff.systemRole || 'MedicalStaff' } 
            if (standardJobs.includes(currentStaff.value.jobTitle)) {
                jobCategory.value = currentStaff.value.jobTitle
            } else {
                jobCategory.value = 'Khác'
            }
        }
    } else {
        currentStaff.value = { fullName: '', email: '', gender: 'Nam', jobTitle: 'Bác sĩ', baseSalary: 1000000, systemRole: 'MedicalStaff' }
        jobCategory.value = 'Bác sĩ'
    }
    showModal.value = true
}

watch(jobCategory, (newVal) => {
    if (newVal !== 'Khác') {
        currentStaff.value.jobTitle = newVal
    } else if (!currentStaff.value.jobTitle || standardJobs.includes(currentStaff.value.jobTitle)) {
        currentStaff.value.jobTitle = '' // Clear if switching to "Khác" from a standard job
    }
})

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
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi khi lưu dữ liệu") 
    }
}

const deleteStaff = async () => {
    if (!confirm("Xóa nhân viên này?")) return
    try {
        await axios.delete(`http://localhost:5283/api/Staffs/${currentStaff.value.staffId}`)
        toast.success("Đã gỡ bỏ!")
        showModal.value = false
        fetchList()
    } catch (e) { 
        toast.error(e.response?.data || "Không thể xóa") 
    }
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
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi tải ảnh") 
    }
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
