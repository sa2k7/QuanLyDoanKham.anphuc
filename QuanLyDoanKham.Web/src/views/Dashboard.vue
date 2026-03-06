<template>
  <div class="min-h-screen flex bg-slate-50 font-sans overflow-hidden">
    <!-- Sidebar Navigation -->
    <aside class="w-72 bg-white border-r border-slate-200 flex flex-col h-screen sticky top-0 z-[60] shadow-sm">
      <!-- Logo Section -->
      <div class="p-8 pb-10 flex items-center gap-3 cursor-pointer group" @click="activeMenu = 'home'">
        <div class="bg-primary p-2.5 rounded-2xl transition-all group-hover:rotate-6 shadow-lg shadow-primary/20">
          <Stethoscope class="text-white w-7 h-7" />
        </div>
        <div>
          <h1 class="font-black text-xl text-slate-900 tracking-tighter leading-none">CÔNG TY <span class="text-primary italic">KHÁM SỨC KHỎE</span></h1>
          <p class="text-[8px] font-black text-slate-400 uppercase tracking-[0.2em] mt-1">Hệ thống Điều hành Nội bộ</p>
        </div>
      </div>

      <!-- Menu Items -->
      <nav class="flex-1 px-4 space-y-2 overflow-y-auto custom-scrollbar">
        <button v-for="item in filteredMenuItems" :key="item.id"
                @click="activeMenu = item.id"
                :class="['w-full flex items-center gap-4 px-5 py-4 rounded-2xl font-black text-sm transition-all group relative overflow-hidden', 
                         activeMenu === item.id ? 'bg-primary text-white shadow-xl shadow-primary/20' : 'text-slate-400 hover:bg-slate-50 hover:text-slate-600']">
          <component :is="item.icon" :class="['w-5 h-5 transition-transform group-hover:scale-110', activeMenu === item.id ? 'text-white' : 'text-slate-300 group-hover:text-primary']" />
          <span class="tracking-tight uppercase">{{ item.name }}</span>
          
          <div v-if="activeMenu === item.id" class="absolute right-0 top-0 bottom-0 w-1.5 bg-white/20"></div>
          <ArrowRight v-if="activeMenu !== item.id" class="w-4 h-4 ml-auto opacity-0 group-hover:opacity-100 transition-all translate-x-2 group-hover:translate-x-0" />
        </button>
      </nav>

      <!-- User Profile Card -->
      <div class="p-6 border-t border-slate-100 bg-slate-50/50">
        <div class="bg-white p-4 rounded-2xl shadow-sm border border-slate-100 group relative">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
              <User class="w-5 h-5" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-black text-slate-800 truncate">{{ authStore.user?.username }}</p>
              <p class="text-[9px] font-black text-primary uppercase tracking-widest">{{ getRoleDisplayName(authStore.user?.role) }}</p>
            </div>
            <button @click="isUserMenuOpen = !isUserMenuOpen" class="p-2 hover:bg-slate-50 rounded-lg transition-all">
              <ChevronDown class="w-4 h-4 text-slate-400" :class="{'rotate-180': isUserMenuOpen}" />
            </button>
          </div>

          <!-- Mini Dropdown -->
          <div v-if="isUserMenuOpen" class="absolute bottom-full left-0 right-0 mb-2 bg-white rounded-2xl shadow-2xl border border-slate-100 p-2 animate-slide-up z-[70]">
            <button @click="showPasswordModal = true; isUserMenuOpen = false" class="w-full flex items-center gap-3 px-4 py-3 rounded-xl hover:bg-slate-50 text-xs font-black text-slate-600 transition-all">
              <KeyRound class="w-4 h-4" /> ĐỔI MẬT KHẨU
            </button>
            <button @click="handleLogout" class="w-full flex items-center gap-3 px-4 py-3 rounded-xl hover:bg-rose-50 text-xs font-black text-rose-500 transition-all">
              <LogOut class="w-4 h-4" /> ĐĂNG XUẤT
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 h-screen overflow-y-auto bg-slate-50 relative custom-scrollbar">
      <!-- Top Header / Global Search -->
      <header class="h-24 bg-white/80 backdrop-blur-xl border-b border-slate-200 flex items-center px-10 sticky top-0 z-50">
        <div class="flex-1 flex items-center gap-8">
          <div class="flex items-center gap-2">
            <h2 class="text-xl font-black text-slate-800 tracking-tight uppercase">{{ activeMenuName }}</h2>
            <div class="w-1 h-1 rounded-full bg-slate-300"></div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">{{ itemsBreadcrumb }}</p>
          </div>

          <!-- Global Search -->
          <div class="relative w-full max-w-xl group">
            <Search class="absolute left-6 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-300 group-focus-within:text-primary transition-colors" />
            <input type="text" 
                   v-model="searchQuery"
                   @input="handleSearch"
                   @focus="isSearchFocused = true"
                   placeholder="Tìm kiếm thông minh (Ctrl+K)..."
                   class="w-full pl-14 pr-20 py-4 bg-slate-100/50 border-2 border-transparent focus:border-primary/20 focus:bg-white rounded-2xl outline-none font-bold text-slate-800 transition-all shadow-inner" />
            <div class="absolute right-6 top-1/2 -translate-y-1/2 flex gap-1">
              <span class="px-2 py-1 bg-white border border-slate-200 rounded-md text-[9px] font-black text-slate-400 shadow-sm">CTRL</span>
              <span class="px-2 py-1 bg-white border border-slate-200 rounded-md text-[9px] font-black text-slate-400 shadow-sm">K</span>
            </div>

            <!-- Enhanced Search Results -->
            <div v-if="searchResults.length > 0 && isSearchFocused" class="absolute top-full left-0 right-0 mt-4 bg-white rounded-[2rem] shadow-2xl border-2 border-slate-50 overflow-hidden z-[100] animate-scale-up">
              <div v-for="res in searchResults" :key="res.id" 
                   @click="navigateSearchResult(res)"
                   class="flex items-center p-5 hover:bg-indigo-50/50 cursor-pointer border-b border-slate-50 last:border-none transition-all group">
                <div :class="['w-12 h-12 rounded-2xl flex items-center justify-center mr-5 shadow-sm group-hover:scale-110 transition-transform duration-500', 
                     res.type === 'company' ? 'bg-sky-50 text-sky-600' : 
                     res.type === 'contract' ? 'bg-teal-50 text-teal-600' :
                     res.type === 'staff' ? 'bg-rose-50 text-rose-600' :
                     res.type === 'group' ? 'bg-primary/10 text-primary' :
                     res.type === 'payroll' ? 'bg-emerald-50 text-emerald-600' :
                     'bg-violet-50 text-violet-600']">
                  <component :is="getSearchIcon(res.type)" class="w-6 h-6" />
                </div>
                <div class="flex-1">
                  <p class="font-black text-slate-800 leading-tight">{{ res.name }}</p>
                  <div class="flex items-center gap-2 mt-0.5">
                    <span class="text-[9px] font-black uppercase text-slate-400 tracking-widest">{{ getResTypeName(res.type) }}</span>
                    <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                    <span class="text-[9px] font-bold text-slate-400">ID: #{{ res.id }}</span>
                  </div>
                </div>
                <div class="w-10 h-10 rounded-full border border-slate-100 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity translate-x-4 group-hover:translate-x-0">
                  <ArrowRight class="w-4 h-4 text-primary" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Action / Notifications -->
        <div class="flex items-center gap-4">
          <button v-if="authStore.role === 'Admin'" 
                  @click="showResetModal = true"
                  class="p-4 bg-slate-50 text-slate-400 rounded-2xl hover:text-primary transition-all relative">
            <Bell class="w-6 h-6" />
            <div v-if="resetRequests.length > 0" class="absolute top-4 right-4 text-[8px] flex items-center justify-center w-5 h-5 bg-rose-500 text-white rounded-full border-2 border-white font-black">
              {{ resetRequests.length }}
            </div>
          </button>
          <button v-else class="p-4 bg-slate-50 text-slate-400 rounded-2xl hover:text-primary transition-all relative">
            <Bell class="w-6 h-6" />
          </button>

          <div class="w-px h-10 bg-slate-100"></div>
          <button @click="activeMenu = 'reports'" class="flex items-center gap-3 px-6 py-3 bg-slate-900 text-white rounded-2xl font-black text-sm uppercase tracking-widest shadow-lg shadow-slate-200 active:scale-95 transition-all">
             <BarChart3 class="w-5 h-5 text-indigo-400" />
             Thống kê
          </button>
        </div>
      </header>

      <!-- Password Reset Requests Modal (UI Update) -->
      <div v-if="showResetModal" class="fixed inset-0 z-[1000] flex items-center justify-center p-6 bg-slate-900/40 backdrop-blur-md animate-fade-in">
          <div class="bg-white w-full max-w-lg rounded-[2.5rem] shadow-2xl p-10 border-2 border-indigo-50 animate-scale-up">
              <div class="flex justify-between items-center mb-8">
                  <div class="flex items-center gap-4">
                      <div class="w-12 h-12 bg-rose-50 text-rose-500 rounded-2xl flex items-center justify-center">
                          <ShieldAlert class="w-6 h-6" />
                      </div>
                      <div>
                        <h3 class="text-xl font-black text-slate-800">Yêu cầu cấp lại mật khẩu</h3>
                        <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">{{ resetRequests.length }} yêu cầu chưa xử lý</p>
                      </div>
                  </div>
                  <button @click="showResetModal = false" class="p-2 hover:bg-slate-50 rounded-xl transition-all"><X class="w-6 h-6 text-slate-300" /></button>
              </div>

              <div class="space-y-4 max-h-80 overflow-y-auto custom-scrollbar pr-2">
                  <div v-for="req in resetRequests" :key="req.id" class="p-6 bg-slate-50 rounded-2xl border-2 border-transparent hover:border-indigo-100 transition-all flex justify-between items-center group">
                      <div>
                          <p class="font-black text-slate-800">@{{ req.username }}</p>
                          <p class="text-[9px] font-bold text-slate-400 uppercase mt-1">{{ new Date(req.requestedDate).toLocaleString('vi-VN') }}</p>
                      </div>
                      <button @click="handleProcessReset(req.id)" class="px-5 py-2.5 bg-slate-900 text-white rounded-xl text-[10px] font-black uppercase tracking-widest shadow-lg shadow-slate-200 hover:bg-indigo-600 transition-all">
                        Cấp lại MK
                      </button>
                  </div>

                  <div v-if="resetRequests.length === 0" class="py-20 text-center">
                      <div class="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-4">
                          <CheckCircle2 class="w-8 h-8 text-slate-200" />
                      </div>
                      <p class="text-xs font-black uppercase tracking-widest text-slate-300">Không có yêu cầu chờ xử lý</p>
                  </div>
              </div>

              <div class="mt-8 pt-8 border-t border-slate-100">
                  <p class="text-[9px] font-bold text-slate-400 text-center uppercase tracking-widest">Mật khẩu mặc định sau khi cấp lại: <span class="text-indigo-600 font-black">HealthCare2026</span></p>
              </div>
          </div>
      </div>

      <!-- View Container -->
      <div class="p-10">
          <div v-if="activeMenu === 'home'" class="animate-fade-in">
              <!-- Advanced Dashboard Home -->
              <div class="mb-12">
                  <h1 class="text-4xl font-black text-slate-800 tracking-tighter mb-2">
                    Chào ngày mới, <span class="text-primary italic">{{ authStore.user?.username }}!</span>
                  </h1>
                  <p class="text-slate-400 font-bold">Hôm nay là {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</p>
              </div>

              <!-- Top Stats View -->
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
                  <div v-for="mod in activeModules" :key="mod.id" 
                       @click="activeMenu = mod.id"
                       class="premium-card p-8 group cursor-pointer relative overflow-hidden">
                      <div class="absolute -right-4 -bottom-4 w-24 h-24 rotate-12 opacity-[0.03] group-hover:opacity-10 transition-all group-hover:scale-125">
                          <component :is="mod.icon" class="w-full h-full" />
                      </div>
                      <div :class="['w-14 h-14 rounded-2xl flex items-center justify-center mb-6 shadow-inner transition-transform group-hover:translate-x-2 duration-500', mod.color]">
                          <component :is="mod.icon" class="w-7 h-7" />
                      </div>
                      <h3 class="text-lg font-black text-slate-800 mb-1 tracking-tight">{{ mod.name }}</h3>
                      <p class="text-xs font-bold text-slate-400 leading-relaxed">{{ mod.desc }}</p>
                  </div>
              </div>

              <!-- Placeholder for Welcome Message -->
              <div class="bg-indigo-600 rounded-[3rem] p-16 text-white relative overflow-hidden shadow-2xl shadow-indigo-200">
                  <div class="absolute right-0 top-0 bottom-0 w-1/3 bg-gradient-to-l from-white/10 to-transparent"></div>
                  <div class="relative z-10 max-w-2xl">
                    <div class="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-full text-[10px] font-black uppercase tracking-[0.2em] mb-8">
                       <Sparkles class="w-3 h-3" /> Digital Healthcare Platform
                    </div>
                    <h2 class="text-5xl font-black tracking-tighter mb-6 leading-none">Vận hành đoàn khám<br/>với tiêu chuẩn số hóa 100%</h2>
                    <p class="text-indigo-100 text-lg font-medium leading-relaxed mb-10">Tối ưu quy trình điều động nhân sự, quản lý vật tư và kiểm soát dòng tiền chuyên nghiệp hàng đầu.</p>
                    <div class="flex gap-4">
                        <button @click="activeMenu = 'groups'" class="px-10 py-5 bg-white text-indigo-600 rounded-2xl font-black uppercase tracking-widest shadow-xl hover:scale-105 active:scale-95 transition-all">Triển khai đoàn</button>
                        <button @click="activeMenu = 'staff'" class="px-10 py-5 bg-indigo-500 text-white border border-indigo-400 rounded-2xl font-black uppercase tracking-widest hover:bg-indigo-400 transition-all">Đội ngũ y tế</button>
                    </div>
                  </div>
                  <Stethoscope class="absolute -right-20 -bottom-20 w-96 h-96 text-white/5 rotate-12" />
              </div>
          </div>

          <div v-else class="animate-fade-in-up">
              <Companies v-if="activeMenu === 'companies'" />
              <Contracts v-if="activeMenu === 'contracts'" />
              <Staff v-if="activeMenu === 'staff'" />
              <Groups v-if="activeMenu === 'groups'" />
              <Supplies v-if="activeMenu === 'supplies'" />
              <Reports v-if="activeMenu === 'reports'" />
              <AccountManager v-if="activeMenu === 'users'" />
              <Payroll v-if="activeMenu === 'payroll'" />
              
              <div v-if="!['companies', 'contracts', 'staff', 'groups', 'supplies', 'users', 'payroll'].includes(activeMenu)" class="flex flex-col items-center justify-center py-40 bg-white rounded-[3rem] border-4 border-dashed border-slate-50">
                <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mb-6">
                    <component :is="activeIcon" class="w-12 h-12 text-slate-100" />
                </div>
                <p class="text-xl font-black text-slate-300 uppercase tracking-widest">Module {{ activeMenuName }} đang sẵn sàng</p>
                <button @click="activeMenu = 'home'" class="mt-8 px-8 py-4 bg-primary text-white rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-primary/20 active:scale-95 transition-all">Quay lại trang chủ</button>
              </div>
          </div>
      </div>
    </main>

    <!-- Change Password Modal (Synchronized Styled) -->
    <div v-if="showPasswordModal" class="fixed inset-0 z-[1001] flex items-center justify-center p-6 bg-slate-900/60 backdrop-blur-xl animate-fade-in">
        <div class="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl p-10 border-2 border-slate-50 animate-scale-up">
            <div class="flex justify-between items-center mb-10">
                <div class="flex items-center gap-4">
                    <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center">
                        <KeyRound class="w-6 h-6" />
                    </div>
                    <h3 class="text-xl font-black text-slate-800">Cập nhật mật khẩu</h3>
                </div>
                <button @click="showPasswordModal = false" class="text-slate-300 hover:bg-slate-50 p-2 rounded-xl transition-all"><X class="w-6 h-6" /></button>
            </div>
            
            <form @submit.prevent="handleChangePassword" class="space-y-6">
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mật khẩu hiện tại</label>
                    <input v-model="passForm.currentPassword" type="password" required class="input-premium" placeholder="••••••••" />
                </div>
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mật khẩu mới</label>
                    <input v-model="passForm.newPassword" type="password" required class="input-premium" placeholder="Tối thiểu 6 ký tự" />
                </div>
                <div class="space-y-2">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Xác nhận mật khẩu mới</label>
                    <input v-model="passForm.confirmPassword" type="password" required class="input-premium" placeholder="Nhập lại mật khẩu mới" />
                </div>
                
                <div class="pt-6 flex gap-4">
                    <button type="button" @click="showPasswordModal = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest">Hủy bỏ</button>
                    <button type="submit" :disabled="isChangingPass" class="flex-1 bg-primary text-white py-4 rounded-2xl font-black text-xs uppercase tracking-widest shadow-xl shadow-primary/20 active:scale-95 disabled:opacity-50">
                        {{ isChangingPass ? 'Đang gửi...' : 'CẬP NHẬT NGAY' }}
                    </button>
                </div>
            </form>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import axios from 'axios'
import { 
  Stethoscope, Building2, FileText, Users as UsersIcon, Package, BarChart3, 
  LogOut, Search, ArrowRight, ArrowLeft, Sparkles, Bot, Shield, Wallet,
  User, KeyRound, X, ChevronDown, Bell, PlusCircle
} from 'lucide-vue-next'

// Import Modules
import Companies from './Companies.vue'
import Contracts from './Contracts.vue'
import Staff from './Staff.vue'
import Groups from './Groups.vue'
import Supplies from './Supplies.vue'
// import Reports from './Reports.vue'
// import Patients from './Patients.vue'
import AccountManager from './Users.vue'
import Payroll from './Payroll.vue'

const getRoleDisplayName = (role) => {
  const roles = {
    'Admin': 'Administrator',
    'PersonnelManager': 'Personnel Manager',
    'ContractManager': 'Contract Manager',
    'PayrollManager': 'Payroll Manager',
    'MedicalGroupManager': 'Medical Group Manager',
    'WarehouseManager': 'Warehouse Manager',
    'MedicalStaff': 'Medical Staff',
    'Customer': 'Customer / Partner'
  }
  return roles[role] || role
}


const router = useRouter()
const authStore = useAuthStore()
const activeMenu = ref('home')
const isUserMenuOpen = ref(false)
const isSearchFocused = ref(false)

// Password Change State
const showPasswordModal = ref(false)
const isChangingPass = ref(false)
const passForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })

