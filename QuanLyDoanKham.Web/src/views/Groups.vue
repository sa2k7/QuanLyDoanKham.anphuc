<template>
  <div class="space-y-8 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Stethoscope class="w-6 h-6" />
          </div>
          Điều hành Đoàn khám
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[9px] mt-2">Nội bộ: Quản lý nhân sự, Vị trí trực & Trạng thái làm việc</p>
      </div>

      <div class="flex items-center gap-4">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                @click="exportGroups" 
                class="btn-premium bg-white border border-slate-200 text-slate-600 px-6 py-3 rounded-xl shadow-sm">
            <Download class="w-4 h-4 mr-2" />
            <span>XUẤT DS ĐOÀN</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                @click="showForm = !showForm" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 rounded-xl shadow-lg">
            <Plus class="w-5 h-5" />
            <span>KHỞI TẠO ĐOÀN</span>
        </button>
      </div>
    </div>

    <!-- Form Tạo Đoàn & Auto Create -->
    <div v-if="showForm" class="premium-card p-8 bg-white border-4 border-indigo-50 mb-8 animate-slide-up">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8 pb-8 border-b border-slate-100">
            <div class="space-y-2">
                <label class="text-[10px] font-black uppercase text-slate-400">Chọn Hợp đồng để Tạo nhanh</label>
                <div class="flex gap-2">
                    <select v-model="selectedContractForAuto" class="input-premium flex-1">
                        <option v-for="c in contracts" :key="c.healthContractId" :value="c.healthContractId">{{ c.companyName }}</option>
                    </select>
                    <button @click="autoCreateGroup" :disabled="!selectedContractForAuto" class="bg-emerald-600 text-white px-4 rounded-xl font-black text-[10px] uppercase shadow-md disabled:bg-slate-200">
                        TẠO TỰ ĐỘNG
                    </button>
                </div>
                <p class="text-[9px] text-slate-400 italic">* Hệ thống tự điền tên và ngày dự kiến (7 ngày sau)</p>
            </div>
        </div>

        <h3 class="text-xs font-black text-slate-400 uppercase mb-4 tracking-widest">Hoặc khai báo thủ công</h3>
        <form @submit.prevent="addGroup" class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="space-y-2">
                <label class="text-[10px] font-black uppercase text-slate-400">Hợp đồng mục tiêu</label>
                <select v-model="newGroup.healthContractId" required class="input-premium">
                    <option v-for="c in contracts" :key="c.healthContractId" :value="c.healthContractId">{{ c.companyName }}</option>
                </select>
            </div>
            <div class="space-y-2">
                <label class="text-[10px] font-black uppercase text-slate-400">Tên đoàn khám</label>
                <input v-model="newGroup.groupName" required class="input-premium" placeholder="VD: Khám sức khỏe CN 2026" />
            </div>
            <div class="space-y-2">
                <label class="text-[10px] font-black uppercase text-slate-400">Ngày triển khai</label>
                <input v-model="newGroup.examDate" type="date" required class="input-premium" />
            </div>
            <div class="md:col-span-3 flex justify-end">
                <button type="submit" class="btn-premium bg-indigo-600 text-white px-10">KÍCH HOẠT ĐOÀN MỚI</button>
            </div>
        </form>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-4 mb-6">
        <button @click="activeTab = 'Open'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Open' ? 'bg-primary text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Đang thực hiện ({{ openGroups.length }})
        </button>
        <button @click="activeTab = 'Finished'" 
                :class="['px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest transition-all', 
                         activeTab === 'Finished' ? 'bg-slate-800 text-white shadow-lg' : 'bg-white text-slate-400 border-2 border-slate-50']">
            Đã hoàn tất ({{ closedGroups.length }})
        </button>
    </div>

    <!-- Danh sách Đoàn -->
    <div class="space-y-6">
        <div v-for="group in filteredGroups" :key="group.groupId" 
             class="premium-card bg-white border-2 border-slate-50 overflow-hidden group/card shadow-sm">
            
            <div class="p-8 bg-slate-900 text-white flex justify-between items-center">
                <div class="flex items-center gap-6">
                    <div class="w-16 h-16 bg-indigo-500 rounded-2xl flex items-center justify-center text-white shadow-xl">
                        <Stethoscope class="w-8 h-8" />
                    </div>
                    <div>
                        <h4 class="text-2xl font-black tracking-tight">{{ group.groupName }}</h4>
                        <p class="text-xs font-bold text-indigo-300 uppercase tracking-widest">{{ group.companyName }} • {{ formatDate(group.examDate) }}</p>
                    </div>
                </div>
                <div class="flex items-center gap-3">
                   <span :class="['px-4 py-1.5 rounded-full text-[9px] font-black uppercase border', getStatusClass(group.status)]">{{ group.status }}</span>
                   <button v-if="group.status === 'Open'" @click="updateStatus(group.groupId, 'Finished')" class="text-[9px] font-black bg-white/10 hover:bg-white/20 px-4 py-2 rounded-xl transition-all">HOÀN TẤT ĐOÀN</button>
                </div>
            </div>

            <div class="p-8">
                <!-- Nhân sự List Table -->
                <div class="space-y-6">
                    <div class="flex justify-between items-center">
                        <h5 class="flex items-center gap-2 text-xs font-black uppercase text-slate-400 tracking-widest">
                            <UsersIcon class="w-4 h-4" /> Đội ngũ đi khám & Vị trí trực
                        </h5>
                        <div class="flex items-center gap-3">
                            <button v-if="group.status === 'Open'" @click="openStaffModal(group.groupId)" class="text-[9px] font-black text-indigo-600 uppercase hover:underline">+ ĐIỀU ĐỘNG & GÁN VỊ TRÍ</button>
                            <div class="w-px h-3 bg-slate-200"></div>
                            <button @click="exportGroupStaff(group.groupId, group.groupName)" class="text-[9px] font-black text-emerald-600 uppercase hover:underline">XUẤT DS ĐI ĐOÀN (EXCEL)</button>
                        </div>
                    </div>
                    
                    <div class="overflow-x-auto">
                        <table class="w-full text-left">
                            <thead class="bg-slate-50 text-[9px] font-black uppercase text-slate-400">
                                <tr>
                                    <th class="p-4">Nhân sự</th>
                                    <th class="p-4">Vị trí làm việc tại đoàn</th>
                                    <th class="p-4 text-center">Ca làm</th>
                                    <th class="p-4 text-center">Trạng thái</th>
                                    <th class="p-4 text-right">Tác vụ</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-slate-50 text-xs">
                                <tr v-for="s in staffDetails[group.groupId]" :key="s.id" class="hover:bg-slate-50/50 transition-all">
                                    <td class="p-4">
                                        <div class="font-black text-slate-800">{{ s.fullName }}</div>
                                        <div class="text-[9px] text-slate-400 uppercase font-bold">{{ s.jobTitle }}</div>
                                    </td>
                                    <td class="p-4">
                                        <span class="px-3 py-1 bg-indigo-50 text-indigo-600 rounded-lg font-black uppercase text-[9px]">
                                            {{ s.workPosition || 'Chưa gán' }}
                                        </span>
                                    </td>
                                    <td class="p-4 text-center font-black">{{ s.shiftType === 1 ? 'Cả ngày' : 'Nửa ngày' }}</td>
                                    <td class="p-4 text-center">
                                        <span :class="['px-3 py-1 rounded-lg font-black text-[9px] uppercase', 
                                                       s.workStatus === 'Đang làm' ? 'bg-emerald-100 text-emerald-600' : 'bg-slate-100 text-slate-400']">
                                            {{ s.workStatus || 'Đang chờ' }}
                                        </span>
                                    </td>
                                    <td class="p-4 text-right">
                                        <button v-if="group.status === 'Open'" @click="removeStaff(s.id, group.groupId)" class="text-rose-400 hover:text-rose-600 p-2">
                                            <Trash2 class="w-4 h-4" />
                                        </button>
                                    </td>
                                </tr>
                                <tr v-if="!staffDetails[group.groupId]?.length">
                                    <td colspan="5" class="p-8 text-center text-slate-300 font-bold uppercase text-[10px] italic">Chưa phân bổ nhân sự</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>

                <!-- Tài liệu đính kèm -->
                <div class="mt-10 pt-8 border-t border-slate-50 flex flex-col md:flex-row justify-between items-center gap-6">
                    <div class="flex items-center gap-4">
                        <FileText class="w-5 h-5 text-slate-400" />
                        <span class="text-[10px] font-black uppercase text-slate-400 tracking-widest">Dữ liệu kết quả & Tài liệu đoàn</span>
                    </div>
                    <div class="flex items-center gap-3">
                        <div v-if="group.importFilePath" class="flex items-center gap-3 p-3 bg-emerald-50 rounded-xl border border-emerald-100">
                             <FileIcon class="w-4 h-4 text-emerald-500" />
                             <span class="text-[10px] font-bold text-slate-600 truncate max-w-[150px]">{{ group.importFilePath.split('/').pop() }}</span>
                             <a :href="'http://localhost:5283/' + group.importFilePath" target="_blank" class="text-[9px] font-black text-emerald-600 uppercase underline ml-2">Tải về</a>
                        </div>
                        <button v-if="group.status === 'Open'" @click="triggerImport(group.groupId)" class="btn-premium bg-white border border-slate-200 text-slate-600 text-[10px] px-6">
                           IMPORT KẾT QUẢ
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Staff Selection Modal -->
    <div v-if="modals.staff.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-lg p-10 rounded-[3rem] shadow-2xl animate-fade-in-up">
            <h3 class="text-xl font-black text-slate-800 mb-8 uppercase tracking-tight flex items-center gap-3">
                <UsersIcon class="w-6 h-6 text-indigo-600" /> Điều động nhân sự
            </h3>
            <form @submit.prevent="addStaff" class="space-y-6">
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Chọn nhân viên (Đảm bảo không trùng lịch)</label>
                    <select v-model="modals.staff.data.staffId" required class="input-premium">
                        <option v-for="s in staffList" :key="s.staffId" :value="s.staffId">{{ s.fullName }} ({{ s.jobTitle }})</option>
                    </select>
                </div>
                
                <div class="grid grid-cols-2 gap-4">
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Loại ca làm</label>
                        <select v-model="modals.staff.data.shiftType" class="input-premium">
                            <option :value="1.0">Cả ngày (1.0)</option>
                            <option :value="0.5">Nửa ngày (0.5)</option>
                        </select>
                    </div>
                    <div class="space-y-2">
                        <label class="text-[10px] font-black uppercase text-slate-400">Trạng thái ban đầu</label>
                        <select v-model="modals.staff.data.workStatus" class="input-premium">
                            <option value="Đang chờ">Đang chờ</option>
                            <option value="Đang làm">Đang làm</option>
                        </select>
                    </div>
                </div>

                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase text-slate-400">Vị trí làm việc tại đoàn</label>
                    <select v-model="modals.staff.data.workPosition" required class="input-premium">
                        <option value="Tiếp nhận">Tiếp nhận</option>
                        <option value="Cân đo huyết áp">Cân đo huyết áp</option>
                        <option value="Khám nội">Khám nội</option>
                        <option value="Khám ngoại">Khám ngoại</option>
                        <option value="Lấy máu">Lấy máu (Cận lâm sàng)</option>
                        <option value="Siêu âm">Siêu âm</option>
                        <option value="Khám sản phụ khoa">Khám sản phụ khoa</option>
                        <option value="Hậu cần">Hậu cần / Khác</option>
                    </select>
                </div>

                <div class="flex gap-4 pt-6">
                    <button type="button" @click="modals.staff.show = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase underline">Hủy bỏ</button>
                    <button type="submit" class="flex-[2] bg-indigo-600 text-white py-4 rounded-xl font-black text-xs uppercase shadow-lg shadow-indigo-200">XÁC NHẬN ĐIỀU ĐỘNG</button>
                </div>
            </form>
        </div>
    </div>

    <!-- Hidden Input for Import -->
    <input type="file" ref="importInput" class="hidden" @change="handleImportFile" />

    <ConfirmDialog v-model="confirmData.show" :title="confirmData.title" :message="confirmData.message" :variant="confirmData.variant" @confirm="confirmData.onConfirm" />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { Stethoscope, Plus, Building2, Calendar, Users as UsersIcon, FileText, Trash2, FileIcon, X, Download, Upload as UploadIcon } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const authStore = useAuthStore()
