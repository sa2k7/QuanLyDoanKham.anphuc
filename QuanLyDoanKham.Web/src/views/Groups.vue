<template>
  <div class="space-y-10 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-indigo-600 text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-indigo-200">
            <Stethoscope class="w-8 h-8" />
          </div>
          Vận hành Đoàn khám
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">Điều phối thực địa & Điều động nhân lực</p>
      </div>

      <div class="flex items-center gap-4 w-full md:w-auto">
        <div class="relative flex-1 md:w-80 group">
            <Search class="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 w-5 h-5 group-focus-within:text-primary transition-colors" />
            <input v-model="searchQuery" placeholder="Tìm đoàn hoặc mã #ID..." 
                   class="w-full pl-16 pr-6 py-4 rounded-2xl bg-white border-2 border-slate-50 focus:border-primary/20 outline-none font-bold text-slate-600 shadow-sm shadow-slate-100" />
        </div>

        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                @click="showForm = !showForm" 
                class="btn-premium bg-slate-900 text-white hover:bg-black shadow-2xl shadow-slate-200 py-4 px-10">
            <Plus class="w-6 h-6" />
            <span>KHỞI TẠO LỆNH</span>
        </button>
      </div>
    </div>

    <!-- Creation Area -->
    <div v-if="showForm" class="premium-card p-10 bg-white border-4 border-indigo-100 mb-12 animate-slide-up">
        <div class="flex items-center gap-4 mb-10">
            <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center">
                <Rocket class="w-7 h-7" />
            </div>
            <div>
                <h3 class="text-2xl font-black text-slate-800 tracking-tight">Kích hoạt Đoàn khám</h3>
                <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mt-1">Phân bổ nguồn lực cho hợp đồng pháp lý</p>
            </div>
        </div>
        <form @submit.prevent="addGroup" class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Hợp đồng mục tiêu</label>
                <select v-model="newGroup.healthContractId" required class="input-premium">
                    <option v-if="availableContracts.length === 0" disabled value="null">Không có hợp đồng sẵn sàng</option>
                    <option v-else disabled value="null">-- Chọn hợp đồng vận hành --</option>
                    <option v-for="c in availableContracts" :key="c.healthContractId" :value="c.healthContractId">{{ c.companyName }} (#{{c.healthContractId}})</option>
                </select>
            </div>
            <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Tên định danh đoàn</label>
                <input v-model="newGroup.groupName" required class="input-premium" placeholder="VD: Khám khối Văn phòng" />
            </div>
            <div class="flex items-end gap-6">
                <div class="flex-1 space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Thời gian triển khai</label>
                    <input v-model="newGroup.examDate" type="date" required class="input-premium" />
                </div>
                <button type="submit" class="btn-premium bg-indigo-600 text-white hover:bg-indigo-700 h-[60px] px-10">XÁC NHẬN</button>
            </div>
        </form>
    </div>

    <!-- Active Groups Area -->
    <div v-if="activeGroups.length > 0" class="space-y-12">
        <h3 class="text-xs font-black text-slate-400 uppercase tracking-[0.3em] flex items-center gap-4">
            <span class="w-12 h-[2px] bg-indigo-100"></span>
            Đoàn khám đang vận hành
            <span class="bg-indigo-600 text-white px-3 py-1 rounded-lg text-[10px]">{{ activeGroups.length }}</span>
        </h3>
        
        <div v-for="group in activeGroups" :key="group.groupId" 
             class="premium-card bg-white border-2 border-slate-50 overflow-hidden group/card animate-fade-in-up">
            
            <!-- Dark Premium Header -->
            <div class="p-10 bg-slate-900 text-white relative overflow-hidden">
                <div class="absolute right-0 top-0 bottom-0 w-1/3 bg-gradient-to-l from-indigo-500/10 to-transparent"></div>
                
                <div class="flex flex-col lg:flex-row justify-between lg:items-center gap-8 relative z-10">
                    <div class="flex items-center gap-8">
                        <div class="w-20 h-20 bg-indigo-500 rounded-[2rem] flex items-center justify-center shadow-2xl shadow-indigo-500/20 group-hover/card:scale-110 transition-transform duration-500">
                            <Stethoscope class="w-10 h-10 text-white" />
                        </div>
                        <div>
                            <div class="flex items-center gap-3 mb-2">
                                <span class="text-[10px] font-black text-indigo-400 uppercase tracking-[0.2em]">Đội ngũ vận hành #{{ group.groupId }}</span>
                                <div class="px-3 py-1 bg-indigo-500/20 text-indigo-400 text-[8px] font-black uppercase rounded-lg border border-indigo-500/30 animate-pulse">Live Now</div>
                            </div>
                            <h4 class="text-3xl font-black tracking-tight mb-2">{{ group.groupName }}</h4>
                            <div class="flex items-center gap-6 text-slate-400 text-xs font-bold uppercase tracking-widest">
                                <span class="flex items-center gap-2 text-indigo-400"><Building2 class="w-4 h-4" /> {{ group.companyName }}</span>
                                <span class="flex items-center gap-2"><Calendar class="w-4 h-4" /> {{ formatDate(group.examDate) }}</span>
                            </div>
                        </div>
                    </div>
                    
                    <div class="flex items-center gap-4">
                        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                                @click="handleFinishGroup(group)"
                                class="px-8 py-4 bg-white/5 hover:bg-rose-500/20 hover:text-rose-400 border border-white/10 hover:border-rose-500/30 rounded-2xl font-black text-[10px] uppercase tracking-widest transition-all">
                            HOÀN TẤT ĐOÀN
                        </button>
                    </div>
                </div>
            </div>

                    <div class="p-10 grid grid-cols-1 lg:grid-cols-2 gap-12">
                        <!-- Staff Section -->
                        <div class="space-y-8">
                            <div class="flex justify-between items-center px-2">
                                <h5 class="text-xs font-black uppercase tracking-[0.2em] text-slate-400 flex items-center gap-3">
                                    <UsersIcon class="w-4 h-4" /> Nhân sự thực địa
                                </h5>
                                <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                                        @click="openAddStaff(group.groupId)" 
                                        class="flex items-center gap-2 text-[10px] font-black uppercase text-indigo-600 hover:text-indigo-800 transition-colors">
                                    <PlusCircle class="w-4 h-4" /> ĐIỀU ĐỘNG
                                </button>
                            </div>
                            
                            <div class="space-y-4">
                                <div v-for="s in staffDetails[group.groupId]" :key="s.id" 
                                     class="flex justify-between items-center bg-white p-5 rounded-[2rem] border-2 border-slate-50 hover:border-indigo-100/50 hover:shadow-xl hover:shadow-indigo-500/5 transition-all group/item">
                                    <div class="flex items-center gap-5">
                                        <div class="w-14 h-14 rounded-2xl border-4 border-slate-50 shadow-sm overflow-hidden group-hover/item:scale-105 transition-transform">
                                            <img :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${s.fullName}`" class="w-full h-full object-cover" />
                                        </div>
                                        <div>
                                            <p class="font-black text-slate-800 text-sm tracking-tight">{{ s.fullName }}</p>
                                            <div class="flex items-center gap-2 mt-1">
                                                <span class="text-[9px] font-black uppercase tracking-wider text-indigo-500">{{ s.jobTitle }}</span>
                                                <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                                                <span class="text-[9px] font-black uppercase tracking-wider text-slate-400">{{ s.shiftType === 1 ? 'Full' : 'Half' }}</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="text-right">
                                        <p class="font-black text-slate-900 tracking-tighter">{{ formatPrice(s.calculatedSalary) }}</p>
                                        <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                                                @click="removeStaff(s.id, group.groupId)" 
                                                class="text-[9px] font-black text-rose-300 hover:text-rose-500 uppercase tracking-widest mt-1 block ml-auto transition-colors">GỠ</button>
                                    </div>
                                </div>
                                <div v-if="!staffDetails[group.groupId]?.length" class="text-center py-20 bg-slate-50/50 border-4 border-dashed border-slate-100 rounded-[2.5rem]">
                                    <UsersIcon class="w-12 h-12 text-slate-100 mx-auto mb-4" />
                                    <p class="text-[10px] font-black text-slate-300 uppercase tracking-widest">Chờ điều phối nhân sự</p>
                                </div>
                            </div>
                        </div>

                        <!-- Supply Section -->
                        <div class="space-y-8">
                            <div class="flex justify-between items-center px-2">
                                <h5 class="text-xs font-black uppercase tracking-[0.2em] text-slate-400 flex items-center gap-3">
                                    <Package class="w-4 h-4" /> Quản trị vật tư
                                </h5>
                                <button v-if="authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager'" 
                                        @click="openAddSupply(group.groupId)" 
                                        class="flex items-center gap-2 text-[10px] font-black uppercase text-indigo-600 hover:text-indigo-800 transition-colors">
                                    <PlusCircle class="w-4 h-4" /> XUẤT KHO
                                </button>
                            </div>
                            
                            <div class="space-y-4">
                                <div v-for="su in supplyDetails[group.groupId]" :key="su.id" 
                                     class="flex justify-between items-center bg-white p-5 rounded-[2rem] border-2 border-slate-50 hover:border-indigo-100/50 hover:shadow-xl hover:shadow-indigo-500/5 transition-all group/supply">
                                    <div class="flex items-center gap-5">
                                        <div class="w-14 h-14 bg-indigo-50 rounded-2xl flex items-center justify-center text-indigo-600 shadow-sm group-hover/supply:rotate-6 transition-transform">
                                            <Package class="w-7 h-7" />
                                        </div>
                                        <div>
                                            <p class="font-black text-slate-800 text-sm tracking-tight">{{ su.supplyName }}</p>
                                            <div class="flex items-center gap-2 mt-1">
                                                <span class="text-[9px] font-black uppercase tracking-wider text-indigo-500">Dùng: {{ su.quantityUsed }}</span>
                                                <div v-if="su.returnQuantity > 0" class="flex items-center gap-2">
                                                    <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                                                    <span class="text-[9px] font-black uppercase tracking-wider text-emerald-500">Trả: {{ su.returnQuantity }}</span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="flex items-center gap-6">
                                        <button v-if="su.quantityUsed > su.returnQuantity && (authStore.role === 'Admin' || authStore.role === 'MedicalGroupManager' || authStore.role === 'WarehouseManager')" 
                                                @click="returnSupplyForm(su, group.groupId)" 
                                                class="text-[9px] font-black uppercase text-emerald-400 hover:text-emerald-600 transition-colors">TRẢ</button>
                                        <p class="font-black text-slate-900 tracking-tighter">{{ formatPrice(su.totalCost) }}</p>
                                    </div>
                                </div>
                                <div v-if="!supplyDetails[group.groupId]?.length" class="text-center py-20 bg-slate-50/50 border-4 border-dashed border-slate-100 rounded-[2.5rem]">
                                    <Package class="w-12 h-12 text-slate-100 mx-auto mb-4" />
                                    <p class="text-[10px] font-black text-slate-300 uppercase tracking-widest">Chờ xuất vật tư tiêu hao</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

    <!-- LỊCH SỬ SECTION -->
    <div v-if="finishedGroups.length > 0" class="space-y-6 pt-10 border-t border-gray-100">
        <h3 class="text-lg font-bold text-gray-400 flex items-center px-4">
            <div class="w-1.5 h-6 bg-emerald-500 rounded-full mr-3"></div>
            LỊCH SỬ ĐOÀN KHÁM
        </h3>
        <div class="grid grid-cols-1 gap-4">
            <div v-for="group in finishedGroups" :key="group.groupId" class="bg-gray-50 p-6 rounded-2xl border border-gray-100 flex justify-between items-center hover:bg-white hover:shadow-md transition-all">
                <div class="flex items-center space-x-4">
                    <div class="bg-emerald-50 p-3 rounded-xl">
                        <CheckCircle2 class="text-emerald-500 w-6 h-6" />
                    </div>
                    <div>
                        <h4 class="font-bold text-gray-700">{{ group.groupName }}</h4>
                        <p class="text-[10px] font-bold text-gray-400 uppercase tracking-wider mt-0.5">{{ group.companyName }} • {{ formatDate(group.examDate) }}</p>
                    </div>
                </div>
                <div class="flex items-center space-x-10">
                    <div class="text-right">
                        <p class="text-[9px] font-bold text-gray-400 uppercase tracking-wider mb-0.5">CHI PHÍ NHÂN SỰ</p>
                        <p class="font-bold text-gray-600">{{ formatPrice(calculateTotalStaffSalary(group.groupId)) }}</p>
                    </div>
                    <div class="text-right border-l border-gray-200 pl-10">
                        <p class="text-[9px] font-bold text-gray-400 uppercase tracking-wider mb-0.5">MÃ ĐOÀN</p>
                        <p class="font-bold text-emerald-600 italic">#{{ group.groupId }}</p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Staff Allocation Modal with Role Filtering -->
    <div v-if="modals.staff.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-lg p-10 rounded-[3rem] shadow-2xl animate-fade-in-up">
            <h3 class="text-2xl font-black text-slate-800 mb-6 text-center uppercase tracking-tighter">Phân bổ nhân sự</h3>
            
            <!-- Role Filter Tabs -->
            <div class="flex space-x-2 mb-6 overflow-x-auto">
                <button v-for="role in staffRoles" :key="role" 
                        type="button"
                        @click="selectedRole = role"
                        :class="['px-4 py-2 rounded-xl font-black text-xs whitespace-nowrap transition-all', 
                                 selectedRole === role ? 'bg-primary text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200']">
                    {{ role }} ({{ getStaffCountByRole(role) }})
                </button>
            </div>
            
            <form @submit.prevent="addStaffToGroup" class="space-y-6">
                <div>
                  <label class="text-xs font-black uppercase tracking-widest text-slate-400">Chọn Nhân sự</label>
                  <select v-model="modals.staff.data.staffId" required class="w-full px-5 py-4 rounded-2xl bg-slate-50 font-bold border-2 border-transparent focus:border-primary/20 outline-none mt-2">
                    <option value="" disabled>-- Chọn nhân viên --</option>
                    <option v-for="s in filteredStaffList" :key="s.staffId" :value="s.staffId">{{ s.fullName }} ({{s.jobTitle}})</option>
                  </select>
                  <p v-if="filteredStaffList.length === 0" class="text-xs text-rose-500 font-bold mt-2">❌ Không còn nhân sự khả dụng trong vai trò này</p>
                </div>
                <div>
                  <label class="text-xs font-black uppercase tracking-widest text-slate-400">Loại ca làm</label>
                  <select v-model="modals.staff.data.shiftType" class="w-full px-5 py-4 rounded-2xl bg-slate-50 font-bold border-2 border-transparent focus:border-primary/20 outline-none mt-2">
                    <option :value="1.0">Cả ngày (100% lương)</option>
                    <option :value="0.5">Sáng/Chiều (50% lương)</option>
                  </select>
                </div>
                <div class="flex space-x-4 pt-4">
                    <button type="button" @click="closeStaffModal" class="flex-1 font-bold text-slate-400 hover:bg-slate-100 rounded-xl py-3 transition-all">Hủy</button>
                    <button type="submit" :disabled="!modals.staff.data.staffId" class="flex-[2] bg-primary text-white py-4 rounded-2xl font-black shadow-lg shadow-primary/20 disabled:opacity-50 disabled:cursor-not-allowed">XÁC NHẬN</button>
                </div>
            </form>
        </div>
    </div>

    <!-- Supply Export Modal -->
    <div v-if="modals.supply.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-md p-8 rounded-2xl shadow-2xl animate-fade-in-up">
            <h3 class="text-xl font-bold text-gray-800 mb-8 text-center uppercase tracking-wider">Xuất vật tư từ kho</h3>
            <form @submit.prevent="addSupplyToGroup" class="space-y-6">
                <div>
                  <label class="text-[10px] font-bold uppercase tracking-widest text-gray-400">Chọn Vật tư</label>
                  <select v-model="modals.supply.data.supplyId" required class="w-full px-4 py-3 rounded-xl bg-gray-50 border border-gray-100 focus:border-primary outline-none mt-2 font-medium">
                    <option value="" disabled>-- Chọn vật tư --</option>
                    <option v-for="s in supplyList" :key="s.supplyId" :value="s.supplyId">{{ s.supplyName }} (Còn: {{s.stockQuantity}})</option>
                  </select>
                </div>
                <div>
                  <label class="text-[10px] font-bold uppercase tracking-widest text-gray-400">Số lượng xuất</label>
                  <input v-model="modals.supply.data.quantityUsed" type="number" min="1" required class="w-full px-4 py-3 rounded-xl bg-gray-50 border border-gray-100 focus:border-primary outline-none mt-2 font-bold" />
                </div>
                <div class="flex space-x-4 pt-4">
                    <button type="button" @click="modals.supply.show = false" class="flex-1 font-bold text-gray-400 hover:bg-gray-50 rounded-xl py-3 transition-all">Hủy</button>
                    <button type="submit" :disabled="!modals.supply.data.supplyId" class="flex-[2] bg-primary text-white py-3 rounded-xl font-bold shadow-md shadow-primary/20 disabled:opacity-50">XUẤT KHO</button>
                </div>
            </form>
        </div>
    </div>

    <!-- Supply Return Modal -->
    <div v-if="modals.returnSupply.show" class="fixed inset-0 z-[100] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4">
        <div class="bg-white w-full max-w-md p-8 rounded-2xl shadow-2xl animate-fade-in-up">
            <h3 class="text-xl font-bold text-gray-800 mb-6 text-center uppercase tracking-wider">Hoàn trả vật tư</h3>
            <div class="bg-primary/5 p-4 rounded-xl mb-6 border border-primary/10">
                <p class="font-bold text-gray-800 mb-2">{{ modals.returnSupply.supply?.supplyName }}</p>
                <div class="flex justify-between text-xs font-medium text-gray-500">
                    <span>Đã xuất:</span>
                    <span class="font-bold text-gray-700">{{ modals.returnSupply.supply?.quantityUsed }}</span>
                </div>
                <div class="flex justify-between text-xs font-medium text-gray-500">
                    <span>Đã trả:</span>
                    <span class="font-bold text-emerald-600">{{ modals.returnSupply.supply?.returnQuantity }}</span>
                </div>
                <div class="flex justify-between text-xs font-bold text-gray-800 border-t border-primary/10 mt-2 pt-2">
                    <span>Có thể trả:</span>
                    <span class="text-primary">{{ modals.returnSupply.supply?.quantityUsed - modals.returnSupply.supply?.returnQuantity }}</span>
                </div>
            </div>
            <form @submit.prevent="confirmReturn" class="space-y-6">
                <div>
                  <label class="text-[10px] font-bold uppercase tracking-widest text-gray-400">Số lượng hoàn trả</label>
                  <input v-model="modals.returnSupply.returnQty" type="number" min="1" :max="modals.returnSupply.supply?.quantityUsed - modals.returnSupply.supply?.returnQuantity" required class="w-full px-4 py-3 rounded-xl bg-gray-50 border border-gray-100 focus:border-primary outline-none mt-2 text-2xl font-bold text-emerald-600" />
                </div>
                <div class="flex space-x-4 pt-4">
                    <button type="button" @click="modals.returnSupply.show = false" class="flex-1 font-bold text-gray-400 hover:bg-gray-50 rounded-xl py-3 transition-all">Hủy</button>
                    <button type="submit" class="flex-[2] bg-emerald-500 text-white py-3 rounded-xl font-bold shadow-md shadow-emerald-500/20">XÁC NHẬN</button>
                </div>
            </form>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { 
  Plus, ArrowDown, Stethoscope, Users, Package, CheckCircle2, 
  Building2, Calendar, Clock, ChevronRight, Search, X 
} from 'lucide-vue-next'
import { useToast } from '../composables/useToast'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const authStore = useAuthStore()
const toast = useToast()
const searchQuery = ref(route.query.q || '')
const groups = ref([])
const contracts = ref([])
const staffList = ref([])
const supplyList = ref([])
const showForm = ref(false)
const loading = ref(false)

// Details for each group
const staffDetails = ref({})
const supplyDetails = ref({})

const newGroup = ref({ healthContractId: null, groupName: '', examDate: new Date().toISOString().split('T')[0] })

// Add watcher for healthContractId
watch(() => newGroup.value.healthContractId, (newId) => {
    if (newId) {
        const selected = contracts.value.find(c => c.healthContractId === newId)
        if (selected) {
            newGroup.value.groupName = `Khám sức khỏe - ${selected.companyName}`
            // Auto fill date from contract start date
            if (selected.startDate) {
                newGroup.value.examDate = selected.startDate.split('T')[0]
            }
        }
    }
})

const modals = ref({
    staff: { show: false, groupId: null, data: { staffId: '', shiftType: 1.0 } },
    supply: { show: false, groupId: null, data: { supplyId: '', quantityUsed: 1 } },
    returnSupply: { show: false, supply: null, returnQty: 0 }
})

// Filtered groups by status
// Filtered groups by status and search query
const activeGroups = computed(() => {
    let list = groups.value.filter(g => !g.isFinished)
    if (searchQuery.value) {
        const q = searchQuery.value.toString().toLowerCase()
        list = list.filter(g => g.groupName.toLowerCase().includes(q) || g.groupId.toString() === q)
    }
    return list
})

const finishedGroups = computed(() => {
    let list = groups.value.filter(g => g.isFinished)
    if (searchQuery.value) {
        const q = searchQuery.value.toString().toLowerCase()
        list = list.filter(g => g.groupName.toLowerCase().includes(q) || g.groupId.toString() === q)
    }
    return list
})

const calculateTotalStaffSalary = (groupId) => {
    return (staffDetails.value[groupId] || []).reduce((sum, s) => sum + s.calculatedSalary, 0)
}

// Filtered contracts that can have a new group created (Only one group per contract)
const availableContracts = computed(() => {
    return contracts.value.filter(c => {
        // Hợp đồng chưa kết thúc hoàn toàn
        if (c.isFinished) return false;
        
        // Kiểm tra xem hợp đồng này đã được tạo đoàn khám (isFinished hay không đều chặn) chưa
        const hasExistingGroup = groups.value.some(g => g.healthContractId === c.healthContractId);
        
        // Chỉ hiện nếu chưa bao giờ được tạo đoàn khám
        return !hasExistingGroup;
    });
});

// Staff role filtering
const staffRoles = ref(['Tất cả', 'Bác sĩ', 'Điều dưỡng', 'Kỹ thuật viên'])
const selectedRole = ref('Tất cả')

// Filtered staff list based on role and availability
const filteredStaffList = computed(() => {
    const groupId = modals.value.staff.groupId
    if (!groupId) return []
    
    const allocatedStaffIds = (staffDetails.value[groupId] || []).map(s => s.staffId)
    
    return staffList.value.filter(s => {
        // Filter by role
        if (selectedRole.value !== 'Tất cả' && s.jobTitle !== selectedRole.value) return false
        
        // Filter out already allocated staff
        return !allocatedStaffIds.includes(s.staffId)
    })
})

// Get staff count by role (for tab badges)
const getStaffCountByRole = (role) => {
    const groupId = modals.value.staff.groupId
    if (!groupId) return 0
    
    const allocatedStaffIds = (staffDetails.value[groupId] || []).map(s => s.staffId)
    
    if (role === 'Tất cả') {
        return staffList.value.filter(s => !allocatedStaffIds.includes(s.staffId)).length
    }
    
    return staffList.value.filter(s => 
        s.jobTitle === role && !allocatedStaffIds.includes(s.staffId)
    ).length
}

const fetchData = async () => {
    loading.value = true
    try {
        const fetchPromises = [
            axios.get('http://localhost:5283/api/MedicalGroups'),
            axios.get('http://localhost:5283/api/HealthContracts')
        ]

        if (authStore.role === 'Admin' || authStore.role === 'Staff') {
            fetchPromises.push(axios.get('http://localhost:5283/api/Staffs'))
            fetchPromises.push(axios.get('http://localhost:5283/api/Supplies'))
        }

        const responses = await Promise.all(fetchPromises)
        
        groups.value = responses[0].data
        contracts.value = responses[1].data
        
        if (responses[2]) staffList.value = responses[2].data
        if (responses[3]) supplyList.value = responses[3].data
        
        // Fetch details for each group
        groups.value.forEach(g => {
            fetchStaffDetails(g.groupId)
            fetchSupplyDetails(g.groupId)
        })
    } catch (e) { 
        console.error(e)
        toast.error("Không thể tải dữ liệu đoàn khám. Vui lòng kiểm tra kết nối mạng.")
    } finally {
        loading.value = false
    }
}

const fetchStaffDetails = async (gid) => {
    const res = await axios.get(`http://localhost:5283/api/GroupDetails/staff/${gid}`)
    staffDetails.value[gid] = res.data
}