const handleChangePassword = async () => {
  if (passForm.value.newPassword !== passForm.value.confirmPassword) {
    alert("Mật khẩu mới không khớp!")
    return
  }
  
  isChangingPass.value = true
  try {
    const res = await axios.post('http://localhost:5283/api/Auth/change-password', {
      currentPassword: passForm.value.currentPassword,
      newPassword: passForm.value.newPassword
    })
    alert(res.data.message)
    showPasswordModal.value = false
    passForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  } catch (err) {
    alert(err.response?.data || "Lỗi khi đổi mật khẩu")
  } finally {
    isChangingPass.value = false
  }
}

const menuItems = [
  { id: 'home', name: 'Dashboard', icon: Bot, color: 'bg-primary-light text-primary', desc: 'Trung tâm điều khiển và phân tích.' },
  { id: 'companies', name: 'Công ty', icon: Building2, color: 'bg-sky-50 text-sky-600', desc: 'Pháp nhân & Doanh nghiệp khách hàng.', roles: ['Admin', 'ContractManager'] },
  { id: 'contracts', name: 'Hợp đồng', icon: FileText, color: 'bg-teal-50 text-teal-600', desc: 'Quản lý pháp lý & ký kết HĐ.', roles: ['Admin', 'ContractManager', 'Customer'] },
  { id: 'groups', name: 'Đoàn khám', icon: Stethoscope, color: 'bg-primary/10 text-primary', desc: 'Vận hành thực địa & điều phối.', roles: ['Admin', 'MedicalGroupManager', 'MedicalStaff', 'Customer'] },
  { id: 'staff', name: 'Nhân sự', icon: UsersIcon, color: 'bg-rose-50 text-rose-600', desc: 'Đội ngũ Y bác sĩ & Vận hành.', roles: ['Admin', 'PersonnelManager', 'MedicalGroupManager'] },
  { id: 'payroll', name: 'Tính lương', icon: Wallet, color: 'bg-emerald-50 text-emerald-600', desc: 'Kế toán thù lao & lương cứng.', roles: ['Admin', 'PayrollManager'] },
  { id: 'supplies', name: 'Vật tư kho', icon: Package, color: 'bg-violet-50 text-violet-600', desc: 'Kho vật tư & Phiếu xuất kho.', roles: ['Admin', 'WarehouseManager'] },
  { id: 'users', name: 'Tài khoản', icon: Shield, color: 'bg-slate-50 text-slate-600', desc: 'Phân quyền & bảo mật tài khoản.', roles: ['Admin'] },
]