const toast = useToast()
const groups = ref([])
const contracts = ref([])
const staffList = ref([])
const staffDetails = ref({})
const showForm = ref(false)
const activeTab = ref('Open')
const importInput = ref(null)
const currentGroupId = ref(null)
const selectedContractForAuto = ref(null)

const newGroup = ref({ healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0] })
const modals = ref({
    staff: { show: false, groupId: null, data: { staffId: null, shiftType: 1.0, workPosition: 'Tiếp nhận', workStatus: 'Đang chờ' } }
})
const confirmData = ref({ show: false, title: '', message: '', variant: 'warning', onConfirm: () => {} })

const openGroups = computed(() => groups.value.filter(g => g.status === 'Open'))
const closedGroups = computed(() => groups.value.filter(g => g.status !== 'Open'))
const filteredGroups = computed(() => activeTab.value === 'Open' ? openGroups.value : closedGroups.value)

const getStatusClass = (status) => {
    switch(status) {
        case 'Open': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'Finished': return 'bg-slate-50 text-slate-500 border-slate-200'
        case 'Locked': return 'bg-rose-50 text-rose-600 border-rose-100'
        default: return 'bg-slate-100 text-slate-400 border-slate-200'
    }
}

const fetchData = async () => {
    try {
        const [gRes, cRes, sRes] = await Promise.all([
            axios.get('http://localhost:5283/api/MedicalGroups'),
            axios.get('http://localhost:5283/api/HealthContracts'),
            axios.get('http://localhost:5283/api/Staffs')
        ]);
        groups.value = gRes.data
        contracts.value = cRes.data
        staffList.value = sRes.data
        groups.value.forEach(g => fetchGroupStaff(g.groupId))
    } catch (e) { toast.error("Lỗi khi tải dữ liệu") }
}

