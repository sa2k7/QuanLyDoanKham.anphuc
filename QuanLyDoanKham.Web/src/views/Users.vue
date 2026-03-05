<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-indigo-600 text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-indigo-200">
            <ShieldCheck class="w-8 h-8" />
          </div>
          {{ isAdmin ? 'Quản trị Hệ thống' : 'Thông tin Tài khoản' }}
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">{{ isAdmin ? 'Phân quyền & Kiểm soát truy cập tập trung' : 'Thông tin cá nhân & Nhật ký hoạt động' }}</p>
      </div>
      <button v-if="isAdmin" @click="openCreateModal" 
              class="btn-premium bg-slate-900 text-white hover:bg-black shadow-2xl shadow-slate-200 px-10 py-4">
        <UserPlus class="w-6 h-6" />
        <span>CẤP TÀI KHOẢN MỚI</span>
      </button>
    </div>

    <!-- ADMIN VIEW: USER LIST -->
    <div v-if="isAdmin" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-20">
        <div v-for="u in users" :key="u.username" 
             class="premium-card p-8 bg-white border-2 border-slate-50 hover:border-indigo-600/20 transition-all group relative overflow-hidden">
            
            <div class="absolute right-0 top-0 p-4">
                <div :class="['w-2 h-2 rounded-full', u.username === 'admin' ? 'bg-emerald-500 animate-pulse' : 'bg-slate-200']"></div>
            </div>

            <div class="flex items-center space-x-5 mb-8">
                <div class="w-16 h-16 rounded-2xl bg-slate-50 border-2 border-slate-100 overflow-hidden flex-shrink-0 shadow-inner group-hover:scale-105 transition-transform">
                    <img v-if="u.avatarPath" :src="`http://localhost:5283/${u.avatarPath}`" class="w-full h-full object-cover" />
                    <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${u.username}`" class="w-full h-full object-cover" />
                </div>
                <div>
                    <h4 class="font-black text-slate-800 text-lg tracking-tight group-hover:text-indigo-600 transition-colors">{{ u.fullName || 'Chưa đặt tên' }}</h4>
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] flex items-center gap-2 mt-1">
                        <AtSign class="w-3 h-3 text-indigo-400" />
                        {{ u.username }}
                    </p>
                </div>
            </div>

            <div class="mb-8 p-4 bg-slate-50 rounded-2xl border border-slate-100 group-hover:bg-indigo-50/30 group-hover:border-indigo-100 transition-colors">
                <div class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-2">Vai trò hệ thống</div>
                <div class="text-xs font-black text-slate-700 tracking-tight">
                    {{ u.roleName === 'PersonnelManager' ? 'Quản lý Nhân sự' :
                       u.roleName === 'ContractManager' ? 'Quản lý Hợp đồng' :
                       u.roleName === 'PayrollManager' ? 'Kế toán hệ thống' :
                       u.roleName === 'MedicalGroupManager' ? 'Điều phối vận hành' :
                       u.roleName === 'WarehouseManager' ? 'Quản lý hậu cần' :
                       u.roleName === 'MedicalStaff' ? 'Bác sĩ/Kỹ thuật viên' : u.roleName === 'Admin' ? 'Quản trị viên' : u.roleName }}
                </div>
            </div>

            <div class="pt-6 border-t border-slate-50 flex items-center justify-between">
                <button @click="openEditModal(u)" class="text-[10px] font-black uppercase tracking-widest text-indigo-600 hover:text-slate-900 transition-all flex items-center gap-2">
                    <Edit3 class="w-4 h-4" />
                    HIỆU CHỈNH
                </button>
                <button v-if="u.username !== 'admin'" @click="handleDelete(u.username)" class="w-10 h-10 flex items-center justify-center bg-rose-50 text-rose-400 rounded-xl hover:bg-rose-500 hover:text-white transition-all">
                    <Trash2 class="w-4 h-4" />
                </button>
            </div>
            
            <div class="absolute -bottom-4 -right-4 w-20 h-20 bg-indigo-50/50 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity"></div>
        </div>
    </div>

    <!-- USER VIEW: PROFILE CARD -->
    <div v-else-if="profile" class="max-w-4xl mx-auto">
        <div class="bg-white rounded-[4rem] neo-shadow border border-slate-100 overflow-hidden relative group/profile">
            <!-- Header Cover -->
            <div class="h-64 bg-slate-900 relative overflow-hidden">
                <div class="absolute inset-0 bg-gradient-to-br from-indigo-600/30 to-cyan-600/30"></div>
                <div class="absolute top-[-50%] left-[-10%] w-[120%] h-[120%] opacity-20" style="background-image: radial-gradient(circle at 1px 1px, white 1px, transparent 0); background-size: 40px 40px;"></div>
                <div class="absolute bottom-10 left-12 flex items-center gap-6">
                    <div class="w-40 h-40 rounded-[3rem] bg-white p-2 shadow-2xl border-4 border-white group-hover/profile:scale-105 transition-transform duration-500">
                        <img v-if="profile.avatarPath" :src="`http://localhost:5283/${profile.avatarPath}`" class="w-full h-full object-cover rounded-[2.5rem] bg-slate-50" />
                        <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${profile.username}`" class="w-full h-full object-cover rounded-[2.5rem] bg-slate-50" />
                    </div>
                    <div class="mb-4">
                        <h3 class="text-4xl font-black text-white tracking-tighter">{{ profile.fullName }}</h3>
                        <div class="flex items-center gap-3 mt-2">
                            <span class="bg-white/20 backdrop-blur-md px-4 py-1.5 rounded-full text-[10px] font-black text-white uppercase tracking-widest border border-white/20">{{ profile.roleName }}</span>
                            <span class="text-white/60 text-xs font-bold uppercase tracking-widest">@{{ profile.username }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Profile Body -->
            <div class="p-16 pt-20 grid grid-cols-1 md:grid-cols-2 gap-12">
                <div class="space-y-10">
                    <h5 class="text-xs font-black text-slate-400 uppercase tracking-[0.2em] flex items-center gap-3 italic">
                        <div class="w-1.5 h-6 bg-indigo-600 rounded-full"></div>
                        Thông tin định danh
                    </h5>
                    <div class="grid grid-cols-1 gap-6">
                        <div class="bg-slate-50/50 p-8 rounded-[2.5rem] border border-slate-100 flex items-center space-x-6 hover:bg-white hover:shadow-xl transition-all">
                            <div class="bg-indigo-50 p-4 rounded-2xl text-indigo-600">
                                <Mail class="w-6 h-6" />
                            </div>
                            <div>
                                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Email Công vụ</p>
                                <p class="font-black text-slate-900 text-lg">{{ profile.username }}@healthcare.vn</p>
                            </div>
                        </div>
                        <div class="bg-slate-50/50 p-8 rounded-[2.5rem] border border-slate-100 flex items-center space-x-6 hover:bg-white hover:shadow-xl transition-all">
                            <div class="bg-cyan-50 p-4 rounded-2xl text-cyan-600">
                                <Building2 class="w-6 h-6" />
                            </div>
                            <div>
                                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Đơn vị công tác</p>
                                <p class="font-black text-slate-900 text-lg">{{ profile.companyName || 'HealthCare HQ (Trung ương)' }}</p>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="space-y-10">
                    <h5 class="text-xs font-black text-slate-400 uppercase tracking-[0.2em] flex items-center gap-3 italic">
                        <div class="w-1.5 h-6 bg-emerald-500 rounded-full"></div>
                        Nhật ký hoạt động
                    </h5>
                    <div class="space-y-4">
                        <div v-for="i in 3" :key="i" class="flex items-center space-x-6 p-6 rounded-3xl bg-white border border-slate-50 hover:bg-slate-50 transition-all cursor-default group/item">
                            <div class="w-3 h-3 rounded-full bg-emerald-500/20 flex items-center justify-center">
                                <div class="w-1 h-1 rounded-full bg-emerald-500"></div>
                            </div>
                            <div>
                                <p class="text-sm font-black text-slate-700">Truy cập trạm điều phối {{ i === 1 ? 'Hôm nay' : i === 2 ? 'Hôm qua' : '2 ngày trước' }}</p>
                                <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mt-1">LÚC 09:15 AM • IP: 192.168.1.10{{ i }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- MODAL: CREATE / EDIT USER -->
    <div v-if="modal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4 overflow-y-auto">
        <div class="bg-white w-full max-w-xl p-10 rounded-[3rem] shadow-2xl relative animate-fade-in-up mt-auto mb-auto">
            <button @click="modal.show = false" class="absolute top-8 right-8 text-slate-300 hover:text-slate-800 transition-all">
                <X class="w-8 h-8" />
            </button>
            
            <h3 class="text-2xl font-black text-slate-800 flex items-center mb-8">
                <div class="w-10 h-10 bg-indigo-600 text-white rounded-2xl flex items-center justify-center mr-4 shadow-lg shadow-indigo-200">
                    <UserPlus v-if="!modal.isEdit" class="w-6 h-6" />
                    <Edit3 v-else class="w-6 h-6" />
                </div>
                {{ modal.isEdit ? 'Cập nhật tài khoản' : 'Cấp tài khoản mới' }}
            </h3>

            <form @submit.prevent="handleSubmit" class="space-y-6">
                <!-- Avatar Upload -->
                <div class="flex flex-col items-center space-y-4 mb-6">
                    <div class="w-24 h-24 rounded-3xl bg-slate-50 border-2 border-dashed border-slate-200 flex items-center justify-center overflow-hidden relative group">
                        <img v-if="avatarPreview" :src="avatarPreview" class="w-full h-full object-cover" />
                        <Camera v-else class="w-8 h-8 text-slate-300" />
                        <label class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center cursor-pointer transition-opacity">
                            <span class="text-white text-[10px] font-black uppercase tracking-widest">Thay ảnh</span>
                            <input type="file" @change="onFileChange" class="hidden" accept="image/*" />
                        </label>
                    </div>
                </div>

                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Tên định danh đăng nhập</label>
                    <input v-model="form.username" :disabled="modal.isEdit" required class="input-premium" placeholder="VD: bacsi_binh" />
                </div>
                
                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Họ và Tên chủ tài khoản</label>
                    <input v-model="form.fullName" required class="input-premium" placeholder="Nhập họ và tên đầy đủ..." />
                </div>

                <div class="grid grid-cols-2 gap-6">
                    <div class="space-y-3">
                        <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Đặc quyền Vai trò</label>
                        <select v-model="form.roleId" class="input-premium">
                            <option :value="1">Admin (Quản trị tối cao)</option>
                            <option :value="2">Quản lý Nhân sự</option>
                            <option :value="3">Quản lý Công ty/Hợp đồng</option>
                            <option :value="4">Kế toán (Quản lý lương)</option>
                            <option :value="5">Quản lý Vận hành Đoàn</option>
                            <option :value="6">Quản lý Vật tư/Kho</option>
                            <option :value="7">Nhân viên Chuyên môn (Đi khám)</option>
                            <option :value="8">Khách hàng (Xem kết quả)</option>
                        </select>
                    </div>
                </div>

                <div v-show="form.roleId === 8" class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Đơn vị quản lý trực tiếp</label>
                    <select v-model="form.companyId" class="input-premium">
                        <option :value="null">-- Hệ điều hành HealthCare --</option>
                        <option v-for="c in companies" :key="c.companyId" :value="c.companyId">{{ c.companyName }}</option>
                    </select>
                </div>

                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">
                        {{ modal.isEdit ? 'Thay đổi mật khẩu hệ thống (Bỏ trống nếu không đổi)' : 'Mật khẩu bảo mật' }}
                    </label>
                    <input v-model="form.password" :required="!modal.isEdit" type="password" class="input-premium" />
                </div>

                <button type="submit" class="w-full btn-premium bg-indigo-600 text-white shadow-indigo-600/20 py-6">
                    {{ modal.isEdit ? 'LƯU THÔNG TIN TÀI KHOẢN' : 'KHOÁI TẠO TÀI KHOẢN TRUY CẬP' }}
                </button>
            </form>
        </div>
    </div>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog 
      v-model="showDeleteConfirm"
      title="Xóa tài khoản?"
      :message="`Bạn có chắc muốn xóa tài khoản &quot;${userToDelete}&quot;? Hành động này không thể hoàn tác.`"
      confirmText="Xóa ngay"
      variant="danger"
      @confirm="deleteUser"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { 
    UserPlus, Edit3, Trash2, Shield, X, MapPin, Mail, Calendar, Camera, ShieldCheck, AtSign, Building2
} from 'lucide-vue-next'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const toast = useToast()
const isAdmin = computed(() => authStore.role === 'Admin')
const profile = computed(() => authStore.profile)

const users = ref([])
const companies = ref([])
const showDeleteConfirm = ref(false)
const userToDelete = ref(null)
const modal = ref({ show: false, isEdit: false })
const form = ref({
    username: '',
    password: '',
    fullName: '',
    roleId: 2,
    companyId: null,
    avatarPath: ''
})

const selectedFile = ref(null)
const avatarPreview = ref(null)

const onFileChange = (e) => {
    const file = e.target.files[0]
    if (!file) return
    selectedFile.value = file
    avatarPreview.value = URL.createObjectURL(file)
}

const fetchUsers = async () => {
    if (!isAdmin.value) return
    try {
        const res = await axios.get('http://localhost:5283/api/Auth/users')
        users.value = res.data
    } catch (e) {
        console.error("Lỗi lấy danh sách user:", e)
    }
}

const fetchCompanies = async () => {
    if (!isAdmin.value) return
    try {
        const res = await axios.get('http://localhost:5283/api/Companies')
        companies.value = res.data
    } catch (e) {
        console.error("Lỗi lấy danh sách công ty:", e)
    }
}

const openCreateModal = () => {
    modal.value = { show: true, isEdit: false }
    form.value = { username: '', password: '', fullName: '', roleId: 2, companyId: null, avatarPath: '' }
    selectedFile.value = null
    avatarPreview.value = null
}

const openEditModal = (u) => {
    modal.value = { show: true, isEdit: true }
    form.value = { 
        username: u.username, 
        password: '', 
        fullName: u.fullName || '', 
        roleId: u.roleId, 
        companyId: u.companyId,
        avatarPath: u.avatarPath || ''
    }
    selectedFile.value = null
    avatarPreview.value = u.avatarPath ? `http://localhost:5283/${u.avatarPath}` : null
}

