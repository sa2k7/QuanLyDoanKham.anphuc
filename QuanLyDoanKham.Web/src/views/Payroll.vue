<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-8 mb-10">
      <div>
        <h2 class="text-4xl font-black tracking-tighter text-slate-800 flex items-center gap-4">
          <div class="w-14 h-14 bg-emerald-600 text-white rounded-[1.5rem] flex items-center justify-center shadow-2xl shadow-emerald-200">
            <Wallet class="w-8 h-8" />
          </div>
          Quản lý Lương & Thù lao
        </h2>
        <p class="text-slate-400 font-bold uppercase tracking-widest text-[10px] mt-4 ml-1">Tổng hợp thu nhập & Thù lao thực địa định kỳ</p>
      </div>
      
      <div class="flex items-center gap-4 bg-white p-2 rounded-3xl shadow-sm border-2 border-slate-50 font-bold">
        <div class="flex items-center gap-2 px-6 py-3 bg-slate-50 rounded-2xl">
            <Calendar class="w-5 h-5 text-indigo-500" />
            <select v-model="selectedMonth" class="bg-transparent font-black text-slate-700 outline-none cursor-pointer text-sm">
                <option v-for="m in 12" :key="m" :value="m">Tháng {{ m }}</option>
            </select>
        </div>
        <div class="flex items-center gap-2 px-6 py-3 bg-slate-50 rounded-2xl">
            <input type="number" v-model="selectedYear" class="w-16 bg-transparent font-black text-slate-700 outline-none text-sm" />
        </div>
        <button @click="fetchPayroll" class="bg-indigo-600 hover:bg-slate-900 text-white p-4 rounded-2xl transition-all active:scale-95 shadow-xl shadow-indigo-100/50">
            <RefreshCcw :class="['w-5 h-5', loading ? 'animate-spin' : '']" />
        </button>
      </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
        <!-- Card 1: Quỹ lương -->
        <div class="premium-card p-10 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <div class="flex justify-between items-start mb-4">
                <div class="w-16 h-16 bg-emerald-50 text-emerald-600 rounded-2xl flex items-center justify-center group-hover:bg-emerald-600 group-hover:text-white transition-all duration-500">
                    <Banknote class="w-8 h-8" />
                </div>
                <div class="text-right">
                    <p class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] mb-2">QUỸ LƯƠNG DỰ KIẾN</p>
                    <p class="text-3xl font-black text-slate-900 tracking-tighter">{{ formatPrice(totalPayroll) }}</p>
                </div>
            </div>
            <div class="grid grid-cols-2 gap-4 mt-8 pt-8 border-t border-slate-50">
                <div>
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">LƯƠNG CỨNG</p>
                    <p class="font-black text-slate-700 text-sm">{{ formatPrice(payrollList.reduce((sum, p) => sum + p.baseSalary, 0)) }}</p>
                </div>
                <div class="text-right">
                    <p class="text-[9px] font-black text-emerald-400 uppercase tracking-widest mb-1">THÙ LAO ĐOÀN</p>
                    <p class="font-black text-emerald-600 text-sm">+ {{ formatPrice(payrollList.reduce((sum, p) => sum + p.groupEarnings, 0)) }}</p>
                </div>
            </div>
            <div class="absolute top-0 right-0 w-32 h-32 bg-emerald-50 opacity-10 rounded-full -mr-16 -mt-16 transition-all group-hover:scale-150"></div>
        </div>

        <!-- Card 2: Nhân sự & Hiệu suất -->
        <div class="premium-card p-10 bg-white border-2 border-slate-50 relative overflow-hidden group">
            <div class="flex justify-between items-start mb-6">
                <div class="w-16 h-16 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center group-hover:bg-indigo-600 group-hover:text-white transition-all duration-500">
                    <Users class="w-8 h-8" />
                </div>
                <div class="text-right">
                    <p class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] mb-2">NHÂN SỰ THỰC ĐỊA</p>
                    <p class="text-5xl font-black text-slate-900 tracking-tighter">{{ payrollList.filter(p => p.groupEarnings > 0).length }}</p>
                </div>
            </div>
            <div class="space-y-4">
                <div class="flex items-center gap-3">
                    <div class="flex-1 h-3 bg-slate-50 rounded-full overflow-hidden border border-slate-100">
                        <div class="h-full bg-indigo-500 transition-all duration-1000" 
                             :style="{ width: `${(payrollList.filter(p => p.groupEarnings > 0).length / (payrollList.length || 1)) * 100}%` }"></div>
                    </div>
                    <span class="text-[10px] font-black text-slate-400 uppercase tracking-widest whitespace-nowrap">Tổng {{ payrollList.length }} NV</span>
                </div>
                <p class="text-[10px] font-black text-indigo-500 uppercase tracking-[0.2em] flex items-center gap-2">
                    <div class="w-2 h-2 bg-indigo-500 rounded-full animate-pulse"></div>
                    {{ payrollList.reduce((sum, p) => sum + p.totalShifts, 0).toFixed(1) }} TỔNG HỆ SỐ CÔNG
                </p>
            </div>
        </div>

        <!-- Card 3: Trạng thái & Thao tác -->
        <div class="premium-card p-10 bg-slate-900 text-white relative overflow-hidden group flex flex-col justify-between">
            <div class="flex justify-between items-start relative z-10">
                <div class="w-16 h-16 bg-white/10 text-amber-400 rounded-2xl flex items-center justify-center">
                    <Layers class="w-8 h-8" />
                </div>
                <div class="flex flex-col items-end">
                    <span class="text-[10px] font-black uppercase text-slate-500 tracking-widest mb-2">Trạng thái kỳ này</span>
                    <span class="px-4 py-2 bg-amber-500/20 text-amber-400 rounded-xl text-[10px] font-black uppercase tracking-widest flex items-center gap-2 border border-amber-500/30">
                        <span class="w-1.5 h-1.5 bg-amber-400 rounded-full animate-pulse"></span>
                        Bản nháp
                    </span>
                </div>
            </div>
            
            <button @click="confirmPayroll" class="mt-8 w-full py-5 bg-indigo-600 text-white rounded-2xl text-[10px] font-black uppercase tracking-[0.3em] hover:bg-white hover:text-slate-900 transition-all active:scale-95 shadow-2xl shadow-indigo-500/20 flex items-center justify-center gap-3 group/btn relative z-10">
                <span>CHỐT & GIẢI NGÂN</span>
                <ChevronRight class="w-4 h-4 group-hover/btn:translate-x-2 transition-transform" />
            </button>
            <div class="absolute bottom-0 right-0 w-40 h-40 bg-white/5 rounded-full -mb-20 -mr-20"></div>
        </div>
    </div>

    <!-- Payroll Table -->
    <div class="premium-card bg-white border-2 border-slate-50 overflow-hidden min-h-[500px] mb-20">
        <div class="p-10 border-b border-slate-50 flex flex-col lg:flex-row justify-between items-center gap-8 bg-slate-50/30">
            <div class="flex flex-col sm:flex-row gap-6 w-full lg:w-auto">
                <div class="relative w-full sm:w-80 group">
                    <Search class="absolute left-6 top-1/2 -translate-y-1/2 text-slate-300 w-5 h-5 group-focus-within:text-indigo-600 transition-colors" />
                    <input v-model="searchQuery" placeholder="Tìm tên nhân viên..." 
                           class="w-full pl-16 pr-6 py-4 rounded-2xl bg-white border-2 border-slate-100 focus:border-indigo-600/20 outline-none transition-all font-black text-slate-600 text-sm shadow-sm" />
                </div>
                <div class="relative w-full sm:w-64 group">
                    <select v-model="selectedRole" class="w-full pl-6 pr-12 py-4 rounded-2xl bg-white border-2 border-slate-100 focus:border-indigo-600/20 outline-none appearance-none font-black text-slate-600 text-sm cursor-pointer shadow-sm">
                        <option v-for="r in staffRoles" :key="r" :value="r">{{ r }}</option>
                    </select>
                    <div class="absolute right-6 top-1/2 -translate-y-1/2 pointer-events-none text-slate-300 group-hover:text-indigo-600 transition-colors">
                        <ArrowDown class="w-4 h-4" />
                    </div>
                </div>
            </div>
            <div class="flex gap-4 w-full lg:w-auto">
                <button @click="exportExcel" class="btn-premium bg-emerald-600 text-white hover:bg-emerald-700 shadow-2xl shadow-emerald-100 px-10 py-4">
                    <Download class="w-6 h-6" />
                    <span>XUẤT BẢNG LƯƠNG</span>
                </button>
            </div>
        </div>

        <div v-if="loading" class="py-32 flex flex-col items-center justify-center gap-6">
            <div class="w-16 h-16 border-4 border-slate-100 border-t-indigo-600 rounded-full animate-spin"></div>
            <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Đang tổng hợp dữ liệu tài chính...</p>
        </div>

        <div v-else-if="filteredList.length === 0" class="py-40 flex flex-col items-center justify-center gap-6">
            <div class="w-20 h-20 bg-slate-50 rounded-full flex items-center justify-center text-slate-100">
                <Wallet class="w-10 h-10" />
            </div>
            <p class="text-slate-300 font-black uppercase tracking-widest text-xs">Không tìm thấy dữ liệu lương phù hợp</p>
        </div>

        <div v-else class="overflow-x-auto">
            <table class="professional-table">
                <thead>
                    <tr>
                        <th>Nhân sự thụ hưởng</th>
                        <th class="text-right">Lương Định mức</th>
                        <th class="text-center">Hệ số công</th>
                        <th class="text-right">Thù lao thực địa</th>
                        <th class="text-center">Trạng thái</th>
                        <th class="text-right">Thực lãnh (NET)</th>
                        <th class="w-20"></th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                    <tr v-for="item in filteredList" :key="item.staffId" 
                        class="hover:bg-slate-50/50 transition-all group"
                        :class="{'bg-emerald-50/20': item.groupEarnings > 0}">
                        <td>
                            <div class="flex items-center gap-4">
                                <div class="w-12 h-12 rounded-2xl bg-slate-100 flex items-center justify-center font-black text-slate-400 group-hover:bg-indigo-600 group-hover:text-white transition-all shadow-sm">
                                    {{ item.fullName.charAt(0) }}
                                </div>
                                <div>
                                    <div class="font-black text-slate-800 text-sm leading-tight">{{ item.fullName }}</div>
                                    <div class="text-[10px] font-bold text-slate-400 uppercase tracking-widest mt-1">{{ item.jobTitle }}</div>
                                </div>
                            </div>
                        </td>
                        <td class="text-right font-bold text-slate-500 text-sm">{{ formatPrice(item.baseSalary) }}</td>
                        <td class="text-center">
                            <span class="inline-flex items-center gap-2 px-3 py-1.5 bg-white border-2 border-slate-50 text-indigo-600 rounded-xl text-xs font-black shadow-sm">
                                {{ item.totalShifts }} <span class="text-[9px] text-slate-300 uppercase tracking-tighter">Hệ số</span>
                            </span>
                        </td>
                        <td class="text-right">
                            <div class="font-black text-indigo-600 text-base tracking-tight">{{ formatPrice(item.groupEarnings) }}</div>
                        </td>
                        <td class="text-center">
                            <div v-if="item.groupEarnings > 0" class="inline-flex items-center gap-2 px-3 py-1.5 bg-emerald-50 text-emerald-600 rounded-xl text-[9px] font-black uppercase tracking-widest border border-emerald-100">
                                <CheckCircle2 class="w-3 h-3" /> Đã quyết toán
                            </div>
                            <div v-else class="inline-flex items-center gap-2 px-3 py-1.5 bg-slate-50 text-slate-400 rounded-xl text-[9px] font-black uppercase tracking-widest">
                                <Clock class="w-3 h-3" /> Chờ đi đoàn
                            </div>
                        </td>
                        <td class="text-right">
                            <div class="px-5 py-3 bg-slate-900 text-white rounded-2xl font-black text-lg inline-block shadow-xl shadow-slate-200 tracking-tighter">
                                {{ formatPrice(item.totalSalary) }}
                            </div>
                        </td>
                         <td class="text-center">
                            <button @click="showDetails(item)" class="w-12 h-12 hover:bg-white rounded-2xl text-slate-300 hover:text-indigo-600 transition-all border border-transparent hover:border-slate-100 active:scale-95 flex items-center justify-center shadow-sm group/btn-v">
                                <FileText class="w-6 h-6 group-hover/btn-v:scale-110 transition-transform" />
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    <!-- Detail Modal (Payslip Style) -->
    <div v-if="detailItem" class="fixed inset-0 z-[100] flex items-center justify-center p-4 backdrop-blur-3xl bg-slate-900/60 animate-fade-in shadow-2xl">
        <div class="bg-white w-full max-w-3xl rounded-[3rem] shadow-2xl overflow-hidden flex flex-col animate-scale-up border-[12px] border-slate-50">
            <!-- Payslip Header -->
            <div class="p-10 border-b border-dashed border-slate-200 flex justify-between items-start relative bg-slate-50/50">
                <div class="flex gap-6">
                    <div class="w-20 h-20 bg-emerald-600 text-white rounded-[2rem] flex items-center justify-center shadow-xl shadow-emerald-100 transform -rotate-3">
                        <Banknote class="w-10 h-10" />
                    </div>
                    <div>
                        <h3 class="text-3xl font-black text-slate-900 tracking-tighter uppercase mb-1">Phiếu Lương Thù Lao</h3>
                        <p class="text-sm font-black text-slate-400 uppercase tracking-[0.2em] mb-4">THÁNG {{ selectedMonth }} • {{ selectedYear }}</p>
                        <div class="flex items-center gap-4">
                            <div class="bg-indigo-50 px-4 py-2 rounded-xl border border-indigo-100">
                                <p class="text-[10px] font-black text-indigo-400 uppercase tracking-widest leading-none mb-1">Nhân viên thụ hưởng</p>
                                <p class="text-base font-black text-indigo-700">{{ detailItem.fullName }}</p>
                            </div>
                            <div class="bg-slate-100 px-4 py-2 rounded-xl border border-slate-200">
                                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest leading-none mb-1">Mã NV</p>
                                <p class="text-base font-black text-slate-600">{{ detailItem.employeeCode || '---' }}</p>
                            </div>
                        </div>
                    </div>
                </div>
                <button @click="detailItem = null" class="p-4 hover:bg-white rounded-2xl transition-all shadow-sm border border-transparent hover:border-slate-100">
                    <X class="w-6 h-6 text-slate-300" />
                </button>
            </div>

            <!-- Content -->
            <div class="p-10 overflow-y-auto max-h-[50vh] custom-scrollbar space-y-10">
                <!-- Bank Info -->
                <div class="grid grid-cols-2 gap-6 p-6 bg-indigo-50/30 rounded-3xl border border-indigo-100/50">
                    <div>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Ngân hàng</p>
                        <p class="font-black text-slate-700">{{ detailItem.bankName || 'Chưa cập nhật' }}</p>
                    </div>
                    <div>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Số tài khoản</p>
                        <p class="font-black text-slate-700 tracking-wider">{{ detailItem.bankAccountNumber || '---' }}</p>
                    </div>
                </div>

                <!-- Earnings Table -->
                <div>
                    <h4 class="text-xs font-black text-slate-400 uppercase tracking-[0.3em] mb-6 flex items-center gap-2">
                        <div class="w-1.5 h-1.5 bg-indigo-500 rounded-full"></div> Chi tiết thù lao đoàn khám
                    </h4>
                    
                    <div v-if="detailItem.details.length === 0" class="text-center py-10 bg-slate-50 rounded-2xl border-2 border-dashed border-slate-100">
                        <p class="text-slate-300 font-bold italic">Không tham gia đoàn khám nào trong tháng này</p>
                    </div>
                    
                    <div v-else class="space-y-3">
                        <div v-for="d in detailItem.details" :key="d.groupId" 
                             @click="showQuickGroup(d)"
                             class="flex justify-between items-center p-5 bg-white rounded-2xl border border-slate-50 shadow-sm hover:shadow-md transition-all cursor-pointer group/item hover:border-indigo-200 relative">
                            <div class="flex items-center gap-4">
                                <div class="w-10 h-10 bg-slate-50 flex items-center justify-center rounded-xl text-slate-400 font-black text-xs group-hover/item:bg-indigo-600 group-hover/item:text-white transition-colors">#{{ d.groupId }}</div>
                                <div>
                                    <p class="font-black text-slate-800 group-hover/item:text-indigo-600 transition-colors">{{ d.groupName }}</p>
                                    <p class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">{{ new Date(d.examDate).toLocaleDateString('vi-VN') }} • Hệ số: {{ d.shiftType }}</p>
                                </div>
                            </div>
                            <div class="flex items-center gap-3">
                                <p class="font-black text-indigo-600">{{ formatPrice(d.calculatedSalary) }}</p>
                                <ChevronRight class="w-4 h-4 text-slate-300 group-hover/item:translate-x-1 transition-transform" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Summary Row -->
                <div class="pt-6 border-t border-slate-100 space-y-4">
                    <div class="flex justify-between items-center font-bold text-slate-500">
                        <span>Lương cố định định mức:</span>
                        <span>{{ formatPrice(detailItem.baseSalary) }}</span>
                    </div>
                    <div class="flex justify-between items-center font-bold text-indigo-500">
                        <span>Tổng thù lao đoàn khám (Hệ số {{ detailItem.totalShifts }}):</span>
                        <span>+ {{ formatPrice(detailItem.groupEarnings) }}</span>
                    </div>
                </div>
            </div>

            <!-- Footer Pay -->
            <div class="p-10 bg-slate-900 border-t border-slate-800 mt-auto flex justify-between items-center">
                <div class="text-left">
                    <p class="text-[10px] font-black text-slate-500 uppercase tracking-widest mb-1">Thực lĩnh cuối cùng</p>
                    <p class="text-4xl font-black text-emerald-400 tracking-tighter">{{ formatPrice(detailItem.totalSalary) }}</p>
                </div>
                <div class="flex gap-4">
                    <button @click="detailItem = null" class="px-8 py-4 bg-slate-800 hover:bg-slate-700 text-slate-400 rounded-2xl font-black transition-all">ĐÓNG</button>
                    <button @click="toast.info('Đã tải phiếu lương PDF')" class="px-8 py-4 bg-emerald-500 hover:bg-emerald-400 text-white rounded-2xl font-black shadow-lg shadow-emerald-900/40 transition-all flex items-center gap-2">
                        <Download class="w-5 h-5" /> TẢI PHIẾU
                    </button>
                </div>
            </div>
        </div>
    </div>
    <!-- Quick View "Bong Bóng" -->
    <div v-if="quickView" class="fixed inset-0 z-[110] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="quickView = null"></div>
        <div class="bg-white w-full max-w-sm rounded-[2rem] shadow-2xl relative animate-scale-up overflow-hidden border-2 border-slate-900">
            <div class="p-6 bg-slate-900 text-white flex justify-between items-center">
                <div>
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">Chi tiết nhanh</p>
                    <h5 class="font-black truncate w-48">{{ quickView.groupName }}</h5>
                </div>
                <button @click="quickView = null" class="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center hover:bg-white/20">✕</button>
            </div>
            
            <div class="p-6 space-y-6">
                <div v-if="quickLoading" class="flex flex-col items-center py-10 space-y-3">
                    <RefreshCcw class="w-8 h-8 text-primary animate-spin" />
                    <p class="text-xs font-bold text-slate-400 uppercase">Đang tải dữ liệu...</p>
                </div>
                <template v-else>
                    <div>
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-3">Nhân sự hiện trường</p>
                        <div class="space-y-2 max-h-32 overflow-y-auto custom-scrollbar pr-2">
                            <div v-for="s in quickData.staff" :key="s.staffId" class="flex justify-between items-center text-sm font-bold text-slate-600">
                                <span>{{ s.fullName }}</span>
                                <span class="text-[10px] px-2 py-0.5 bg-slate-100 rounded-md">x{{ s.shiftType }}</span>
                            </div>
                        </div>
                    </div>
                    
                    <div class="pt-4 border-t border-slate-50">
                        <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-3">Vật tư tiêu hao</p>
                        <div class="space-y-2 max-h-32 overflow-y-auto custom-scrollbar pr-2">
                            <div v-for="v in quickData.supplies" :key="v.id" class="flex justify-between items-center text-sm font-bold text-slate-600">
                                <span>{{ v.supplyName }}</span>
                                <span class="text-primary">{{ v.quantityUsed }} {{ v.unit }}</span>
                            </div>
                        </div>
                    </div>
                </template>
            </div>
            
            <button @click="router.push({ path: '/groups', query: { q: quickView.groupId } })" 
                    class="w-full py-4 bg-slate-50 text-primary text-[10px] font-black uppercase tracking-widest hover:bg-primary hover:text-white transition-all">
                ĐẾN TRANG QUẢN LÝ ĐOÀN KHÁM
            </button>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { 
  Wallet, Calendar, RefreshCcw, Banknote, Users, Layers, 
  Search, Download, Eye, FileText, X, ArrowUpRight, ArrowLeft, ArrowDown,
  CheckCircle2, ChevronRight
} from 'lucide-vue-next'
import { useToast } from '../composables/useToast'