const activeModules = computed(() => {
    return menuItems.filter(i => {
        if (i.id === 'home') return false;
        if (!i.roles) return true;
        return i.roles.includes(authStore.role);
    });
})

const filteredMenuItems = computed(() => {
    return menuItems.filter(i => {
        if (!i.roles) return true;
        return i.roles.includes(authStore.role);
    });
})

const activeMenuName = computed(() => menuItems.find(i => i.id === activeMenu.value)?.name || '')
const itemsBreadcrumb = computed(() => activeMenu.value === 'home' ? 'Hệ thống quản lý' : 'Module quản trị')
const activeIcon = computed(() => menuItems.find(i => i.id === activeMenu.value)?.icon || Bot)

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

// Search Logic
const searchQuery = ref('')
const searchResults = ref([])
const isDataSyncing = ref(false)
const allData = ref({ companies: [], contracts: [], staff: [], groups: [], supplies: [] })

// Password Reset Requests (Admin only)
const resetRequests = ref([])
const showResetModal = ref(false)
const processingResetId = ref(null)
const newPasswordForReset = ref('HealthCare2026')

const fetchResetRequests = async () => {
    if (authStore.role !== 'Admin') return
    try {
        const res = await axios.get('http://localhost:5283/api/Auth/reset-requests')
        resetRequests.value = res.data
    } catch (err) {
        console.warn("Failed to fetch reset requests", err)
    }
}