const fetchSupplyDetails = async (gid) => {
    const res = await axios.get(`http://localhost:5283/api/GroupDetails/supply/${gid}`)
    supplyDetails.value[gid] = res.data
}

const handleFinishGroup = async (group) => {
    if (!confirm(`Bạn có chắc muốn kết thúc đoàn khám "${group.groupName}"? Sau khi kết thúc, bạn sẽ không thể phân bổ thêm nhân sự hoặc vật tư.`)) return
    
    try {
        await axios.put(`http://localhost:5283/api/MedicalGroups/${group.groupId}/finish`)
        toast.success('Đã kết thúc đoàn khám thành công!')
        fetchData()
    } catch (e) {
        toast.error(e.response?.data?.message || 'Không thể kết thúc đoàn khám')
    }
}

const addGroup = async () => {
    try {
        await axios.post('http://localhost:5283/api/MedicalGroups', newGroup.value)
        toast.success('Đã tạo đoàn khám mới thành công!')
        showForm.value = false
        fetchData()
    } catch (e) {
        console.error(e)
        // API returns string directly for BadRequest
        const errorMsg = typeof e.response?.data === 'string' ? e.response.data : (e.response?.data?.message || "Không thể tạo đoàn khám")
        toast.error(errorMsg)
    }
}

const openAddStaff = (gid) => { 
    modals.value.staff.groupId = gid
    modals.value.staff.data = { staffId: '', shiftType: 1.0 }
    selectedRole.value = 'Tất cả'
    modals.value.staff.show = true 
}