const router = useRouter()
const toast = useToast()
const loading = ref(false)
const selectedMonth = ref(new Date().getMonth() + 1)
const selectedYear = ref(new Date().getFullYear())
const payrollList = ref([])
const searchQuery = ref('')
const selectedRole = ref('Tất cả')
const staffRoles = ref(['Tất cả', 'Bác sĩ', 'Điều dưỡng', 'Kỹ thuật viên', 'Kế toán'])
const detailItem = ref(null)

const quickView = ref(null)
const quickLoading = ref(false)
const quickData = ref({ staff: [], supplies: [] })

const showQuickGroup = async (group) => {
    quickView.value = group
    quickLoading.value = true
    try {
        const [sRes, vRes] = await Promise.all([
            axios.get(`http://localhost:5283/api/GroupDetails/staff/${group.groupId}`),
            axios.get(`http://localhost:5283/api/GroupDetails/supply/${group.groupId}`)
        ])
        quickData.value = { staff: sRes.data, supplies: vRes.data }
    } catch (e) {
        toast.error("Lỗi khi tải thông tin đoàn")
    } finally {
        quickLoading.value = false
    }
}

const totalPayroll = computed(() => payrollList.value.reduce((sum, item) => sum + item.totalSalary, 0))

const filteredList = computed(() => {
    let list = payrollList.value
    
    if (selectedRole.value !== 'Tất cả') {
        list = list.filter(p => p.jobTitle === selectedRole.value)
    }

    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase().trim()
        list = list.filter(p => 
            p.fullName.toLowerCase().includes(q) || 
            p.employeeCode?.toLowerCase().includes(q)
        )
    }
    
    return list
})