const handleProcessReset = async (id) => {
    try {
        await axios.post('http://localhost:5283/api/Auth/process-reset', {
            id: id,
            newPassword: newPasswordForReset.value
        })
        alert(`Đã cấp lại mật khẩu mới cho tài khoản. Mật khẩu mặc định là: ${newPasswordForReset.value}`)
        fetchResetRequests()
    } catch (err) {
        alert("Lỗi khi xử lý yêu cầu")
    }
}

const fetchSearchData = async () => {
    if (isDataSyncing.value) return
    isDataSyncing.value = true
    try {
        const allEndpoints = [
            { key: 'companies', url: 'http://localhost:5283/api/Companies', roles: ['Admin', 'ContractManager'] },
            { key: 'contracts', url: 'http://localhost:5283/api/HealthContracts', roles: ['Admin', 'ContractManager', 'Customer'] },
            { key: 'staff', url: 'http://localhost:5283/api/Staffs', roles: ['Admin', 'PersonnelManager', 'MedicalGroupManager'] },
            { key: 'groups', url: 'http://localhost:5283/api/MedicalGroups', roles: ['Admin', 'MedicalGroupManager', 'MedicalStaff', 'Customer'] },
            { key: 'supplies', url: 'http://localhost:5283/api/Supplies', roles: ['Admin', 'WarehouseManager'] }
        ]

        const endpoints = allEndpoints.filter(ep => ep.roles.includes(authStore.role))

        await Promise.all(endpoints.map(async (ep) => {
            try {
                const res = await axios.get(ep.url)
                allData.value[ep.key] = res.data
            } catch (err) {
                console.warn(`Could not sync ${ep.key} for search`, err)
            }
        }))
    } catch (e) {
        console.error("Global search sync failed", e)
    } finally {
        isDataSyncing.value = false
    }
}