const fetchGroupStaff = async (id) => {
    try {
        const res = await axios.get(`http://localhost:5283/api/MedicalGroups/${id}/staffs`)
        staffDetails.value[id] = res.data
    } catch (e) { console.error(e) }
}

const addGroup = async () => {
    try {
        await axios.post('http://localhost:5283/api/MedicalGroups', newGroup.value)
        toast.success("Khởi tạo đoàn thành công!")
        showForm.value = false
        fetchData()
    } catch (e) { toast.error("Lỗi khi tạo đoàn khám") }
}

const autoCreateGroup = async () => {
    if (!selectedContractForAuto.value) return
    try {
        await axios.post(`http://localhost:5283/api/MedicalGroups/auto-create/${selectedContractForAuto.value}`)
        toast.success("Đã tạo đoàn khám tự động từ hợp đồng!")
        showForm.value = false
        fetchData()
    } catch (e) { toast.error(e.response?.data || "Lỗi tạo tự động") }
}

const updateStatus = async (id, status) => {
    try {
        await axios.put(`http://localhost:5283/api/MedicalGroups/${id}`, { status: status })
        toast.success(`Đã cập nhật trạng thái: ${status}`)
        fetchData()
    } catch (e) { toast.error("Lỗi cập nhật trạng thái") }
}