const openAddSupply = (gid) => { 
    modals.value.supply.groupId = gid
    modals.value.supply.data = { supplyId: '', quantityUsed: 1 }
    modals.value.supply.show = true 
}

const closeStaffModal = () => {
    modals.value.staff.show = false
    modals.value.staff.data = { staffId: '', shiftType: 1.0 }
    selectedRole.value = 'Tất cả'
}

const addStaffToGroup = async () => {
    try {
        const gid = modals.value.staff.groupId
        await axios.post('http://localhost:5283/api/GroupDetails/staff', { ...modals.value.staff.data, groupId: gid })
        toast.success('Đã phân bổ nhân sự thành công!')
        closeStaffModal()
        fetchStaffDetails(gid)
    } catch (e) {
        console.error(e)
        const errorMsg = e.response?.data || 'Không thể phân bổ nhân sự'
        toast.error(errorMsg)
    }
}

const addSupplyToGroup = async () => {
    try {
        const gid = modals.value.supply.groupId
        const supplyId = modals.value.supply.data.supplyId
        const qty = modals.value.supply.data.quantityUsed
        
        // Front-end Validation
        const selectedSupply = supplyList.value.find(s => s.supplyId === supplyId)
        if (selectedSupply && selectedSupply.stockQuantity < qty) {
            toast.warning(`Số lượng trong kho không đủ! (Hiện có: ${selectedSupply.stockQuantity})`)
            return
        }

        await axios.post('http://localhost:5283/api/GroupDetails/supply', { ...modals.value.supply.data, groupId: gid })
        toast.success('Xuất vật tư thành công!')
        modals.value.supply.show = false
        fetchSupplyDetails(gid)
        fetchData()
    } catch (e) {
        console.error(e)
        const errorMsg = e.response?.data || "Không thể xuất kho"
        toast.error(errorMsg)
    }
}