const handleSubmit = async () => {
    try {
        // Upload avatar first if exists
        if (selectedFile.value) {
            const formData = new FormData()
            formData.append('file', selectedFile.value)
            const uploadRes = await axios.post('http://localhost:5283/api/Auth/upload-avatar', formData)
            form.value.avatarPath = uploadRes.data.path
        }

        if (modal.value.isEdit) {
            await axios.put(`http://localhost:5283/api/Auth/users/${form.value.username}`, form.value)
            toast.success("Cập nhật thành công!")
        } else {
            await axios.post('http://localhost:5283/api/Auth/register', form.value)
            toast.success("Tạo tài khoản thành công!")
        }
        modal.value.show = false
        fetchUsers()
    } catch (e) {
        console.error(e)
        toast.error(e.response?.data || "Thao tác thất bại. Vui lòng thử lại.")
    }
}

const handleDelete = (username) => {
    userToDelete.value = username
    showDeleteConfirm.value = true
}

const deleteUser = async () => {
    try {
        await axios.delete(`http://localhost:5283/api/Auth/users/${userToDelete.value}`)
        toast.success("Đã xóa tài khoản!")
        fetchUsers()
    } catch (e) {
        toast.error("Không thể xóa tài khoản này.")
    }
}

onMounted(async () => {
    if (isAdmin.value) {
        await fetchUsers()
        await fetchCompanies()
    } else {
        await authStore.fetchProfile()
    }
})
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.8s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