const handleSearch = () => {
    if (!searchQuery.value) {
        searchResults.value = []
        return
    }

    const q = searchQuery.value.toLowerCase().trim()
    const matches = []
    const matchesQuery = (text) => text?.toLowerCase().includes(q)

    if (Array.isArray(allData.value.companies)) {
        allData.value.companies.forEach(c => {
            if (matchesQuery(c.companyName)) matches.push({ id: c.companyId, name: c.companyName, type: 'company', target: 'companies' })
        })
    }
    if (Array.isArray(allData.value.contracts)) {
        allData.value.contracts.forEach(c => {
            if (matchesQuery(c.companyName)) matches.push({ id: c.healthContractId, name: `HĐ ${c.companyName}`, type: 'contract', target: 'contracts' })
        })
    }
    if (Array.isArray(allData.value.staff)) {
        allData.value.staff.forEach(s => {
            if (matchesQuery(s.fullName) || matchesQuery(s.jobTitle)) matches.push({ id: s.staffId, name: s.fullName, type: 'staff', target: 'staff' })
        })
    }
    if (Array.isArray(allData.value.groups)) {
        allData.value.groups.forEach(g => {
            if (matchesQuery(g.groupName)) matches.push({ id: g.medicalGroupId, name: g.groupName, type: 'group', target: 'groups' })
        })
    }
    if (Array.isArray(allData.value.supplies)) {
        allData.value.supplies.forEach(s => {
            if (matchesQuery(s.supplyName)) matches.push({ id: s.supplyId, name: s.supplyName, type: 'supply', target: 'supplies' })
        })
    }

    searchResults.value = matches.slice(0, 8) 
}