const returnSupplyForm = (supply, groupId) => {
    modals.value.returnSupply = {
        show: true,
        supply: { ...supply, groupId },
        returnQty: 1
    }
}

const confirmReturn = async () => {
    try {
        const { supply, returnQty } = modals.value.returnSupply
        const maxReturn = supply.quantityUsed - supply.returnQuantity
        
        if (returnQty > maxReturn) {
            toast.warning(`Chỉ có thể trả tối đa ${maxReturn} ${supply.supplyName}`)
            return
        }
        
        await axios.put(`http://localhost:5283/api/GroupDetails/supply/${supply.id}/return`, { returnQuantity: returnQty })
        toast.success(`Đã hoàn trả ${returnQty} ${supply.supplyName}`)
        modals.value.returnSupply.show = false
        fetchSupplyDetails(supply.groupId)
        fetchData()
    } catch (e) {
        console.error(e)
        const errorMsg = e.response?.data || 'Không thể hoàn trả vật tư'
        toast.error(errorMsg)
    }
}

const removeStaff = async (id, gid) => {
    try {
        await axios.delete(`http://localhost:5283/api/GroupDetails/staff/${id}`)
        toast.success('Đã xóa nhân sự khỏi đoàn khám')
        fetchStaffDetails(gid)
    } catch (e) {
        console.error(e)
        toast.error('Không thể xóa nhân sự')
    }
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)
const formatDate = (d) => new Date(d).toLocaleDateString('vi-VN')

onMounted(fetchData)
</script>
