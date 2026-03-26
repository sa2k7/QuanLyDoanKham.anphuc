<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-rose-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <ShieldCheck class="w-6 h-6" />
          </div>
          {{ isAdmin ? 'Quản trị Hệ thống' : 'Thông tin Tài khoản' }}
          <span v-if="isAdmin" class="text-slate-200 ml-2 font-black">/</span>
          <span v-if="isAdmin" class="text-rose-600 font-black tabular-nums">{{ String(users.length).padStart(3, '0') }}</span>
        </h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.3em] text-[10px] mt-2">{{ isAdmin ? 'Phân quyền & Kiểm soát truy cập tập trung' : 'Thông tin cá nhân & Nhật ký hoạt động' }}</p>
      </div>
      <button v-if="isAdmin" @click="openCreateModal" 
              class="btn-premium bg-slate-900 text-white px-8 py-3 shadow-lg">
        <UserPlus class="w-5 h-5" />
        <span class="">CẤP TÀI KHOẢN MỚI</span>
      </button>
    </div>

    <!-- Admin Stats Section -->
    <div v-if="isAdmin" class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
        <StatCard 
            title="Định danh hệ thống"
            :value="String(users.length).padStart(3, '0')"
            :icon="AtSign"
            variant="slate"
            subtext="Tổng tài khoản"
        />
        <StatCard 
            title="Quản trị viên root"
            :value="String(users.filter(u => u.roleName === 'Admin').length).padStart(3, '0')"
            :icon="ShieldCheck"
            variant="rose"
            subtext="Quyền tối cao"
        />
        <StatCard 
            title="Nhân sự y tế"
            :value="String(users.filter(u => u.roleName === 'MedicalStaff').length).padStart(3, '0')"
            :icon="Stethoscope"
            variant="indigo"
            subtext="Đi đoàn khám"
        />
    </div>

    <!-- ADMIN VIEW: USER LIST -->
    <div v-if="isAdmin" class="premium-card bg-white rounded-[2rem] shadow-[4px_4px_0px_#0f172a] border-2 border-slate-900 overflow-hidden mb-20">
        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <tr>
                        <th class="p-4 text-center w-16">STT</th>
                        <th class="p-4">Tài khoản</th>
                        <th class="p-4">Vai trò</th>
                        <th class="p-4">Mật khẩu</th>
                        <th class="p-4 text-center">Thao tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="(u, index) in users" :key="u.username" class="text-xs hover:bg-slate-50/50 transition-all">
                        <td class="p-4 text-center font-black text-slate-400 tabular-nums">
                            {{ String(index + 1).padStart(3, '0') }}
                        </td>
                        <td class="p-4">
                            <div class="flex items-center gap-4">
                                <div class="w-10 h-10 rounded-xl bg-slate-50 border border-slate-100 overflow-hidden flex-shrink-0 relative">
                                    <div v-if="u.username === 'admin'" class="absolute -top-1 -right-1 w-2.5 h-2.5 bg-emerald-500 rounded-full border-2 border-white animate-pulse z-10"></div>
                                    <img v-if="u.avatarPath" :src="`http://localhost:5283/${u.avatarPath}`" class="w-full h-full object-cover" />
                                    <img v-else :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${u.username}`" class="w-full h-full object-cover" />
                                </div>
                                <div>
                                    <h4 class="font-black text-slate-800 uppercase tracking-widest group-hover:text-indigo-600 transition-colors">{{ u.fullName || 'Chưa đặt tên' }}</h4>
                                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-[0.3em] mt-1 flex items-center gap-1">
                                        <AtSign class="w-3 h-3 text-indigo-400" /> {{ u.username }}
                                    </p>
                                    <p v-if="u.email" class="text-[9px] font-black text-emerald-500 uppercase tracking-[0.3em] flex items-center gap-1">
                                        <Mail class="w-3 h-3" /> {{ u.email }}
                                    </p>
                                </div>
                            </div>
                        </td>
                        <td class="p-4">
                            <span class="inline-flex items-center px-3 py-1 rounded-lg bg-indigo-50 text-indigo-600 text-[10px] font-black uppercase tracking-[0.3em]">
                                {{ i18n.t('roles.' + u.roleName) }}
                            </span>
                        </td>
                        <td class="p-4">
                            <!-- Hiển thị mật khẩu reset nếu có -->
                            <div v-if="getTempPass(u.username)" class="inline-flex items-center gap-3 px-3 py-1 bg-emerald-50 rounded-lg border border-emerald-100 animate-pulse mb-1">
                                <span class="text-[10px] font-black text-slate-800">{{ getTempPass(u.username) }}</span>
                                <span class="text-[8px] font-black text-emerald-500 uppercase tracking-widest">Reset</span>
                            </div>
                            <!-- Mật khẩu vừa cập nhật trong phiên (Hiển thị trong 10 phút) -->
                            <div v-else-if="sessionPasswords[u.username] && (Date.now() - (sessionPasswords[u.username].timestamp || 0) < 600000)" class="inline-flex items-center gap-3 px-3 py-1 bg-indigo-50 rounded-lg border border-indigo-100 mb-1">
                                <span class="text-[10px] font-black text-indigo-600 font-mono tracking-tighter">{{ sessionPasswords[u.username].password || sessionPasswords[u.username] }}</span>
                                <span class="text-[8px] font-black text-indigo-400 uppercase tracking-widest leading-none">MỚI</span>
                            </div>
                            <!-- Hiển thị mật khẩu mặc định dự kiến cho nhân sự mới -->
                            <div v-else class="flex flex-col gap-1">
                                <span class="text-[10px] font-black text-slate-400 font-mono tracking-tighter">
                                    {{ u.username === 'admin' ? '••••••••' : (u.username.startsWith('nv') ? 'Password@123' : 'HealthCare2026') }}
                                </span>
                                <span class="text-[8px] font-black text-slate-300 uppercase tracking-widest leading-none">Mặc định</span>
                            </div>
                        </td>
                        <td class="p-4 text-center">
                            <div class="flex items-center justify-center gap-3">
                                <button @click="openEditModal(u)" class="btn-action-premium variant-indigo text-slate-400" title="Hiệu chỉnh">
                                    <Edit3 class="w-5 h-5" />
                                </button>
                                <button v-if="u.username !== 'admin'" @click="handleDelete(u.username)" class="btn-action-premium variant-rose text-slate-400" title="Xóa tài khoản">
                                    <Trash2 class="w-5 h-5" />
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr v-if="users.length === 0">
                        <td colspan="5" class="py-20 text-center">
                            <div class="flex flex-col items-center justify-center gap-4">
                                <ShieldCheck class="w-10 h-10 text-slate-200" />
                                <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không có dữ liệu tài khoản</p>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
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
                        <h3 class="text-4xl font-black text-white ">{{ profile.fullName }}</h3>
                        <div class="flex items-center gap-3 mt-2">
                            <span class="bg-white/20 backdrop-blur-md px-4 py-1.5 rounded-full text-[10px] font-black text-white uppercase tracking-widest border border-white/20">{{ profile.roleName }}</span>
                            <span class="text-white/60 text-xs font-black uppercase tracking-widest ">@{{ profile.username }}</span>
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
                                <p class="font-black text-slate-900 text-lg">{{ profile.email || profile.username + '@healthcare.vn' }}</p>
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
                                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">LÚC 09:15 AM • IP: 192.168.1.10{{ i }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- MODAL: CREATE / EDIT USER -->
    <Teleport to="body">
      <div v-if="modal.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-md p-4 overflow-y-auto">
          <div class="bg-white w-full max-w-xl rounded-[3rem] border-2 border-slate-900 shadow-2xl animate-fade-in-up relative overflow-hidden mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Accent Line -->
              <div class="absolute top-0 left-0 right-0 h-4 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>
              
              <button @click="modal.show = false" class="absolute top-8 right-8 bg-white p-2 rounded-full hover:bg-slate-100 transition-all text-slate-400 z-[60] flex items-center justify-center border-2 border-slate-900 shadow-[2px_2px_0px_#0f172a]">
                  <X class="w-5 h-5" />
              </button>
              
              <div class="relative z-10 pt-12">
                  <div class="p-10 pb-6">
                      <div class="flex items-center gap-4 mb-8">
                          <div class="w-14 h-14 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center shadow-inner border border-indigo-100">
                              <UserPlus v-if="!modal.isEdit" class="w-7 h-7" />
                              <Edit3 v-else class="w-7 h-7" />
                          </div>
                          <div>
                              <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest">{{ modal.isEdit ? 'Cập nhật tài khoản' : 'Cấp tài khoản mới' }}</h3>
                              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-1">Hệ thống phân quyền & Bảo mật</p>
                          </div>
                      </div>

              <form id="userForm" @submit.prevent="handleSubmit" class="space-y-6">
                  <!-- Avatar Upload -->
                  <div class="flex flex-col items-center space-y-4 mb-6">
                      <div class="w-24 h-24 rounded-3xl bg-slate-50 border-2 border-dashed border-slate-200 flex items-center justify-center overflow-hidden relative group cursor-pointer hover:border-indigo-400 transition-colors">
                          <img v-if="avatarPreview" :src="avatarPreview" class="w-full h-full object-cover" />
                          <Camera v-else class="w-8 h-8 text-slate-300 group-hover:text-indigo-400 transition-colors" />
                          <label class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center cursor-pointer transition-opacity">
                              <span class="text-white text-[10px] font-black uppercase tracking-widest ">Thay ảnh</span>
                              <input type="file" @change="onFileChange" class="hidden" accept="image/*" />
                          </label>
                      </div>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-5">
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Định danh đăng nhập</label>
                          <input v-model="form.username" :disabled="modal.isEdit" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="VD: bacsi_binh" />
                      </div>
                      
                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Họ và Tên chủ khoản</label>
                          <input v-model="form.fullName" required class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="Nhập họ tên đầy đủ..."
                                 @input="generateUsername(form.fullName)" />
                      </div>

                      <div class="flex flex-col gap-2 md:col-span-2">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Địa chỉ Email (Gmail)</label>
                          <input v-model="form.email" type="email" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full" placeholder="vidu@gmail.com..." />
                      </div>

                      <div class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Đặc quyền Vai trò</label>
                          <select v-model="form.roleId" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full">
                              <option :value="1">{{ i18n.t('roles.Admin') }}</option>
                              <option :value="2">{{ i18n.t('roles.PersonnelManager') }}</option>
                              <option :value="3">{{ i18n.t('roles.ContractManager') }}</option>
                              <option :value="4">{{ i18n.t('roles.PayrollManager') }}</option>
                              <option :value="5">{{ i18n.t('roles.MedicalGroupManager') }}</option>
                              <option :value="6">{{ i18n.t('roles.WarehouseManager') }}</option>
                              <option :value="7">{{ i18n.t('roles.MedicalStaff') }}</option>
                              <option :value="8">{{ i18n.t('roles.Customer') }}</option>
                          </select>
                      </div>

                      <div class="flex flex-col gap-2" :class="{ 'opacity-50 pointer-events-none': form.roleId !== 8 }">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Đơn vị quản lý trực tiếp</label>
                          <select v-model="form.companyId" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full">
                              <option :value="null">-- Hệ điều hành HealthCare --</option>
                              <option v-for="c in companies" :key="c.companyId" :value="c.companyId">{{ c.companyName }}</option>
                          </select>
                      </div>

                      <div class="flex flex-col gap-2 md:col-span-2">
                          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">
                              {{ modal.isEdit ? 'Đổi mật khẩu (Bỏ trống nếu không đổi)' : 'Mật khẩu bảo mật' }}
                          </label>
                          <div class="relative">
                              <input v-model="form.password" 
                                     @input="form.password = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
                                     :required="!modal.isEdit" type="text" class="input-premium bg-slate-50 border-slate-200 focus:bg-white w-full font-mono" />
                              <span v-if="!modal.isEdit" class="absolute right-4 top-1/2 -translate-y-1/2 text-[9px] font-black text-emerald-500 uppercase tracking-widest">Mặc định</span>
                          </div>
                      </div>
                  </div>

                  </form>
                  </div>

                  <div class="p-10 pt-0 bg-white border-t border-slate-50 relative z-20">
                      <button form="userForm" type="submit" class="w-full btn-premium bg-indigo-600 text-white shadow-indigo-600/20 py-6">
                          {{ modal.isEdit ? 'LƯU THÔNG TIN TÀI KHOẢN' : 'KHOÁI TẠO TÀI KHOẢN TRUY CẬP' }}
                      </button>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

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
    UserPlus, Edit3, Trash2, Shield, X, MapPin, Mail, Calendar, Camera, ShieldCheck, AtSign, Building2, Stethoscope
} from 'lucide-vue-next'
import { useToast } from '../composables/useToast'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import StatCard from '../components/StatCard.vue'
import { useAuthStore } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'

const authStore = useAuthStore()
const i18n = useI18nStore()
const toast = useToast()
const isAdmin = computed(() => authStore.role === 'Admin')
const profile = computed(() => authStore.profile)

const users = ref([])
const companies = ref([])
const resetRequests = ref([])
const showDeleteConfirm = ref(false)
const userToDelete = ref(null)
const modal = ref({ show: false, isEdit: false })
const form = ref({
    username: '',
    password: '',
    fullName: '',
    email: '',
    roleId: 2,
    companyId: null,
    avatarPath: ''
})

const getTempPass = (username) => {
    const req = resetRequests.value.find(r => r.username === username && r.isProcessed)
    return req ? req.newPassword : null
}

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

const fetchResets = async () => {
    if (!isAdmin.value) return
    try {
        const res = await axios.get('http://localhost:5283/api/Auth/reset-requests')
        // Lấy cả những yêu cầu đã xử lý để xem mật khẩu mới
        resetRequests.value = res.data 
    } catch (e) {}
}

const DEFAULT_PASSWORD = 'HealthCare2026'
const newlyCreatedUser = ref(null) // Track newly created user for password display
// Khởi tạo từ localStorage để không bị mất khi F5
const sessionPasswords = ref(JSON.parse(localStorage.getItem('user_session_passwords') || '{}')) 

// Remove Vietnamese diacritics (e.g. 'Hoàng' -> 'Hoang')
const removeDiacritics = (str) => str
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd').replace(/Đ/g, 'D')

// Generate username: last word (first name) + initials of preceding words + 4-digit counter
// E.g. "Nguyễn Huy Hoàng" → "hoangnh<count>"
const generateUsername = async (fullName) => {
    if (!fullName || modal.value.isEdit) return
    const parts = fullName.trim().split(/\s+/).filter(Boolean)
    if (parts.length === 0) return
    const firstName = parts[parts.length - 1]  // 'Hoàng'
    const initials = parts.slice(0, -1).map(p => p[0]).join('') // 'NH'
    const base = removeDiacritics(firstName + initials).toLowerCase().replace(/[^a-z]/g, '')
    if (!base) return
    // Count existing users with same base to generate 4-digit suffix
    const count = users.value.filter(u => u.username.startsWith(base)).length
    const suffix = String(count + 1).padStart(4, '0')
    form.value.username = base + suffix
}

const openCreateModal = () => {
    modal.value = { show: true, isEdit: false }
    form.value = { username: '', password: DEFAULT_PASSWORD, fullName: '', email: '', roleId: 2, companyId: null, avatarPath: '' }
    selectedFile.value = null
    avatarPreview.value = null
    newlyCreatedUser.value = null
}

const openEditModal = (u) => {
    modal.value = { show: true, isEdit: true }
    form.value = { 
        username: u.username, 
        password: '', 
        fullName: u.fullName || '', 
        email: u.email || '',
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
            // Lưu vào cache & localStorage để hiển thị lại trên bảng (vì API không trả về Pass)
            if (form.value.password) {
                sessionPasswords.value[form.value.username] = { 
                    password: form.value.password, 
                    timestamp: Date.now() 
                }
                localStorage.setItem('user_session_passwords', JSON.stringify(sessionPasswords.value))
            }
            toast.success("Cập nhật thành công!")
            modal.value.show = false
        } else {
            await axios.post('http://localhost:5283/api/Auth/register', form.value)
            // Lưu vào cache & localStorage
            sessionPasswords.value[form.value.username] = { 
                password: form.value.password || DEFAULT_PASSWORD, 
                timestamp: Date.now() 
            }
            localStorage.setItem('user_session_passwords', JSON.stringify(sessionPasswords.value))
            // Store newly created user info to display default password
            newlyCreatedUser.value = {
                username: form.value.username,
                password: form.value.password || DEFAULT_PASSWORD
            }
            toast.success(`✅ Đã tạo tài khoản @${form.value.username} — Mật khẩu: ${newlyCreatedUser.value.password}`)
            modal.value.show = false
        }
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
        await fetchResets()
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