const fetchPayroll = async () => {
    loading.value = true
    try {
        const res = await axios.get(`http://localhost:5283/api/Payroll/monthly`, {
            params: { month: selectedMonth.value, year: selectedYear.value }
        })
        payrollList.value = res.data
    } catch (e) {
        toast.error("Không thể lấy dữ liệu lương!")
    } finally {
        loading.value = false
    }
}

const confirmPayroll = async () => {
    try {
        const res = await axios.post(`http://localhost:5283/api/Payroll/confirm`, null, {
            params: { month: selectedMonth.value, year: selectedYear.value }
        })
        toast.success(res.data.message)
    } catch (e) {
        toast.error("Lỗi khi chốt lương!")
    }
}

const exportExcel = async () => {
    try {
        loading.value = true
        const response = await axios.get(`http://localhost:5283/api/Payroll/export-monthly`, {
            params: { month: selectedMonth.value, year: selectedYear.value },
            responseType: 'blob'
        })
        
        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `BangLuong_${selectedMonth.value}_${selectedYear.value}.xlsx`)
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
        toast.success("Đã xuất bảng lương thành công!")
    } catch (e) {
        toast.error("Lỗi khi xuất bảng lương!")
    } finally {
        loading.value = false
    }
}

const showDetails = (item) => {
    detailItem.value = item
}

const formatPrice = (p) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p)

onMounted(fetchPayroll)

watch([selectedMonth, selectedYear], fetchPayroll)
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.animate-scale-up {
  animation: scale-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes scale-up {
  from { opacity: 0; transform: scale(0.9) translateY(20px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}
</style>