const openStaffModal = (gid) => {
    modals.value.staff.groupId = gid
    modals.value.staff.show = true
    modals.value.staff.data = { staffId: null, shiftType: 1.0, workPosition: 'Tiếp nhận', workStatus: 'Đang chờ' }
}

const addStaff = async () => {
    try {
        const gid = modals.value.staff.groupId
        await axios.post(`http://localhost:5283/api/MedicalGroups/${gid}/staffs`, modals.value.staff.data)
        toast.success("Đã phân công nhân sự!")
        modals.value.staff.show = false
        fetchGroupStaff(gid)
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi khi phân công. Nhân viên có thể bị trùng lịch!") 
    }
}

const exportGroups = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/MedicalGroups/export', { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'DanhSachDoanKham.xlsx')
        document.body.appendChild(link)
        link.click()
        toast.success("Đã xuất danh sách đoàn khám!")
    } catch (e) { toast.error("Lỗi xuất file") }
}

const exportGroupStaff = async (id, name) => {
    try {
        const res = await axios.get(`http://localhost:5283/api/MedicalGroups/${id}/export-staff`, { responseType: 'blob' })
        const url = window.URL.createObjectURL(new Blob([res.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `NhanSu_${name}.xlsx`)
        document.body.appendChild(link)
        link.click()
        toast.success(`Đã xuất danh sách nhân sự cho đoàn ${name}!`)
    } catch (e) { toast.error("Lỗi xuất file") }
}

const removeStaff = async (detailId, gid) => {
    try {
        await axios.delete(`http://localhost:5283/api/MedicalGroups/staffs/${detailId}`)
        toast.success("Đã gỡ nhân sự")
        fetchGroupStaff(gid)
    } catch (e) { toast.error("Lỗi khi gỡ nhân sự") }
}

const triggerImport = (id) => {
    currentGroupId.value = id
    importInput.value.click()
}

const handleImportFile = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    const formData = new FormData()
    formData.append('file', file)
    try {
        const res = await axios.post(`http://localhost:5283/api/MedicalGroups/upload-data`, formData)
        await axios.put(`http://localhost:5283/api/MedicalGroups/${currentGroupId.value}`, { importFilePath: res.data.path })
        toast.success("Đã Import dữ liệu đoàn khám!")
        fetchData()
    } catch (e) { toast.error("Lỗi Import file") }
}

const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')
const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchData)
</script>