const navigateSearchResult = (res) => {
    searchQuery.value = ''
    searchResults.value = []
    isSearchFocused.value = false
    activeMenu.value = res.target
}

const getSearchIcon = (type) => {
    switch(type) {
        case 'company': return Building2
        case 'contract': return FileText
        case 'staff': return UsersIcon
        case 'group': return Stethoscope
        case 'supply': return Package
        default: return Bot
    }
}

const getResTypeName = (type) => {
    const map = { company: 'Cơ quan/Đối tác', contract: 'Hợp đồng pháp lý', staff: 'Nhân sự y tế', group: 'Đoàn đang vận hành', supply: 'Vật tư trong kho' }
    return map[type] || 'Dữ liệu hệ thống'
}

// Global Keyboard Shortcut (Ctrl+K)
const handleGlobalKeyDown = (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault()
        const searchInput = document.querySelector('input[placeholder*="Ctrl+K"]')
        searchInput?.focus()
    }
    if (e.key === 'Escape') {
        isSearchFocused.value = false
        isUserMenuOpen.value = false
    }
}

onMounted(() => {
    fetchSearchData()
    fetchResetRequests()
    
    // Poll for reset requests every 30s
    if (authStore.role === 'Admin') {
        setInterval(fetchResetRequests, 30000)
    }

    window.addEventListener('keydown', handleGlobalKeyDown)
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.group') && !e.target.closest('input')) {
            isSearchFocused.value = false
        }
    })
})
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in-up {
  animation: fade-in-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes scale-up {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.animate-scale-up {
  animation: scale-up 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
